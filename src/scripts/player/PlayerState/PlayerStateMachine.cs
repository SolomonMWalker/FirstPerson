using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerStateMachine : BasePlayerStateMachine
{
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
        return States.OfType<CompoundState>().FirstOrDefault(s => s.Name == "MovementState")
            ?.ActiveState.Name;
    }

    public string GetAirborneState()
    {
        return States.OfType<CompoundState>().FirstOrDefault(s => s.Name == "AirborneState")
            ?.ActiveState.Name;
    }
}
