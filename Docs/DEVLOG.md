# DEVLOG

A running log of meaningful changes, decisions, and gotchas for Lost Chambers: Idle Relics. Entries are dated and appended at the top (newest first). This file is intended to be read by both Fabian and Claude at the start of new sessions so context carries across conversations.

**Conventions:** Each entry has a date, a one-line summary, a *What changed* section, a *Why* section for non-obvious decisions, and an *Open items* section for things deliberately left unfinished.

---

## 2026-04-25 — PlayerAdventurer (Player6): male Indiana-Jones-style character via Hunyuan3D + full pipeline

**Summary.** Created a new male Indiana-Jones-archetype character (Player6, internal name "Adventurer") from a reference image using Hunyuan3D, then processed through the full 3D pipeline: decimation, axis alignment, X=0 mirror symmetrize, 9-region atlas + UV assignment, rigging with rigid-region weight cleanup, symmetric weight averaging via mirror-pair, preflight pass, FBX export, Unity asset wiring. PlayerExplorer (also Player6) had been reverted in commit `da1a322`, so the Player6 slot was free; we reused it. Setup of the Unity-side PlayerConfig + GameConfig registration is gated on the user running an Editor menu item once.

### What changed

- **`Desktop/IdleRelic/PlayerAdventurer_*.blend`** — Blender source files. Checkpoints: `_pre_generate`, `_pre_decimate`, `_pre_material`, `_pre_rig`, `_pre_export`.
- **`ResourcesStatic/Models/Units/PlayerAdventurer.fbx`** (+ `.meta`, GUID `f033a9792b3a4273be25c7351b2819b1`) — Exported character mesh. **3054 verts, 5876 faces, 22-bone armature** matching PlayerA exactly. Single welded island, 0 boundary edges, perfect X=0 mirror symmetry (max mirror dist 0.0000mm), 0 unweighted verts. `materialImportMode: 0` (None) — no work_mat sub-asset.
- **`ResourcesStatic/Textures/PlayerAdventurerAtlas.png`** (+ `.meta`, GUID `8297094d7d0b44c3ac4163401248e1af`) — 9-pixel horizontal atlas: Skin, Hair, Hat, Hatband, Jacket, Shirt, Belt, Pants, Boots. Same import settings as PlayerArchaeologistAtlas (filterMode Point, no mipmaps, maxTextureSize 32).
- **`ResourcesStatic/Materials/PlayerAdventurerMaterial.mat`** (+ `.meta`, GUID `f17be10dd4144c0db637412bf2d0bca6`) — Standard shader material referencing the new atlas.
- **`Scripts/Config/PlayerConfig/PlayerConfig.cs`** — Added `Player6 = 6` to PlayerIndex enum.
- **`Scripts/States/GamePlayState.cs`** — Temporary test override changed from `player = 5` (Archaeologist) to `player = 6` (Adventurer), still marked `// TODO: REMOVE`.
- **`Assets/Editor/PlayerAdventurerSetup.cs`** — One-shot Editor utility (`Tools > Setup PlayerAdventurer`) that reads the deterministic FBX mesh sub-asset fileID, creates `Resources/PlayerConfigs/6Adventurer.asset` with proper Body + BodyMaterial references, and appends it to `GameConfig._playerConfigs`. **User must run this once after Unity reimports the new assets**, then delete the script.

### Why

This is the male character from the design doc ("Male: Indiana Jones archetype — fedora, leather jacket, brown/khaki"). The current shipped PlayerArchaeologist (Player5) is functional but had 5 rounds of repair behind it and one accepted cosmetic artifact. The user provided a cleaner reference image and asked for a fresh character via the standard pipeline. We slotted it as Player6 (Explorer's freed slot) rather than overwriting Archaeologist, so both old and new can coexist.

### Pipeline iteration notes (gotchas hit and resolved)

1. **`bpy.ops.wm.read_homefile()` killed the Blender MCP socket** on first attempt to reset the scene. Workaround: don't use `read_homefile`; just manually delete all objects and add a temp cube to give FBX import an active object for its mode_set poll.
2. **First Hunyuan3D call returned "no data received"** but actually completed in the background; the second retry caused a duplicate import (`geometry_0` and `geometry_0.001`). Lesson: when the API errors, check `get_scene_info` before retrying — the import may already have happened.
3. **Hunyuan output came in Z-up at correct meter scale**, unlike PlayerExplorer (which was Z-up but unscaled and needed 67x). No rotation needed for the mesh, just a 1.054x uniform scale to match PlayerA's 2.099m height.
4. **PlayerA's armature came in Y-up while the mesh was Z-up** — orientation mismatch was the root cause of `ARMATURE_AUTO` failing with "Bone Heat Weighting: failed to find solution for one or more bones" and all 3055 verts unweighted. Fix: rotate the armature 90° around X and apply, so its bone-data Y-axis becomes the world Z-axis aligning with the mesh. *And* an earlier `transform_apply(scale=True)` on the armature had collapsed bone-internal cm units into meter units — so the armature's edit-bone data is at 100x the display scale and the bone envelopes were 1mm radius (envelope-parent path also failed). The fix sequence: (a) rescale armature 0.01x and apply to bake cm→m into bone data, (b) rotate 90° around X and apply to switch Y-up→Z-up, (c) restore 100x scaling on bone envelope radii so envelope/heat have something to work with.
5. **Mesh object scale silently became 0.01x** during the parent-set/clear-parent dance, making `gen.dimensions` show 13mm × 9mm × 21mm. Fix: explicitly `gen.scale = (1,1,1)`. Visible only via `view_layer.update()` afterwards — the dimensions cache is stale until then.
6. **`img.pixels = list(...)` and `img.pixels.foreach_set(...)` both silently produced an all-zero atlas** in this Blender version. Workaround: write the PNG bytes directly using stdlib `struct + zlib` (105-byte minimal RGBA PNG), then `bpy.data.images.load(path)`. Pixel verification confirmed correct values immediately. *Add to Section 7 (Blender MCP Gotchas) of `3D_PIPELINE.md`.*
7. **Hand-zone SKIN classification at `cz < 1.00, |x| > 0.42` bled up into the lower sleeve** (130 SKIN faces in the cuff zone where they should be JACKET). Tightening to `0.50 < cz < 0.82` cleaned it. Lesson: position-based UV classification needs both lower AND upper bounds for hand-zone, not just an upper bound.
8. **Face/hair boundary on the head sides** — initial `ny < -0.3` test was too strict for cheek/jaw verts (those normals point sideways or slightly down). Switched to `cy < 0.08` (front half of head in scene's coordinate frame) plus an optional sliver-side rule for ear-level skin. Verified visually before rigging.
9. **Bone Heat algorithm worked once orientation was fixed** — full ARMATURE_AUTO produced a proper distribution (Spine=132, Spine1=84, Spine2=73, Hips=37) that's typical for a chibi humanoid. Asymmetry from auto-weights was nuked by mirror-pair averaging (max diff after: LeftUpLeg=169 vs RightUpLeg=164, +5 floating-point tie-break). All Left/Right pairs match exactly otherwise.
10. **8 boundary edges appeared after deleting 6 zero-area faces** post-decimate. `mesh.fill_holes(sides=4)` closed them with 2 new faces; mesh ended at 5876 faces, 0 boundary edges, 1 island. Re-symmetrized weights afterward.

### Final mesh state

```
3054 verts, 5876 faces, 1 mesh island, 0 boundary edges
22 bind bones (matching PlayerA exactly, AimNode deleted)
Bind pose asymmetry preserved: LEFT chain UP, RIGHT chain DOWN
Symmetric weights:
  LeftHand=234   RightHand=234
  LeftArm=67     RightArm=67
  LeftForeArm=47 RightForeArm=47
  LeftFoot=111   RightFoot=111
  LeftUpLeg=169  RightUpLeg=164  (5 verts of FP tie-break)
  Hips=39   Spine=129  Spine1=84  Spine2=73  Neck=38  Head=1204
0 unweighted verts
```

### Open items

- **User must run `Tools > Setup PlayerAdventurer`** in Unity once the new assets are reimported. This creates `6Adventurer.asset`, sets its Body/BodyMaterial refs (with the deterministic FBX mesh fileID), and appends it to `GameConfig._playerConfigs`. **After running, delete `Assets/Editor/PlayerAdventurerSetup.cs`.**
- **Temporary `player = 6` test override in `GamePlayState.cs` is in place.** Marked `// TODO: REMOVE`. Must be reverted before any commit that ships.
- **Icon is null** on `6Adventurer.asset`. Placeholder needed eventually.
- **Unity Play mode verification pending** — need to confirm idle/walk/carry animations look correct, no weight-bleed artifacts, atlas colors render correctly.
- **Eye/eyebrow geometry from Hunyuan kept as dark patches** — they happen to fall into the HAIR atlas pixel because of their normals/position, which is the right look for chibi eyes (per reference image). Not a defect, just worth noting.

---

## 2026-04-12 — PlayerExplorer (Player6): female explorer character created via Hunyuan3D + full pipeline

**Summary.** Created a new female explorer character (Player6) from a reference image using Hunyuan3D for mesh generation, then processed through the full 3D pipeline: decimation, symmetry enforcement, material assignment, rigging with rigid-region weight cleanup, symmetric weight averaging, preflight check, FBX export, and Unity registration. Character follows the Archaeologist (Player5) per-character material pattern.

### What changed

- **`Desktop/idle Relic/PlayerExplorer_final.blend`** — Final Blender source file. Multiple checkpoints saved during pipeline: `_pre_generate`, `_pre_decimate`, `_pre_material`, `_pre_rig`, `_pre_export`, `_final`.
- **`ResourcesStatic/Models/Units/PlayerExplorer.fbx`** — Exported character mesh. 2051 verts, 3954 faces, 22-bone armature matching PlayerA exactly. Single welded island, 0 boundary edges, perfect X=0 symmetry.
- **`ResourcesStatic/Textures/PlayerExplorerAtlas.png`** (+ `.meta`) — 7-pixel wide atlas with color swatches: Skin, Hair (auburn), Shirt (khaki), Pants (brown), Belt (dark leather), Boots, Satchel. Import settings: `filterMode: 0` (Point), `enableMipMap: 0`, `maxTextureSize: 32`, `textureCompression: 0`.
- **`ResourcesStatic/Materials/PlayerExplorerMaterial.mat`** (+ `.meta`) — Standard shader material referencing the explorer atlas.
- **`Resources/PlayerConfigs/6Explorer.asset`** (+ `.meta`) — PlayerConfig: Index=6, Sex=Female, LabelKey=EXPLORER, Body=FBX mesh, BodyMaterial=explorer material, same attribute bonuses as Archaeologist, FreeConditionConfig unlock.
- **`Scripts/Config/PlayerConfig/PlayerConfig.cs`** — Added `Player6 = 6` to PlayerIndex enum.
- **`Resources/GameConfig.asset`** — Added 7th entry to `_playerConfigs` array (6Explorer GUID).
- **`Scripts/States/GamePlayState.cs`** — Temporary test override changed from `player = 5` to `player = 6`.

### Why

This is the "Second Playable Character" from the game design — a female explorer archetype (auburn hair, khaki shirt, no Lara Croft copy). Used the same per-character material override pattern established for the Archaeologist, avoiding changes to the shared `AtlasMaterial.mat` / `AtlasTexture.png`.

The Hunyuan3D generation ran locally and produced a ~275K face mesh that was decimated to ~4K faces. The mesh cleanup required: vertex-level coordinate rotation (Z-up to Y-up to match armature), uniform scaling (~67x to match armature dimensions), mirror-modifier symmetrization, dissolve of non-manifold verts to close persistent boundary edges, and final re-symmetrize.

### Open items

- **Temporary `player = 6` test override in `GamePlayState.cs` is in place.** Marked `// TODO: REMOVE`. Must be reverted before any commit that ships.
- **Icon is null** on `6Explorer.asset`. Placeholder needed eventually.
- **Material zone refinement** — the satchel strap and bag are not assigned as a separate color zone. The current assignment uses 5 zones (skin, hair, shirt, pants, belt, boots) with the satchel color unused. A future pass could add the satchel strap as a diagonal band across the front torso.
- **Unity Play mode verification pending** — need to confirm idle/walk animations look correct, no weight-bleed artifacts.

---

## 2026-04-11 — PlayerArchaeologist hidden skin body removed; rig saga signed off

**Summary.** Final cleanup pass on the PlayerArchaeologist mesh: deleted the hidden inner skin body that was sitting underneath the pants/jacket and causing visible "skin color stretch" artifacts during walk. Mesh dropped from 3815 → 3763 verts (−52, −1.4%) and from 5762 → 5751 faces. One known cosmetic artifact remains (a small jacket-colored patch visible only from a back-of-character camera angle that the game doesn't normally use); accepted as ship-acceptable, see Open items.

### What changed

- **`Desktop/idle Relic/PlayerArchaeologist.blend`** — deleted 174 hidden SKIN-UV faces in the under-clothing zone, filled the resulting 5 holes at the wrist/cuff seams with new triangles, deleted a stray 88cm wire-edge artifact left by `fill_holes`, re-symmetrized weights.
- **`Assets/.../PlayerArchaeologist.fbx`** — re-exported. Mesh sub-asset name and fileID still unchanged, so `5Archaeologist.asset` Body reference still resolves.
- **`Assets/Editor/PlayerArchaeologistMeshDiagnostic.cs`** + meta + folder — **deleted**. The diagnostic probe is no longer needed.

### Why

The user reported a "stretch of skin color" appearing in the inner-thigh / between-the-legs area during the walk cycle. Initial wrist-area weight cleanup (round 3e) reduced it but didn't eliminate it. Read-only investigation in Blender via `bmesh` + UV inspection revealed the real cause:

The atlas pixel-to-color mapping had to be read directly from `PlayerArchaeologistAtlas.png` to identify which UV is which color. Pixel 0 (UV `0.056`) is **SKIN** (RGB 133,66,36 — that orange-peach color); pixel 4 (UV `0.5`) is **JACKET** (very dark brown). Earlier I had assumed UV `0.5` was skin because I sampled "hand verts" at `|x|>0.5, z=0.78-0.86` and they all had UV `0.5` — but at that z they're actually the cuff/sleeve area, not the visible hand. The actual visible hand sits **lower**, at z≈0.40-0.55 (where the hand naturally hangs at the side in A-pose), and has UV `0.056`.

After re-searching with the correct skin UV, found **174 SKIN-colored faces in the under-clothing zone** (`z<1.05` outside the visible hand region) — an underlying skin body the artist had modeled and then covered with pants/jacket geometry. These hidden skin verts had inherited some auto-weight bleed from the leg bones, so during walk they were getting pulled and producing the "skin stretch" through whatever rendering quirk made them visible.

**Removing the hidden body fixed the root cause** instead of patching the symptom. The 40 misweighted hand-area verts I was about to weight-patch individually (round 3e+) **were** part of the hidden body — they got swept up in the cleanup automatically. Hand primary-vert counts dropped from 416 → 391 (−25 per side), which matches the predicted loss of inner-side wrist verts.

### Stage 2 cleanup considered, deferred

Survey found **804 inward-facing faces** total (~14% of the mesh) — the inner sides of clothing tubes that are never visible. A Stage 2 pass could remove most of those for a further ~13–18% vert reduction. **Not done in this round** — the rig is at "good enough to ship," more aggressive cleanup risks new artifacts, and we already spent significant iteration on this character. Save Stage 2 for a future optimization pass if the game ever needs it.

### Workflow lessons (added to the running list)

7. **Read atlas pixel colors directly before identifying body regions by UV.** Sampling "known body parts" by position can mislead — at a glance the wrist looks like the hand, but it's actually the cuff. The atlas is the ground truth; sample its pixels and look at the RGB values.
8. **Hidden geometry is often the root cause of "weight bleed" symptoms.** When the auto-weights diagnostic shows mysteriously misweighted verts in the wrong place, check if those verts belong to a hidden inner shell that was never supposed to be skinned. Removing the hidden geometry can be a faster fix than re-weighting it.
9. **Save a checkpoint .blend before AND after each destructive UV/topology edit.** I had a checkpoint before the cleanup but not between cleanup-with-jacket-fills and cleanup-with-skin-fills, which made the recolor revert harder than it needed to be.
10. **`fill_holes` can create wire-edge artifacts** (an edge with no faces) when it tries to bridge two unrelated boundary verts. Always check for boundary edges after a fill operation, even if the count looks like "almost done."

### Final mesh state

```
3763 verts, 5751 faces, 1 mesh island, 0 boundary edges
22 bind bones (matching PlayerA exactly)
Symmetric weights:
  LeftHand=391  RightHand=391
  LeftArm=24    RightArm=24
  LeftForeArm=79 RightForeArm=79
  LeftFoot=78   RightFoot=78
  Hips=185      Head=1034
0 unweighted verts
SKIN faces: 750 (face/head/neck/visible hand only — 0 hidden)
```

### Open items

- **Cosmetic artifact: small jacket-colored patch visible from a back-of-character below-the-hand angle.** When deleting the hidden skin body, the holes left at the wrist/cuff seams were filled with jacket-colored faces (UV `0.5`). A few of those fill faces are part of the back-side of the wrist surface and visible when the camera looks at the back of the hand from below. An attempt to recolor them to skin reintroduced the "skin stretch" symptom (they still have residual weight from neighboring verts), so reverted. **Accepted as ship-acceptable** because the typical idle-game camera angle (top-down 3/4 from above-and-behind) doesn't expose this view. Future fix options if it ever matters: (a) move fill verts slightly inside the hand to hide them, (b) introduce a transparent material slot for the fill faces, (c) sculpt the visible hand to cover them. None are blocking.
- **Stage 2 cleanup not done.** ~800 inward-facing faces (clothing inner shells) could be removed for ~15% further vert reduction. Not blocking; future optimization opportunity.
- **Temporary `player = 5` test override in `GamePlayState.cs` is still in place.** Marked `// TODO: REMOVE`. **Must be reverted before any commit that ships.**
- **Icon is still null** on `5Archaeologist.asset`. Placeholder needed eventually.
- **`PlayerArchaeologist.fbm/`** subfolder still contains a duplicate copy of the atlas extracted from the FBX. Harmless dead weight; could be eliminated by flipping `materialImportMode` in the .meta to `0` (None).

---

## 2026-04-11 — PlayerArchaeologist rig and skinning fixed in-place via Blender MCP

**Summary.** Five iterative rounds of mesh and weight surgery in Blender (live, via MCP) to make the new PlayerArchaeologist character animate without visible defects. Started from "visible but mostly broken" — head + boots floating apart, arms misweighted, body distorting on every step — and ended with "symmetric, no holes, no body distortion, hands tracking correctly with one minor stretching artifact left to address." All edits happened in the live Blender session against `Desktop/idle Relic/PlayerArchaeologist.blend` with the FBX re-exported to its project path each round. Multiple `*_pre_*.blend` checkpoints were saved as recovery snapshots.

### What changed

- **`Desktop/idle Relic/PlayerArchaeologist.blend`** — extensively rewritten in-place. Final state: 3815-vertex symmetric mesh (was 3683 asymmetric), 22-bone armature matching PlayerA exactly, vertex weights cleaned via rigid-region rules.
- **`Assets/.../PlayerArchaeologist.fbx`** — re-exported many times during the iteration. Final export uses `apply_scale_options='FBX_SCALE_ALL'`, `bake_space_transform=True`, no leaf bones, Y-up / -Z-forward. Mesh sub-asset fileID `-6388267144930243731` is unchanged from the round-1 import, so the existing `5Archaeologist.asset` Body reference still resolves without edits.
- No Unity-side files changed in round 3. All round-1 wiring (PlayerConfig field, GameConfig registration, material/texture, GamePlayState test override) is still in place.

### The five iterations (in order)

**Round 3a — bone-mirror misadventure (REVERTED).**
After diagnosing that the LEFT arm chain bones pointed UP while the RIGHT arm chain pointed DOWN, "fixed" it by mirroring RIGHT to LEFT. **This was wrong.** PlayerA has the *exact same* asymmetric bone layout (LEFT=0/-1.39/-1.39/0 rolls vs RIGHT=180/24.77/24.77/-180) — it's intentional, and the existing animation clips depend on it. The mirror broke the LEFT arm rest pose; visible result was "left hand sticking up like a broken arm." Reverted from `_pre_round3` checkpoint. Lesson: **always inspect the reference rig first before assuming asymmetry is a bug.**

**Round 3b — mesh hole fix via merge-by-distance.**
The mesh imported from Cowork had 4 separate islands (body 2615v, right arm 543v, left arm 522v, 3-vert stray fragment) and 186 boundary edges in 4 hole loops at the shoulder seams. Diagnosed first as missing topology, but `bpy.ops.mesh.remove_doubles(threshold=0.0001)` revealed the truth: the 93 duplicate-position vertices were the un-merged seam verts between arm islands and body. Welding them collapsed 4 islands into 1 with 0 boundary edges. **The "Bridge Edge Loops" step in the original plan was unnecessary.** Holes in the body and the hat shadow disappeared after this single operation.

**Round 3c — weight bleed cleanup pass 1 (arms onto body).**
The auto-weights gave 78% of core torso verts some arm-bone weight, including 16% with > 0.3 weight. Targeted cleanup: zeroed `LeftShoulder/Arm/ForeArm/Hand` and right-side equivalents on verts in the `z<1.0, |x|<0.32` zone, then renormalized. 50 verts ended up with no weight at all (they only had arm weight); fallback-assigned them to the geometrically nearest spine bone (mostly Spine1/Spine2). Result: walking no longer pulled the body sideways with the arms, but the *hips* still distorted because UpLeg bone bleed onto the waist was untouched.

**Round 3d — mesh asymmetry diagnosis and symmetrize.**
Symptom: even with symmetric cleanup rules, the LEFT and RIGHT primary-vert counts kept coming out asymmetric (e.g. LeftLeg=468 vs RightLeg=49 at one point), and the user reported the idle pose was visibly tilted (one ear visible, the other not). KDTree-based mirror-distance check confirmed the mesh itself was sculpted asymmetrically: mean mirror distance 1.54cm, max 8cm at the face, 7cm at the hip. Fixed with `bpy.ops.mesh.symmetrize(direction='POSITIVE_X', threshold=0.0001)` which mirrors the +X half onto -X. After symmetrize: every vertex has a perfect mirror within 0.00000 (3815 / 3815 verts). Re-skinned with auto-weights afterward.

**Round 3e — symmetric weight averaging + rigid-region cleanup.**
Even with a perfectly symmetric mesh, auto-weights still produced asymmetric weights because the bones themselves are asymmetric (LEFT bones point up, RIGHT bones point down). Custom Python pass: for each vertex pair `(v_left, v_right)`, average their weights with group-name flipping (`Left↔Right`). This forces true bilateral symmetry of weights. Then layered the rigid-region cleanup: face (z≥1.30) → 100% Head only; neck (1.10-1.30, |x|<0.20) → no arms/legs; chest (0.95-1.10, |x|<0.18) → spine only; belly (0.80-0.95, |x|<0.20) → spine only; lower torso (0.65-0.80, |x|<0.20) → spine only. The tight `|x|` filters spare the upper-arm verts. Final UpLeg cutoff (z≥0.72 → zero, falloff 0.65-0.72) on top of all this. Final symmetric averaging pass to wash out tie-break deltas.

### Why the rigid-region approach was needed

Auto-weights via Bone Heat is geometric: it assigns each vertex to the bones whose medial axis is closest. For a humanoid in A-pose with arms hanging at the sides, this gives weird results: a vertex on the upper-left chest at world `(0.30, 0, 0.95)` is geometrically closer to the `LeftHand` bone at `(0.49, 0.86, 0)` than to `Spine2` at `(0, 0.92, 0)`. The algorithm correctly identifies the closer bone — but the closer bone is semantically wrong. Result before cleanup: **the chest's primary bone was LeftHand/RightHand** (64 verts each). The face had 25% of its weight on shoulders/arms/neck. When the shoulders rotated during walk, the entire face deformed.

The fix is to override the geometric algorithm with semantic body-region rules. Anything in the head region is *only* weighted to Head. Anything in the chest center is *only* weighted to Spine2. Etc. The tight `|x|` filters are critical — they spare the upper arm and shoulder verts which sit at `|x| > 0.18` even when arms hang down.

### Final weight distribution (perfectly symmetric)

```
LeftHand=416   RightHand=416   d=0
LeftArm=24     RightArm=24     d=0
LeftForeArm=76 RightForeArm=76 d=0
LeftFoot=78    RightFoot=78    d=0
Hips=185       Head=1034
Face non-Head weight: 0.000
0 unweighted verts
1 mesh island, 0 boundary edges
22 bind bones, max bone idx 21
```

### Key lessons

1. **Always inspect the reference rig before assuming asymmetry is a bug.** PlayerA's bone bind pose is intentionally asymmetric — the mirror "fix" cost a round.
2. **Merge-by-distance can fix mesh holes that look like missing topology.** Don't reach for Bridge Edge Loops until you've confirmed the verts at the seam are actually different verts.
3. **Mesh symmetry is a precondition for symmetric skinning.** No amount of weight cleanup can compensate for an asymmetric mesh.
4. **Auto-weights is geometric, not semantic.** For humanoids in A-pose, you usually need to override the auto-weights with body-region rules to keep the chest from being skinned to the hands.
5. **`bake_space_transform=True` and `apply_scale_options='FBX_SCALE_ALL'` are required for Blender→Unity FBX exports.** Wrong export settings turned a previous export into a 1cm character on its back.
6. **Save checkpoint .blend files before each destructive edit.** Several rounds of this work were salvaged via `_pre_*.blend` snapshots.

### Open items

- **Hand stretching during walk**: minor artifact still visible. The wrist/cuff area on the arm stretches noticeably during the walk cycle. Likely caused by the upper-arm vert that was reassigned during the rigid-region cleanup having an awkward weight blend at the cuff. To investigate next.
- **Temporary `player = 5` test override in `GamePlayState.cs`** is still in place. **Must be reverted before any commit that ships.** It's clearly marked with `// TODO: REMOVE`.
- **Diagnostic probe `Assets/Editor/PlayerArchaeologistMeshDiagnostic.cs`** is committed for now (still useful for further iteration). Can be deleted once the rig is fully signed off.
- **Asymmetric tie-break deltas** in `LeftUpLeg`/`RightUpLeg` (28-vert diff) and `LeftLeg`/`RightLeg` (16-vert diff) are floating-point tie-break artifacts from the auto-weights step. Shouldn't be visible but if any remaining hip-area asymmetry surfaces, this is the cause.
- **Icon is still null** on `5Archaeologist.asset`. Won't crash anything; placeholder needed eventually.
- **`PlayerArchaeologist.fbm/`** subfolder contains a duplicate copy of the atlas PNG that Unity extracted from the FBX (because `materialImportMode: 2`). It's harmless dead weight; could be eliminated by flipping the FBX import to `materialImportMode: 0` once we're done iterating.

---

## 2026-04-11 — PlayerArchaeologist registered as Player5 + per-character material support

**Summary.** Wired the re-rigged `PlayerArchaeologist.fbx` into the player system as a new playable character at index 5. Along the way, extended the player system with optional per-character material overrides — the existing assumption that *all* players share `AtlasMaterial.mat` + `AtlasTexture.png` did not hold once the archaeologist arrived with its own custom 9-color atlas, so `PlayerView` now also swaps `sharedMaterial` when the config provides one. Forced index 5 in `GamePlayState` with a temporary override for visual verification.

### What changed

- **`Scripts/Config/PlayerConfig/PlayerConfig.cs`** — Added `Player5 = 5` to the `PlayerIndex` enum. Added a new serialized `Material BodyMaterial` field on `PlayerConfig` (nullable; null = keep prefab default).
- **`Scripts/Level/Player/PlayerController.cs`** — Added `Material BodyMaterial` field on `PlayerModel` and assigned it from `config.BodyMaterial` in the constructor, mirroring the existing `BodyMesh = config.Body` line.
- **`Scripts/Level/Player/PlayerView.cs`** — `OnModelChanged` now also runs `_body.sharedMaterial = model.BodyMaterial` when the model provides a non-null material. Behavior is unchanged for every existing character (their PlayerConfig leaves `BodyMaterial` null, so the prefab's `AtlasMaterial.mat` continues to be used).
- **`Scripts/States/GamePlayState.cs`** — Added a temporary `if (_config.PlayersMap.ContainsKey(5)) player = 5;` override right after the existing fallback block, with a `// TODO: REMOVE` comment. Purpose: force the archaeologist to spawn for visual verification regardless of save state.
- **`ResourcesStatic/Textures/PlayerArchaeologistAtlas.png`** (+ `.meta`) — Copied from `Desktop/idle Relic/`. Custom `TextureImporter` settings: `filterMode: 0` (Point — no bilinear blur on the 9-pixel atlas), `enableMipMap: 0`, `textureCompression: 0`, `maxTextureSize: 32`. GUID `94a4ccd463fd49aea1dc71969f20ca72`.
- **`ResourcesStatic/Materials/PlayerArchaeologistMaterial.mat`** (+ `.meta`) — New material, hand-authored YAML modeled on `AtlasMaterial.mat` (same Standard shader, same property defaults). `_MainTex` references the new atlas PNG. GUID `d4250ef98c73475e893fe6f933b23a65`.
- **`Resources/PlayerConfigs/5Archaeologist.asset`** (+ `.meta`) — New PlayerConfig: Index 5, Sex Male, LabelKey `ARCHAEOLOGIST`, Body referencing the FBX mesh sub-asset (`fileID: -6388267144930243731`, guid `8f31670f5ed2674408cfa69562c572ea`), `BodyMaterial` referencing the new material, Infos matching PlayerA (Capacity 1, WalkSpeed 0.1, RotateSpeed 1), unlock = `FreeConditionConfig`. Icon left null (placeholder). GUID `b87ee2591fc44b40a8f4d018f4596be3`.
- **`Resources/GameConfig.asset`** — Appended the new PlayerConfig GUID to `_playerConfigs` (now 6 entries: BellhopMan, BellhopWoman, Gustav, PlayerA, PlayerB, Archaeologist).

### Why

The task brief assumed registering a new character was a config-only job: drop a PlayerConfig in, point it at the new mesh, register in GameConfig, done. That assumption broke against the actual prefab architecture. The shared `Player.prefab`'s `SkinnedMeshRenderer` has exactly **one** material slot pointing at `AtlasMaterial.mat`, which samples a single 1024×1024 `AtlasTexture.png` color palette. All four existing player FBXs (PlayerA, PlayerB, PlayerLora, PlayerBomber) were UV-mapped at authoring time to sample colors from *that specific texture* — verified by binary-scanning each FBX (each contains 6 references to `AtlasTexture`). Cowork's `PlayerArchaeologist.fbx`, by contrast, was UV-mapped to a brand-new 9-pixel atlas — the mesh's UV coordinates point at pixel positions that, when sampled from the 1024-pixel shared atlas, would have produced visual garbage.

Three possible fixes were considered: (1) re-UV the mesh in Blender to sample colors from the existing shared atlas (no code change, but another Cowork round-trip and you lose the exact color choices), (2) paint the new colors into unused space in `AtlasTexture.png` (no code change, but modifies a shared asset that 4 other characters depend on — risk of clobbering existing UVs), or (3) add per-character material override support. Option 3 won because it's the smallest total change consistent with where the architecture needs to go anyway — Lora and Bomber happen to share the existing palette by accident of how they were authored, and that won't hold for every future character. The two existing `OnModelChanged` lines became four; every existing PlayerConfig still works because `BodyMaterial` is null on them and the prefab default is preserved.

The temporary `player = 5` override in `GamePlayState` exists because `_gameManager.Model.Player` defaults to 0 from save state, and there's no in-game character selection UI invoked during normal play startup. Forcing 5 is the simplest way to verify the new wiring without touching save data. **It must be reverted before shipping** — revert is a single line delete.

### How the FBX mesh sub-asset fileID was found

`PlayerConfig.Body` is a `Mesh` reference written as `{fileID: <int>, guid: <fbxGuid>, type: 3}`. The integer fileID is Unity's deterministic local ID for the mesh sub-asset inside the FBX, and there's no clean way to read it from outside the editor. The Unity MCP `execute_code` tool is broken in this environment (mono temp-path "filename too long" error on every invocation). Workaround: wrote a one-shot Editor script `Assets/Editor/PlayerArchaeologistMeshIdProbe.cs` exposing a `[MenuItem]` that called `AssetDatabase.LoadAllAssetsAtPath` + `TryGetGUIDAndLocalFileIdentifier`, ran it via `execute_menu_item`, read the result from the console (`Mesh name='PlayerArchaeologist' fileID=-6388267144930243731`), then deleted the script and the now-empty `Assets/Editor/` folder. The probe also incidentally confirmed the new FBX has the right 23-bone skeleton including `AimNode`, `Hips`, `Spine`/`Spine1`/`Spine2`, the `Left*`/`Right*` arm/leg/toe chains — i.e. Cowork's re-rig was structurally faithful to PlayerA.

### Open items

- **Visual verification in Play mode is still pending.** The archaeologist needs to actually be loaded in the editor and inspected: mesh appears, bones deform without distortion under the existing `UnitAC.controller` clips (Walk, Idle, Carry, etc.), atlas colors render correctly, character isn't giant/tiny/offset. If anything looks wrong, the first suspect is the new texture/material setup (most likely candidates: filter mode bleeding adjacent pixels, sRGB gamma mismatch, or the new mesh's UVs not actually targeting the centers of the 9 pixels Cowork promised).
- **The temporary `player = 5` override in `GamePlayState.Initialize()` MUST be reverted before any commit that ships.** It's deliberately wrapped in a `if (_config.PlayersMap.ContainsKey(5))` guard so it fails gracefully if Player5 ever gets removed, but that's a safety net, not a substitute for deleting the line. Marked with a `// TODO: REMOVE` comment.
- **Icon is null** on the new PlayerConfig. Won't crash anything, but the UI character-select element (if/when it shows Player5) will render a missing-sprite slot. A placeholder Sprite can be slotted in later.
- **PlayerArchaeologist.fbx has `materialImportMode: 2` (Import)** which causes Unity to extract a sub-asset Material called `work_mat` from the FBX. This is harmless because the prefab's SMR ignores it (the runtime swap to `PlayerArchaeologistMaterial.mat` overrides it), but it's a tiny bit of dead asset bloat. Could be flipped to `materialImportMode: 0` (None) in the .meta if we want it cleaned up — not blocking.
- **The `BodyMaterial` swap in `PlayerView.OnModelChanged` only fires once at character load.** If the player ever needs to change materials at runtime (e.g. a damage flash, a transformation effect) the existing pattern still works — just call `OnModelChanged` with a new model. Not a current requirement.

---

## 2026-04-11 — PlayerArchaeologist re-rigged to canonical player skeleton

**Summary.** Re-rigged the PlayerArchaeologist mesh from its original 20-bone Blender armature to the project's canonical 23-bone player skeleton (matching PlayerA.fbx exactly). Baked 9 flat-color materials into a single atlas texture. The FBX is now skeleton-compatible with the shared `Player.prefab` and ready for Unity-side `PlayerConfig` registration.

### What changed

- **`ResourcesStatic/Models/Units/PlayerArchaeologist.fbx`** — Overwritten with re-rigged version. 23 bones (was 20), 1 material `work_mat` (was 9), 5,322 faces, 3,683 verts. Bone names, hierarchy, order, and bind pose match PlayerA.fbx exactly. No baked animations (Unity's `UnitAC.controller` provides all animation).
- **`Desktop/idle Relic/PlayerArchaeologistAtlas.png`** — New 9×1 pixel atlas texture containing the 9 flat material colors (Skin, Hair, Hat, HatBand, Jacket, Shirt, Belt, Pants, Boots). UVs on the mesh map each face to the correct color patch.
- **`Desktop/idle Relic/PlayerArchaeologist_pre_rerig_backup.blend`** — Pre-edit backup of the Blender file.
- **`Docs/3D_PIPELINE.md`** — Updated asset log with re-rigged FBX entry.

### Why

The player system uses a single shared skeleton on `Player.prefab`. `PlayerView.OnModelChanged()` assigns `_body.sharedMesh = model.BodyMesh` without rebinding the `bones[]` array — so the new mesh's bone weights must index into the same bone slots as the prefab. The original Blender rig used different bone names (`Chest` vs `Spine1`/`Spine2`, `.L`/`.R` suffixes vs `Left`/`Right` prefixes), a different hierarchy (extra `Root` bone, missing `AimNode`), and was missing toe bones. Rather than renaming, we imported PlayerA's armature directly and re-parented the mesh with automatic weights — guaranteeing byte-level skeleton compatibility.

The 9-to-1 material atlas was required because the prefab's `SkinnedMeshRenderer` has exactly 1 material slot. All existing player FBXs (PlayerA, PlayerB, PlayerLora, PlayerBomber) follow this same single-atlas convention.

### Open items

- **Unity-side PlayerConfig registration** — A new `PlayerConfig` ScriptableObject needs to be created at `Resources/PlayerConfigs/` (next index = 5), referencing the re-rigged mesh. This is being handled in a separate Claude Code session, not part of this work.
- **Atlas texture location** — The atlas PNG currently lives on the desktop. It may need to be moved into the Unity project if the URP material needs to reference it directly (depends on how the shared material is set up — it might just use vertex colors or a shared texture).
- **Face count difference** — The re-rigged mesh has 5,322 faces vs the original 7,320. The difference is from the scaling + reimport process. Visually identical; worth noting if anyone checks polygon counts later.

---

## pre-2026-04-08 — Chamber lighting, torch-lighting maid, and collector office design (backfilled)

**Summary.** Chain of decisions from an earlier Cowork session that established the chamber lighting loop, re-fictioned the maid as a torch-lighter, added player-vs-NPC item stealing, and locked in the collector office design. Backfilled into this log after the fact.

### Chamber lighting is driven by room state, not worker count

Chambers start **dark** and only light up while the maid's torches are burning. The lighting trigger lives in the room state machine, not in the worker-count events:

- `RoomView.Awake` — chambers spawn dark.
- `RoomAvailableState` — lit (torches burning, room ready for workers).
- `RoomOccupiedState` — lit (workers digging under torchlight).
- `RoomUsedState` — dark (torches burned out, maid coming to relight).

An earlier attempt tied lighting to `OnWorkersChanged` (lit while any worker is physically inside). It was correct under the fiction "lit = someone is in the room," but got reverted once the maid-as-torch-lighter frame took over. State transitions are simpler and match the new fiction exactly.

### The maid is now a torch-lighter, not a cleaner (pure re-skin)

The existing cleaner NPC, pathfinding, queue logic, and 3-point item loop are all reused verbatim. Only the *fiction* changed: the maid lights 3 torches instead of cleaning 3 dirty points. **No new NPC, no new code path.**

One real behavior change: newly purchased chambers now transition into `RoomUsedState` instead of `RoomAvailableState` after `RoomReadyToPurchaseState`. This means the very first thing that happens to a fresh chamber is the maid arriving to light it — a small discovery moment for the player, and it keeps the loop consistent (every lit chamber was lit *by the maid*).

Loop: Purchased → dark → maid lights 3 torches → Available (lit) → workers arrive and dig → `StayDuration` expires → Used (dark, torches burned out) → maid re-queued.

Eventual rename candidate (deferred): `RoomAvailableState` → `RoomLitState`, `RoomUsedState` → `RoomUnlitState`, etc. Not worth the churn until the mechanic is fully locked in.

### Player can steal torch-lighting tasks from the maid (soft-claim system)

**Why:** the player should always feel more powerful than the NPC. Previously, the cleaner would call `_gameManager.RemoveItem()` the moment she picked a target, silently removing it from the player's `FindClosestUsedItem` lookup. Result: one of the 3 torches was effectively invisible to the player until the maid finished it.

**How it works now:**
- `ItemController.Claim(claimer)` / `ReleaseClaim()` — items carry a soft claim tag.
- `ItemRegistry.FindUsedItem` skips claimed items for *cleaners* (so cleaners don't fight over the same torch).
- Player's `FindClosestUsedItem` is **intentionally unfiltered** — player sees everything.
- When the player walks onto a claimed torch, `PlayerItemState.Initialize` calls `_item.Claim(this)`. The claimer change fires `CLAIM_REVOKED`. Both `CleanerWalkToItemState` and `CleanerCleaningState` subscribe and abort to `CleanerIdleState` on revoke.
- If the player walks away mid-light (joystick interrupt), they release the claim before re-adding the item to the registry, so the maid can pick it back up.
- When the cleaner finishes a torch normally, she now releases the claim *and* removes the item from the registry — without this, the `ClaimedBy` reference persisted across dig cycles (ghost claims), and the item stayed in the registry causing the maid to re-target the just-finished torch at zero distance (the "shaking maid" bug).

### Collector office design (agreed but mostly uncoded)

The plan for turning toilets into a relics collector office:

- Single inventory type: **parchment** (not parchment + ink). Keeps the change minimal and leaves a second inventory slot free for a future soda-machine-equivalent station where the helper has a separate job.
- Re-uses the toilet cabin/seat system verbatim (`ItemToiletController` stays as-is conceptually). Worker walks to free seat, hands over relic, collector logs it for `StayDuration`, then available again until parchment runs out.
- **Workers visit the collector only if they found a relic.** Empty-handed workers go home directly.
- `HasRelic` is hardcoded at **75%** for now — deliberately a placeholder so a future Power-Up system can modulate the find rate.
- **Relic finding does not replace cash generation.** Cash still trickles during `RoomOccupiedState`. The relic is a *separate post-dig outcome* that plays out when the worker leaves the chamber. This is why the collector visit can be theatrical/optional without breaking the economy.
- Class renames (`RelicCollectorController`, `CollectorOfficeModule`, etc.) deliberately deferred until the mechanic is locked in.

### Open items

- Empty-handed worker visual (slumped walk / "—" particle / nothing) — not decided.
- `RoomLit`/`RoomUnlit` rename pass — deferred.
- Collector class/module rename pass — deferred.
- Unified "dig outcome roll" that produces both the `HasRelic` boolean and (eventually) the rarity tier from a single random call — not built yet; current code is the 75% placeholder only.

---

## 2026-04-08 — Rule 0 added: read DEVLOG at task start

**Summary.** Added a "Rule 0" to CLAUDE.md's Working Rules mandating that every new task read `Docs/DEVLOG.md` (at minimum the most recent 3–4 entries) before answering the user's first substantive question.

### Why

First real test of the cross-session memory system revealed the gap: a new task started in the project loaded CLAUDE.md fine, but when asked "what did we do today?" the new Claude answered from inference instead of reading the devlog. CLAUDE.md *mentioned* the devlog existed but didn't *require* reading it, so the instruction was interpreted as "this file exists" rather than "open this file now." Rule 0 closes the gap by making the read explicit and positioning it *before* Rule 1 (the write rule) so it's the first thing a new session sees.

### Open items

- This is still a norm, not a hard guarantee. If a future task's first response feels uninformed, the reliable manual workaround is for Fabian to say "read Docs/DEVLOG.md first" as the opening message.

---

## 2026-04-08 — Chamber upgrade economics: dual-lever design

**Summary.** Locked in the upgrade-economics rule: chamber level controls both worker capacity *and* `StayFee` per dig. Verified against the existing `RoomOccupiedState` code, which already supports this model — it's a config decision, not a code change.

### The rule

Upgrading a chamber raises its **level**, and the level drives two things:

1. **Worker capacity.** Level 0 = 1 worker slot, level 1 = 2, level 2 = 3. The chamber's physical size gates how many workers can dig in parallel.
2. **`StayFee` per dig.** Each level tier in `RoomConfig.Lvls[]` can list a higher `StayFee`, so a higher-level chamber is intrinsically richer per completed dig.

The two levers compound: a level-2 chamber earns `3 workers × higher StayFee × faster throughput` compared to a level-0 chamber. This is deliberate — it's the classic idle-game curve and it gives upgrading a meaningful punch at every step.

### How it actually works in `RoomOccupiedState`

Cash and dig duration are both scaled by `ActiveWorkerCount` in `OnTick`:

```csharp
scaledDt = dt * workers;
_stayDuration -= scaledDt;                        // dig finishes worker× faster
int trickle = _baseTrickleAmount * workers;       // cash arrives worker× faster
```

Important nuance: the **total payout for a single dig** is always `StayFee`, regardless of how many workers are inside. Workers don't each print independent money — they *speed up* the dig, which raises throughput (digs-per-minute), which is what translates into more cash per minute. The per-dig amount is set once in `Initialize()` from `_room.Model.StayFee`, which itself comes from `RoomController.StayFee = Lvls[Lvl].StayFee`.

This means there are exactly two places to tune chamber economy:

- **`RoomConfig.Lvls[Lvl].StayFee`** — the per-dig payout for each level tier. Bump this to make upgrades feel richer per dig.
- **Worker capacity per level** — currently 1/2/3, could be tuned later if throughput feels off.

Duration tuning lives on `StayDuration`, which is also per-level via the same `Lvls[]` array, so you can independently decide whether higher-level chambers dig *longer* (more cash per dig but slower loop) or *shorter* (faster loop, less per dig).

### Why the dual-lever model instead of "workers are the only lever"

A pure "workers = money, chamber is just a container" rule was considered (flat `StayFee` across all levels, upgrading only unlocks worker slots). Rejected because it collapses two useful tuning knobs into one and makes late-game chambers feel economically identical to early-game ones except for being bigger. The dual-lever version lets Site 2's chambers feel genuinely richer per dig *as well as* bigger, which matters for per-site progression and for the eventual Site 2 unlock.

### Open items

- Actual `StayFee` / `StayDuration` / worker-cap numbers per level — not tuned, just the shape is decided.
- Whether the `StayFee` curve should be linear, exponential, or hand-tuned per level — deferred until there's enough chambers to playtest.
- Worker-hiring and worker-pool mechanics — deliberately deferred, not part of this decision.

---

## 2026-04-08 — Working Rules added to CLAUDE.md

**Summary.** Promoted two working rules (update DEVLOG after any non-trivial work; open prefabs before trusting class-name grep) from auto-memory into `idlerelics/CLAUDE.md`, so they apply to every Claude session on this repo automatically instead of depending on Fabian remembering to ask.

### What changed

- **`idlerelics/CLAUDE.md`** — added a new `## Working Rules (for Claude)` section immediately before `## Architecture`. Rule 1 makes devlog updates a required part of task completion. Rule 2 codifies the "open the .prefab, check `m_Script` GUID" investigation order to prevent repeats of the Hotel 2 vending-machine mis-read.

### Why

The previous setup relied on Claude's auto-memory (`.auto-memory/`), which is machine-scoped and not part of the repo. That meant the rules only fired when Claude happened to load memory, and any new Claude environment would start blind. Putting the rules in `CLAUDE.md` makes them travel with the repo in git — visible to Fabian, versioned, and loaded automatically by any Claude agent that reads the project's CLAUDE.md at startup.

### Open items

None — self-referential entry; the rule was applied to document its own addition.

---

## 2026-04-08 — Inspector-driven hotel start override

**Summary.** Replaced the earlier hardcoded `_hotel = 2;` debug hack with a proper Inspector-driven override on `GameConfig`, so switching test hotels no longer requires editing code (and no longer risks an accidental commit of a hack).

### What changed

- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Config/GameConfig.cs`** — added a new `[Header("Debug")]` section with a `StartHotelOverride` int field (default `0`). `[Min(0)]` prevents negative values; a `[Tooltip]` explains the behavior in the Inspector. Field is set on the `Resources/GameConfig.asset` ScriptableObject and therefore Inspector-editable.
- **`Assets/GorodiskiGames/PerfectHotel/Scripts/States/GameLoadLevelState.cs`** — after the existing save-load-clamp-and-save-back block, the state now checks `_config.StartHotelOverride`. If greater than 0, it replaces `_hotel` with the override (re-clamping against build settings as a safety). Crucially, the override is applied **after** `model.Save()`, so the save file is untouched — toggling the override back to 0 returns to real progress with zero lingering side effects.

### Why

The earlier approach (edit a line in `GameLoadLevelState.cs`, press Play, revert before commit) worked but had two failure modes: (1) easy to forget to revert, risking a committed hack, and (2) required a recompile every time we wanted to flip hotels. The Inspector-driven override fixes both — you flip a number in the `GameConfig.asset` Inspector and press Play, no code change, no recompile, no risk of committing a test hack.

### How to use

1. Open `Assets/GorodiskiGames/PerfectHotel/Resources/GameConfig.asset` in the Inspector.
2. Scroll to the **Debug** section at the bottom.
3. Set `Start Hotel Override` to the desired hotel scene index (`1` = Hotel1, `2` = Hotel2, etc.).
4. Press Play.
5. To return to normal play, set it back to `0`.

### Open items

- The previous day's *"Hotel 2 debug override still in place"* open item is now **resolved**. `GameLoadLevelState.cs` is back to its original form; no hardcoded override remains.

---

## 2026-04-08 — Relic placeholder prop and collector office architecture

**Summary.** Investigated the Hotel 2 vending machine system, confirmed it is implemented as a reskinned toilet, and added a placeholder cube prop so workers visibly carry a "relic" from chamber to collector office.

### What changed

- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Level/Units/UnitView.cs`** — added `AttachRelicPlaceholder()` and `DetachRelic()` methods plus a private `_carriedRelic` field. `AttachRelicPlaceholder` instantiates a primitive cube at runtime, strips its collider, parents it to the unit transform at local position `(0, 1.25, 0.35)` with scale `0.3`, and activates animation layer 1 so the existing carry upper-body pose blends in. `DetachRelic` destroys the cube and resets the layer. Idempotent.
- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Level/Units/States/UnitInRoomState.cs`** — after `RollHasRelic()` returns true and before routing to the collector, calls `_unit.View.AttachRelicPlaceholder()`.
- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Level/Units/States/UnitInToiletCabineState.cs`** — `Initialize()` now calls `DetachRelic()` at the top. The cube disappears the moment the worker enters the cabine, representing the handoff to the collector.
- **`Assets/GorodiskiGames/PerfectHotel/Scripts/Level/Units/States/UnitWalkToRemoveState.cs`** — safety net: also calls `DetachRelic()` in `Initialize()` so workers who fail to reach a collector (full office, no office in area) don't walk off-screen holding the cube.

### Key architectural finding: there is no separate "collector" entity

What looks like a vending machine in Hotel 2 is mechanically a `ToiletView` with a vending machine mesh on top. Verified by inspecting `SodaBaseHotel2.prefab` — its root component script GUID matches `ToiletView.cs.meta`, not `SodaView.cs.meta`. The `SodaView` class exists in `Assets/.../Level/Entity/Soda/SodaView.cs` but is **orphaned**: no controller, no module, no references anywhere in the code. The `Soda/` folder and `EntityType.Soda` enum value are dead code from an abandoned refactor attempt.

Because the vending machine IS a toilet, the entire worker flow already exists:
1. `UnitInRoomState.LeaveChamber()` calls `_gameManager.RollHasRelic()`.
2. If true, it calls `_gameManager.FindToilet(_unit.Area)` and routes the worker to an available cabine, or to a queue slot on `toilet.Line`, or to despawn if both are full.
3. The loader NPC keeps the cabines stocked with `InventoryType.SodaCan` (not ToiletPaper) via `UtilityModuleView.InventoryMap`.
4. The in-chamber comment on `UnitInRoomState.LeaveChamber` explicitly refers to "log at the collector office" — the intent was always there, just hidden under toilet naming.

### Economy — cash trickles during excavation, not at the collector

The comment on line 100 of `UnitInRoomState.cs` confirms: *"Cash was already trickled during the dig (RoomOccupiedState), so this is purely about whether the worker carries a physical artifact out to log at the collector office."* The collector visit is **theatrical**, not transactional — it's a visual payoff, not an economic one.

**Decision (2026-04-08):** Keep this model for now. Idle players spend a lot of time not watching the screen, and cash that only exists on successful collector delivery would punish AFK play. If we ever want a "bonus on delivery" tier (e.g., rare relics that pay extra when logged), that would layer on top of the trickle, not replace it.

### Design conversation: intended collector office workflow

Fabian confirmed the target workflow matches the current implementation:

1. Worker finishes excavating; if they found a relic, walk to collector office.
2. Office has 3 collectors ("vending machines") behind a table. Each needs parchment (currently `SodaCan`) to accept a relic.
3. Workers queue waiting for a collector to be free and stocked.
4. Parchment delivered by loader or player (already works).

Every step is already implemented. The only visible gap was the worker not carrying a visible prop — fixed above with the cube.

### Open items

- **Hotel 2 debug override still in place.** `GameLoadLevelState.cs` lines ~45–49 contain `_hotel = 2;` hardcoded to force-load Hotel 2 for testing. **Must be reverted before shipping or normal play.**
- **Orphaned `Soda/` folder and `EntityType.Soda` enum value.** Dead code. Fabian OK'd deletion in principle; not yet done.
- **Rename pass `Toilet` → `Collector`.** Three layers of naming mismatch exist: `ToiletController`/`ToiletModule`/etc. in code, vending machine art, "collector office" in the design. Deferred until gameplay is stable; doing it mid-feature would double the debugging surface.
- **Relic prop is a primitive cube.** Placeholder. Needs a proper relic mesh + a hand-bone anchor on the worker rig once art is ready. Current position `(0, 1.25, 0.35)` is a guess and may need tweaking per worker model scale.
- **Workers don't carry different relic types.** All relics look the same. If/when we build the artifact rarity system (Phase 3 of the original plan), the prop would ideally vary by tier.

### Gotchas and lessons learned

- **Open the `.prefab` files directly when investigating what a scene entity actually is.** Folder names and class names lie; prefab `m_Script` GUIDs don't. Grepping for references to a class file name tells you nothing if the prefab uses a *different* class with the same conceptual role.
- **`UnitView` already has a carry-animation layer system** (`SetLayerWeight`, `AnimationType.Carry`) used by staff units via `UnitStaffView.InventoryHolder`. Regular guest/worker units don't have the holder transform, which is why the relic placeholder is parented to the unit transform directly with a hardcoded offset rather than a rigged bone.
- **`UnitStaffView : UnitView`** has an `_inventoryHolder` Transform for properly anchored staff-carried items. If we later upgrade workers to use a rigged carry point, mirror this pattern on `UnitView` (or introduce a `UnitGuestView` subclass).
