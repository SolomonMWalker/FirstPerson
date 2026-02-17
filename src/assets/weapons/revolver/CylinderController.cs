using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CylinderController : Node
{
    [Export] public Node3D CylinderParent {get; set;}
    [Export] public Node3D ForwardNode {get; set;}
    [Export] public CylinderRotationEventController CylinderRotationEventController {get; set;}
    [Export] public int ReloadRotationState = 2;
    
    public int CylinderBoneIndex;
    public Dictionary<int, Basis> BulletInTopLeftBasis = [];

    private Vector3 LocalForward {get; set;}
    private float CylinderRotation {get; set;} = 0;

    public override void _Ready()
    {
        base._Ready();
        SetLocalForward();
        BulletInTopLeftBasis.Add(6, CylinderParent.Basis);
        BulletInTopLeftBasis.Add(5, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(60)));
        BulletInTopLeftBasis.Add(4, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(120)));
        BulletInTopLeftBasis.Add(3, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(180)));
        BulletInTopLeftBasis.Add(2, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(240)));
        BulletInTopLeftBasis.Add(1, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(300)));
        //copy of 6, allows for full rotation when out of ammo
        BulletInTopLeftBasis.Add(0, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(360)));
    }

    public void SetLocalForward()
    {
        LocalForward = CylinderParent.Position.DirectionTo(ForwardNode.Position);
    }

    public void RotateCylinderHammerDown(float timeInSeconds, int currentAmmo)
    {
        SetLocalForward();
        CylinderRotationEventController.StartRotation(timeInSeconds, BulletInTopLeftBasis[currentAmmo-1], 
            CylinderParent);
    }

    public void RotateCylinderOpenCylinder(float timeInSeconds, int currentAmmo)
    {
        SetLocalForward();
        CylinderRotationEventController.StartRotation(timeInSeconds, BulletInTopLeftBasis[currentAmmo+1], 
            CylinderParent);
    }

    public void RotateCylinderReloadTurn(float timeInSeconds, int currentAmmo)
    {
        SetLocalForward();
        CylinderRotationEventController.StartRotation(timeInSeconds, BulletInTopLeftBasis[currentAmmo+1], 
            CylinderParent);
    }

    public void RotateCylinderCloseCylinder(float timeInSeconds, int currentAmmo)
    {
        SetLocalForward();
        CylinderRotationEventController.StartRotation(timeInSeconds, BulletInTopLeftBasis[currentAmmo], 
            CylinderParent);
    }
}
