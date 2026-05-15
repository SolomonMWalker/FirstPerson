# Custom Brush Entity Guide: FuncGodot + TrenchBroom

A brush entity is geometry you paint directly in TrenchBroom (like a moving platform,
trigger volume, or door) that FuncGodot converts into a fully configured Godot node
— complete with collision, mesh, and your own script and properties.

---

## Concepts

| Term | What it is |
|---|---|
| **Brush entity** | A map entity made of one or more brushes. In FuncGodot terms, defined by a `FuncGodotFGDSolidClass` resource. |
| **FGD** | Forge Game Data — the file TrenchBroom reads to know what entities exist and what properties they have. |
| **FuncGodotFGDFile** | The Godot resource that builds and exports your FGD. |
| **Classname** | The string key that links a TrenchBroom entity to its FGD/Godot definition (e.g. `func_movingPlatform`). |
| **Class Properties** | Key-value pairs defined on an entity; appear as editable fields in TrenchBroom and are passed to your node script on map build. |

---

## Step 1 — Write the Godot Script

Your node script must be a `@tool` script (C# uses `[Tool]`). It needs to handle the
`_func_godot_apply_properties` callback, which FuncGodot calls once after building the
map, passing every property set in TrenchBroom.

```csharp
using Godot;

[Tool]
public partial class MyBrushEntity : StaticBody3D  // or AnimatableBody3D, Area3D, etc.
{
    // Export these so their values persist after the map is built
    [Export] public float MyFloat { get; set; }
    [Export] public Vector3 MyDirection { get; set; }
    [Export] public bool MyFlag { get; set; }

    // FuncGodot calls this once for every built entity, passing TrenchBroom properties
    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> props)
    {
        MyFloat     = (float)  props["my_float"];
        MyDirection = (Vector3) props["my_direction"];
        MyFlag      = (int)    props["my_flag"] == 1;
    }

    // Called deferred after ALL entities have had _func_godot_apply_properties called.
    // Safe to look up sibling entities here.
    public void _func_godot_build_complete() { }

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
        // runtime logic here
    }
}
```

**Key points:**
- The class **must** be partial and must inherit a `Node` type.
- Property keys in `props` must exactly match the keys in **Class Properties** (case-sensitive).
- `[Tool]` is required for FuncGodot to call the callbacks inside the editor.

### Choosing a Node Type

| Use case | Node class |
|---|---|
| Static geometry with collision | `StaticBody3D` |
| Moving geometry (e.g. platform) | `AnimatableBody3D` |
| Trigger/volume (no collision response) | `Area3D` |
| Decoration, no collision | `Node3D` |

The `node_class` set in the FGD resource must match (or inherit from) this type.

---

## Step 2 — Create the FGD Solid Class Resource

In Godot's FileSystem dock:

1. Right-click your FGD folder (e.g. `trenchbroom/fgd/solid/`)
2. **New Resource** → search for `FuncGodotFGDSolidClass` → Create

Set these fields in the Inspector:

### Entity Definition

| Field | What to set |
|---|---|
| **Classname** | Unique snake_case name, e.g. `func_my_entity`. Must match what you type in TrenchBroom. |
| **Description** | Short description shown in TrenchBroom's entity browser. |
| **Base Classes** | (Optional) Drag in `phong_base.tres` / `vertex_merge_distance_base.tres` from `addons/func_godot/fgd/` for standard build helpers. |
| **Class Properties** | Dictionary of key → default value. Keys become property names in TrenchBroom. Value **type** sets the property type (see table below). |
| **Class Property Descriptions** | Dictionary of key → String description. Shown as tooltips in TrenchBroom. |

### Class Properties Type Reference

| Godot Variant | TrenchBroom field type | Notes |
|---|---|---|
| `float` | float | |
| `int` | integer | |
| `String` | string | |
| `bool` | choices (Yes/No) | Auto-formatted; don't add description for this |
| `Vector3` | string `"X Y Z"` | |
| `Color` | color255 `"R G B"` | Alpha always 1.0 |
| `Dictionary` | choices (dropdown) | Keys = option labels, values = int/string |
| `Array` | flags (bitmask) | Each element = `[description, bit_value, default_state]` |
| `NodePath` | target_destination | For linking entities (like Quake `target`) |

**Example Class Properties dictionary:**
```
{
  "my_float":     0.0,
  "my_direction": Vector3(0, 1, 0),
  "my_flag":      false,
  "my_mode":      { "Walk": 0, "Run": 1, "Fly": 2 }
}
```

**Example Class Property Descriptions:**
```
{
  "my_float":     "Speed in Quake units per second",
  "my_direction": "Direction of movement",
  "my_flag":      "Enable special behavior",
  "my_mode":      ["Movement mode", 0]   ← for choices: [description, default_value]
}
```

### Node Generation

| Field | What to set |
|---|---|
| **Node Class** | The Godot class name string: `"StaticBody3D"`, `"AnimatableBody3D"`, `"Area3D"`, etc. |
| **Script Class** | Drag in your `.cs` script file. |
| **Node Groups** | Optional array of group strings the generated node will be added to. |

### Solid Class Settings

| Field | Notes |
|---|---|
| **Spawn Type** | `ENTITY` for most custom brush entities (spawns as its own object). Use `MERGE_WORLDSPAWN` for decorative geometry that should bake into the world mesh. |
| **Origin Type** | `BRUSH` is recommended — FuncGodot looks for an "origin" brush to set the entity's pivot. `BOUNDS_CENTER` is a safe fallback. |
| **Build Visuals** | `true` to generate a MeshInstance3D. Set `false` for invisible triggers. |
| **Collision Shape Type** | `Convex` for moving/dynamic bodies and Area3D. `Concave` for complex static shapes. `None` for purely visual entities. |
| **Collision Layer / Mask** | Set to match your project's physics layers (see CLAUDE.md for layer names). |

**Existing project example** — `func_movingPlatform` (`trenchbroom/fgd/solid/func_MovingPlarform.tres`):
```
classname:   "func_movingPlatform"
node_class:  "AnimatableBody3D"
script_class: MovingPlatform.cs
class_properties: {
  "MoveDirection": Vector3(0,0,0),
  "MoveDistance":  0.0,
  "MoveTime":      0.0
}
```

---

## Step 3 — Register the Entity in Your Master FGD

Open your master FGD file (`definitions/my_func_godot_fgd.tres`).

In the **Entity Definitions** array, add your new `FuncGodotFGDSolidClass` resource
(drag and drop it from the FileSystem dock).

> Your master FGD should have `func_godot_fgd.tres` in its **Base Fgd Files** array so all
> the default entities (worldspawn, func_geo, etc.) are included automatically.

Order matters in some editors — **BaseClass resources must come before any Solid/Point
Class that inherits from them** in the Entity Definitions array.

---

## Step 4 — Export the FGD to TrenchBroom

### Via TrenchBroomGameConfig (recommended)

Open your `TrenchBroomGameConfig` resource (e.g.
`addons/func_godot/game_config/trenchbroom/func_godot_tb_game_config.tres`).

Make sure the **Fgd File** field points to your master FGD. Then click **Export File**.

FuncGodot writes the `.cfg` and `.fgd` files to the folder set in your
**FuncGodot Local Config → TrenchBroom Game Config Folder**.

### Via FuncGodotFGDFile directly

Open your master FGD resource and click the **Export FGD** button. The `.fgd` is written
to the **Map Editor Game Config Folder** from your Local Config.

---

## Step 5 — Use the Entity in TrenchBroom

1. Open TrenchBroom. If the game config was just exported, restart TrenchBroom so it picks up the new FGD.
2. Open or create a map for your game.
3. **Create a brush** in the 3D view (drag to draw).
4. With the brush selected, right-click → **Create Brush Entity** (or press the entity button in the toolbar and type your classname).  
   Alternatively: open the **Entity Browser** tab, find your entity by classname, and drag it into the viewport.
5. Your entity's Class Properties appear in the **Entity Properties** panel on the right. Edit them as needed.

> **Origin brush:** If your entity uses `BRUSH` origin type, add a small brush textured
> with the special "origin" texture inside the entity. FuncGodot uses its bounding box
> center as the node's pivot/origin in Godot.

---

## Step 6 — Build the Map in Godot

1. In Godot, open your level scene containing the `FuncGodotMap` node.
2. Make sure **Map Settings → Entity Fgd** points to your master FGD.
3. Select the `FuncGodotMap` node and click **Build** in the Inspector.

FuncGodot will:
- Parse the `.map` file
- Find every `func_my_entity` brush entity
- Generate an `AnimatableBody3D` (or whatever `node_class` you set) with a `MeshInstance3D` and `CollisionShape3D` children
- Attach your script
- Call `_func_godot_apply_properties` with the TrenchBroom key-value pairs
- Call `_func_godot_build_complete` deferred after all entities are processed

---

## Quick Reference: Full Workflow

```
1. Write MyBrushEntity.cs   ← [Tool] + _func_godot_apply_properties
2. Create FuncGodotFGDSolidClass resource
     classname, node_class, script_class
     class_properties (keys + default values)
3. Add to master FGD entity_definitions array
4. Export FGD (via TrenchBroomGameConfig or FGD resource button)
5. Restart TrenchBroom → place + configure entity in map
6. In Godot → FuncGodotMap → Build
```

---

## Common Pitfalls

| Problem | Fix |
|---|---|
| Entity doesn't appear in TrenchBroom entity browser | Re-export FGD and restart TrenchBroom |
| Properties not applied at runtime | Script must be `[Tool]`, property keys must exactly match `class_properties` keys |
| Node spawns at world origin | Set `origin_type` to `BOUNDS_CENTER` or add an origin brush |
| No collision | `node_class` must inherit `CollisionObject3D`; `collision_shape_type` must not be `None` |
| Mesh not visible | `build_visuals` must be `true` |
| Build clears manually placed children | Put manual nodes as siblings to `FuncGodotMap`, not children of it |
| BaseClass properties missing | BaseClass `.tres` must be listed **before** the SolidClass in `entity_definitions` |
