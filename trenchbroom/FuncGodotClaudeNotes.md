# FuncGodot — Claude's Notes

Sourced from the FuncGodot Manual. Covers the full pipeline for creating a Godot object and placing it in TrenchBroom.

---

## One-Time Project Setup

These steps only need to be done once per project.

### 1. Configure the FuncGodot Local Config

Open `addons/func_godot/func_godot_local_config.tres` in the Godot inspector and set:

- **Trenchbroom Game Config Folder** — the folder inside TrenchBroom's `games/` directory for your game (e.g. `C:/TrenchBroom/games/MyGame/`)
- **Map Editor Game Path** — your Godot project root or trenchbroom subfolder (e.g. `C:/GodotProjects/FirstPerson/trenchbroom/`)

Then click **Export Func Godot Settings**.

> **Note:** Run the project at least once before exporting so that the `user://` folder is created. Otherwise the export will fail.

---

### 2. Create Your Master FGD File Resource

In Godot: right-click in the FileSystem → **New Resource** → `FuncGodotFGDFile`. Set:

- **Fgd Name** — your game name (e.g. `MyGame`)
- **Base Fgd Files** — drag `addons/func_godot/fgd/func_godot_fgd.tres` into this array

Including the default FGD gives you the built-in entities: `worldspawn`, `func_geo`, `func_detail`, `func_illusionary`, `func_detail_illusionary`.

> **Never modify `func_godot_fgd.tres` directly.** It gets overwritten on plugin updates. Always work from your own FGD file that includes it as a base.

---

### 3. Create and Export a TrenchBroom Game Config

**New Resource** → `TrenchBroomGameConfig`. Key fields:

| Field | What to set |
|-------|-------------|
| Game Name | Your game's display name in TrenchBroom |
| Fgd File | Your master FGD resource |
| Textures Root Folder | Path to textures relative to the game path |

Click **Export File**. TrenchBroom now sees your game in its game list.

---

### 4. Set TrenchBroom's Game Path

In TrenchBroom: **Preferences** → find your game → set the game path to whatever you put in **Map Editor Game Path** in the Local Config. Your textures and entities should now be visible in TrenchBroom.

---

## Per-Entity Workflow: Adding a New Placeable Object

Do this every time you want a new entity type (enemy spawn, trigger, pickup, light, etc.).

---

### Step 1 — Build Your Godot Scene / Script

Create the scene or node that should be spawned (e.g. a `CharacterBody3D` with children). To receive properties from TrenchBroom, add a `@tool` script:

```gdscript
@tool
extends CharacterBody3D

@export var func_godot_properties: Dictionary = {}

func _func_godot_apply_properties(entity_properties: Dictionary) -> void:
    # Pull values out of the dictionary and apply them to this node.
    # Called on every entity after the map builds.
    # Example:
    # health = entity_properties.get("health", 100)
    pass

func _func_godot_build_complete() -> void:
    # Deferred call — runs after ALL entities have finished _func_godot_apply_properties.
    # Safe to reference other generated nodes here.
    pass
```

> `@tool` is **required** for `_func_godot_apply_properties` and `func_godot_properties` to work during the editor build.

---

### Step 2 — Create a FuncGodotFGDPointClass Resource

**New Resource** → `FuncGodotFGDPointClass`. Fill in:

| Field | Purpose |
|-------|---------|
| **Classname** | The name TrenchBroom uses (e.g. `monster_grunt`). Must be unique. This is how FuncGodot matches map entities to Godot nodes. |
| **Description** | Tooltip shown in TrenchBroom. Optional but helpful. |
| **Scene File** | Your `.tscn` file. FuncGodot instantiates this on map build. Overrides Node Class and Script Class. |
| **Class Properties** | Dictionary of key/value pairs that appear as editable properties in TrenchBroom (e.g. `"health": 100`). The value's type determines the editor widget. |
| **Class Property Descriptions** | Matching keys with String descriptions shown as tooltips in TrenchBroom. |

**For a brush entity** (geometry-based, like a trigger volume or moving platform), use `FuncGodotFGDSolidClass` instead, and set **Spawn Type** to `ENTITY`.

#### Supported Class Property Types

| Godot Type | TrenchBroom widget |
|------------|--------------------|
| `int` | Number field |
| `float` | Number field |
| `String` | Text field |
| `bool` | Yes / No dropdown |
| `Dictionary` | Choices dropdown |
| `Array` | Bit flags checkboxes |
| `Color` | `R G B` (0–255) |
| `Vector3` | `X Y Z` string |
| `NodePath` | Target destination (entity link) |

---

### Step 3 — Add the Entity to Your Master FGD File

Open your `FuncGodotFGDFile` resource → **Entity Definitions** array → add your new `FuncGodotFGDPointClass` resource.

---

### Step 4 — Re-Export the FGD

Open your `TrenchBroomGameConfig` resource → click **Export File**.

This regenerates the `.fgd` and game config files in TrenchBroom's game folder.

---

### Step 5 — Reload Entity Definitions in TrenchBroom

In TrenchBroom: `File → Reload Entity Definitions` (shortcut: `F6`).

Your new entity now appears in the **Entity Browser** and can be dragged into the map.

---

### Step 6 — Build the Map in Godot

In your Godot level scene:

1. Create a root `Node` (not the FuncGodotMap itself — see warning below).
2. Add a `FuncGodotMap` as a child.
3. Set **Local Map File** to your `.map` file.
4. Set **Map Settings** to your `FuncGodotMapSettings` resource (which references your master FGD).
5. Click **Build**.

FuncGodot reads every entity in the `.map` file, matches classnames to your FGD definitions, instantiates the corresponding scene or node, and calls `_func_godot_apply_properties` to push TrenchBroom key/value pairs into it.

> **Warning:** The Build button **wipes all children** of the FuncGodotMap node. Never put hand-placed or manually edited nodes inside FuncGodotMap. Put them as siblings instead.

---

## Key Rules Summary

| Rule | Why |
|------|-----|
| Never edit `func_godot_fgd.tres` | Overwritten on plugin update |
| Don't put nodes inside FuncGodotMap | Build wipes all children |
| Re-export FGD after any entity change | TrenchBroom reads the file on disk |
| Reload entity defs in TrenchBroom (`F6`) | Editor won't pick up FGD changes automatically |
| Mark entity scripts `@tool` | Required for build-time property application |
| FuncGodotMap should be a child, not the root | So siblings survive rebuilds |

---

## Default Entity Types (from `func_godot_fgd.tres`)

| Entity | Generates | Notes |
|--------|-----------|-------|
| `worldspawn` | `StaticBody3D` + mesh + collision + occluder | One per map. Main geometry. |
| `func_geo` | Same as worldspawn | Use multiple of these instead of worldspawn. |
| `func_detail` | `StaticBody3D` + mesh + collision | No occluder generated. |
| `func_illusionary` | `Node3D` + mesh + occluder | No collision. Decorative. |
| `func_detail_illusionary` | `Node3D` + mesh | No collision, no occluder. |

---

## Inverse Scale Factor

Set on your `FuncGodotMapSettings` resource. Default is `32`, meaning 32 Quake Units = 1 Godot meter. Larger values = smaller maps. Adjust to match your game's intended scale.
