<!-- Sync Impact Report
- Version change: (new) → 1.0.0
- Modified principles: 
  - [PRINCIPLE_1_NAME] → I. Open Source First
  - [PRINCIPLE_2_NAME] → II. Infrastructure as Code (IaC)
  - [PRINCIPLE_3_NAME] → III. Observability-Driven Development
  - [PRINCIPLE_4_NAME] → IV. Uncompromising Quality
  - [PRINCIPLE_5_NAME] → V. Progress Over Perfection
- Added sections: 
  - Additional Constraints and Standards (with Technology Stack, Security Requirements, Performance Standards)
  - Development Workflow and Quality Gates (with Process Requirements, Testing Mandates, Review Standards)
  - Governance (amendment procedure, versioning policy, compliance expectations)
- Removed sections: None (all template sections utilized)
- Templates requiring updates:
  - ✅ .specify/templates/plan-template.md (Constitution Check section updated)
  - ✅ .specify/templates/spec-template.md (Functional Requirements expanded)
  - ✅ .specify/templates/tasks-template.md (Phase 1 setup tasks added)
  - ⚠ .specify/templates/agent-file-template.md (not found, skipped)
  - ⚠ .specify/templates/checklist-template.md (not reviewed)
  - ⚠ .opencode/command/speckit.*.md (command files not reviewed for principle references)
- Follow-up TODOs: Review and update command files and remaining templates for principle consistency
-->

# DevStack Constitution

## Core Principles

### I. Open Source First
All code, documentation, and infrastructure configurations must be developed as open source by default. Proprietary components require explicit justification and approval. Contributions must be welcomed through transparent processes, and all design decisions should be documented for community scrutiny.

### II. Infrastructure as Code (IaC)
All infrastructure must be provisioned, managed, and versioned through declarative code. Manual infrastructure changes are prohibited except for emergency recovery. Infrastructure definitions must be stored in version control, tested, and follow the same quality standards as application code.

### III. Observability-Driven Development
Systems must be designed with observability as a primary concern. All services must emit structured logs, distributed traces, and meaningful metrics. Dashboards and alerts must be created alongside features, not as afterthoughts. Debugging must be possible through available telemetry without requiring reproduction environments.

### IV. Uncompromising Quality
Every change must maintain or improve overall system quality. Code must be readable, testable, and maintainable. Automated testing is mandatory for all functionality. Technical debt must be tracked and regularly addressed. Performance, security, and reliability requirements are non-negotiable baseline expectations.

### V. Progress Over Perfection
Value delivery takes precedence on technical perfection. Work should be broken into smallest valuable increments that can be independently tested, reviewed, and deployed. Experiments are encouraged to validate assumptions before major investments. Perfect solutions that delay value delivery are rejected in favor of iterative improvement.

## Additional Constraints and Standards

### Technology Stack
All technology choices must favor open source solutions with active communities. Vendor lock-in must be avoided through abstraction layers and portable standards. When multiple options exist, preference is given to solutions with strong observability support and IaC compatibility.

### Security Requirements
Security must be integrated into all development phases. Secrets must never be stored in code or configuration files. All infrastructure provisioning must follow least privilege principles. Regular security scanning and dependency updates are mandatory.

### Performance Standards
Systems must meet documented performance benchmarks under expected load conditions. Performance testing is required for all user-facing features. Resource utilization must be monitored and optimized continuously.

## Development Workflow and Quality Gates

### Process Requirements
All work must follow the GitHub flow with feature branches. Pull requests require at least one approval and must pass all automated checks. Direct commits to main branches are prohibited except for critical hotfixes.

### Testing Mandates
Every PR must include tests for new or modified functionality. Test coverage must not decrease below project baselines. Integration tests must verify cross-component contracts. Performance regression tests are required for performance-sensitive changes.

### Review Standards
Code reviews must assess adherence to these principles, not just functional correctness. Reviews should consider maintainability, observability, and long-term technical impact. Reviewers have authority to block merges that violate constitutional principles.

## Governance

This constitution supersedes all other team practices and guidelines. Amendments require:
1. Documentation of the proposed change with rationale
2. Team discussion and consensus (or supermajority approval if consensus cannot be reached)
3. Migration plan for existing practices that conflict with the amendment
4. Update of all dependent templates and guidance documents

Versioning follows semantic versioning:
- MAJOR: Backward-incompatible principle changes or removals
- MINOR: New principles added or existing ones materially expanded
- PATCH: Clarifications, wording improvements, or non-substantive refinements

Compliance is verified through regular audits and automated checks in the development pipeline. Violations must be justified with explicit Cost of Delay analysis and approved through the governance process.

**Version**: 1.0.0 | **Ratified**: 2026-03-26 | **Last Amended**: 2026-03-26