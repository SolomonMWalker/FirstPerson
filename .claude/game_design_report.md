# FirstPerson — Code & Game Design Review
*Generated: 2026-05-09 by Claude Sonnet 4.6*
*Based on full read of every .cs file, all scene/resource listings, and project.godot*

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Project Overview](#2-project-overview)
3. [Critical Bugs](#3-critical-bugs)
4. [Code Quality Issues](#4-code-quality-issues)
5. [Architecture Analysis](#5-architecture-analysis)
   - 5.1 State Machine System
   - 5.2 Player System
   - 5.3 Enemy / AI System
   - 5.4 Weapon System
   - 5.5 Component System
   - 5.6 Global Architecture
6. [Game Design Analysis](#6-game-design-analysis)
   - 6.1 Movement & Traversal Feel
   - 6.2 Combat Design
   - 6.3 Enemy Design & AI
   - 6.4 Weapon Design
   - 6.5 Level Design (func_godot)
   - 6.6 Audio
   - 6.7 UI / UX
7. [Prioritized Recommendations](#7-prioritized-recommendations)
8. [Future Development Suggestions](#8-future-development-suggestions)

---

## 1. Executive Summary

This is a **well-designed, ambitious first-person shooter** built on solid architectural foundations. The hierarchical state machine framework, component-based damage system, and dual-viewport weapon rendering are all above-average decisions for a project at this stage. The revolver's single-action mechanics are a standout feature — detailed, tactile, and thematically strong.

**The core concerns are:**
- Four **confirmed bugs** that cause wrong behavior in shipping code (not just edge cases)
- Widespread use of **GetFirstNodeInGroup()** to find singleton dependencies — fragile and bypasses the existing Managers autoload
- **No null safety** on exported references throughout the codebase — one misconfigured scene causes a crash
- Game feel is *almost there* but is missing several key feedback systems (footstep audio, hit confirmation sounds, distinct weakspot feedback)
- Enemy AI is functionally correct but mechanically shallow — both current enemies share a similar behavior profile

**Overall assessment:** Very promising foundation. Fix the bugs, tighten the wiring, then invest in game feel and enemy variety.

---

## 2. Project Overview

### Tech Stack
| Concern | Choice | Assessment |
|---|---|---|
| Engine | Godot 4.6 | ✅ Excellent — latest stable, best C# support |
| Physics | Jolt Physics | ✅ Great — more stable than GodotPhysics for complex scenes |
| Physics interpolation | Enabled | ✅ Correct — essential for smooth first-person feel |
| Rendering | Forward Plus | ✅ Right choice for dynamic lighting in indoor levels |
| Language | C# (.NET 8) | ✅ Good for a typed, OOP-heavy codebase like this |
| Level editor | func_godot (TrenchBroom) | ✅ Inspired choice — Quake workflow fits the genre perfectly |
| Animation | AnimationTree + custom state machine wrapper | ✅ Clever CustomAnimationTree abstraction |

### What Has Been Built
- **Player:** Full movement system (walk/sprint/crouch/jump/coyote/clamber), step climbing, mouse look, camera effects (headbob, tilt, fall/damage/weapon kick, screen shake), spring-physics weapon sway, dual-viewport weapon rendering
- **Weapons:** One weapon (revolver) with full single-action state machine (hip/aim × idle/hammer-down/fire/empty/reload + interrupt), hitscan and projectile support, accuracy penalty system
- **Enemies:** Two enemies — Grunt (hitscan shooter with patrol) and Dog (melee + leap attack with zigzag approach). Both have ragdoll, stagger, combat/non-combat animation split
- **AI:** Four navigation components (follow, stop, patrol, zigzag) + custom LeapAttack component
- **Damage:** Three-layer health system (shield → health, with stagger running in parallel), weakspot multipliers, directional damage kicks
- **World:** func_godot integration, encounter zones, spawner system, three levels (test, sample, first_level)

### Current State Assessment
The game is in **vertical slice / proof-of-concept** territory. The systems are real and interconnected, but feel and polish are still work-in-progress. The architecture is good enough to scale — the main risk is the pattern of using GetFirstNodeInGroup() spreading further before being addressed.

---

## 3. Critical Bugs

These are confirmed incorrect behaviors in the current code — not style issues.

---

### BUG-01: WeaponSubViewport always has wrong size
**File:** `src/player/WeaponSubViewport.cs`
**Severity:** High — weapon renders at 0×0 or wrong resolution

```csharp
// WRONG (current)
_Ready():
    ScreenSize = Size;          // assigns 0,0 to ScreenSize at startup

_Process():
    if (GetWindow().Size != ScreenSize)
        Size = ScreenSize;      // keeps setting weapon viewport to 0,0
```

The assignment is backwards. `Size` at `_Ready()` is the *editor-set* default, not the window size. The weapon viewport never gets the real window size.

**Fix:**
```csharp
public override void _Ready()
{
    Size = GetWindow().Size;
}

public override void _Process(double delta)
{
    var windowSize = GetWindow().Size;
    if (Size != windowSize)
        Size = windowSize;
}
```

---

### BUG-02: RevolverAimHammerDownState missing return causes double state transition
**File:** `src/assets/weapons/revolver/states/RevolverAimHammerDownState.cs`
**Severity:** High — can trigger two state changes in one frame, corrupting state machine

```csharp
StateProcessing(double delta):
    if (!IsAnimationPlaying() && Input.IsActionJustPressed("Fire") && WeaponController.CanFire())
        OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimFireState"));
        // MISSING return; here
    if (!IsAnimationPlaying() && !Input.IsActionPressed("Aim"))
        OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipHammerDownState"));
    if (!IsAnimationPlaying() && Input.IsActionJustPressed("Reload") && WeaponController.CanReload())
        OnStateChangeRequired(new ChangeStateEventArgs("RevolverReloadState"));
```

If the fire condition is met, all three branches evaluate. `OnStateChangeRequired` fires 2-3 times in the same frame. The state machine will attempt to navigate to multiple states simultaneously.

**Fix:** Add `return;` (or use `else if`) after every `OnStateChangeRequired` call throughout all state files. Audit all other states for the same pattern.

---

### BUG-03: StateMachine._Ready() silently overwrites the exported RootState
**File:** `src/CustomTypes/StateMachine/StateMachine.cs`
**Severity:** Medium-High — inspector assignment is ignored

```csharp
[Export] public State RootState;   // set in inspector

public override void _Ready()
{
    // Overwrites inspector value by searching children
    RootState = GetChildren()
        .OfType<State>()
        .FirstOrDefault(c => c.Name.ToString().Contains("RootState"));
}
```

The `[Export]` attribute is misleading — the value is always overwritten by the name search. Additionally, `.Contains("RootState")` is a loose match (any child named e.g. "MyRootStateV2" also matches). This means the inspector export does nothing.

**Fix:** Either remove the `[Export]` and rely entirely on name-based discovery (with an exact match), or trust the export and remove the `_Ready()` override entirely:
```csharp
// Option A: Remove export, use exact name match
RootState = GetChildren().OfType<State>()
    .FirstOrDefault(c => c.Name == "RootState")
    ?? throw new InvalidOperationException($"{Name}: No child named 'RootState' found.");

// Option B: Remove _Ready() override, trust [Export]
// (Don't assign RootState in _Ready() at all — CLAUDE.md already says children must be named *RootState*)
```

---

### BUG-04: ParallelState.GetFullStateString() produces a trailing comma
**File:** `src/CustomTypes/StateMachine/ParallelState.cs`
**Severity:** Low — cosmetic/debug only, but indicates a code review gap

```csharp
// Produces: "ParallelState(child1, child2, )"
ChildrenStates.Aggregate("", (acc, state) => acc + state.GetFullStateString() + ", ")
```

**Fix:**
```csharp
string.Join(", ", ChildrenStates.Select(s => s.GetFullStateString()))
```

---

### BUG-05: DeadState sets navigation target to itself (no-op)
**File:** `src/agents/enemies/baseClasses/baseStates/DeadState.cs`
**Severity:** Medium — NavigationAgent3D continues path-finding after death

```csharp
StateEntered():
    CombatAgent.NavigationAgent3D.TargetPosition = CombatAgent.GlobalPosition;
    // This is effectively a no-op — NavigationAgent3D will still exist and tick
```

The agent's position changes frame-to-frame (ragdoll drags the CharacterBody3D). Setting target to `GlobalPosition` once doesn't stop navigation. Also the NavigationAgent3D is never explicitly disabled.

**Fix:**
```csharp
StateEntered():
    CombatAgent.NavigationAgent3D.SetNavigationMap(new Rid()); // unmap
    // OR
    CombatAgent.SetPhysicsProcess(false);
    CombatAgent.SetProcess(false);
```

---

## 4. Code Quality Issues

### 4.1 Debug code left in shipping paths (Should Fix Before Testing)

| File | Issue |
|---|---|
| `CameraEffects.cs` line 85-88 | `if (Input.IsActionJustPressed("Test")) AddScreenShake(...)` runs every frame in production |
| `Hitbox.cs` line 95 | `GD.Print("emitting signal")` fires on every hit |
| `StateMachine.cs BuildPaths()` | Commented-out GD.Print debug block left in |

None of these are behind `#if DEBUG`. They add noise and tiny per-frame cost.

**Fix:** Either delete them or wrap in `#if DEBUG ... #endif`.

---

### 4.2 All typed states find their controller via GetFirstNodeInGroup() — fragile

Every typed state base class (`PlayerAtomicState`, `EnemyAtomicState`, `WeaponAtomicState`, etc.) does this in `_Ready()`:
```csharp
PlayerController = GetTree().GetFirstNodeInGroup("player") as PlayerController;
```

This is called 20+ times across the codebase. Problems:
- **Order-dependent:** If the group isn't populated yet (early frame), returns null silently
- **Bypasses Managers:** The `Managers` autoload exists specifically to avoid this pattern
- **Not type-safe:** Silent null if group name changes or wrong type

**Better pattern:**
```csharp
// In PlayerAtomicState._Ready():
PlayerController = GetTree().GetFirstNodeInGroup("player") as PlayerController
    ?? throw new InvalidOperationException($"State {Name} could not find PlayerController");
```

At minimum, add null checks + error logs so you know immediately when wiring fails.

---

### 4.3 Tween leaks in CameraController

```csharp
// In CameraController — called on every crouch/sprint/aim:
private Tween _crouchTween;

void TweenCrouchDown() {
    _crouchTween = CreateTween();   // old tween NOT killed first
    _crouchTween.TweenProperty(...);
}
```

`CreateTween()` creates a new Tween without stopping the previous one. If you crouch then quickly uncrouch, two tweens are fighting over the same property. Godot Tweens are also never garbage-collected while the node exists unless explicitly killed.

**Fix:**
```csharp
void TweenCrouchDown() {
    _crouchTween?.Kill();
    _crouchTween = CreateTween();
    _crouchTween.TweenProperty(...);
}
```

Apply this pattern to every tween in `CameraController` and `CameraEffects`.

---

### 4.4 StepHandlerComponent allocates PhysicsRayQueryParameters3D every physics frame

```csharp
// HandleStepClimbing() — called every physics frame:
var query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask, excludeList);
// New allocation each frame
```

On a 60 Hz physics tick, this is 60 heap allocations per second. Not catastrophic but avoidable.

**Fix:** Cache the query object and mutate `From`/`To` each frame, or use `PhysicsDirectSpaceState3D` directly with a pre-allocated parameters object.

---

### 4.5 Unused fields / dead code

| File | Field / Code | Action |
|---|---|---|
| `RevolverRig.cs` | `_isHammerDown` declared, never read | Delete |
| `FallingState.cs` | `_doneFalling` declared, never set or read | Delete |
| `Dog.cs` | `leapAttackOffset` declared, never used | Delete |
| `CombatAgent.cs` | Multiple commented-out animation parameter calls in `DeadState` | Delete or uncomment |
| `WeaponController.cs` | Accuracy comment "first pellet always centered" contradicts the code | Fix comment or fix code (see §5.4) |

---

### 4.6 Magic numbers without named constants

These appear throughout and make tuning opaque:

| Value | Location | Meaning |
|---|---|---|
| `0.618f` | `WeaponController`, `CameraEffects` | Golden ratio — used in sine wave for idle sway |
| `0.01f` | `CameraController` | Step smooth completion threshold |
| `0.0001f` | `ClamberController` | Y-epsilon for clamber point comparison |
| `20` | `CombatAgent.StartRagdoll()` | Ragdoll impulse force |
| `50f` | `LeapAttackAiComponent` | Max fall speed clamp |
| `5f` | `CombatAgent.OnDeath()` | Seconds before QueueFree |
| `0.2f` | `RevolverReloadState` | Timer delay before state can exit |
| `3f` | `Projectile`, `RevolverBulletCasingPhysics` | Seconds before auto-cleanup |

**Fix:** Add named constants or `[Export]` for any value a designer might want to tune.

---

### 4.7 String-based state transitions — no compile-time safety

Every state transition is a raw string:
```csharp
OnStateChangeRequired(new ChangeStateEventArgs("PlayerWalkingState"));
```

A typo silently does nothing (the state machine finds no matching path, transition is dropped). There's no editor warning.

**Better approach (minimal change):** Add a `static class StateNames` with `const string` fields:
```csharp
public static class PlayerStateNames
{
    public const string Walking = "PlayerWalkingState";
    public const string Sprinting = "PlayerSprintingState";
    // ...
}
// Usage:
OnStateChangeRequired(new ChangeStateEventArgs(PlayerStateNames.Walking));
```

This costs almost nothing and makes refactoring safe.

---

### 4.8 Inconsistent namespace usage

| Pattern | Files affected |
|---|---|
| `FirstPerson.*` namespace | Most player/weapon files |
| Global namespace (no `namespace` declaration) | `CombatAgent`, `Grunt`, `Dog`, `FuzzyStartTimer`, `CustomAnimationTree` |
| `FirstPerson.scenes.enemies.test` | `HitInformation` — clearly an old dev namespace, not reflective of actual file location |

**Fix:** Establish one namespace convention per folder (e.g., `FirstPerson.Agents.Enemies`) and apply consistently. Remove the stale `FirstPerson.scenes.enemies.test` namespace from `HitInformation`.

---

### 4.9 Animation StringName strings duplicated across weapon states

Every revolver state hardcodes animation names as string literals. `RevolverRig.cs` exports dozens of `StringName` fields but state files access animation names independently. If an animation is renamed in the AnimationPlayer, multiple state files silently break.

**Fix:** Create a `RevolverAnimationNames` static class or read the names from the RevolverRig reference rather than hardcoding them in individual states.

---

### 4.10 WeaponController finds WeaponManager via GetFirstNodeInGroup()

```csharp
// WeaponController._Ready():
WeaponManager = GetTree().GetFirstNodeInGroup("WeaponManager") as WeaponManager;
```

`WeaponManager` is already accessible through the `Managers` autoload singleton. Using GetFirstNodeInGroup bypasses it.

**Fix:**
```csharp
WeaponManager = Managers.Instance.WeaponManager;
// Or with null check:
WeaponManager = Managers.Instance.WeaponManager
    ?? throw new InvalidOperationException("WeaponManager not found via Managers singleton");
```

---

## 5. Architecture Analysis

### 5.1 State Machine System

**What's working well:**
- The three-type hierarchy (AtomicState / CompoundState / ParallelState) cleanly maps onto both player and enemy behaviors
- Path caching in `PathToAtomicState` makes transitions O(1) after startup
- Event-driven architecture (`StateChangeRequired`) keeps states decoupled from the StateMachine internals
- The typed base states (`PlayerAtomicState`, `EnemyAtomicState`, etc.) are a clean pattern — each domain gets its own typed reference with zero casting at call sites

**Architectural concerns:**

*The StateMachine is not strict about its contract.* The `RootState` lookup uses `.Contains("RootState")` — this is a convention, not an enforced contract. If you add a child node called "MyRootStateDebug", the machine finds it instead of the real root. Make the lookup exact: `c.Name == "RootState"`.

*StateProcessing is called in _Process AND in CompoundState's own _PhysicsProcess.* The base `State` class has both `StateProcessing` and `StatePhysicsProcessing` as separate virtuals mapped to `_Process` / `_PhysicsProcess`. However, `CompoundState` also calls `StatePhysicsProcessing` in its own `_PhysicsProcess` — meaning physics processing flows through two call paths. Double-check there is no double-invocation on nested states.

*The `Transition` class exists but is never used.* `Transition.cs` defines a `Transition` class with a virtual `OnTransition()` hook, but no state machine code ever constructs a `Transition` object. Either this was planned and abandoned, or it's dead code. If abandoned: delete it. If planned: document the intent.

**Recommended: Add state validation in development builds.**
A common pitfall with HSMs is silently dropped transitions (typo in state name). Add a debug mode to StateMachine that logs a warning when `HandleChangeStateEvent` receives a state name not found in `PathsToAtomicStatesDict`:
```csharp
#if DEBUG
if (!PathsToAtomicStatesDict.ContainsKey(args.StateName))
    GD.PrintErr($"StateMachine: State '{args.StateName}' not found. Available: {string.Join(", ", PathsToAtomicStatesDict.Keys)}");
#endif
```

---

### 5.2 Player System

**What's working well:**
- Clean separation between `PlayerController` (movement), `CameraController` (look/raycast), `CameraEffects` (feel), `WeaponController` (weapon), `ClamberController` (traversal), `StepHandlerComponent` (step climbing)
- The spring-physics weapon sway system (`SwayWeaponRig`) is a genuine game-feel win — idle sway (figure-8 sine), mouse sway, and air offset all stack cleanly
- `CameraEffects` stacking additive effects (tilt + headbob + fall kick + damage kick + weapon kick + screen shake) is the right architecture — each effect is independent and all composable
- `ClamberController` using a 2D grid of raycasts for ledge detection is robust and creative
- Coyote time is implemented properly (timer-based, separate state)
- The dual-viewport weapon rendering (WeaponSubViewport) solves depth-sorting correctly

**Architectural concerns:**

*PlayerController has become a god object.* It exports 20+ references: both collision shapes, both hitboxes, UI labels, the state machine, camera, weapon, clamber, step handler, health, shield, effects, reticle, and animation player. The class itself is relatively thin (mostly delegation), but it's the "everything bag" that every other system reaches into. This works fine at current scale but will become painful when you add more systems.

*Suggested split:* Consider an explicit `PlayerState` (not the HSM kind — just a plain C# class that holds all the runtime state booleans like `Idling`, `InAir`, `Sprinting`, `Aiming`, `Clambering`) as a shared, injected reference. This makes the data flow explicit instead of implicit (everything reaching into `PlayerController`).

*MouseCaptureComponent's RelativeMouseInputWithSens API is confusing.* It accumulates mouse delta during `_UnhandledInput` and then resets to zero in `_Process`. Systems reading this in `_PhysicsProcess` (which runs *before* `_Process` in Godot's update order) read the *previous* frame's value, not the current one. This is a subtle timing bug. Consider converting to a per-frame snapshot that's clearly stamped for a specific frame.

*StepHandlerComponent assumes CapsuleShape3D without validation:*
```csharp
var capsule = (CapsuleShape3D)PlayerController.StandingCollisionShape.Shape;
```
If the standing collision shape is ever swapped to a different shape type (e.g., during development), this throws a cast exception with no helpful message. Add a runtime check.

---

### 5.3 Enemy / AI System

**What's working well:**
- The three-tier state machine (BehaviorState / ActionState / CombatState in `EnemyStateMachine`) elegantly separates *what the enemy is doing* (following, patrolling) from *how it's physically behaving* (moving, staggered, falling, ragdolling) from *whether it's in combat*
- Swappable nav components via `SetCurrentNavComponent()` is clean and extensible — adding new movement behaviors requires only a new `BaseAiNavComponent` subclass
- `FuzzyStartTimer` is a small detail with big impact — staggered first attack times prevent all enemies of the same type from attacking in sync
- `CustomAnimationTree` with reflection-based parameter discovery is clever and eliminates the problem of managing multiple AnimationStateMachinePlayback references manually
- Dog's leap attack as a custom nav component (`LeapAttackAiComponent`) is architecturally coherent — the leap is just another navigation behavior that happens to use impulse physics

**Architectural concerns:**

*CombatAgent has 10+ boolean flags instead of using its own state machine.* The class tracks: `ragdoll`, `dead`, `falling`, `inCombat`, `freezeRotation`, `Staggered`, `_attacking`, and the enemy-specific booleans in Dog (`meleeAttacking`, `leapAttacking`, `leapAttackInProgress`, `leapAttackDone`, `nextAttackIsLeap`). These booleans are set from multiple locations (states, physics callbacks, animation callbacks), making it hard to reason about legal flag combinations.

The irony: there's a perfectly good state machine right there (`EnemyStateMachine`). Consider expressing more of this as state rather than flags. For example, `dead` + `ragdoll` could be a single `DeadState` with an `IsRagdoll` sub-property; `falling` is already a state but the flag is also maintained separately.

*Grunt.StartRagdoll() calls QueueFree on AnimationPlayer.*
```csharp
AnimationPlayer.QueueFree();
```
The `AnimationTree` node almost certainly holds a reference to the `AnimationPlayer`. QueueFree defers deletion to end of frame, but the AnimationTree may continue ticking on that AnimationPlayer reference and produce errors or undefined behavior. The safer approach is `AnimationPlayer.Stop()` followed by setting `Active = false` on the AnimationTree, not QueueFree.

*Dog.HandleMeleeAttackCollision() — meleeAttackObjectsHit doesn't persist across frames correctly.* The list is used to track "already hit" objects during a melee swing, but it's a field that's populated across multiple frames with no explicit clear on swing start. If the list is not cleared when a new melee begins, old targets are excluded from the new swing.

*No line-of-sight check before Grunt fires.* The Grunt's `Fire()` method uses a `ShootRaycast` against the player's `shootTargetRelativePosition`. If the raycast is pointed at the right target but the wall is in the way, it might miss — but there's no check that stops the Grunt from *deciding* to fire through walls. The `GruntAimingState` should verify line-of-sight before transitioning to the firing state, not just after.

*No aggro radius / alert propagation.* If the player shoots a Grunt in a group, only that Grunt enters `inCombat`. Nearby Grunts are unaware. This is fine for a prototype but is a significant gap for final game feel. Enemies in real FPS games alert nearby allies — it makes them feel alive and coordinated.

---

### 5.4 Weapon System

**What's working well:**
- The `Weapon` resource is data-driven and comprehensive — a designer can tune every aspect from the inspector (damage, range, accuracy at speed, sway parameters, recoil, projectile vs. hitscan, pellet count, spread angle)
- The single-action revolver state machine is a detailed, authentic implementation. The hammer-down intermediate state before firing is exactly how a single-action revolver works — this kind of authenticity reads to players even if they can't articulate why
- The cylinder controller precomputes all 6 chamber basis rotations at startup (60° increments) — efficient and exact
- The reload interrupt system (early interrupt = full abort, late interrupt = finish and stop) is nuanced game design
- The `WeaponRig` template pattern (virtual methods for every animation action) makes adding new weapon types straightforward

**Architectural concerns:**

*The first-pellet accuracy logic in WeaponController.PerformHitscan() appears inverted.*
```csharp
// Comment says: "first pellet always centered"
for (int i = 0; i < pelletCount; i++)
{
    float angleOffset = (i == 0) ? AccuracyAngle : AccuracyAngle; // same value both paths?
    // ...
}
```
The comment and the code don't agree. In shotgun-style games, the first pellet is typically perfect-accuracy (guaranteed to hit where you're aiming), with subsequent pellets spreading randomly. Verify the intent and fix the implementation: the first pellet should use `0f` for its angle offset, not `CurrentAccuracyAnglePenalty`.

*All weapon states use GetFirstNodeInGroup() to find WeaponController.* This is the same pattern criticized in §4.2. Weapon states should receive their controller reference through the `WeaponStateMachine` (which is their parent), not by searching the scene tree.

*WeaponManager creates input actions at runtime via InputMap.* This is clever but fragile:
```csharp
InputMap.AddAction($"weapon_{i}");
```
If the game is launched, these actions are created. But if the Managers autoload is restarted mid-session (e.g., scene reload), they're created again — potentially doubling up. And they don't persist between editor runs. A better approach is to pre-declare weapon_1 through weapon_9 in the Input Map in project settings, matching the current layout for the other actions. This makes them visible in the remapping UI (if one is ever added) and eliminates the runtime registration fragility.

---

### 5.5 Component System

**What's working well:**
- Shield → Health → Stagger layering is clean and well-separated. `HealthComponent.DepleteHealth` delegates to `ShieldComponent.TryBlockWithShield` first, which is the right behavior order
- `ShieldComponent` and `StaggerComponent` are structurally identical (pool, recharge timer, recharge delay) — good consistency
- `HitInformation` as a plain data class with nullable fields is the right way to pass optional damage components without creating multiple overloads
- `Hitbox` correctly distinguishes Regular vs Weakspot types and applies appropriate multipliers, while also emitting signals to (a) set the combat target on the agent and (b) trigger camera impulse on the player — two completely different systems notified from one hit event

**Concerns:**
- `Hitbox.cs` is doing too much: it applies health damage, stagger damage, checks for weakspot, emits combat-target signal, emits impulse-reaction signal, and manages debug visualization. The debug visualization should be a child node, not interleaved with gameplay logic.
- `HitInformation` uses namespace `FirstPerson.scenes.enemies.test` — clearly a leftover from early development. Move to `FirstPerson.Components` or similar.
- Neither `HealthComponent` nor `StaggerComponent` validates that `StartingHealth` / `StartingAmount` is > 0. A designer setting these to 0 in the inspector produces an entity that dies/staggers immediately on spawn with no error.

---

### 5.6 Global Architecture

**Managers is underutilized.**
```csharp
// Managers.cs currently:
public WeaponManager WeaponManager;
```
That's all it holds. Yet every other system (PlayerController, WeaponController, all enemy states) finds its dependencies via `GetFirstNodeInGroup()`. The Managers singleton is the right pattern — it just needs to be used consistently. Consider adding:
```csharp
public PlayerController Player { get; private set; }
public GameManager GameManager { get; private set; }
```
And update all callers. Then you have one place to look for the global dependency graph.

**WeaponManager should not be a scene node that Managers finds via a group.** If `WeaponManager` is truly global (it manages a weapon slot dictionary), it should be either an autoload itself or an inner class of `Managers`. As a scene node, it can be accidentally duplicated if you instantiate a scene that contains it.

**GameManager is very thin** — it handles pause/restart/quit and wires pause menu signals. These could live in a root scene script. As the game grows (score tracking, chapter management, save/load), `GameManager` will need to grow too. The infrastructure is fine; just note it'll need expansion.

**EncounterZone and Spawner are good architectural choices.** Trigger-based combat encounters and spawners are the right level-design building blocks for a Quake-style game. They're not over-engineered.

---

## 6. Game Design Analysis

### 6.1 Movement & Traversal Feel

**Current state:** The core movement is solid. Sprint, crouch, jump, coyote time, and step climbing all work. Camera effects (headbob, tilt, fall kick) provide physical feedback. The spring-physics weapon sway is a significant game-feel win.

**What the movement is missing:**

**No footstep audio or visual feedback.** The `StepHandlerComponent` detects every step — it's the *perfect* hook for footstep audio. Without footstep sounds, the game feels disconnected and floaty even when movement math is correct. This is one of the highest-return additions possible.

**No momentum preservation between air and ground.** In Quake-style FPS games (Doom, Quake, Ultrakill, Amid Evil), air strafing preserves and even builds horizontal momentum. The current implementation uses lerp-based acceleration that applies equally on ground and in air. Adding **Quake air strafing** — where `MoveAndSlide` direction changes the velocity vector rather than overriding it — dramatically increases movement skill ceiling and "flow" feel.

Quake-style air movement simplified:
- On ground: normal acceleration toward input direction
- In air: you can only *rotate* your velocity vector by strafing, not override it
- Result: speed is preserved mid-jump, skilled players can chain jumps to build speed

**Sprint-to-slide is an expected mechanic** in modern FPS games (Doom Eternal, HALO Infinite, Ultrakill). Holding sprint and pressing crouch while moving at speed initiates a slide — different physics from normal crouch (slides further, lower collision, can slide under gaps). This would have massive impact on movement feel with relatively small implementation cost. The infrastructure (StepHandlerComponent, CrouchState, SprintState) is already there.

**Variable jump height is absent.** Tap = small hop; hold = full jump. This costs one additional input check in `PlayerJumpingState` and makes platforming feel much more controllable. Currently all jumps are the same height.

**Clamber needs a ceiling check.** The `ClamberController` verifies the top of the ledge but doesn't check if there's enough vertical clearance above the landing point for the player's full height. A low ceiling above a clamberable ledge should block the clamber entirely (otherwise the player clips into geometry).

---

### 6.2 Combat Design

**Current state:** The revolver is the only weapon, using single-action mechanics (hammer → fire). Damage uses a three-layer system (shield/health/stagger). Enemies can enter ragdoll on death. Accuracy penalizes movement.

**Hit feedback is incomplete.** Hit confirmation is one of the most important feedback systems in an FPS. Players need to know *immediately* when they hit something and what kind of hit it was. Currently:
- There appears to be no hitmarker / crosshair flash on hit
- There appears to be no distinct audio for enemy hit vs. wall hit
- Weakspot hits may not be visually distinct from regular hits

**Recommendations:**
1. **Hitmarker**: A brief (0.1s) crosshair flash or mark when a projectile/hitscan hits an enemy — ideally a different color for weakspot hits (white for normal, gold/orange for weakspot)
2. **Hit sound**: A distinct "thud" or "crack" on flesh vs. a "ping" on metal/wall. Even a subtle difference reads to players subconsciously
3. **Enemy hit reaction animation**: When an enemy takes damage (not full stagger), they should flinch slightly — a 1-2 frame "impact" blend. The `StaggerComponent` handles full stagger; a lighter flinch is the missing middle ground

**The stagger system is architecturally correct but game-feel incomplete.** The stagger state exists and plays animations, but there's no camera impulse or slowdown that communicates to the player that they successfully staggered an enemy. Compare to Doom (2016): a staggered demon is clearly vulnerable (glowing, slower, specific audio). This moment should feel rewarding.

**No player dodge/dash.** The enemies (especially the Dog) close distance quickly. Without a dodge option, the player's only avoidance option is movement speed. A short-range dash (1-2 second cooldown, short invincibility frames) would significantly improve the combat loop against melee enemies. This is especially important as you add more enemy types.

**Accuracy penalty direction:** Penalizing accuracy for movement is correct design. However, consider whether the penalty should only apply while *firing* (a single frame) rather than accumulating over time while moving. In most FPS games, inaccuracy is felt at the moment of firing (your crosshair bloom shows you the cone); you don't need to pre-aim for a continuous penalty. The current system may be penalizing more than intended.

---

### 6.3 Enemy Design & AI

**Current enemies:**

| Feature | Grunt | Dog |
|---|---|---|
| Range | Long (hitscan) | Short (melee + leap) |
| Non-combat | Patrol or idle | Zigzag approach |
| Combat movement | Stop and aim | Zigzag + close |
| Attack | Single shot, timed | Melee alternating with leap |
| Stagger | Yes | Yes |
| Ragdoll | Yes | Yes |

This is a good starting contrast — one ranged (stationary shooter) and one melee (aggressive closer). The Dog's alternating melee/leap attack pattern and `LeapAttackAiComponent` show real design thought.

**What's missing / what to improve:**

**No line-of-sight gating on enemy attacks.** Grunt can fire through walls because the aiming decision is not preceded by an LOS check. There's a `ShootRaycast` used *during* firing, but the decision to enter `GruntAimingState` (which commits to firing) should also be LOS-gated. Add: "if I can't see the player, stay in movement state instead of transitioning to aim."

**No group awareness.** Shooting one Grunt doesn't alert nearby Grunts. This is a very common AI feature even in early FPS prototypes. Add: when a `CombatAgent` enters combat, iterate `EncounterZone.GetEnemies()` (EncounterZone already tracks enemies) and call `SetTarget()` on nearby agents within a radius.

**Grunt telegraphing needs work.** Currently Grunt aims and fires. There's no audio tell before firing, no visible wind-up that reads as "I'm about to shoot you." The `FuzzyStartTimer` creates good variance but the player can't anticipate the shot. Consider an audio cue (grunt vocalization, weapon charge sound) in `GruntAimingState`.

**Dog attack variety is good but needs feel.**
- The zigzag approach is smart design (hard to track, feels unpredictable)
- Leap attack is mechanically interesting
- But both attacks use the same hit type — just damage. Adding a knockback impulse to the leap attack (push player away) would make it distinctly feel different from the melee claw.

**Missing enemy archetype: Heavy.** A slow, high-health enemy with a telegraphed powerful attack would complete the classic triad (fast/melee, medium/ranged, slow/heavy). This design space is completely open.

**Patrol behavior on Grunt:** Patrol is set up via `AgentPatrolComponent` and `PatrolPoints` array. This is a solid implementation. One missing piece: the Grunt doesn't *search* after losing sight of the player (returning to patrol vs. holding last known position for a few seconds). Consider adding a brief "search" phase before returning to non-combat — it makes enemies feel less robotic.

---

### 6.4 Weapon Design

**Revolver as the hero weapon is an excellent choice.** Single-action revolvers are thematically rare in FPS games and the mechanical implementation (hammer-down state between idle and fire, cylinder rotation visible to player, reload state machine) creates tactile feedback that most FPS weapons lack. The design decisions here are strong.

**Reload interrupt system:** Two interrupt paths (early = full abort, late = finish partial load and stop) is nuanced and mirrors how real revolvers work. This is excellent design.

**Gaps / recommendations:**

**No empty-cylinder click sound.** When attempting to fire on an empty chamber, there should be a distinctive dry-fire click. `RevolverHipEmptyState` handles this case but apparently just redirects to reload. A brief empty-fire animation and sound before the redirect would be satisfying.

**No melee/pistol-whip.** With a close-range attacker like the Dog, having a melee option (pistol-whip or kick) gives the player an emergency tool. Very low ammo cost — this could even share the revolver's `WeaponRig` with a dedicated melee animation.

**Weapon variety planning:** The `Weapon` resource is already designed for multiple weapon types (it has `PelletCount`, `SpreadAngle`, `IsAutomatic`, `ProjectileScene`). The system is ready for a shotgun, SMG, or burst-fire rifle. Plan the next weapon so its firing behavior is meaningfully different from the revolver (the revolver is slow, high damage, deliberate — the next weapon should be fast, lower damage, spray-and-pray, or have a unique mechanic).

**Weapon sway air offset needs a cap.** In `WeaponController.SwayWeaponRig()`, the weapon applies an air offset (lower position when airborne). If combined with the aim sway multiplier, this can put the weapon in an awkward screen position. Add a clamp on the total offset applied.

---

### 6.5 Level Design (func_godot)

**Using TrenchBroom for level design is an inspired choice.** Quake-style BSP levels have several properties that suit this genre:
- Natural corridors and rooms that control sightlines and create cover
- The Quake aesthetic works extremely well with a revolver-based FPS
- func_godot's integration with Godot 4 is mature and production-ready
- The model_point system allows placing Godot scenes (enemies, pickups) directly from TrenchBroom

**Current level setup:** Three functional levels (test_scene, sample_level, first_level) plus func_godot test maps. The level_skeleton.tscn suggests a template approach — good practice.

**Recommendations:**

**Leverage func_godot entity types more.** TrenchBroom allows custom entity definitions (your `definitions/` folder has these). Add:
- `trigger_encounter` — maps to EncounterZone
- `info_player_start` — spawn point
- `item_weapon_revolver` — pickup that gives the revolver
- `item_health` — health pickup (the `BasePickup` exists in code, connect it)

This lets level designers set up encounters and pickups entirely from TrenchBroom without needing to open the Godot editor.

**Encounter zones need visual design guidance.** Encounter zones work programmatically but there's no guidance on how many enemies per zone, what spawn timing feels right, or how to place spawners for interesting combat. Establish some design rules:
- Rule of 3: No more than 3 active threats simultaneously per encounter zone in early game
- Spread spawn points around the perimeter of the arena, not clustered behind the player
- At least one high-ground position per encounter zone for Grunt placement

---

### 6.6 Audio

Based on the code review, the following audio systems appear to be **entirely absent or minimal:**

| Audio System | Status | Impact |
|---|---|---|
| Footstep sounds | Not found | Very high — movement feels floaty without |
| Enemy impact / flinch sounds | Minimal (revolver has AudioStreamPlayer3D) | High — hit feedback is incomplete |
| Player damage sounds | Not found | High — getting hit is silent |
| Ambient / environmental audio | Not found | Medium — levels feel empty |
| UI sounds (menu clicks) | Not found | Low |
| Enemy voice / vocalization | Not found | Medium — grunts, growls, death sounds |
| Shell casing sounds on floor | Not found | Low — nice detail |
| Revolver hammer sound | Unclear | Medium — single-action feel requires this |

**Audio is the lowest-cost, highest-return area for feel improvement.** A few well-designed sounds can transform a prototype into something that feels shipped. Priorities:
1. Footsteps (already have a hook via StepHandlerComponent)
2. Enemy hit sound (hook via Hitbox signals)
3. Player hurt sound (hook via HealthComponent.OnHealthDepleted)
4. Revolver dry fire click (hook in RevolverHipEmptyState)

---

### 6.7 UI / UX

**Current UI:**
- Health label (text)
- Shield label (text)
- Interact label (text, shown on hover)
- Reticle (exists as a node, details not visible in CS files)
- Pause menu
- Debug menu (registered by Dog, accessible during play)

**Recommendations:**

**Replace text health/shield with visual bars.** Text labels (`HealthLabel`, `ShieldLabel`) work but feel prototype-level. Health/shield bars (or a ring reticle that shows health) are standard. The `OnHealthDepleted` signal from `HealthComponent` is the exact right hook.

**Ammo counter.** The WeaponManager tracks ammo, but there's no clear ammo counter in the HUD. This is table stakes for any weapon with limited ammo (especially a 6-shot revolver). Display: `[current ammo] / [max ammo]` with optional bullet sprite visualization.

**No damage direction indicator.** When the player takes damage, they see the health number drop but have no idea where the damage came from. A directional damage indicator (a brief flash in the direction of the hit source) is standard in modern FPS games and significantly reduces "what hit me?" frustration. The data is available — `HitInformation.SourceGlobalPosition` and `AddDamageKick(pitch, roll, source)` already receive the damage source position.

**Hitmarker.** As mentioned in §6.2, a brief crosshair response when hitting an enemy is critical feedback. Even a single pixel change for 0.1 seconds registers subconsciously.

---

## 7. Prioritized Recommendations

### Priority 1: Fix Confirmed Bugs (do now)

1. **BUG-01** — Fix `WeaponSubViewport` size assignment (backwards, weapon always wrong res)
2. **BUG-02** — Add `return;` after every `OnStateChangeRequired` call in revolver states (especially `RevolverAimHammerDownState`)
3. **BUG-03** — Fix `StateMachine._Ready()` RootState overwrite / use exact name match
4. **BUG-04** — Fix `ParallelState.GetFullStateString()` trailing comma
5. **BUG-05** — Fix `DeadState` — disable NavigationAgent3D on death properly

### Priority 2: Safety & Stability (before adding features)

6. Add null checks + error throws to all `GetFirstNodeInGroup()` calls
7. Kill tweens before creating new ones in `CameraController` and `CameraEffects`
8. Remove test input from `CameraEffects._Process()` (or `#if DEBUG` guard it)
9. Remove `GD.Print("emitting signal")` from `Hitbox.cs`
10. Fix Grunt.StartRagdoll(): use `AnimationPlayer.Stop()` instead of `QueueFree()`
11. Delete unused fields: `_isHammerDown`, `_doneFalling`, `leapAttackOffset`
12. Fix `WeaponController.PerformHitscan()` first-pellet accuracy logic
13. Fix `HitInformation` namespace from `FirstPerson.scenes.enemies.test` to something correct
14. Validate `StartingHealth > 0` in HealthComponent / StaggerComponent `_Ready()`

### Priority 3: Architecture (before scaling to more enemies/weapons)

15. Route all singleton lookups through `Managers` autoload (not `GetFirstNodeInGroup`)
16. Add `PlayerStateNames`, `EnemyStateNames`, `RevolverStateNames` const classes to eliminate magic strings
17. Move WeaponManager to Managers autoload directly (not a scene node found via group)
18. Pre-declare weapon_1–9 input actions in project settings (not created at runtime)
19. Cache PhysicsRayQueryParameters3D in StepHandlerComponent (not per-frame allocation)
20. Add `#if DEBUG` state-name validation to StateMachine (warn on dropped transitions)
21. Establish consistent namespace convention and apply project-wide
22. Delete `Transition.cs` or document its intended future use

### Priority 4: Game Feel (high impact, moderate effort)

23. **Footstep audio** — hook into StepHandlerComponent, play sound on step detection
24. **Hitmarker** — brief crosshair change (0.1s) when Hitbox.Hit() fires for an enemy
25. **Enemy hit flinch** — brief impact blend in AnimationTree when damaged (not full stagger)
26. **Player damage indicator** — directional flash using HitInformation.SourceGlobalPosition
27. **Variable jump height** — tap = small, hold = full; check in PlayerJumpingState
28. **Revolver dry-fire audio** — click sound in RevolverHipEmptyState before reload redirect
29. **Enemy hit audio** — distinct flesh vs. surface impact sounds via Hitbox signals
30. **Health/shield HUD bars** — visual bars instead of text labels

### Priority 5: Design Expansion (after core is solid)

31. Sprint-to-slide mechanic (massive movement feel improvement)
32. Line-of-sight gating on Grunt aiming decision
33. Enemy group alert propagation (alert nearby enemies on hit)
34. Quake-style air strafing / momentum preservation
35. Ammo HUD counter with visual bullet indicators
36. Damage direction HUD indicator
37. Enemy search phase after losing player (pause before returning to patrol)
38. Heavy enemy archetype (slow, telegraphed, hard to kill)
39. Melee backup weapon / pistol whip
40. func_godot entity definitions for encounter zones and pickups

---

## 8. Future Development Suggestions

### Movement: The Quake Model
The game's aesthetic (revolver, TrenchBroom levels, quick enemies) aligns perfectly with Quake-style movement — fast, skilled, momentum-based. Implementing Quake air strafing and a slide mechanic would make this game feel distinct in the modern market rather than "another boomer shooter." The infrastructure is there. The investment is modest. The payoff in feel and replayability is enormous.

### Weapons: Build a Roster With a Role for Each
The `Weapon` resource is designed for this. A recommended progression:
| Weapon | Role | Unique mechanic |
|---|---|---|
| Revolver (current) | Precision / single target | Single-action, must cock before fire |
| Shotgun | Close range burst | Pellet spread, high stagger |
| SMG / Carbine | Sustained DPS | Automatic, accuracy degrades with sustained fire |
| Something weird | Skill expression | Bounce shots, gravity gun, explosive ammo |

The weird fourth weapon is where the game's identity comes from. Ultrakill's railgun, Doom's BFG, Quake's nailgun — the signature weapon defines the game.

### Enemy: The Combat Trinity
Every great action game has three enemy archetypes:
- **Fodder** (Grunt as-is — easy to kill, teaches shooting)
- **Aggressor** (Dog as-is — punishes standing still, teaches movement)
- **Heavy** (missing — punishes recklessness, teaches positioning)

Add a Heavy that requires multiple shots and has a visible wind-up before powerful attacks. The codebase can support this with minimal new infrastructure.

### Level Design: Embrace TrenchBroom Fully
The func_godot integration is a competitive advantage. Most Godot FPS projects use the built-in editor for everything. Your workflow — design in TrenchBroom, import to Godot — produces levels that *feel* Quake because they use the same construction process. Double down on this: learn TrenchBroom's entity system deeply, add custom entity types for every gameplay element (spawners, encounter zones, pickups, doors), and keep Godot scene editing to a minimum.

### Audio: Prioritize Early
Audio is almost always done last in game development and the result is always last-minute, low-quality sound that doesn't fit the game. Placeholder audio early — even free Creative Commons samples — trains you to hear the game as it should sound and informs design decisions (is the revolver punchy enough? Do the footsteps communicate surface material?).

### Save System Foundation
The `WeaponManager` already tracks ammo and unlocks. As soon as you have more than one weapon, you'll need persistence. Plan the save system now — even a simple JSON serialization of the WeaponData array — before the unlock system gets more complex.

---

## Appendix: File Coverage

All source files reviewed:

**Player system** (23 files): PlayerController, CameraController, CameraEffects, MouseCaptureComponent, WeaponController, ClamberController, StepHandlerComponent, WeaponSubViewport, WeaponViewportCamera3d, PlayerUserInterface, PlayerStateMachine, PlayerAtomicState, PlayerCompoundState, PlayerParallelState + 9 concrete states

**Custom Types / State Machine** (7 files): StateMachine, State, AtomicState, CompoundState, ParallelState, PathToAtomicState, Transition, ChangeStateEventArgs

**Agents / Enemy** (45 files): Spawner, Spawnable, SpawnableCollection, MovingAgent, CombatAgent, FuzzyStartTimer, CustomAnimationTree, Grunt + 9 grunt states, Dog + 8 dog states + LeapAttackAiComponent, EnemyStateMachine + EnemyAtomicState + EnemyCompoundState + EnemyParallelState, 9 base behavior states, EncounterZone

**Components** (6 files): HealthComponent, ShieldComponent, StaggerComponent, Hitbox, InteractHitbox, HitInformation

**AI Nav Components** (5 files): BaseAiNavComponent, AgentFollowComponent, AgentPatrolComponent, AgentStopComponent, AgentZigzagComponent

**Weapon system** (20+ files): Weapon, WeaponData, WeaponRig, RevolverRig, CylinderController, CylinderRotationEventController, RevolverBulletCasingPhysics, Projectile, WeaponStateMachine, WeaponAtomicState, WeaponCompoundState, WeaponParallelState + 9 revolver states

**System** (3 files): Managers, WeaponManager, GameManager

**Environment / Utilities** (7 files): DynamicChain, MovingPlatform, BasePickup, TestInteractable, EncounterZone, DisappearingWall, HelperMethods

**UI** (2 files): PauseMenu, Reticle

**Total: ~120 C# files reviewed**

---

*Report compiled from full static code review. No runtime testing performed — recommendations are based on code analysis and domain knowledge of Godot 4 / FPS design patterns.*

*To continue this report at a later time:*
*Load this file, then ask Claude to review any specific section in detail, request implementation plans for specific recommendations, or ask for a follow-up focusing on a particular system (e.g., "Write implementation plan for sprint-to-slide from §6.1").*
