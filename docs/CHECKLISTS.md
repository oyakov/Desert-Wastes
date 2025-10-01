# Checklists

## Definition of Done (Per Feature)
- [ ] Feature documented in relevant `docs/` file with references.
- [ ] EditMode and/or PlayMode tests implemented per `docs/TEST_PLAN.md`.
- [ ] Performance impact measured or rationale documented (profiling notes where applicable).
- [ ] Telemetry/events wired into legends log or analytics hooks as appropriate.
- [ ] Accessibility reviewed (controls, UI, color contrast) (`docs/INPUTS_AND_ACCESSIBILITY.md`).
- [ ] Naming conventions adhered to (`docs/NAMING.md`).
- [ ] Code reviewed and merged via PR checklist (`docs/CONTRIBUTING.md`).

## Release Checklist
- [ ] All milestones objectives met for release scope (`docs/ROADMAP.md`).
- [ ] Automated test suites passing in CI.
- [ ] Manual regression on critical paths: worldgen, embark, base tick, save/load.
- [ ] Documentation updated: README, change log (future), system docs.
- [ ] Version numbers bumped in settings/configs.
- [ ] Release notes drafted with known issues and accessibility considerations.
