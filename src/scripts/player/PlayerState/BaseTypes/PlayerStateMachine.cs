using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.Scenes.Player.PlayerState;

[GlobalClass]
public partial class PlayerStateMachine: StateMachine
{
    [Export] public Player Player { get; set; }
    [Export] public Label StateLabel { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (StateLabel.Text != GetStateMachineString())
        {
            StateLabel.Text = GetStateMachineString();
        }
    }

    public string GetMovementState()
    {
        return States.OfType<CompoundState>().FirstOrDefault(s => s.Name == "PlayerMovementState")
            ?.ActiveState.Name;
    }

    public string GetAirborneState()
    {
        return States.OfType<CompoundState>().FirstOrDefault(s => s.Name == "PlayerAirborneState")
            ?.ActiveState.Name;
    }
}