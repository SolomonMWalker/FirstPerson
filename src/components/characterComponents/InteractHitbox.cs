using Godot;
using System;
using FirstPerson.scenes.enemies.test;
using FirstPerson.Scenes.Player;

[GlobalClass]
public partial class InteractHitbox : Area3D
{
    [Signal]
    public delegate void OnInteractEventHandler();
    
    [Export] public bool IsDebug { get; private set; }

    public void Interact()
    {
        EmitSignalOnInteract();    
    }
    
}
