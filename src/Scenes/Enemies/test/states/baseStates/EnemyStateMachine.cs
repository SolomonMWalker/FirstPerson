using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.scenes.enemies.test.states;

[GlobalClass]
public partial class EnemyStateMachine : StateMachine
{
    [Export] public Grunt Grunt { get; set; }
}