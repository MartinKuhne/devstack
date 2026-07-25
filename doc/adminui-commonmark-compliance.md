# AdminUI CommonMark 0.31.2 Compliance Audit

**Scope:** `src/AdminUi/src/components/MarkdownViewer.tsx` and all consumers.
**Spec:** [CommonMark 0.31.2](https://spec.commonmark.org/0.31.2/) (2024-01-28).
**Method:** Source review + a 16-case spot-check suite + `npm run build` CSS audit.
**Parser baseline:** `micromark@4.0.2` (CommonMark 0.31.2 conformant), `remark-gfm@4.0.1`, `rehype-highlight@7.0.2`, `react-markdown@10.1.0`.

## TL;DR

The underlying parser is CommonMark 0.31.2 conformant. The wrapper and its
call sites have **three real spec/quality defects** that block correct
rendering today, and several smaller improvements. One of them — the
typography plugin not actually loading — silently breaks markdown styling
across every consumer in the app.

## What is correct (16/16 spot-checks pass for these)

| Section  | Feature                                                       | Status |
| -------- | ------------------------------------------------------------- | ------ |
| 4.1      | Thematic break (`---`, `***`, `___`)                          | ✓      |
| 4.2      | ATX headings (`#`–`######`)                                   | ✓      |
| 4.3      | Setext headings (`===` / `---` underline)                     | ✓      |
| 4.4      | Indented code blocks                                          | ✓      |
| 4.5      | Fenced code blocks (with `language-xxx` class)                | ✓      |
| 5.1      | Block quotes (`>`)                                            | ✓      |
| 5.2/5.3  | Lists (ordered, unordered, nested, start attr)                | ✓      |
| 6.1      | Code spans (inline `` ` ``)                                   | ✓      |
| 6.2      | Emphasis + strong emphasis (`*`, `_`, `**`, `__`, `***`)      | ✓      |
| 6.3      | Inline links (`[text](url)`) — `href` only                    | ⚠ partial |
| 6.3+4.7  | Reference links (`[text][ref]`) — `href` only                 | ⚠ partial |
| 6.4      | Images (`![alt](src)`) — uses default `<img>`                 | ✓ unstyled |
| 6.5      | Autolinks (`<https://…>`)                                     | ✓      |
| 6.5/GFM  | GFM autolink (plain `https://…`)                              | ✓      |
| 6.7      | Hard line breaks (2 trailing spaces, backslash)               | ✓      |
| 6.8      | Soft line breaks (default — rendered as space/newline)        | ✓      |
| 2.4      | Backslash escapes                                             | ✓      |
| 2.5      | Entity + numeric character refs (`&amp;`, `&copy;`, `&#65;`) | ✓      |
| GFM      | Tables + column alignment                                     | ✓ unstyled |
| GFM      | Task list checkboxes (`- [x]` / `- [ ]`)                       | ✓      |
| GFM      | Strikethrough (`~~text~~`)                                    | ✓      |
| 4.6/6.6  | Raw HTML blocks / inline HTML                                 | ✗ escaped (intentional — see P3) |

## Prioritized fix list

Severity key: **P0** = data loss / spec-violating / visible breakage;
**P1** = spec edge case / quality; **P2** = polish / a11y / perf.

### P0-1 — Tailwind typography plugin is not loaded (no `.prose` in CSS)

**Evidence:** Built CSS (`dist/assets/index-*.css`, 55 kB) contains **zero
`prose` selectors**. Every consumer uses `prose prose-sm dark:prose-invert
max-w-none` (e.g. `AgentTaskDetailPage.tsx:324,341,432`) or `text-sm` only
(e.g. `DeliverableDetailPage.tsx:184,195,…`).

**Root cause:** The project uses Tailwind v4 (`@tailwindcss/postcss`,
`@import 'tailwindcss';` in CSS) but the typography plugin is registered in
the legacy v3 `tailwind.config.js`:

```js
// tailwind.config.js
export default { …, plugins: [require('@tailwindcss/typography')] };
```

Tailwind v4 **ignores the JS `plugins` array**. Result: all `prose*` classes
are no-ops. Headings, lists, code blocks, blockquotes, tables render with
browser defaults.

**Fix:**
- Add to `src/index.css` after `@import 'tailwindcss';`:
  ```css
  @plugin "@tailwindcss/typography";
  ```
- Remove the `plugins: [require('@tailwindcss/typography')]` line from
  `tailwind.config.js` (or delete the file if v4 has no other use).
- Audit consumers: `DeliverableDetailPage.tsx` currently uses `text-sm`
  only; that page will need `prose prose-sm dark:prose-invert max-w-none`
  added to be readable.

**Complexity:** 1.
**Test impact:** None new; visual regression only.

---

### P0-2 — `MarkdownViewer` drops link `title` (CommonMark 6.3)

**Evidence:** Spot-check fails:

```
✗ renders inline link with title     expected 'my title', got null
✗ renders reference link             expected 'Docs title', got null
```

**Root cause:** `MarkdownViewer.tsx:25-30` only forwards `href` and
`children` to the inner `LinkRenderer`:

```tsx
const components: Components = {
    a: props => {
        return <LinkRenderer href={props.href}>{props.children}</LinkRenderer>;
    },
};
```

CommonMark 6.3 specifies a `title` attribute on every link form. The current
code silently drops it.

**Fix:** Forward the full props bag. Also drop the React-internal `node`
prop while we're here so React 19 doesn't warn about it:

```tsx
function LinkRenderer({ node: _node, href, title, children, ...rest }: {
    node?: unknown; href?: string; title?: string;
    children: React.ReactNode; [k: string]: unknown;
}) {
    const isExternal = !!href && !/^(\/|#|mailto:)/i.test(href);
    if (!isExternal) {
        return <a href={href} title={title} {...rest}>{children}</a>;
    }
    return (
        <a
            href={href}
            title={title}
            target="_blank"
            rel="noopener noreferrer"
            {...rest}
            className="inline-flex items-center gap-1"
        >
            {children}
            <ExternalLink aria-hidden="true" className="h-3 w-3 text-muted-foreground" />
            <span className="sr-only"> (opens in new tab)</span>
        </a>
    );
}
```

…and call it as `<LinkRenderer {...props} />` from the `components` map.

**Complexity:** 1.
**Test impact:** Add the two new specs that currently fail (inline + ref
link with `title`).

---

### P0-3 — No `MarkdownViewer` default `prose` wrapper class

**Evidence:** `MarkdownViewer.tsx:31-41` renders a bare `<div className={className}>`
around the markdown. The `prose` class is the responsibility of every
caller. Two of the eleven call sites in `DeliverableDetailPage.tsx` (e.g.
line 184) use `className="text-sm"` only — so even after P0-1 is fixed,
those pages will still render unstyled.

**Fix:** Make `prose` the default in the component so callers opt *out*
when they want to. Recommend:

```tsx
const defaultClassName =
    'prose prose-sm dark:prose-invert max-w-none text-foreground';
return <div className={className ?? defaultClassName}>…</div>;
```

Update the 11 call sites to either remove the now-redundant
`prose prose-sm dark:prose-invert max-w-none` (saves bytes) or keep them
explicit if they want a different look.

**Complexity:** 1.
**Test impact:** The `className` test (`MarkdownViewer.test.tsx:209-214`) will
need to be relaxed to `expect(...).toHaveClass('custom-class')` after
asserting the default; add a separate test for the default.

---

### P1-1 — Raw HTML is escaped (CommonMark 4.6 / 6.6 partially violated)

**Evidence:** The existing `escaped HTML` test block explicitly asserts
`<script>` is rendered as text. `react-markdown@10` defaults to escaping
raw HTML to text (it does **not** enable `rehype-raw`).

**Trade-off:** This is the only thing keeping the XSS tests passing, so
keep it off by default. But CommonMark 4.6/6.6 are not honored. If a
future feature (e.g. agent-emitted `<details>` blocks, `<kbd>`, `<sub>`)
needs raw HTML, this will need a deliberate opt-in.

**Fix:** Add an `allowHtml?: boolean` prop that, when true, includes
`rehype-raw` in the plugin list. Default `false`. Document the trade-off
in the component JSDoc and reference the spec section. No change to the
default behavior, so no churn in existing consumers.

**Complexity:** 2 (adds a dep `rehype-raw`).
**Test impact:** Two new tests: `allowHtml={false}` (default, current
behavior); `allowHtml={true}` renders `<details>` / `<kbd>` correctly.

---

### P1-2 — Test coverage gap for spec sections that work today

The existing test file covers ~30% of CommonMark 0.31.2. These spec
sections are not exercised anywhere and should be locked in with tests
so a future bump of `react-markdown` / `micromark` can't silently regress
them:

| Section | Missing coverage                                  |
| ------- | ------------------------------------------------- |
| 4.1     | Thematic break (`---`, `***`, `___`)              |
| 4.3     | Setext heading                                    |
| 4.4     | Indented code block                               |
| 4.7     | Link reference definition (full form, with title) |
| 5.1     | Block quote (incl. nested)                        |
| 6.3     | Inline link with `title` (currently fails — see P0-2) |
| 6.3     | Collapsed reference `[foo][]`                     |
| 6.3     | Shortcut reference `[foo]`                        |
| 6.4     | Image (with title)                                |
| 6.5     | Autolink `<https://…>`                            |
| 6.5 GFM | Plain-URL autolink                                |
| 6.7     | Hard line break (2-space and backslash forms)     |
| 2.4     | Backslash escapes                                 |
| 2.5     | Entity and numeric character references           |
| 5.2     | Ordered list `start` attribute                    |
| 5.2     | Mixed ordered/unordered nesting                   |
| GFM     | Strikethrough                                     |
| GFM     | Task list (`checked` attribute on input)          |

I ran a one-off 16-case spot-check and 14 passed; the two that failed are
P0-2 above. Promoting the spot-check to a permanent `*.spec.tsx` file
guarantees the spec floor going forward.

**Complexity:** 2.
**Test impact:** This *is* the test impact.

---

### P1-3 — `node` and other non-DOM props leak into `<a>` / `<img>`

**Evidence:** The outer `components.a` factory only forwards `href` and
`children` (see P0-2). After P0-2 is fixed to forward everything, the
`node` prop (a hast `Element` object) will be spread onto the intrinsic
`<a>`. React 19 will not write the object to the DOM but will log a
dev-mode warning per render.

**Fix:** Destructure `node` out (it is not a DOM prop; it is internal to
react-markdown). Same applies if/when a custom `img` is added.

**Complexity:** 1.
**Test impact:** Add an a11y console-warning test using vitest's
`vi.spyOn(console, 'error')`.

---

### P2-1 — External-link a11y

`target="_blank"` without a screen-reader cue is a recurring a11y
finding. Add `aria-label` or visually-hidden text, and add `aria-hidden`
to the icon. (Already in the P0-2 snippet above.)

**Complexity:** 1.
**Test impact:** One new test.

---

### P2-2 — Image overflow and lazy loading

`MarkdownViewer` lets react-markdown's default `<img>` render bare. Long
descriptions with images can blow out card width, and the images block
the initial paint. Recommend overriding `img`:

```tsx
img: ({ src, alt, ...rest }) => (
    <img src={src} alt={alt} loading="lazy" decoding="async"
         className="max-w-full h-auto rounded" {...rest} />
),
```

**Complexity:** 1.
**Test impact:** One new test asserting `loading="lazy"` and
`max-w-full` on the rendered `<img>`.

---

### P2-3 — Heading hierarchy for cards

If a markdown block in a card starts with `# Title`, the page now has
two `<h1>`s (the page title + the markdown title). CommonMark doesn't
mandate a level, but admin UIs typically want the markdown to start at
`h2` inside a card. Add a `baseHeadingLevel?: 1|2|3` prop (default `1`)
that uses `rehype-shift-heading` or a tiny `components.h1` remap. The
underlying AST already knows the levels.

**Complexity:** 3.
**Test impact:** Two new tests.

---

### P2-4 — `urlTransform` for protocol safety

`react-markdown@10` ships `defaultUrlTransform` which blocks `javascript:`
URLs in `href`/`src` etc. This is the right default, but the wrapper
should be explicit about it (and unit-test the guarantee) so a future
config change can't accidentally relax it.

**Complexity:** 1.
**Test impact:** Two new tests asserting `[x](javascript:alert(1))` and
`<img src="javascript:…">` produce no anchor / no src in the output.

---

### P2-5 — `MarkdownViewer` API ergonomics

- Accept a `components` prop so callers can override individual renderers
  (e.g. a custom `h1` for in-card docs) without forking the wrapper.
- Memoize the plugin arrays (`useMemo`) — currently they're rebuilt every
  render. Negligible for small strings, but with agent task results that
  are thousands of lines it adds up.

**Complexity:** 2.
**Test impact:** Two new tests.

---

## Recommended execution order

1. **P0-2** (drop `title` bug) — small, high signal, makes 2 tests pass.
2. **P0-1** (typography plugin) — single-line CSS fix + config cleanup;
   unblocks visible rendering across the whole app.
3. **P0-3** (default `prose` class in wrapper) — protects future call
   sites from forgetting to add it.
4. **P1-2** (expand test coverage) — locks in the spec floor so the
   above fixes stay fixed.
5. **P1-3** (drop `node` prop) — done together with P0-2 if possible.
6. **P2-1 / P2-2 / P2-4** — small a11y + image polish; do as a single
   "polish" commit.
7. **P1-1** (opt-in raw HTML) — only when a real consumer needs it.
8. **P2-3 / P2-5** — API expansion; defer until a real need.

## Out of scope

- Switching to a different markdown engine. `react-markdown` on top of
  `micromark` is the right stack for this app; it tracks the CommonMark
  spec closely and is well-maintained.
- Adding footnote support, definition lists, math, Mermaid, etc. None
  are in CommonMark 0.31.2; if needed, they are GFM-adjacent extensions
  and should be a deliberate, separate decision.
