# TrenchBroom — Claude's Notes

Sourced from the TrenchBroom 2026.1-RC2 Reference Manual.

---

## Groups — Basic Reuse

Groups let you treat several objects as one named unit.

1. Select the objects you want to group
2. `Ctrl+G` → give the group a name
3. Click the group to select/move it as one object
4. **Double-click** to open the group and edit individual objects inside
5. **Double-click outside** the group to close it

Groups are the foundation. For actual reuse across multiple places in a map, use **Linked Groups**.

---

## Linked Groups — True Instancing

All linked copies **stay in sync** — editing any one of them updates all others automatically.

**Workflow:**

1. Build your structure (e.g. a doorway, a room module, a repeating detail)
2. Select everything → `Ctrl+G` to group it and name it
3. Select the group → `Ctrl+Shift+D` (**Create Linked Duplicate**)
4. Move/rotate the duplicate into position
5. Repeat steps 3–4 for as many copies as you need
6. To edit later: **double-click any linked copy**, make your changes — all other copies update immediately

**Per-instance overrides (Protected Properties):**

If copies need to differ slightly (e.g. doors with different `targetname` values):

- Open a linked group → select an entity inside → the Entity Inspector shows a checkbox column next to each property
- Check the box to **protect** that property — it won't sync to other copies, and won't be overwritten by changes from other copies
- To clear all protections: select the group → `Edit → Clear Protected Properties` (`Ctrl+Shift+G` ungroups; clearing protections is menu only)

**Unlinking:**
- Select a linked group → `Edit → Separate Linked Groups` to turn it back into a regular independent group

---

## Cross-Map Reuse (Copy/Paste Between Files)

In TrenchBroom, a level/scene is called a **map** and is stored as a `.map` text file. TrenchBroom's clipboard format is literally plain `.map` text, which means cross-map reuse is built on top of plain copy/paste.

### Basic: copy from one map and paste into another

1. Open the source map. Select the objects you want to reuse (select a whole group with one click, or select individual brushes/entities).
2. `Ctrl+C` — TrenchBroom writes the selection to the clipboard as `.map` text.
3. Open the destination map (File → Open, or open a second TrenchBroom window).
4. Paste using one of two modes:
   - **`Ctrl+V` (Paste)** — positions the pasted objects near your current mouse cursor in the viewport. The bounding box snaps to the grid. Use this when you want to place the piece somewhere new.
   - **`Ctrl+Shift+V` (Paste at Original Position)** — pastes at the exact world coordinates from the source map. Use this when coordinates matter (e.g. a modular room that must align on a grid).

### Saving a reusable piece as a snippet file

Because the clipboard is plain `.map` text, you can persist any piece as its own file:

1. Select the objects in TrenchBroom and `Ctrl+C`.
2. Open a text editor and paste — you'll see raw `.map` format (brush plane definitions, entity properties, etc.).
3. Save the file with a `.map` extension somewhere you can find it (e.g. `trenchbroom/snippets/doorway_arch.map`).
4. To reuse later: open the snippet file in a text editor, `Ctrl+A` → `Ctrl+C`, then switch back to TrenchBroom and `Ctrl+V`.

Alternatively, just open the snippet file directly in TrenchBroom, select all (`Ctrl+A`), copy, then switch to your working map and paste.

### When to use snippets vs linked groups

| Situation | Best tool |
|---|---|
| Multiple copies **in the same map** that must stay in sync | Linked Groups (`Ctrl+Shift+D`) |
| A piece you want to place in **different maps** | Snippet `.map` file + copy/paste |
| A piece that recurs often and needs editable properties | Define it as a brush or point entity class (see entity browser section) |

---

## Summary

| Goal | Method |
|------|--------|
| Treat multiple objects as one | `Ctrl+G` — Group |
| Reuse a structure many times, kept in sync | `Ctrl+Shift+D` — Create Linked Duplicate |
| Allow one copy to differ from others | Protected Properties (checkbox in Entity Inspector) |
| Unlink a copy from the group | `Edit → Separate Linked Groups` |
| Reuse between different map files | `Ctrl+C` / `Ctrl+V` across open maps |