# Prototype Launch Milestones

This milestone plan focuses on delivering a locally playable Desert Wastes prototype. Each milestone is designed to unlock the next, ensuring the experience can be built, launched, and evaluated by team members without cloud infrastructure.

## Milestone P0 – Local Environment Ready
- **Goal:** Ensure developers and playtesters can clone and run the project in a consistent environment.
- **Scope:**
  - Verify and document supported Unity LTS version and required packages.
  - Automate project setup (install dependencies, configure git-lfs, import packages) via scripts referenced in `docs/LOCAL_SETUP.md`.
  - Smoke-test asset import pipeline and address blocking warnings/errors.
- **Exit Criteria:** Fresh clone can enter Play Mode without import errors, following only the documented setup steps.

## Milestone P1 – Core Loop Prototype
- **Goal:** Establish a vertical slice that demonstrates the primary gameplay loop without full feature depth.
- **Scope:**
  - Instantiate a minimal overworld with deterministic seed selection.
  - Hook TickManager to drive world simulation and resource ticks.
  - Provide a basic base scene with at least one interactive job or action for the player.
  - Persist state to disk using the JSON save seams defined in `docs/DATA_MODEL.md`.
- **Exit Criteria:** Player can start a session, trigger the core loop for several ticks, and save/load without crashes.

## Milestone P2 – Playtest-Ready UX & Feedback
- **Goal:** Smooth out the slice so internal testers can evaluate the experience without guidance.
- **Scope:**
  - Layer minimal UI for core stats, next-step prompts, and alerts.
  - Add onboarding tooltips or log entries that explain current limitations.
  - Integrate logging/analytics hooks for capturing playtest notes locally.
  - Resolve priority accessibility and input considerations from `docs/INPUTS_AND_ACCESSIBILITY.md`.
- **Exit Criteria:** Unassisted tester can launch the prototype, understand the objective, and complete the loop within ~15 minutes.

## Milestone P3 – Stability & Packaging
- **Goal:** Deliver a downloadable artifact that mirrors the editor experience.
- **Scope:**
  - Set up CI or local build scripts to produce platform-specific builds (Windows/macOS/Linux).
  - Run targeted regression tests from `docs/TEST_PLAN.md` in CI or via scripted local execution.
  - Add crash logging and version stamping to builds.
  - Update release notes and QA checklist in `docs/STATUS_REPORT.md` and `docs/CHECKLISTS.md`.
- **Exit Criteria:** Prototype zip/executable can be distributed internally, launched without editor, and evaluated for stability over multiple sessions.

## Milestone P4 – Feedback Synthesis & Iteration Gate
- **Goal:** Collect learnings and decide on scope for the next development phase.
- **Scope:**
  - Aggregate feedback from playtests, analytics logs, and issue tracker.
  - Hold a milestone review referencing `docs/MILESTONE_EVALUATION.md` to score outcomes vs. expectations.
  - Prioritize fixes and features for post-prototype roadmap.
  - Archive builds, reports, and decision logs for future reference.
- **Exit Criteria:** Documented go/no-go decision with actionable backlog items for the next development cycle.

---

These milestones complement the broader roadmap in `docs/ROADMAP.md` and focus specifically on the steps required to achieve a shareable, locally playable prototype.
