# 3D Pipeline Cookbook - Lost Chambers: Idle Relics

> Lessons learned creating characters and assets via Blender MCP + AI generation.  
> Living document - update after each modeling session.

---

## 0. CRITICAL RULES — Read first (lessons from PlayerArchaeologist)

The PlayerArchaeologist character took **5+ rounds of in-Blender repair** to ship, after Cowork delivered an FBX that was nominally "done." Every round of pain traced back to a small number of root causes — all of which are preventable. **For every future player or NPC rig, the rules in this section are non-negotiable.**

### 0.1 — The single most important rule

**Author the mesh AROUND the target rig, not in isolation.** Open `PlayerA.fbx` in Blender FIRST, append its armature, and use that as the constraint set the entire mesh is built against. Don't sculpt a "natural human" in some other scale and expect to retrofit it later. The retrofit is what cost us 5 rounds of cleanup.

The target constraints are non-negotiable and must be respected from the very first vertex placed:

| Constraint | Value | Why |
|---|---|---|
| **Total height** | ~2.1 m (matches PlayerA Y-extent) | Skeleton bones are positioned for this exact height |
| **Width** | ~1.24 m (matches PlayerA X-extent) | Hand bones are at `\|x\|=0.49` — your mesh's hands must reach there |
| **Bind pose** | A-pose, arms hanging at sides | What `UnitAC.controller` clips were authored against |
| **Bone count** | exactly 22 deformation bones | The prefab `SkinnedMeshRenderer.bones[]` is 22 entries |
| **Bone names** | exact PlayerA names | Same `Hips, LeftUpLeg, ... Head` (see Section 4) |
| **Bone bind orientations** | LEFT chain points UP, RIGHT chain points DOWN | Intentional asymmetry — DO NOT "fix" this |
| **AimNode** | Hierarchy helper only — NOT a deformation bone | Including it = vertices reference out-of-range bones |
| **Texture** | Sample colors from shared `AtlasTexture.png` (1024×1024) | Avoids per-character material override |
| **Mesh symmetry** | Perfect mirror across X=0 plane | Auto-weights cannot symmetrize an asymmetric mesh |
| **Topology** | Single welded mesh, no separate islands, no hidden inner body | Hidden geometry causes weight-bleed stretch artifacts |

### 0.2 — The second most important rule

**Test in Unity against `Player.prefab` BEFORE saying "done."** Blender pose mode is insufficient. The first real test is:

1. Export the FBX into the project at the canonical path.
2. Force the character to spawn in-game (temporary `player = N` override in `GamePlayState.cs`).
3. Watch idle, walk, and the carry layer.
4. Inspect from front, back, and 3/4 angles.

If those four checks pass, it's done. Not before.

### 0.3 — Pre-flight checklist (run before declaring an FBX ready)

Run this in Blender immediately before the final export. If any line fails, fix it; do not export.

```python
# Save in a Blender Text Editor block as "preflight_check.py" and run it
import bpy, bmesh
arch = bpy.context.scene.objects.get("PlayerArchaeologist")  # or whatever your mesh is named
arm = bpy.context.scene.objects.get("Armature")
mesh = arch.data
ok = True

# 1. Bone count
n_bones = len(arm.data.bones)
if n_bones != 22:
    print(f"FAIL: bone count is {n_bones}, expected 22")
    ok = False

# 2. Bone names match PlayerA exactly
expected = {"Hips","Spine","Spine1","Spine2","Neck","Head",
            "LeftShoulder","LeftArm","LeftForeArm","LeftHand",
            "RightShoulder","RightArm","RightForeArm","RightHand",
            "LeftUpLeg","LeftLeg","LeftFoot","LeftToeBase",
            "RightUpLeg","RightLeg","RightFoot","RightToeBase"}
actual = {b.name for b in arm.data.bones}
if actual != expected:
    print(f"FAIL: bone names mismatch. Missing: {expected - actual}. Extra: {actual - expected}")
    ok = False

# 3. Mesh dimensions roughly match PlayerA (2.1m tall ± 5%)
h = arch.dimensions[2] if arch.dimensions[2] > arch.dimensions[1] else arch.dimensions[1]
if not (2.0 < h < 2.2):
    print(f"FAIL: height is {h:.2f}m, expected ~2.1m")
    ok = False

# 4. Single mesh island, no boundary edges
bm = bmesh.new(); bm.from_mesh(mesh)
boundary = sum(1 for e in bm.edges if e.is_boundary)
if boundary > 0:
    print(f"FAIL: {boundary} boundary edges (mesh has holes)")
    ok = False
visited = set(); islands = 0
for v in bm.verts:
    if v.index in visited: continue
    islands += 1
    stack = [v]
    while stack:
        w = stack.pop()
        if w.index in visited: continue
        visited.add(w.index)
        for e in w.link_edges:
            o = e.other_vert(w)
            if o.index not in visited: stack.append(o)
if islands > 1:
    print(f"FAIL: {islands} mesh islands, expected 1")
    ok = False
bm.free()

# 5. Mesh symmetry (every vert has a mirror within 1mm)
from mathutils.kdtree import KDTree
n = len(mesh.vertices)
kdt = KDTree(n)
for i, v in enumerate(mesh.vertices): kdt.insert(v.co, i)
kdt.balance()
max_d = 0
for v in mesh.vertices:
    _, _, d = kdt.find((-v.co.x, v.co.y, v.co.z))
    if d > max_d: max_d = d
if max_d > 0.001:
    print(f"FAIL: mesh asymmetric, max mirror distance {max_d*1000:.1f}mm (expected <1mm)")
    ok = False

# 6. No unweighted vertices
unw = sum(1 for v in mesh.vertices if sum(g.weight for g in v.groups if g.weight > 0) < 0.001)
if unw > 0:
    print(f"FAIL: {unw} unweighted vertices")
    ok = False

# 7. No skin-UV faces in non-face/non-hand zones (hidden inner body check)
uv_layer = mesh.uv_layers.active
SKIN_UV = 0.056  # if using shared AtlasTexture, replace with the right pixel
hidden_skin = 0
for poly in mesh.polygons:
    avg = sum(uv_layer.uv[li].vector.x for li in poly.loop_indices) / len(poly.loop_indices)
    if abs(avg - SKIN_UV) < 0.02:
        z, x = poly.center.z, abs(poly.center.x)
        is_visible = (z >= 1.20 or 1.05 <= z < 1.20 or (x >= 0.42 and 0.30 <= z < 1.0))
        if not is_visible: hidden_skin += 1
if hidden_skin > 0:
    print(f"FAIL: {hidden_skin} skin-UV faces in hidden body areas — delete the inner body shell")
    ok = False

print("PREFLIGHT PASS" if ok else "PREFLIGHT FAIL — fix issues above before exporting")
```

### 0.4 — What NOT to do

| Don't | Why it bites |
|---|---|
| Don't sculpt the mesh first and then "fit it to the rig" | Causes scale/proportion mismatch. We had to scale up 38% to fit PlayerA's bones. |
| Don't include `AimNode` as a deformation bone | It's a non-deforming helper. Its presence causes 2068 verts to reference out-of-range bones in Unity. |
| Don't try to "fix" the asymmetric bind pose | LEFT bones point UP, RIGHT point DOWN — this is intentional and `UnitAC.controller` clips depend on it. Mirroring breaks the rig. |
| Don't sculpt without a Mirror modifier | Manual sculpting drifts off-symmetric. Auto-weights cannot recover symmetry. |
| Don't model an inner skin body under the clothing | The hidden inner shell causes weight-bleed artifacts (skin color appears during animation). Outer surface only. |
| Don't create a new pixel-art atlas | The project uses a single shared `AtlasTexture.png` that all characters sample from. Per-character atlases require code changes. |
| Don't leave seam verts un-merged | Causes the mesh to appear as 4+ islands with visible boundary holes. Always run Merge by Distance after combining body parts. |
| Don't use Blender's `bpy.ops.armature.symmetrize` to mirror weights | It copies, doesn't average. Use mirror-pair averaging (see Section 4.5) for true symmetric weights. |
| Don't trust auto-weights for face/chest verts | Bone Heat is geometric: it gives chest verts to LeftHand bones because the bone is closer than Spine2. Always apply rigid-region cleanup. |
| Don't say "done" until you've tested in Unity Play mode | Blender pose mode cannot detect bone-count mismatches, animation clip incompatibilities, or hidden-geometry artifacts. |

### 0.5 — When you screw up anyway

Save a `_pre_*.blend` checkpoint before EVERY destructive edit. Naming: `<basename>_pre_<operation>.blend`. We salvaged ~3 of 5 rounds of the archaeologist work via these checkpoints. Project rule already documented in CLAUDE.md.

---

## 1. AI Model Generation (Hunyuan3D)

### What works
- **Transparent PNG backgrounds are critical.** Hunyuan3D interprets JPEG backgrounds as geometry and produces flat slabs. Always export the reference image as `.png` with transparency before feeding it to the generator.
- **Single front-facing reference** works well enough for chibi characters. Multi-view wasn't necessary.
- The generated mesh comes out at ~563K faces - way too heavy. Plan for aggressive decimation down to 5-7K faces for mobile.

### What doesn't work
- JPEG or any image with a solid background = garbage output (flat plane with texture on top).
- ~~Hyper3D text-to-3D: expensive, requires separate Blender panel activation (`create_rodin_job`), and wasn't needed once Hunyuan worked.~~ Superseded — see Section 1b.

---

## 1b. AI Model Generation (Hyper3D Rodin) — alternate path with baked textures

### When to use this path vs Hunyuan3D
- **Hunyuan3D (Section 1)**: produces **untextured** mesh, needs a hand-built 9-pixel atlas + per-zone UV assignment. Right for stylized flat-color characters (Archaeologist, Adventurer).
- **Hyper3D Rodin (this section)**: produces **textured** mesh with full UV unwrap and a 512×512 baked texture. Right for photo-textured characters where the reference has color richness you want to preserve (Adventuress and future Rodin characters).

### Key gotcha: use the Rodin Blender plugin DIRECTLY, not the MCP tool
- `mcp__blender__generate_hyper3d_model_via_images` consistently fails with `Error: Input buffer contains unsupported image format` (Sharp library, server-side). Tried original PNG, Blender re-saved PNG, hand-written stdlib PNG, square 1024×1024, portrait 1024×1536 — all rejected. The BlenderMCP addon's transmission of the image bytes is somehow not what Sharp expects on the server side. **Workaround: install the official Rodin Blender plugin (Hyper3D's own addon) and run the generation from inside Blender's UI.** Then proceed with the rest of this section in MCP/Python as usual.
- `get_hyper3d_status` via MCP is still useful for confirming subscription mode (MAIN_SITE vs FAL_AI).

### Texture handling
- Rodin's output texture is **packed** into the .blend AND lives in a temp folder (`%LOCALAPPDATA%\Temp\<uuid>_base_basic_shaded\<hash>_shaded.png`). Save it to a permanent path next to the .blend before doing any pipeline work, so it survives temp-folder cleanup:
  ```python
  img.file_format = 'PNG'
  img.filepath_raw = "C:/.../PlayerCharacter_textures/PlayerCharacterTexture.png"
  img.save()
  ```
- **Texture import settings are different from atlas characters.** Use `maxTextureSize: 512`, `enableMipMap: 1`, `filterMode: 1` (Bilinear), `textureCompression: 1`. The atlas-character settings (32px, point filter, no mipmaps) would destroy the photo-textured look.
- **No 9-pixel atlas.** Skip Section 3 entirely. The Rodin texture replaces it.
- **Keep Rodin's UV map** (`'st'` is the layer name). Do not re-UV.

### Decimation gotchas (Rodin output)
- Rodin output is ~17K faces (lighter than Hunyuan's 563K). Decimate Collapse modifier may need 2 passes — the modifier hits topology limits and produces ~2x the requested face count on the first try. Run again at `target / current_faces` ratio to converge.
- Decimate Collapse **breaks mesh symmetry** even when the input is perfectly symmetric. After decimating a symmetric body, re-run `mesh.symmetrize(POSITIVE_X)` to restore symmetry.

### Asymmetric hair (or other intentionally-asymmetric features)
The pipeline's "Perfect mirror across X=0" rule applies to the body — auto-weights and mirror-pair averaging require it. Hair (or any feature locked to a single rigid bone like Head) can be asymmetric. Process:

1. **Classify hair vs body via per-vertex texture-color sampling.** For each vertex, average the UVs across all faces touching it, sample the texture, classify HAIR if HSV-hue ∈ [30°,60°] AND saturation ≥ 0.30 AND value ≥ 0.55 AND Z ≥ 1.45 (the Z filter prevents jacket-buckle / belt-trim false positives below the head). Verify by setting vertex colors and viewport-screenshotting before you destructively separate.
2. **Separate hair into a child object** via `bpy.ops.mesh.separate(type='SELECTED')`.
3. **Symmetrize the body only.**
4. **Decimate body and hair to mobile budget separately.** Re-symmetrize body after decimating (Collapse breaks symmetry).
5. **Re-join hair into body.** Apply progressive merge-by-distance in the head zone (Z≥1.40), starting at 5mm and working up to 30mm. **Do not exceed 30mm** — at 50mm, bangs verts get dragged down onto the eye area, displacing hair-color UVs onto skin texture (visible "tear streaks"). 30mm leaves ~80 interior boundary edges; accepted as ship-acceptable for the hair-shell hybrid (the typical idle-game camera angle doesn't expose them).
6. **Preflight: relax mesh-symmetry assertion to body-only (Z<1.30).** Hair zone is intentionally asymmetric and locked 100% to the Head bone (single rigid bone, no left/right pair to mirror against, so weights don't need to be symmetric there).

### Reference: PlayerAdventuress (full pipeline run)
See DEVLOG `2026-04-26` entry for the complete iteration log including all gotchas and per-step face counts.

---

## 2. Mesh Cleanup & Decimation

### Target specs (mobile idle game)
- **Faces:** 5,000 - 8,000
- **Vertices:** ~3,500 - 4,000
- **Single mesh** (all-in-one, no separate objects)

### Decimation workflow
1. Start with AI-generated mesh (~563K faces)
2. Decimate ratio ~0.009 to reach ~5K faces
3. **Add geometry back** at material transition zones (wrists, neckline, belt line) using targeted subdivision
4. Final count after subdivision: ~7.3K faces

### Key lesson
Low-poly meshes don't have enough faces at boundary zones (where two materials meet) for clean color transitions. You *must* subdivide selectively at those spots — trying to assign materials without enough geometry leads to endless back-and-forth.

---

## 3. Material Assignment Strategy

### Materials (9 total for archaeologist character)
| Index | Material   | Color                    |
|-------|-----------|--------------------------|
| 0     | Skin      | Peach (0.87, 0.65, 0.48) |
| 1     | Hair      | Dark brown (0.15, 0.08, 0.03) |
| 2     | Hat       | Brown (0.35, 0.2, 0.1)   |
| 3     | Hatband   | Olive green (0.25, 0.3, 0.12) |
| 4     | Jacket    | Brown (0.36, 0.2, 0.08)  |
| 5     | Shirt     | White (0.9, 0.88, 0.85)  |
| 6     | Belt      | Dark brown (0.2, 0.12, 0.06) |
| 7     | Pants     | Brown (0.3, 0.18, 0.08)  |
| 8     | Boots     | Dark (0.12, 0.07, 0.04)  |

### Assignment approach (what finally worked)
The trick is using **three signals together**, not just height:

1. **Normalized Z height (nz)** — coarse body zone (boots < 0.18, pants < 0.33, torso 0.33-0.60, head > 0.72)
2. **Face normal direction** — front vs back (`normal.y < -0.3` = front-facing). Essential for shirt (front only) and hair vs skin on the head.
3. **Bone vertex group weights** — which bone owns a face tells you arm vs torso vs leg.

### "Reset then carve" pattern
Don't try to be precise on the first pass. Instead:
1. Set an entire zone to the **dominant material** (e.g., all arm faces = jacket)
2. Then **carve out exceptions** with tighter thresholds (e.g., only nz < 0.35 in arm zone = skin for hand tips)

This avoids the back-and-forth of "oops, now there's skin on the jacket / jacket on the hand."

### Specific thresholds that worked (PlayerArchaeologist)
- **Wrist/hand boundary:** `nz < 0.35` = skin, above = jacket (for faces with `abs(x) > 0.10`)
- **Shirt front:** requires `normal.y < -0.3` (front-facing) AND torso height zone
- **Hair vs skin on head:** front-facing faces at head height = skin (face), others = hair
- **Hatband:** narrow Z band on the hat, slightly above hat brim

---

## 4. Rigging

### Armature (22 bones, matching PlayerA exactly)

**Do not invent your own armature.** Append PlayerA's armature into Blender (`File → Append → PlayerA.fbx → Object → Armature`) and use that. The 22-bone hierarchy:

```
Hips
  Spine → Spine1 → Spine2
    Neck → Head
    LeftShoulder  → LeftArm  → LeftForeArm  → LeftHand
    RightShoulder → RightArm → RightForeArm → RightHand
  LeftUpLeg  → LeftLeg  → LeftFoot  → LeftToeBase
  RightUpLeg → RightLeg → RightFoot → RightToeBase
```

`AimNode` exists in `Player.prefab`'s GameObject hierarchy as a non-deforming helper between `Hips` and `Spine`. **It is NOT in the prefab's `SkinnedMeshRenderer.bones[]` array.** When you append PlayerA's armature, AimNode comes along for the ride — **delete it before parenting**, or its presence as a deformation bone will produce out-of-range bone references in Unity.

### Bone bind pose is intentionally asymmetric

PlayerA's LEFT arm chain bones have tails pointing UP (head→tail goes vertical-up); the RIGHT arm chain points DOWN. Bone rolls are also asymmetric: LEFT=`0/-1.39/-1.39/0` vs RIGHT=`180/24.77/24.77/-180`. **This is intentional and `UnitAC.controller` clips depend on it.** Do not mirror or "fix" this asymmetry. Verified PlayerA bone values are at the bottom of this section (4.6).

### Auto-weights with rigid-region cleanup (NOT just `ARMATURE_AUTO`)

`bpy.ops.object.parent_set(type='ARMATURE_AUTO')` is the starting point but it is **NOT sufficient** for chibi humanoids. Bone Heat is a geometric algorithm — it assigns each vertex to the bone whose medial axis is closest. For an A-pose character:

- The chest vertex at `(0.30, 0, 0.95)` is geometrically closer to the `LeftHand` bone (at `0.49, 0.86`) than to `Spine2` (at `0, 0.92`). So the algorithm gives chest verts a primary `LeftHand` weight. **Wrong.**
- The face vertex near the shoulder gets 25% weight on `LeftShoulder`. So the face deforms when the shoulders rotate during walk. **Wrong.**
- The hand vertex at the inner wrist gets weight on `LeftUpLeg`. So the hand swings with the leg. **Wrong.**

**You must layer rigid-region cleanup on top.** After parenting with auto-weights, run a pass that overrides body-region weights based on position:

```python
# After parent_set(type='ARMATURE_AUTO'):
# - z >= 1.30           : 100% Head only (face/skull/hat)
# - 1.10 <= z < 1.30, |x|<0.20 : Head/Neck only
# - 0.95 <= z < 1.10, |x|<0.18 : Spine2 (chest center)
# - 0.80 <= z < 0.95, |x|<0.20 : Spine1 (mid torso)
# - 0.65 <= z < 0.80, |x|<0.20 : Spine (lower torso)
# - z >= 0.65 (any |x|), zero out UpLeg weights gradually 0.65→0.72
# Then renormalize.
```

The tight `|x|` filter is critical — anything broader catches the upper-arm verts and breaks the arms. See the round-3e DEVLOG entry for the exact code that worked.

### 4.5 — Symmetric weights via mirror-pair averaging

Auto-weights is non-deterministic and produces asymmetric weight distributions even on a symmetric mesh, because the bones themselves are asymmetric in bind pose. Force symmetry with mirror-pair averaging:

```python
import bpy
from mathutils.kdtree import KDTree
arch = bpy.data.objects.get("PlayerArchaeologist")  # your mesh
mesh = arch.data
n = len(mesh.vertices)
kdt = KDTree(n)
for i, v in enumerate(mesh.vertices): kdt.insert(v.co, i)
kdt.balance()
def vw(vidx):
    return {arch.vertex_groups[g.group].name: g.weight for g in mesh.vertices[vidx].groups if g.weight > 0}
def flip(name):
    if name.startswith("Left"): return "Right" + name[4:]
    if name.startswith("Right"): return "Left" + name[5:]
    return name
new_w = {}
for vidx, v in enumerate(mesh.vertices):
    co, idx, dist = kdt.find((-v.co.x, v.co.y, v.co.z))
    if dist > 0.001:
        new_w[vidx] = vw(vidx); continue
    self_w = vw(vidx)
    mirror_w_flipped = {flip(name): w for name, w in vw(idx).items()}
    all_g = set(self_w) | set(mirror_w_flipped)
    new_w[vidx] = {g: (self_w.get(g, 0) + mirror_w_flipped.get(g, 0)) / 2.0 for g in all_g}
for vg in arch.vertex_groups:
    additions = [(vidx, w[vg.name]) for vidx, w in new_w.items() if vg.name in w]
    removals = [vidx for vidx, w in new_w.items() if vg.name not in w]
    for vidx, weight in additions: vg.add([vidx], weight, 'REPLACE')
    if removals:
        try: vg.remove(removals)
        except RuntimeError: pass
# Then renormalize all in weight paint mode
```

**Don't use** `bpy.ops.object.vertex_group_mirror` — that one COPIES from one side to the other instead of averaging, which just swaps the asymmetry.

### 4.6 — PlayerA reference bone values (in armature local space, scaled to meters)

Authoritative values to compare against if your rig diverges:

```
Hips           head=(-0.000, 0.639,  0.003)  tail=(-0.000, 0.707,  0.003)  roll=  0.0
Spine          head=(-0.000, 0.749,  0.002)  tail=(-0.000, 0.833,  0.002)  roll=  0.0
Spine1         head=(-0.000, 0.833, -0.005)  tail=(-0.000, 0.918, -0.005)  roll=  0.0
Spine2         head=(-0.000, 0.917, -0.013)  tail=(-0.000, 1.124, -0.013)  roll=  0.0
Neck           head=(-0.000, 1.124, -0.036)  tail=(-0.000, 1.277, -0.036)  roll=  0.0
Head           head=(-0.000, 1.277, -0.031)  tail=(-0.000, 1.430, -0.031)  roll=  0.0
LeftShoulder   head=( 0.037, 1.117,  0.022)  tail=( 0.037, 1.269,  0.022)  roll=  0.0   ← LEFT chain points UP
LeftArm        head=( 0.171, 1.144, -0.047)  tail=( 0.169, 1.372, -0.041)  roll= -1.4
LeftForeArm    head=( 0.340, 0.995, -0.077)  tail=( 0.339, 1.218, -0.071)  roll= -1.4
LeftHand       head=( 0.490, 0.864,  0.023)  tail=( 0.490, 1.087,  0.023)  roll=  0.0
RightShoulder  head=(-0.037, 1.117,  0.022)  tail=(-0.037, 0.964,  0.022)  roll=180.0   ← RIGHT chain points DOWN
RightArm       head=(-0.171, 1.144, -0.047)  tail=(-0.172, 0.916, -0.053)  roll= 24.8
RightForeArm   head=(-0.340, 0.995, -0.077)  tail=(-0.342, 0.772, -0.083)  roll= 24.8
RightHand      head=(-0.490, 0.864,  0.023)  tail=(-0.490, 0.642,  0.023)  roll=180.0
LeftUpLeg      head=( 0.156, 0.639,  0.000)  tail=( 0.156, 0.394,  0.000)  roll=  ~
LeftLeg        ... (same chain pattern)
LeftFoot       ...
LeftToeBase    ...
RightUpLeg/Leg/Foot/ToeBase: mirror of left
```

(Bone Y axis = mesh Z (height) after the 90° X-axis rotation Blender's FBX import applies.)

---

## 5. Animation

### Current animations
- **Idle:** Subtle breathing motion (spine/chest Y-scale oscillation + slight arm sway), 60 frames
- **Walk:** Alternating arm/leg swing with spine bounce, 30 frames

### Gotchas
- Blender 4.x/5.x changed the Action API — `action.fcurves` doesn't exist directly on Action objects anymore. Skip fcurve smoothing or use the newer API.
- Clean up duplicate actions (Idle.001, Walk.001 etc.) before export — they accumulate fast during iteration.

---

## 6. FBX Export Settings (for Unity URP)

These are the exact settings that produce a working FBX. **Three of them are non-obvious and silently ruin the export if wrong** — see notes below.

```python
bpy.ops.export_scene.fbx(
    filepath=...,
    use_selection=True,
    object_types={'ARMATURE', 'MESH'},
    global_scale=1.0,
    apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_ALL',   # ← REQUIRED. Wrong value = 100x undersize.
    use_space_transform=True,
    bake_space_transform=True,             # ← REQUIRED. Wrong value = character on its back.
    use_mesh_modifiers=True,
    mesh_smooth_type='OFF',
    add_leaf_bones=False,                  # ← REQUIRED. Wrong value = extra bones break SMR.
    primary_bone_axis='Y',
    secondary_bone_axis='X',
    armature_nodetype='NULL',
    bake_anim=False,                       # animation comes from UnitAC.controller, not embedded
    path_mode='AUTO',
    axis_forward='-Z',
    axis_up='Y',
)
```

### The three silent killers (each cost a round of iteration on PlayerArchaeologist)

1. **`apply_scale_options='FBX_SCALE_ALL'`** — without this, the FBX writes scale=0.01 in the file header, and Unity imports the mesh at 1/100 size (a 1cm-tall character). The other valid value is `FBX_SCALE_NONE`, which produces the same bug. Always `FBX_SCALE_ALL`.
2. **`bake_space_transform=True`** — without this, Blender writes the mesh in Z-up coordinates and Unity reads it in Y-up, so the character is **lying on its back** in-game. With this, Blender bakes the axis conversion into vertex coordinates at export.
3. **`add_leaf_bones=False`** — Blender's default is `True`, which adds an extra "leaf" bone at the tail of every chain. Those phantom bones get into the SMR's `bones[]` array and shift indices, breaking compatibility with the prefab.

### Verification of the exported FBX in Unity

After import, the mesh should report (via diagnostic probe or direct inspection):

```
bindposes = 22
subMeshCount = 1
max bone index used = 21
vertices touching bone >= 22 = 0
SMR bones[] length = 22
vertex range x ≈ [-0.62, 0.62]   y ≈ [0.00, 2.10]   z ≈ [-0.31, 0.34]
```

If `bindposes != 22` or `max bone index >= 22`, you have AimNode or a leaf bone in the deformation list — fix the source mesh and re-export. If the vertex range is much smaller or rotated, your export settings are wrong.

### Unity import settings (`<Mesh>.fbx.meta`)

These should be set on import (or copied from PlayerA.fbx.meta):

```yaml
animationType: 2          # Generic (NOT Humanoid)
useFileScale: 1
globalScale: 1
optimizeBones: 1
optimizeGameObjects: 0
materialImportMode: 0     # None — we override the material in PlayerConfig.BodyMaterial
```

### Unity import location
`Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Models/Units/`  
Existing characters: PlayerA.fbx, PlayerB.fbx, PlayerBomber.fbx, PlayerLora.fbx, **PlayerArchaeologist.fbx**

---

## 7. Blender MCP Gotchas

- **ALWAYS BACKUP FIRST:** Before ANY mesh, weight, or bone modification via MCP, create a backup copy: `bpy.ops.wm.save_as_mainfile(filepath=original_path.replace('.blend', '_pre_edit_backup.blend'), copy=True)`. Lost hours of face-by-face coloring work when multiple saves overwrote the only copy (2026-04-11 incident).
- **Module imports:** It's `import mathutils` or `from mathutils import Vector`, NOT `bpy.mathutils`.
- **Enum values:** `ORIGIN_CURSOR` not `ORIGIN_3D_CURSOR`.
- **File paths:** Blender runs on Windows. Use `os.path.expanduser("~")` to get the correct home directory, not hardcoded Linux paths.
- **Viewport screenshots** via MCP are the fastest way to verify material changes — use them liberally.
- **Step-by-step execution:** Break Blender Python scripts into small chunks. Large monolithic scripts fail silently or produce confusing errors.
- **`img.pixels = list(...)` and `img.pixels.foreach_set(...)` silently produce all-zero textures** in some Blender versions (hit on 2026-04-25 with PlayerAdventurer). The image looks correct in the data block (`source: GENERATED`, right size) but every pixel is `(0,0,0,1)` after the assignment. Workaround: write the PNG bytes directly using stdlib `struct + zlib` (a 105-byte minimal RGBA PNG works fine) and reload via `bpy.data.images.load(path)`. Always verify pixel values via `list(img.pixels[:36])` immediately after creating an atlas.
- **`bpy.ops.wm.read_homefile()` can disconnect the MCP server.** When you need to reset the scene, manually delete all objects via `bpy.data.objects.remove()` instead. If you must reload, ask the user to reconnect the BlenderMCP addon afterward.
- **Mesh and PlayerA armature can land in different axis orientations** (Z-up vs Y-up) depending on import path. Hunyuan3D output is Z-up; PlayerA's FBX armature comes in Y-up. Symptom: ARMATURE_AUTO fails with "Bone Heat Weighting: failed to find solution for one or more bones" with all verts unweighted. Fix: rotate the armature 90° around X and apply, before parenting. **Also**: applying a uniform scale to an armature object can collapse the internal cm→m unit baking; if Hips bone shows up at y=63 instead of y=0.63 after the rotation, the armature also needs a 0.01x rescale-and-apply.
- **`bpy.ops.object.transform_apply(rotation=True)` can silently fail** in Blender 5.x even though the operator returns successfully. Symptom: after setting `gen.rotation_euler = (math.pi/2, 0, 0)` and calling `transform_apply(rotation=True)`, `gen.dimensions` still reports un-rotated values *and* the actual vertex coordinates haven't moved. Workaround: skip the operator entirely and apply the rotation directly to vertex data via `mesh.transform(mathutils.Matrix.Rotation(angle, 4, 'X'))`. Same approach works for translate (`Matrix.Translation`) and uniform scale (`Matrix.Scale`). The direct `mesh.transform` path worked first try when the operator silently no-op'd. Hit on 2026-04-25 during the SarcophagusA pipeline.
- **Hunyuan3D "No data received" error often means the model was created anyway.** Confirmed across PlayerAdventurer, PlayerAdventuress, and SarcophagusA runs (2026-04-25): the API returned this error on every call but `get_scene_info` immediately afterward showed `geometry_0` had been imported. **Never auto-retry the generation tool on this error** — always check the scene first; otherwise you get duplicate `geometry_0` and `geometry_0.001` imports. When working with the user, ask them to verify the model arrived in Blender before retrying.

---

## Asset Log

| Asset | File | Faces | Bones | Animations | Status |
|-------|------|-------|-------|-----------|--------|
| PlayerArchaeologist (original Cowork rig) | `Desktop/idle Relic/PlayerArchaeologist.blend` (now rewritten) | ~7,320 | 20 | Idle, Walk | Superseded by the round-5 cleanup below |
| **PlayerArchaeologist (final, shipped)** | `ResourcesStatic/Models/Units/PlayerArchaeologist.fbx` | **5,751** | **22** | None (uses UnitAC.controller) | **Done.** Symmetric mesh, single welded island, 0 boundary edges, 22-bone skeleton matching PlayerA exactly, hidden inner skin body removed, per-character material override (`PlayerArchaeologistMaterial.mat` + `PlayerArchaeologistAtlas.png`). Took 5 rounds of iteration; full saga in `Docs/DEVLOG.md`. One known cosmetic artifact (jacket-colored patch visible only from back-of-character below-the-hand camera angle, accepted ship-acceptable). |
| **PlayerAdventurer (Player6, shipped)** | `ResourcesStatic/Models/Units/PlayerAdventurer.fbx` | **5,876** | **22** | None (uses UnitAC.controller) | **Done (pending Unity verification).** Indiana-Jones-style male character. 3054 verts, perfect X=0 symmetry (max mirror dist 0mm), 0 boundary edges, 1 mesh island, 0 unweighted verts, bind pose asymmetry preserved. Per-character material (`PlayerAdventurerMaterial.mat` + `PlayerAdventurerAtlas.png`). Pipeline ran cleaner than Archaeologist — only 1 round of iteration, no hidden-body cleanup needed. Source: `Desktop/IdleRelic/PlayerAdventurer_*.blend`. Setup of Unity-side PlayerConfig + GameConfig registration via `Tools > Setup PlayerAdventurer` editor menu (one-shot). |
| **PlayerAdventuress (Player7, shipped)** | `ResourcesStatic/Models/Units/PlayerAdventuress.fbx` | **3,902** | **22** | None (uses UnitAC.controller) | **Done (pending Unity verification).** Female adventurer — first character via the **Hyper3D Rodin** path (not Hunyuan3D). 1958 verts, **80 boundary edges accepted** (interior holes from the symmetric-body + asymmetric-hair-shell hybrid). Body region (Z<1.30) perfectly mirror-symmetric; **hair intentionally asymmetric** — high side-swept ponytail. Uses Rodin's baked **512×512 photo texture** (not a 9-pixel atlas) — different material/import settings (`PlayerAdventuressMaterial.mat` + `PlayerAdventuressTexture.png`, mipmaps on, bilinear filter, max 512). Key new pipeline pieces: Rodin Blender plugin used directly (MCP path failed), per-vertex texture-color sampling to classify hair vs body, separate-then-symmetrize-then-rejoin sequence for asymmetric hair. Source: `Desktop/IdleRelic/PlayerAdventuress_*.blend`. Setup of Unity-side PlayerConfig + GameConfig registration via `Tools > Setup PlayerAdventuress` editor menu (one-shot). |
| **SarcophagusA (static, shipped)** | `ResourcesStatic/Models/SarcophagusA.fbx` | **6,902** | n/a (static) | None | **Done (pending Unity verification).** First static asset via the pipeline. Egyptian pharaoh sarcophagus, authored standing 1.2m tall on Z axis. 3587 verts, X=0 symmetric, 0 boundary edges, 1 island. 4-pixel atlas (Gold/DarkBlue/LightBlue/Outline) instead of the 9-pixel character pattern. Output is a Prefab (`ResourcesStatic/Prefabs/SarcophagusA.prefab`) created via `Tools > Setup SarcophagusA` editor menu. Designer rotates -90° X in scene to lay it flat. Material: `SarcophagusAMaterial.mat` + `SarcophagusAAtlas.png`. |
