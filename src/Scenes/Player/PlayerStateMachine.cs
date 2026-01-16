using Godot;
using System;
using FirstPerson.Scenes.Player;

public partial class PlayerStateMachine : Node
{
    [Export] public bool debug;
    [ExportCategory("References")]
    [Export] public Player PlayerController { get; set; }
}
