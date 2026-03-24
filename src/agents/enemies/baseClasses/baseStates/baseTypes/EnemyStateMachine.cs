using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.scenes.enemies.test.states;

[GlobalClass]
public partial class EnemyStateMachine : StateMachine
{
    [Export] public CombatAgent CombatAgent { get; set; }
    [Export] public EnemyCompoundState BehaviorState { get; set; }
    [Export] public EnemyCompoundState ActionState { get; set; }
    [Export] public EnemyCompoundState CombatState { get; set; }

    public string CurrentBehaviorState => BehaviorState.ActiveState.Name;
    public string CurrentActionState => ActionState.ActiveState.Name;
    public string CurrentCombatState => CombatState.ActiveState.Name;
}