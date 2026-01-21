using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.Scenes.Player.PlayerState;

[GlobalClass]
public partial class BasePlayerStateMachine: StateMachine
{
    [Export] public Player Player { get; set; }
}