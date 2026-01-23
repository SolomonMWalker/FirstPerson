using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.Scenes.Player.PlayerState;

[GlobalClass]
public partial class BasePlayerCompoundState: CompoundState
{
    public Player Player { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Player = GetNode<PlayerStateMachine>("%PlayerStateMachine").Player;
    }
}