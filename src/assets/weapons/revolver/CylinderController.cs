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

    //Can be 1-6
    //Movement from 1 -> 2 requires adding 60deg to bone rotation
    //all rotation is adding, so counter-clockwise rotation

    public int CylinderBoneIndex;

    private Vector3 LocalForward {get; set;}
    private float CylinderRotation {get; set;} = 0;
    public void SetLocalForward()
    {
        LocalForward = CylinderParent.Position.DirectionTo(ForwardNode.Position);
    }

    public void RotateCylinderByOneBulletHammerDown(float timeInSeconds)
    {
        SetLocalForward();
        RotateClockwise(1, timeInSeconds);
    }

    public void RotateCylinderByTwoBulletsStartReload(float timeInSeconds)
    {
        SetLocalForward();
        RotateCounterClockwise(2, timeInSeconds);
    }
    public void RotateCylinderByOneBulletReloadTurn(float timeInSeconds)
    {
        SetLocalForward();
        RotateCounterClockwise(1, timeInSeconds);
    }

    public void RotateCylinderByOneBulletEndReload(float timeInSeconds)
    {
        SetLocalForward();
        RotateClockwise(1, timeInSeconds);
    }

    public void RotateCylinderByOneBulletInterruptReload(float timeInSeconds)
    {
        SetLocalForward();
        RotateCounterClockwise(1, timeInSeconds);
    }

    private void RotateCounterClockwise(int spots, float timeInSeconds)
    {
        SetLocalForward();
        CylinderRotationEventController.StartRotation(timeInSeconds, Mathf.DegToRad(-60f * spots), 
            LocalForward, CylinderParent);
    }

    private void RotateClockwise(int spots, float timeInSeconds)
    {
        SetLocalForward();
        CylinderRotationEventController.StartRotation(timeInSeconds, Mathf.DegToRad(60f * spots), 
            LocalForward, CylinderParent);
    }

    
}
