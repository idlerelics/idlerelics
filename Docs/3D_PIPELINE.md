# 3D Pipeline Cookbook - Lost Chambers: Idle Relics

> Lessons learned creating characters and assets via Blender MCP + AI generation.  
> Living document - update after each modeling session.

---

## 1. AI Model Generation (Hunyuan3D)

### What works
- **Transparent PNG backgrounds are critical.** Hunyuan3D interprets JPEG backgrounds as geometry and produces flat slabs. Always export the reference image as `.png` with transparency before feeding it to the generator.
- **Single front-facing reference** works well enough for chibi characters. Multi-view wasn't necessary.
- The generated mesh comes out at ~563K faces - way too heavy. Plan for aggressive decimation down to 5-7K faces for mobile.

### What doesn't work
- JPEG or any image with a solid background = garbage output (flat plane with texture on top).
- Hyper3D text-to-3D: expensive, requires separate Blender panel activation (`create_rodin_job`), and wasn't needed once Hunyuan worked.

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

### Armature (20 bones)
Standard humanoid rig: Hips > Spine > Chest > Neck > Head, plus shoulder/arm/hand chains and hip/leg/foot chains on each side.

### Auto-weights
`ARMATURE_AUTO` (Blender automatic weights) works fine for chibi proportions. No manual weight painting needed.

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

```
Forward:           -Z
Up:                Y
Scale:             FBX_SCALE_ALL
Object types:      ARMATURE, MESH
Deform only:       True
Leaf bones:        False (no leaf bones)
Bake animation:    True
All actions:       True
NLA strips:        False
Simplify factor:   0.0
Path mode:         COPY
Embed textures:    False
```

### Unity import location
`Assets/GorodiskiGames/PerfectHotel/ResourcesStatic/Models/Units/`  
Existing characters: PlayerA.fbx, PlayerB.fbx, PlayerBomber.fbx, PlayerLora.fbx

---

## 7. Blender MCP Gotchas

- **ALWAYS BACKUP FIRST:** Before ANY mesh, weight, or bone modification via MCP, create a backup copy: `bpy.ops.wm.save_as_mainfile(filepath=original_path.replace('.blend', '_pre_edit_backup.blend'), copy=True)`. Lost hours of face-by-face coloring work when multiple saves overwrote the only copy (2026-04-11 incident).
- **Module imports:** It's `import mathutils` or `from mathutils import Vector`, NOT `bpy.mathutils`.
- **Enum values:** `ORIGIN_CURSOR` not `ORIGIN_3D_CURSOR`.
- **File paths:** Blender runs on Windows. Use `os.path.expanduser("~")` to get the correct home directory, not hardcoded Linux paths.
- **Viewport screenshots** via MCP are the fastest way to verify material changes — use them liberally.
- **Step-by-step execution:** Break Blender Python scripts into small chunks. Large monolithic scripts fail silently or produce confusing errors.

---

## Asset Log

| Asset | File | Faces | Bones | Animations | Status |
|-------|------|-------|-------|-----------|--------|
| PlayerArchaeologist | `Desktop/idle Relic/PlayerArchaeologist.blend` | 7,320 | 20 | Idle, Walk | Materials WIP (wrist boundary improved, may need final polish) |
