# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A first-person shooter game built with **Godot 4.6** using **C# (.NET 8)**. The project uses the Jolt Physics engine and the `func_godot` addon for TrenchBroom/Quake map importing.

## Building & Running

This is a Godot project — there is no CLI build command. Open and run from the Godot editor. The C# assembly (`FirstPerson.csproj`) is compiled automatically by Godot on play. To build the assembly only (without running):

```
dotnet build FirstPerson.csproj
```

There are no automated tests in this project.

## Architecture

### State Machine System (`src/CustomTypes/StateMachine/`)

The core abstraction used throughout the project. Three state types:
- **`AtomicState`** — leaf state, runs its own `StateProcessing`/`StatePhysicsProcessing` when enabled.
- **`CompoundState`** — has child states, only one active at a time. Requires a `DefaultStateName`. Uses `ChangeState(stateName)` to transition.
- **`ParallelState`** — has child states, all run simultaneously.

**`StateMachine`** is the root manager. It indexes all `AtomicState` nodes by building `PathToAtomicState` entries at startup. State transitions are triggered by raising `StateChangeRequired` event with a `ChangeStateEventArgs(stateName)`, which the `StateMachine` handles by walking the path tree and calling `ChangeState()` on each `CompoundState` along the path. The state machine requires a child node named `*RootState*`.

All domain-specific state machines (player, enemy, weapon) extend this base system with typed base states (e.g., `PlayerAtomicState`, `EnemyAtomicState`, `WeaponAtomicState`) that expose the relevant controller/agent reference via `[Export]`.

### Player System (`src/player/`)

- **`PlayerController`** (CharacterBody3D) — owns movement, crouch, interact, and references all sub-controllers. Exports `PlayerStateMachine`, `CameraController`, `WeaponController`, `ClamberController`, `HealthComponent`, `ShieldComponent`.
- **`PlayerStateMachine`** — two parallel compound states: `PlayerMovementState` (Idle/Walking/Sprinting/Crouching) and `PlayerAirborneState` (Grounded/InAir/Jumping/Coyote/Clambering).
- **`CameraController`** — handles mouse look, crouch tween, and interact raycast.
- **`WeaponController`** — manages equipped weapon, delegates to `WeaponStateMachine`.
- **`WeaponSubViewport`** — weapon rendering is done in a separate viewport to avoid depth-sorting issues with the world geometry.

### Enemy / AI System (`src/agents/`)

Inheritance chain: `CharacterBody3D` → `MovingAgent` → `CombatAgent` → specific enemies (e.g., `Grunt`, `testDog`).

- **`MovingAgent`** — abstract base with `NavigationAgent3D` and a `CurrentNavComponent` (swappable `BaseAiNavComponent`). Navigation components (`AgentFollowComponent`, `AgentStopComponent`, `AgentPatrolComponent`) plug in to provide different movement behaviors.
- **`CombatAgent`** — adds health/stagger/hitbox management, ragdoll, `EncounterZone` integration, and `EnemyStateMachine`. Switches nav component between `defaultCombatAi` and `defaultNoncombatAi` depending on `inCombat`.
- **`Grunt`** — concrete enemy: hitscan shooter with patrol support, aiming/firing cycle, ragdoll on death.

Enemy states follow the same pattern as player states, with `EnemyAtomicState` exposing a `CombatAgent` reference.

### Component System (`src/components/`)

Reusable nodes attached to characters:
- **`HealthComponent`** — health pool with `OnDeath` and `OnHealthDepleted` signals. Optionally delegates to `ShieldComponent` first.
- **`StaggerComponent`** — separate stagger pool; `OnStagger` fires when depleted.
- **`Hitbox`** (Area3D) — receives `HitInformation` and routes health/stagger damage. Supports `Weakspot` type with damage multipliers. Emits `OnHitSetCombatTarget` (alerts the enemy AI) and `OnHitSetImpulseReaction` (triggers camera kick).

### Weapon System (`src/assets/weapons/`)

- **`Weapon`** (Resource) — data-only resource: damage, range, accuracy, sway, kick parameters.
- **`WeaponRig`** / **`RevolverRig`** — scene nodes that implement the visual weapon behavior and own a `WeaponStateMachine`.
- Revolver has a detailed state machine: hip/aim modes each with Idle, HammerDown, Fire, Empty, Reload sub-states.

### Global Systems (`src/system/`)

- **`Managers`** (Autoload singleton) — locates manager nodes via groups at startup. Currently holds `WeaponManager`.
- **`GameManager`** — scene-level manager.

### Physics Layers

| Layer | Name |
|---|---|
| 3D Physics 1 | StaticGeometry |
| 3D Physics 2 | DynamicPhysics |
| 3D Physics 3 | Clamberable |
| 3D Physics 4 | Player |
| 3D Physics 5 | Interactable |
| 3D Physics 6 | Enemy |
| 3D Physics 7 | HittableNonCharacters |
| 3D Render 1 | Default |
| 3D Render 2 | Weapon |

### Global Groups

`player`, `weaponController`, `projectileParent`, `enemies`, `WeaponManager`

## Key Conventions

- State transitions are always triggered by calling `OnStateChangeRequired(new ChangeStateEventArgs("TargetStateName"))` from within a state. The state machine resolves the path automatically — never call `ChangeState()` directly from outside.
- `CompoundState.DefaultStateName` must match an immediate child state's node name.
- Enemy `_Ready()` must call `Callable.From(ActorSetup).CallDeferred()` to defer navigation setup until after the physics frame.
- `[Export]` is used heavily for wiring references in the Godot editor — avoid constructor injection.
- Namespace: most player/system code is under `FirstPerson.*`; some older/base classes are in the global namespace.
