using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.scenes.enemies.test.states;

[GlobalClass]
public partial class EnemyStateMachine : StateMachine
{
    [Export] public Grunt Grunt { get; set; }
    [Export] public Label Label { get; set; }


    public override void _Process(double delta)
    {
        base._Process(delta);
        Label.Text = GetStateMachineString();
    }
}