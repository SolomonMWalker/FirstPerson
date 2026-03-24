using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.scenes.enemies.test.states;

[GlobalClass]
public partial class EnemyParallelState: ParallelState
{
    [Export] public CombatAgent CombatAgent { get; set; }
}