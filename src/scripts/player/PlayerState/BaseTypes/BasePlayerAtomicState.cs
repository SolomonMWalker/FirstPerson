using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.Scenes.Player.PlayerState;

[GlobalClass]
public partial class BasePlayerAtomicState: AtomicState
{
    public PlayerStateMachine PlayerStateMachine { get; set; }
    public Player Player { get; set; }

    public override void _Ready()
    {
        base._Ready();
        PlayerStateMachine = GetNode<PlayerStateMachine>("%PlayerStateMachine");
        Player = PlayerStateMachine.Player;
    }
}