using Godot;
using System;

public partial class CoverSpot : Node3D
{
    public bool occupied = false;
    public ShootableCharacterBody3D occupier = null;
}
