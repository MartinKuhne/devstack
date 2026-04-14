# Open Questions

## Phase 3

### Step 3.4 — Status transition component

**Question:** This repository contains only a .NET backend API. There is no frontend code (no React/TypeScript files, no package.json, no admin-ui directory). The task requires creating React components like `StatusTransitionPanel.tsx` and `EditFeatureDialog.tsx`.

**Options:**
1. Create a new frontend project in this repository (e.g., `src/frontend/` or `admin-ui/`)
2. Create the frontend as a separate repository
3. Use a different frontend framework (Blazor, etc.) within the existing .NET project

**Recommendation:** Create a new React frontend project in `src/frontend/` or `admin-ui/` as a separate project that consumes the GraphQL API.

**Status:** Blocked until frontend architecture decision is made.

---

## General

### Frontend Architecture

**Question:** What type of frontend should be built for the Admin UI?

**Options:**
- React + TypeScript SPA (single-page application)
- Blazor WebAssembly ( Razor components in .NET)
- Blazor Server (real-time UI via SignalR)
- Mixed approach (React for complex UI, Blazor for admin sections)

**Current state:** No frontend code exists. All projects are .NET backend-only.

**Recommendation:** React + TypeScript SPA consumed by the existing GraphQL API.

**Status:** Open - requires user decision.

---
