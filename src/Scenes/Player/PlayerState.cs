using System;
using Godot;

namespace FirstPerson.Scenes.Player;

[GlobalClass]
public partial class PlayerState : Node
{
    [Export] public bool debug;
    protected Player PlayerController { get; set; }

    public override void _Ready()
    {
        base._Ready();
        var parent = GetParent();
        PlayerController = parent switch
        {
            PlayerStateMachine playerStateMachine => playerStateMachine.PlayerController,
            PlayerState playerState => playerState.PlayerController,
            _ => throw new Exception("Parent was not a state or a state machine.")
        };
    }
}