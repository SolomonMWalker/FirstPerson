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

TrenchBroom copies objects as plain text in `.map` format, so you can:

- Copy a group or structure in one map (`Ctrl+C`)
- Open a different map and paste (`Ctrl+V`)

You can also save that clipboard text into a `.map` snippet file to paste into future projects.

---

## Summary

| Goal | Method |
|------|--------|
| Treat multiple objects as one | `Ctrl+G` — Group |
| Reuse a structure many times, kept in sync | `Ctrl+Shift+D` — Create Linked Duplicate |
| Allow one copy to differ from others | Protected Properties (checkbox in Entity Inspector) |
| Unlink a copy from the group | `Edit → Separate Linked Groups` |
| Reuse between different map files | `Ctrl+C` / `Ctrl+V` across open maps |