using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.scenes.enemies.test.states;

[GlobalClass]
public partial class EnemyAtomicState: AtomicState
{
    [Export] public CombatAgent CombatAgent { get; set; }
}