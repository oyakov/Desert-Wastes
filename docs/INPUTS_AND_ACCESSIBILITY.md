# Inputs & Accessibility Planning

## Input Actions (Unity Input System)
| Action Map | Action | Binding Examples | Notes |
| --- | --- | --- | --- |
| Global | Pause | Keyboard `Esc`, Controller `Start` | Toggles simulation tick. |
| Global | Speed Cycle | Keyboard `+/-`, Mouse Wheel Click | Adjusts time scale respecting determinism constraints. |
| Overworld | Move Camera | WASD, Arrow Keys, Middle Mouse Drag | Smooth pan across map. |
| Overworld | Select | Left Click, Controller `A` | Select factions, tiles, embark sites. |
| Overworld | Alerts | Keyboard `Tab`, Controller `Y` | Focus next alert or event. |
| Base | Move Camera | WASD, Edge Scroll, Controller Stick | Grid-based pan with smoothing. |
| Base | Issue Order | Right Click, Controller `X` | Opens indirect order radial UI. |
| Base | Toggle Overlays | Keyboard `F1-F5`, Controller `Bumpers` | Cycle heatmaps (morale, hazards, logistics). |
| Global | Toggle Overworld/Base | Keyboard `Space`, Controller `Back` | Switch scenes when allowed. |

## Accessibility Goals
- **Scalable UI**: Dynamic layout anchored to relative units; font size slider stored in settings (`Assets/_Project/Settings`).
- **Colorblind-Safe Palettes**: Provide palettes tested for deuteranopia, protanopia, tritanopia; overlay icons differentiate shapes.
- **High Contrast Mode**: Optional shader variant toggles for UI elements (planned in UI assembly).
- **Text Legibility**: Minimum 14pt default fonts, option for dyslexic-friendly typeface.
- **Input Remapping**: Full rebinding support via Input System; saved to JSON config for portability.
- **Event Feedback**: Audio/visual cues paired with text logs to support multi-modal feedback (see `docs/AI_COMBAT_INDIRECT.md`).
- **Tutorial & Tooltips**: Context-sensitive tips referencing legends and social data to reduce cognitive load.

## Testing Accessibility
- Automated checks for color contrast on UI prefabs (post-M1 via editor tooling).
- PlayMode tests ensure rebinding persistence and time scaling obeys determinism (ties to `docs/TEST_PLAN.md`).
