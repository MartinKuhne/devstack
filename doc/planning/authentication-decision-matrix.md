# Authentication & Authorization Strategy Decision Support Matrix

## Executive Summary

This document evaluates authentication and authorization architecture options for **DevStack**. It provides a comprehensive analysis of approach options ranging from lightweight, internal API Key / Personal Access Token (PAT) models to full OpenID Connect (OIDC) identity provider setups (self-hosted and cloud-managed).

The goal is to establish a clear decision-making framework for securing both programmatic access (GraphQL API, CLI, subagents) and human interaction (Admin UI).

---

## Evaluation of Authentication Options

### Option 1: API Key & Personal Access Token (PAT) Model (Internal DB)

In this model, identity and access management is self-contained within the application's core database using hashed API keys and Personal Access Tokens.

#### How It Works
- **API & CLI Access**: Requests include an HTTP header (`Authorization: Bearer ds_pat_...` or `X-API-Key: ds_pat_...`). The server looks up the token using a high-performance hash index (`SHA-256` or `Argon2`) and assigns scopes/roles.
- **Admin UI Login**: 
  1. User enters an API Key / PAT (or root bootstrap secret) into the Admin UI login page.
  2. The Admin UI posts the key to `/api/auth/login-with-key`.
  3. The server validates the key and responds with an `HttpOnly`, `Secure`, `SameSite=Strict` session cookie.
  4. Subsequent Admin UI browser requests rely on automatic session cookie transmission, avoiding token storage in `localStorage`.

#### Trade-offs
- **Pros**:
  - **Zero External Dependencies**: No extra containers, third-party services, or external identity providers required.
  - **Developer-Friendly**: Ideal for CLI tools, automation scripts, and background subagents.
  - **Low Latency & Fast Setup**: Simple local database lookup or in-memory cache lookup.
  - **Fine-Grained Scopes**: Scopes (e.g., `admin:write`, `projects:read`) can be attached directly to individual tokens.
- **Cons**:
  - **No Native Enterprise SSO**: Does not support SAML 2.0 or corporate directory integration without building custom adapters.
  - **Manual User Features**: User self-service (password resets, magic links, MFA) must be implemented manually if expanded beyond key-based access.

---

### Option 2: Internal User Database (Password/Session & Local JWT)

A traditional embedded identity system managed directly within the backend database (e.g., PostgreSQL/SQLite via EF Core).

#### How It Works
- **Storage**: A `users` table holds usernames/emails and password hashes (`Argon2id` or `bcrypt`).
- **Admin UI Login**: Standard email/password login form emitting an `HttpOnly` session cookie or short-lived signed JWT.
- **API Access**: Users can generate PATs inside their Admin UI account settings for programmatic use.

#### Trade-offs
- **Pros**:
  - Standard, familiar user experience for web applications.
  - Keeps all user data strictly local and within application boundaries.
  - Eliminates reliance on external cloud services or separate authentication infrastructure.
- **Cons**:
  - Requires writing and maintaining security-critical code (password hashing, rate-limiting login attempts, session invalidation, password reset flows).
  - MFA (TOTP / WebAuthn) must be built and maintained internally.

---

### Option 3: Self-Hosted OIDC Provider (Keycloak / Authentik)

Deploying a dedicated open-source OpenID Connect (OIDC) & OAuth 2.0 Identity Provider alongside the application backend.

#### How It Works
- **Identity Server**: Keycloak or Authentik runs as a separate container service in `docker-compose.yml`.
- **Admin UI Login**: Admin UI redirects users to the IdP login page using OIDC Authorization Code Flow with PKCE. Upon authentication, the IdP redirects back with an authorization code exchanged for ID and Access Tokens (JWTs).
- **API Validation**: Backend API validates incoming JWT signatures asynchronously using the IdP's JSON Web Key Set (JWKS) endpoint.

#### Trade-offs
- **Pros**:
  - **Enterprise Feature Set**: Standard SAML 2.0, LDAP/Active Directory integration, Social Logins, multi-factor authentication (MFA), and RBAC out of the box.
  - **Centralized User Management**: Pre-built admin console for managing users, roles, sessions, and audit logs.
  - **Standard Compliance**: Adheres to strict OAuth 2.0 / OIDC specifications.
- **Cons**:
  - **High Operational Overhead**: Additional infrastructure component requiring database backing, upgrades, monitoring, and backup procedures.
  - **Heavy Resource Footprint**: Keycloak (Java-based) requires noticeable memory/CPU overhead relative to the core application.
  - **Integration Complexity for CLIs**: Command-line tools require Device Authorization Grant (`urn:ietf:params:oauth:grant-type:device_code`) or local callback servers to authenticate humans.

---

### Option 4: Cloud-Managed Identity Provider (Auth0, Okta, Clerk, AWS Cognito)

Delegating authentication entirely to a cloud SaaS provider.

#### How It Works
- **Authentication Flow**: Managed SDKs or hosted login pages handle authentication entirely on cloud infrastructure.
- **Tokens**: Cloud provider issues signed JWTs that the DevStack backend validates via remote JWKS URL.

#### Trade-offs
- **Pros**:
  - **Zero Maintenance**: No auth servers to run, patch, or scale.
  - **Turnkey Security**: Immediate access to anomaly detection, breached password monitoring, passkeys, and multi-factor auth.
  - **Rapid Time-to-Market**: Fast frontend integration via official SDKs.
- **Cons**:
  - **Vendor Lock-in & Monthly Cost**: Monthly Active User (MAU) pricing scales up quickly.
  - **External Dependency**: Requires Internet connectivity; unsuitable for air-gapped or strictly local developer environments.
  - **Privacy & Residency**: User identity data resides on third-party cloud infrastructure.

---

## Strategy Comparison Matrix

| Dimension | Option 1: API Keys + Session Cookie | Option 2: Internal User DB | Option 3: Self-Hosted OIDC (Keycloak) | Option 4: Cloud IdP (Auth0 / Clerk) |
| :--- | :--- | :--- | :--- | :--- |
| **Initial Implementation Effort** | Very Low (1-2 days) | Medium (1-2 weeks) | High (2-3 weeks) | Low (2-4 days) |
| **Operational Overhead** | None (embedded) | None (embedded) | High (Separate container/DB) | Very Low (SaaS managed) |
| **Admin UI User Experience** | Simple Key/Token entry | Standard Form (Email/Password) | Redirection to IdP portal | Hosted / Embedded Modal |
| **CLI & API Compatibility** | Native / Excellent | Requires PAT generation | Requires OAuth Device Flow | Requires OAuth Device Flow |
| **Enterprise SSO (SAML/LDAP)** | No | Requires custom adapters | Native / Built-in | Native / Built-in |
| **Infrastructure & Hosting Cost** | $0 | $0 | Additional container resources | SaaS subscription per MAU |
| **Air-gapped / Local Support** | 100% Offline Compatible | 100% Offline Compatible | 100% Offline Compatible | Internet connection required |
| **Security Risk Profile** | Low (if token hashing used) | Medium (security maintenance) | Low (hardened identity core) | Low (handled by vendor) |

---

## Recommended Security Model & Role-Based Access Control (RBAC)

Regardless of the authentication mechanism chosen (API Keys vs. OIDC), authorization should be structured around a unified **Role-Based & Scope-Based Access Control (RBAC/SBAC)** model.

### 1. Principal & Context Construction
Every authenticated request populates an execution context (`AuthContext`) containing:
- **`SubjectId`**: Unique identifier for the authenticated user, machine account, or subagent.
- **`PrincipalType`**: `User`, `ServiceAgent`, or `System`.
- **`Roles`**: Assigned high-level functional roles (e.g., `Admin`, `Maintainer`).
- **`Scopes`**: Fine-grained capability permissions attached to the session or token (e.g., `projects:write`).

### 2. Recommended Roles

| Role | Scope / Purpose | Key Capabilities | Target User / Subject |
| :--- | :--- | :--- | :--- |
| **`Admin`** | System & Tenant Management | Full system access, manage API keys/users, configure global settings, override all projects | Operations, Lead Devs, Bootstrap Root Key |
| **`Maintainer`** | Project Administration | Manage specific projects, create/delete deliverables, assign tasks, manage project members | Team Leads, Project Owners |
| **`Developer`** | Standard Operations | Create/update tasks, update deliverable statuses, trigger execution runs | Core Developers, Engineers |
| **`ServiceAgent`** | Programmatic Automation | Scope-bound execution for CLI tools, background runners, and AI subagents | Subagents, CI/CD Pipelines, CLI |
| **`Viewer`** | Read-Only Audit | Inspect projects, deliverables, tasks, and system logs without mutation rights | Stakeholders, Auditors, Guests |

### 3. Scope / Permission Granularity Matrix

To support fine-grained authorization for API Keys and PATs, roles map to specific permission scopes:

```
Scope Taxonomy:  <resource>:<action>
Examples:
  - projects:read, projects:write, projects:delete
  - tasks:read, tasks:write, tasks:assign
  - deliverables:read, deliverables:write
  - system:admin, keys:manage
```

| Role | Assigned Scopes |
| :--- | :--- |
| **`Admin`** | `*` (All scopes) |
| **`Maintainer`** | `projects:*`, `tasks:*`, `deliverables:*`, `keys:read` |
| **`Developer`** | `projects:read`, `tasks:read`, `tasks:write`, `deliverables:read`, `deliverables:write` |
| **`ServiceAgent`** | `tasks:read`, `tasks:write`, `deliverables:read`, `deliverables:write` (Customizable per PAT) |
| **`Viewer`** | `*:read` |

### 4. Enforcement Strategy
- **GraphQL API**: Enforce permissions via schema directives (`@authorize(roles: ["Admin"])` or `@authorize(scope: "tasks:write")`).
- **REST & MCP Endpoints**: Enforce permissions using ASP.NET Core policy authorization (e.g., `[Authorize(Policy = "RequireDeveloperRole")]`).
- **Data-Level Security**: Validate resource boundary ownership (e.g. `project.TenantId == authContext.TenantId`) in application request handlers.

---

## Recommendation & Phased Architecture Roadmap

### Recommended Approach: Hybrid API Key + Session Cookie (Phase 1) progressing to Modular OIDC (Phase 2)

#### Phase 1: API Key & PAT-Centered Architecture (Current Milestone)
- Implement lightweight database-backed API Key / PAT authentication for the backend API and CLI.
- For the **Admin UI**:
  - Provide a clean login interface accepting an Admin API Key / PAT or bootstrap environment key (`DEVSTACK_ADMIN_KEY`).
  - Exchange the valid key for an `HttpOnly`, `SameSite=Strict` session cookie.
  - Secure Admin UI GraphQL calls seamlessly using standard HTTP cookie authorization.
- **Rationale**: Minimal complexity, 100% offline capability, zero hosting cost, perfect fit for CLI and subagent developer workflows.

#### Phase 2: Decoupled Authentication Middleware (Future Expansion)
- Design the backend authentication middleware using ASP.NET Core `IAuthenticationHandler` abstractions (`ApiKeyAuthenticationHandler` alongside `JwtBearerAuthenticationHandler`).
- Allow enterprise deployments to optionally toggle on OIDC integration (Keycloak / Okta / Azure AD) via `appsettings.json` configuration without refactoring core business logic.

---

## Related Documents
- [AGENTS.md](file:///C:/Users/mkuhn/src/devstack/AGENTS.md) - Project principles and scope rules
- [INSTALLATION.md](file:///C:/Users/mkuhn/doc/INSTALLATION.md) - System setup and deployment instructions
