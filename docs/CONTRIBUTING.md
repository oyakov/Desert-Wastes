# Contributing Guide

## Branching Strategy
- `main`: Stable milestone branch. Only release-ready commits.
- `develop`: Integration branch for upcoming milestone work.
- Feature branches: `feature/<short-description>`
- Bugfix branches: `fix/<issue-id>`

## Workflow
1. Create an issue describing the change. Link relevant docs (`docs/` references encouraged).
2. Branch from `develop` (unless hotfix to `main`).
3. Implement change with accompanying tests and documentation updates.
4. Run EditMode and PlayMode tests locally.
5. Submit PR targeting `develop`; request review from at least one maintainer.

## Code Style (C#)
- Follow Microsoft C# conventions (PascalCase for types/methods, camelCase for locals).
- Prefer explicit access modifiers.
- Use dependency injection; avoid static state in core logic (`docs/ARCHITECTURE.md`).
- Keep methods < 40 lines when possible; refactor into pure functions.
- Document public APIs with XML comments referencing relevant docs.

## Commit Messages
- Format: `type(scope): summary`
  - Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`.
  - Scope references assembly or feature (e.g., `core`, `base-mode`).
- Body (optional) explains rationale and links to issues.
- Include breaking change notes when applicable.

## Pull Request Checklist
- [ ] Tests cover new/changed logic (`docs/TEST_PLAN.md`).
- [ ] Documentation updated (`docs/` and inline as needed).
- [ ] Naming conventions followed (`docs/NAMING.md`).
- [ ] No deterministic seams broken (RNG, Time, IO) (`docs/ARCHITECTURE.md`).
- [ ] Accessibility impact considered (`docs/INPUTS_AND_ACCESSIBILITY.md`).

## Review Expectations
- Reviewers verify adherence to DoD (`docs/CHECKLISTS.md`).
- Constructive feedback; request clarifications instead of assumptions.
- Approvals require passing CI and resolved comments.
