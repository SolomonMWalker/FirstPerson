using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.Scenes.Player.PlayerState;

[GlobalClass]
public partial class PlayerCompoundState: CompoundState
{
    public PlayerStateMachine PlayerStateMachine { get; set; }
    public PlayerController PlayerController { get; set; }

    public override void _Ready()
    {
        base._Ready();
        PlayerStateMachine = GetNode<PlayerStateMachine>("%PlayerStateMachine");
        PlayerController = PlayerStateMachine.PlayerController;
    }
}