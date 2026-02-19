using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CylinderController : Node
{
    [Export] public Node3D CylinderParent {get; set;}
    [Export] public Node3D ForwardNode {get; set;}
    [Export] public CylinderRotationEventController CylinderRotationEventController {get; set;}

    private readonly Dictionary<int, Basis> _bulletInTopLeftBasis = [];
    private Vector3 LocalForward {get; set;}
    
    public override void _Ready()
    {
        base._Ready();
        SetLocalForward();
        _bulletInTopLeftBasis.Add(6, CylinderParent.Basis);
        _bulletInTopLeftBasis.Add(5, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(60)));
        _bulletInTopLeftBasis.Add(4, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(120)));
        _bulletInTopLeftBasis.Add(3, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(180)));
        _bulletInTopLeftBasis.Add(2, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(240)));
        _bulletInTopLeftBasis.Add(1, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(300)));
        //copy of 6, allows for full rotation when out of ammo
        _bulletInTopLeftBasis.Add(0, CylinderParent.Basis.Rotated(LocalForward, Mathf.DegToRad(360)));
    }

    private void SetLocalForward()
    {
        LocalForward = CylinderParent.Position.DirectionTo(ForwardNode.Position);
    }

    public void RotateCylinderHammerDown(float timeInSeconds, int currentAmmo)
    {
        CylinderRotationEventController.StartRotation(timeInSeconds, _bulletInTopLeftBasis[currentAmmo-1], 
            CylinderParent);
    }

    public void RotateCylinderOpenCylinder(float timeInSeconds, int currentAmmo)
    {
        CylinderRotationEventController.StartRotation(timeInSeconds, _bulletInTopLeftBasis[currentAmmo+1], 
            CylinderParent);
    }

    public void RotateCylinderReloadTurn(float timeInSeconds, int currentAmmo)
    {
        CylinderRotationEventController.StartRotation(timeInSeconds, _bulletInTopLeftBasis[currentAmmo+1], 
            CylinderParent);
    }

    public void RotateCylinderCloseCylinder(float timeInSeconds, int currentAmmo)
    {
        CylinderRotationEventController.StartRotation(timeInSeconds, _bulletInTopLeftBasis[currentAmmo], 
            CylinderParent);
    }
}
