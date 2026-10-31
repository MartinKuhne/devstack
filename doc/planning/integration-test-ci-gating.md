# INF-04 — Gating the integration suites in CI

Status: **proposal, awaiting decision** · 2026-10-31 · Lane G
Scope: how (and whether) `DevStack.Tests.Integration.GraphQL` and `DevStack.Tests.Integration.MCP` should block merges.
This document changes nothing. No workflow file was edited.

---

## 1. The problem in one paragraph

169 GraphQL scenarios and 55 MCP scenarios — including the entire 63-case error matrix — currently gate **nothing**.
`build-api.yml` runs `dotnet test --filter "FullyQualifiedName!~Integration"`, which explicitly excludes both suites.
`integration-tests.yml` is triggered **only** by `workflow_dispatch`; its `dorny/paths-filter` job is dead weight because no
`pull_request` trigger exists to filter. Net effect: a PR that breaks every GraphQL mutation passes CI.

| Workflow | Trigger today | Runs integration suites? |
|---|---|---|
| `build-api.yml` | push/PR on `src/Server/**` | ❌ filtered out by name |
| `integration-tests.yml` | `workflow_dispatch` only | ✅ but only when a human clicks it |

---

## 2. What a run actually costs

Measured inputs: each feature-class fixture builds its own app image via `ImageFromDockerfileBuilder` with
`WithImageBuildPolicy(_ => true)` (i.e. **rebuild on every fixture**) and then boots `postgres:17-alpine` + the app
container. The `DevStack.Api/Dockerfile` does a full `dotnet restore` + `dotnet publish` inside the image, so a cold
build is several minutes; overlay layer cache makes a rebuild ~1 min.

| Fact | Value | Source |
|---|---|---|
| GraphQL feature classes (own env each) | 7 | `*.feature` in the project |
| MCP feature classes (own env each) | 13 | incl. Lane F's `ApiHostedMcp`, `FeatureFlagOff` |
| GraphQL parallelism | **disabled** | `TestParallelization.cs` (INF-05: global `deleteTestData`) |
| MCP parallelism | enabled (xUnit default) | no `TestParallelization.cs` in that project |
| Cold image build | ~4–6 min | local observation |
| Warm (layer-cached) rebuild | ~1 min | local observation |
| Postgres + app boot + health check | ~30–45 s | local observation |

**Estimated wall-clock per suite** (estimates, not CI-measured — assumptions: 4 vCPU / 16 GB Linux runner,
Docker layer cache warm *within* a job, cold *between* jobs):

| Suite | Cold cache | Warm cache |
|---|---|---|
| GraphQL (7 envs, strictly serial) | ~35–40 min | ~14–19 min |
| MCP (13 envs, ~3–4 effective concurrent envs) | ~22–28 min | ~9–12 min |
| **Both, as two parallel jobs** | **~40 min** | **~19 min** |

**Runner cost** at the standard GitHub-hosted Linux rate (~$0.008/min; double it if Docker needs the larger runner).
Assumes 20 PRs/week, ~60% touching `src/Server/**`:

| Option | Runner-minutes / event | Estimated cost |
|---|---|---|
| A — required on every PR | ~30 warm / ~80 cold per PR | ~$10–13 / week warm, ~$32 / week cold |
| B — nightly only | ~50 per night | ~$2.40 / week |
| C — path-filtered on PR | ~25 warm per *server* PR (~12/week) | ~$2.40 / week warm, ~$9.60 cold |

The spread between cold and warm is the whole decision: **image caching is worth more than any scheduling choice.**

---

## 3. Options

### Option A — required integration job on every PR

Add both suites to `build-api.yml` (or promote `integration-tests.yml` to a `pull_request` trigger with no filter) and
mark them required in branch protection.

- ✅ Maximum safety; every regression is caught before merge.
- ❌ +19 min (warm) / +40 min (cold) on *every* PR, including AdminUi-only PRs.
- ❌ At cold-cache cost this is ~4× the current CI bill and the slowest feedback in the repo.
- **Viability prerequisites:** all of A/B/C below, plus capacity confidence that a 40-min cold run never exceeds
  `timeout-minutes`.

### Option B — nightly full run

`schedule: cron: '0 3 * * *'`, both suites sequentially, results published as an artifact and failures notified.

- ✅ Cheapest (~$2.40/week), no impact on PR latency, exercises the full suite including the serial GraphQL run.
- ❌ Regressions merge and are discovered up to 24 h later by whoever opens the run — nobody owns the break.
- ❌ With 6 known product defects already pinned by tests (§6 of the ledger), a nightly-only signal means the pin
  drifts silently.
- **Viability prerequisites:** artifact upload + a failure notification, otherwise nobody reads it.

### Option C — path-filtered on `pull_request` (the filter that already exists, actually wired up)

Keep `dorny/paths-filter` exactly as written and add `pull_request: branches: [main, dev]` to `on:`. The filter already
covers `DevStack.Api/**`, `DevStack.Application/**`, `DevStack.Domain/**`, `DevStack.Infrastructure/**`,
`DevStack.Persistence/**`, `DevStack.Contracts/**` plus each test project, so AdminUi-only and docs-only PRs pay nothing.

- ✅ Catches ~all real regressions: the suites only exercise server code, so a PR that cannot break them does not run them.
- ✅ ~$2.40/week warm — roughly the nightly price for near-Option-A safety.
- ✅ Zero new filter logic to write or maintain.
- ⚠️ Blind spot: a change *outside* the filter list that breaks the suites (e.g. a root `Directory.Build.props`,
  a NuGet bump, or the Dockerfile itself — `src/Server/**` covers the Dockerfiles, but a solution-wide props file
  one level up would not). Option B is the safety net for exactly this.
- **Viability prerequisites:** see §4.

---

## 4. What must change first (ordered, applies to every option)

1. **INF-05 — scoped cleanup before any parallel/required GraphQL run.** `deleteTestData` in `AfterScenario` wipes
   *every* `[DeleteAfterTest]` row globally, which is why `TestParallelization.cs` serialises the whole suite. Until a
   scenario deletes only the ids it created (the pattern Lane F already shipped for MCP), the GraphQL job is
   structurally ≥ 7 × (rebuild + boot) serial and cannot be made fast enough to be a pleasant required check.
   **This is the blocking prerequisite for Option A and for making Option C tolerable.**
2. **Build the app image once per job, not once per fixture.** `WithImageBuildPolicy(_ => true)` forces 7 (GraphQL) and
   13 (MCP) rebuilds per job. Either cache the built image in a session-wide static fixture, or pre-build in a
   `build-image` step and let the fixtures hit the warm layer cache. Best version: build once in a `setup-image` job,
   `docker save` → artifact → both test jobs `docker load`, which also removes the cross-job cold build. This single
   change is what turns a 40-min cold run into a ~19-min run.
3. **Publish test results as artifacts.** Add `--logger "trx;LogFileName=results.trx"` plus
   `actions/upload-artifact@v4` (and a TRX→summary step) to both jobs, and capture `docker logs <app>` /
   `docker logs <postgres>` **on failure only**. Without this, a red required check on a runner nobody can inspect is
   worse than no check, because it will get re-run until it goes green.
4. **Keep image tags per-job.** INF-01's `DEVSTACK_TEST_API_IMAGE` / `DEVSTACK_TEST_MCP_IMAGE` overrides are what make
   two jobs on one runner safe; any new job must set them to a job-unique value (e.g. `${{ github.run_id }}-graphql`).
5. **Do not let the two suites share one environment.** The global `deleteTestData` wipe means GraphQL and MCP must stay
   in separate jobs with separate containers, as today.
6. **Revisit `timeout-minutes: 30`** after step 2: it is enough warm and too tight cold.
7. **Rollout, not switch:** land steps 1–4, run the path-filtered job as an *advisory* (non-required) check for ~2 weeks
   to measure real wall-clock and flake rate, then mark it required in branch protection.

---

## 5. Recommendation

**Adopt Option C now, with Option B as a nightly safety net — and do §4 steps 1–4 before making anything required.**

Concretely:

1. Wire `integration-tests.yml` to `pull_request` (the path filter is already correct) and leave it **non-required**.
2. Fix INF-05 scoped cleanup for the GraphQL suite so it can stop being globally serial.
3. Build the image once per job (or once per run via artifact) and add TRX + container-log artifacts.
4. Add a nightly scheduled run of both suites so filter blind spots and dependency bumps are still caught.
5. After ~2 weeks of green advisory runs, mark the path-filtered job required.

Why this combination: the integration suites only exercise server code, so path filtering already captures essentially
all regressions they can detect, at roughly the nightly price (~$2.40/week warm vs ~$10–13/week for Option A).
Option A buys marginal safety for a 4× cost and 19–40 minutes on PRs that cannot possibly affect the suites;
Option B alone is too slow a signal for a suite whose job is to catch error-contract regressions while the author is
still around. The nightly run covers Option C's only real weakness — changes outside the filter list — for pennies.

**Headline: path-filter the existing integration workflow onto `pull_request` as an advisory check, gate it behind
scoped cleanup (INF-05) + build-the-image-once + test-result artifacts, and back it with a nightly full run.**
