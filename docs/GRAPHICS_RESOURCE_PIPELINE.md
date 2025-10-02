# Graphics Resource Pipeline

This guide describes how to plan, produce, and integrate graphical resources for Desert Wastes so that new sprites, tiles, and UI icons remain stylistically cohesive and technically correct. Follow it alongside `docs/NAMING.md`, `docs/LOCAL_SETUP.md`, and `docs/CONTRIBUTING.md` when preparing art for review.

## 1. Define the Request
1. **Identify the gameplay need.** Document the in-game system, screen, or encounter that requires art. Include the camera context (overworld, base interior, UI overlay) and how the asset will be used (tile, character, prop, icon, VFX frame).
2. **Collect references.** Pull in visual references that match the square-tile pixel-art style, desaturated desert palette, and silhouettes readable at zoomed-out scale.
3. **Write acceptance criteria.** Capture expected dimensions, animation frames, palette constraints, and any gameplay readability requirements (e.g., faction color bands, hazard icons needing high contrast).
4. **Log the work item.** Track the request in the project board or issue tracker so programming, design, and QA contributors can review the brief.

## 2. Prepare the Production Environment
1. **Tooling.** We support Aseprite, Photoshop, and Krita for 2D spritesheets; Blender or MagicaVoxel can be used for kitbashing references before pixelizing. Keep the source file (`.aseprite`, `.psd`, `.kra`, `.blend`) under version control in `Assets/_Project/Art/Source/` (create the folder if it does not exist) so the team can re-export sprites later.
2. **Template setup.** Start from a 32×32 base tile grid. Larger props should scale in increments of 16 pixels to remain aligned with Unity’s 32 Pixels Per Unit baseline. Reserve a transparent 1px padding gutter around sprites to avoid texture bleeding after import.
3. **Palette management.** Use indexed color layers when possible. Stick to the shared swatches defined in the project color sheet (add or adjust swatches only after art direction approval). Preserve palette indices during export so re-coloring scripts remain deterministic.

## 3. Produce the Asset
1. **Concept pass.** Iterate quickly in grayscale silhouettes to confirm readability on the target background. Validate that major shapes snap to the 32px grid.
2. **Detail pass.** Apply lighting and material cues using 3–5 values per material. Avoid sub-pixel detail; clarity at 100% zoom is a hard requirement.
3. **Animation (if applicable).** Block primary keyframes first (idle, anticipation, impact). Keep animation loops to multiples of 4 frames to sync with the simulation tick cadence. Export each frame in sequence on a single spritesheet row.
4. **Self QA.** Zoom out to the in-game scale (≈25% in Aseprite) and confirm the asset remains readable against a dark-sand test background layer. Verify icons retain silhouette clarity at 24×24 crop size for UI use.

## 4. Export Assets for Unity
1. **Spritesheets.** Export to PNG with nearest-neighbor resampling disabled and color depth set to 8-bit RGBA. Maintain consistent naming using the `spr_<context>_<descriptor>.png` convention from `docs/NAMING.md`.
2. **Icons & UI elements.** Export at 128×128 (or native frame size if smaller) to give Unity room for mipmaps. Name files `spr_ui_<descriptor>.png`.
3. **Animation metadata.** For animated sprites, include a matching JSON or Aseprite export (`spr_<name>_anim.json`) that records frame durations. Commit it alongside the PNG so Unity importer scripts or editor tooling can reconstruct animations.
4. **Source archive.** Save the layered source file inside `Assets/_Project/Art/Source/` with naming `src_<context>_<descriptor>.<ext>`.

## 5. Import into Unity
1. **Copy files.** Place exported PNGs under the appropriate runtime folder (`Assets/_Project/Art/Sprites`, `.../Tiles`, or `.../Icons`). Ensure folder structure mirrors the in-game category so automated addressables stay organized.
2. **Select asset(s) in Unity.** In the Inspector set:
   - **Texture Type:** Sprite (2D and UI).
   - **Sprite Mode:** Single for standalone icons; Multiple for spritesheets. Use the Sprite Editor to slice tiles on the 32×32 grid or by defined frame size.
   - **Pixels Per Unit:** `32` (unless a documented exception applies).
   - **Filter Mode:** Point (no filter).
   - **Compression:** None during development. Switch to `Low Quality` once artifact-free results are confirmed.
   - **Generate Mip Maps:** Off for pixel sprites, On for UI elements needing smooth scaling.
3. **Prefab / Scriptable setup.** Attach sprites to prefabs, tile palettes, or ScriptableObject databases as required by the requesting feature. Follow `docs/SCENES_AND_FLOW.md` for scene organization.

## 6. Validation Checklist
- Asset passes the acceptance criteria defined in Section 1.
- File naming matches `docs/NAMING.md` and assets live in the correct folders.
- Source files are committed and export settings documented in the asset’s issue or pull request.
- Unity import settings match the pixel-art defaults.
- In-game preview (Play Mode or dedicated test scene) confirms readability against UI and gameplay backgrounds.
- Automated tests referencing the asset (if any) have been updated.

## 7. Review & Iteration
1. **Peer review.** Include screenshots or GIF captures of the asset in context within the pull request. Annotate any known follow-ups.
2. **Feedback loop.** Address review comments by updating the source file, re-exporting, and re-importing. Keep previous exports until QA signs off to make rollbacks easy.
3. **Final approval.** Merge only after art direction and feature owner confirm the asset meets both stylistic and gameplay readability requirements.

## 8. Maintenance & Updates
- When adjusting an existing sprite, bump the version in the pull request description and note whether dependent prefabs or animations were touched.
- Batch similar assets in a single PR to simplify regression testing, but avoid mixing unrelated visual themes.
- Archive deprecated art in `Assets/_Project/Art/_Archive/` (create if missing) so builds stay lean while preserving history for reference.

Following this pipeline keeps Desert Wastes’ visuals cohesive, performant, and easy to maintain as the game evolves.
