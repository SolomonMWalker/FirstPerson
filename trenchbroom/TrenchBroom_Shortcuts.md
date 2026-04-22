# TrenchBroom 2026.1-RC2 Keyboard Shortcut Cheat Sheet

---

## File Operations

| Action | Shortcut |
|--------|----------|
| New | `Ctrl+N` |
| Open | `Ctrl+O` |
| Save | `Ctrl+S` |
| Save As | `Ctrl+Shift+S` |
| Close | `Ctrl+W` |
| Reload Material Collections | `F5` |
| Reload Entity Definitions | `F6` |

---

## Edit — Undo / Redo / Clipboard

| Action | Shortcut |
|--------|----------|
| Undo | `Ctrl+Z` |
| Redo | `Ctrl+Shift+Z` |
| Cut | `Ctrl+X` |
| Copy | `Ctrl+C` |
| Paste (near cursor) | `Ctrl+V` |
| Paste at Original Position | `Ctrl+Alt+V` |
| Duplicate in Place | `Ctrl+D` |
| Delete | `Del` |
| Repeat Last Commands | `Ctrl+R` |
| Clear Repeatable Commands | `Ctrl+Shift+R` |

---

## Selection

| Action | Shortcut |
|--------|----------|
| Select All | `Ctrl+A` |
| Deselect All | `Ctrl+Shift+A` |
| Invert Selection | `Ctrl+Alt+A` |
| Select Siblings (same entity/group) | `Ctrl+B` |
| Select Touching (using selection brush) | `Ctrl+T` |
| Select Inside (using selection brush) | `Ctrl+E` |
| Select Tall (2D view lasso — projects brush onto view plane) | `Ctrl+Shift+E` |
| Add/remove individual object | `Ctrl+Left Click` |
| Select brush face | `Shift+Left Click` (3D viewport) |
| Select all faces of a brush | `Shift+Double Click` |
| Add face to selection | `Ctrl+Shift+Left Click` |
| Drill selection through overlapping objects | `Ctrl+Scroll` |
| Cancel / Deselect | `Esc` |

---

## Tools

| Tool | Shortcut | Notes |
|------|----------|-------|
| (Return to move/default mode) | `Shift+Esc` | Deactivates current modal tool |
| Complex Shape (Brush) Tool | `B` | Draw custom convex brush |
| Clip Tool | `C` | Clip brushes with a plane |
| Vertex Tool | `V` | Edit vertices |
| Edge Tool | `E` | Edit edges |
| Face Tool | `F` | Edit faces |
| Rotate Tool | `R` | Rotate objects with a handle |
| Scale Tool | `T` | Scale objects |
| Shear Tool | `G` | Shear objects |

---

## Camera Navigation (3D Viewport)

| Action | Shortcut / Mouse |
|--------|-----------------|
| Look around | `Right Mouse + Drag` |
| Move forward | `W` |
| Move backward | `S` |
| Move left | `A` |
| Move right | `D` |
| Move up | `Q` |
| Move down | `X` |
| Move horizontally (tablet) | `Alt` |
| Move forward/backward | `Scroll Wheel` |
| Pan sideways / up / down | `Middle Mouse + Drag` |
| Orbit around clicked point | `Alt + Right Mouse + Drag` |
| Change orbit radius | `Scroll` (during orbit) |
| Zoom temporarily | `Shift + Scroll` |
| Reset camera zoom | `Ctrl+Alt+Z` |
| Focus camera on selection | `Ctrl+U` |
| Reset 2D Cameras | `Ctrl+Shift+U` |
| Move to next point file point | `.` |
| Move to previous point file point | `,` |

---

## 2D Viewport Navigation

| Action | Mouse |
|--------|-------|
| Pan | `Middle Mouse + Drag` or `Right Mouse + Drag` |
| Zoom (linked across all 2D views) | `Scroll Wheel` |

---

## Viewport Management

| Action | Shortcut |
|--------|----------|
| Cycle current viewport type | `Space` |
| Maximize / Restore current view | `Ctrl+Space` |

---

## Moving Objects (Arrow Keys)

> Behavior depends on which viewport has focus.

| Direction | 2D Viewport | 3D Viewport | Shortcut |
|-----------|-------------|-------------|----------|
| Left | Left | Left | `←` |
| Right | Right | Right | `→` |
| Up (2D) / Forward (3D) | Up axis | Toward camera | `↑` |
| Down (2D) / Backward (3D) | Down axis | Away from camera | `↓` |
| Forward (2D) / Up (3D) | Normal axis | +Z (up) | `PgUp` |
| Backward (2D) / Down (3D) | Depth axis | -Z (down) | `PgDn` |

- Each press moves by the **current grid size**.
- Hold `Alt` while mouse-dragging to move **vertically** in the 3D viewport.
- Hold `Shift` while mouse-dragging to **lock movement to one axis**.
- Use `Ctrl+Alt+M` to move by a precise numeric offset.

---

## Duplicating and Moving

> Same as Move Object shortcuts above, but hold `Ctrl`.

| Action | Shortcut |
|--------|----------|
| Duplicate + move left | `Ctrl+←` |
| Duplicate + move right | `Ctrl+→` |
| Duplicate + move up (2D) / forward (3D) | `Ctrl+↑` |
| Duplicate + move down (2D) / backward (3D) | `Ctrl+↓` |
| Duplicate + move forward (2D) / up (3D) | `Ctrl+PgUp` |
| Duplicate + move backward (2D) / down (3D) | `Ctrl+PgDn` |

> Also: Hold `Ctrl` while **left-dragging** a selected object to duplicate-and-move.

---

## Rotating Objects (Quick Rotation — 90° without Rotate Tool)

> With the Rotate Tool active, uses its current center and angle instead.

| Rotation | Shortcut |
|----------|----------|
| Roll clockwise (about view/normal axis) | `Alt+↑` |
| Roll counter-clockwise | `Alt+↓` |
| Yaw clockwise (about Z) | `Alt+←` |
| Yaw counter-clockwise | `Alt+→` |
| Pitch clockwise (about right axis) | `Alt+PgUp` |
| Pitch counter-clockwise | `Alt+PgDn` |

---

## Flipping Objects

| Action | Shortcut |
|--------|----------|
| Flip horizontally (mirror on right axis) | `Ctrl+F` |
| Flip vertically (mirror on Z / up axis) | `Ctrl+Alt+F` |

---

## Extrusion (Resize Brushes)

| Action | Mouse |
|--------|-------|
| Extrude face along its normal | `Shift + Left Drag` on a face |
| Split brush by extruding | `Ctrl + Left Drag` on a face |
| Move face freely in 2D (not along normal) | `Alt+Shift + Left Drag` in 2D viewport |

---

## CSG Operations

| Action | Shortcut |
|--------|----------|
| Convex Merge (union of selected brushes) | `Ctrl+J` |
| Subtract (cut selected from all others) | `Ctrl+K` |
| Hollow (subtract smaller version from itself) | `Ctrl+Shift+K` |
| Intersect (keep only overlapping volume) | `Ctrl+L` |

---

## Brush Creation (Complex Shape Tool — `B`)

| Action | Shortcut / Mouse |
|--------|-----------------|
| Place a point | `Left Click` on a face |
| Place points on all vertices of a face | `Left Double Click` on face |
| Draw a rectangle of 4 points | `Left Drag` on face |
| Extrude polygon of placed points | `Shift + Left Drag` polygon |
| Confirm — create the brush | `Enter` |
| Cancel all points | `Esc` |

---

## Clip Tool (`C`)

| Action | Shortcut / Mouse |
|--------|-----------------|
| Place clip point | `Left Click` |
| Place two clip points at once | `Left Drag` |
| Match clip plane to a face | `Double Click` on face (3D) |
| Cycle clip mode (keep front / keep both / keep back) | `Ctrl+Enter` |
| Apply clip | `Enter` |
| Remove last clip point | `Del` |
| Cancel / exit tool | `Esc` |

---

## Vertex / Edge / Face Tool

| Action | Mouse / Shortcut |
|--------|-----------------|
| Select handle | `Left Click` |
| Add to selection | `Ctrl+Left Click` |
| Lasso select | `Left Drag` |
| Move handle | `Left Drag` on handle |
| Move vertically in 3D | `Alt` (during drag) |
| Toggle relative/absolute snapping | `Ctrl` (during drag) |
| Snap vertex onto adjacent vertex | `Shift+Alt + Click` target vertex |
| Add new vertex | `Shift + hover` to preview, then `Left Click+Drag` |
| Delete selected vertices/edges/faces | `Del` |
| Snap vertices to integer | `Ctrl+Shift+V` |
| Snap vertices to grid | `Ctrl+Alt+Shift+V` |
| UV Lock (preserve UV during vertex edits) | `U` |

---

## Texture / Material Alignment (3D Viewport, Face/Brush Selected)

### Offset

| Action | Shortcut |
|--------|----------|
| Move texture (grid size) | `↑` `↓` `←` `→` |
| Move texture (fine — 1 unit) | `Ctrl+↑` `Ctrl+↓` `Ctrl+←` `Ctrl+→` |
| Move texture (coarse — 2× grid size) | `Shift+↑` `Shift+↓` `Shift+←` `Shift+→` |

### Angle (Rotation)

| Action | Shortcut |
|--------|----------|
| Rotate CW (15°) | `PgDn` |
| Rotate CCW (15°) | `PgUp` |
| Rotate CW (fine — 1°) | `Ctrl+PgDn` |
| Rotate CCW (fine — 1°) | `Ctrl+PgUp` |
| Rotate CW (coarse — 90°) | `Shift+PgDn` |
| Rotate CCW (coarse — 90°) | `Shift+PgUp` |

### Flip & Reset

| Action | Shortcut |
|--------|----------|
| Flip texture horizontally | `Ctrl+F` |
| Flip texture vertically | `Ctrl+Alt+F` |
| Reset alignment | `Shift+R` |
| Reset alignment to world-aligned | `Alt+Shift+R` |

### Texture Transfer (from selected face)

| Modifier | Action |
|----------|--------|
| `Alt` + click/drag | Transfer material + attributes (projected) |
| `Alt+Shift` + click/drag | Transfer material + attributes (rotated, Valve 220 only) |
| `Alt+Ctrl` + click/drag | Transfer material only (keep target attributes) |

### UV Lock Toggle

| Action | Shortcut |
|--------|----------|
| Toggle UV Lock | `U` |

---

## Grid

| Action | Shortcut |
|--------|----------|
| Show / Hide grid | `0` |
| Snap to grid (toggle) | `Alt+0` |
| Increase grid size | `+` |
| Decrease grid size | `-` |
| Set grid size 1 | `1` |
| Set grid size 2 | `2` |
| Set grid size 4 | `3` |
| Set grid size 8 | `4` |
| Set grid size 16 | `5` |
| Set grid size 32 | `6` |
| Set grid size 64 | `7` |
| Set grid size 128 | `8` |
| Set grid size 256 | `9` |
| Adjust grid size with mouse | `Alt+Ctrl + Scroll` |

---

## Groups & Linked Groups

| Action | Shortcut |
|--------|----------|
| Group selected objects | `Ctrl+G` |
| Ungroup | `Ctrl+Shift+G` |
| Rename selected groups | `Ctrl+Alt+G` |
| Create linked duplicate (mirrors edits) | `Ctrl+Shift+D` |

---

## View — Visibility

| Action | Shortcut |
|--------|----------|
| Isolate selection (hide everything else) | `Ctrl+I` |
| Hide selection | `Ctrl+Alt+I` |
| Show all (un-hide everything) | `Ctrl+Shift+I` |

---

## Inspector Pages

| Action | Shortcut |
|--------|----------|
| Switch to Map Inspector | `Ctrl+1` |
| Switch to Entity Inspector | `Ctrl+2` |
| Switch to Face Inspector | `Ctrl+3` |
| Toggle Info Panel (console / issues) | `Ctrl+4` |
| Toggle Inspector (whole panel) | `Ctrl+5` |
| Toggle Toolbar | `Ctrl+Alt+T` |

---

## Entity Properties Editor

| Action | Shortcut |
|--------|----------|
| Move between fields | `Tab` / `Shift+Tab` |
| Move between property rows | `↑` / `↓` or `Enter` |
| Add new property | `Ctrl+Enter` |

---

## Make Structural

| Action | Shortcut |
|--------|----------|
| Move brushes back into worldspawn / clear content flags | `Alt+S` |

---

## Quick Reference — Mouse Modifiers Summary

| Modifier | Context | Effect |
|----------|---------|--------|
| `Ctrl` + click | Object in viewport | Add/remove from selection |
| `Shift` + click | Face in 3D | Select brush face |
| `Ctrl+Shift` + drag | Face in 3D | Paint-select faces |
| `Alt` + drag (3D) | Moving/editing | Move vertically (Z axis) |
| `Shift` + drag | Moving | Lock to dominant axis |
| `Shift` + drag (brush draw) | Equal X/Y axes | Square footprint |
| `Shift+Alt` + drag | Brush draw | Cube (equal X/Y/Z) |
| `Alt` + drag (brush draw) | Change height only | Adjust Z while drawing |
| `Ctrl` + drag | Extrude face | Split brush instead |
| `Shift` | Scale tool | Proportional scale |
| `Alt` | Scale tool | Scale from center |
| `Ctrl` | Scroll in 3D | Drill through overlapping selections |

---

## Help

| Action | Shortcut |
|--------|----------|
| Open TrenchBroom Manual | `F1` |