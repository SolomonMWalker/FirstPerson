using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.Scenes.Player.PlayerState;

[GlobalClass]
public partial class BasePlayerAtomicState: AtomicState
{
    [Export] public Player Player { get; set; }
}