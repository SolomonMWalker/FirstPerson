using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.scenes.enemies.test.states;

[GlobalClass]
public partial class EnemyCompoundState: CompoundState
{
    [Export] public Grunt Grunt { get; set; }
}