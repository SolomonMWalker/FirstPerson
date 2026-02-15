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
    private int RotationState {get; set;} = 1;
    private float CylinderRotation {get; set;} = 0;
    private Basis? RotationDestinationBasis {get;set;} = null;
    private Tween RotationTween {get; set;}
    private Dictionary<int, int> CylinderRotationStateToDegreeRotation {get; set;} = [];

    public override void _Ready()
    {
        base._Ready();
        CylinderRotationStateToDegreeRotation.Add(1, 0);
        CylinderRotationStateToDegreeRotation.Add(2, 60);
        CylinderRotationStateToDegreeRotation.Add(3, 120);
        CylinderRotationStateToDegreeRotation.Add(4, 180);
        CylinderRotationStateToDegreeRotation.Add(5, 240);
        CylinderRotationStateToDegreeRotation.Add(6, 300);
    }

    public void SetLocalForward()
    {
        LocalForward = CylinderParent.Position.DirectionTo(ForwardNode.Position);
    }

    public void RotateCylinderByOneBullet(float timeInSeconds)
    {
        RotationState += 1;
        SetLocalForward();
        CylinderRotationEventController.StartRotation(timeInSeconds, Mathf.DegToRad(60f), LocalForward, CylinderParent);
    }

    public void RotateCylinderToReload(float timeInSeconds)
    {
        if(RotationState == 2) return;
        var stepsToRotate = 0;
        if(RotationState > 2) 
        {
            //rotation steps to 1, then 1 more rotate to 2
            stepsToRotate = 7 - RotationState + 1;
        }
        else if(RotationState < 2)
        {
            //turn 1 to 2
            stepsToRotate = 2 - RotationState;
        }
        var rotationAmount = CylinderRotation + 60f * stepsToRotate;
        rotationAmount = Mathf.DegToRad(rotationAmount);
        SetLocalForward();
        CylinderRotationEventController.StartRotation(timeInSeconds, rotationAmount, LocalForward, CylinderParent);
    }

    public void RotateCylinderToFireReady(float timeInSeconds)
    {
        if(RotationState == 1) return;
        var stepsToRotate = 7 - RotationState;
        var rotationAmount = CylinderRotation + 60f * stepsToRotate;
        rotationAmount = Mathf.DegToRad(rotationAmount);
        SetLocalForward();
        CylinderRotationEventController.StartRotation(timeInSeconds, rotationAmount, LocalForward, CylinderParent);
    }
}
