using Godot;
using System;
using System.Collections.Generic;

public partial class CylinderController : Node
{
    [Export] Node3D Cylinder;
    [Export] int ReloadRotationState = 2;

    //Can be 1-6
    //Movement from 1 -> 2 requires adding 60deg to bone rotation
    //all rotation is adding, so counter-clockwise rotation

    private int RotationState {get; set;} = 1;
    private float CylinderRotation {get; set;} = 0;
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

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(Cylinder.Rotation.Z != CylinderRotation)
        {
            Cylinder.Rotation = Cylinder.Rotation with { Z = CylinderRotation };
        }
        if(Cylinder.Rotation.Z == 360)
        {
            Cylinder.Rotation = Cylinder.Rotation with { Z = 0 };
        }
    }


    public void RotateCylinderByOneBullet(float timeInSeconds)
    {
        RotationState += 1;
        var rotationAmount = CylinderRotation + 60f;
        rotationAmount = Mathf.DegToRad(rotationAmount);
        RotationTween = CreateTween();
        RotationTween.TweenProperty(this, "CylinderRotation", rotationAmount, timeInSeconds);
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
        RotationTween = CreateTween();
        RotationTween.TweenProperty(this, "CylinderRotation", rotationAmount, timeInSeconds);
    }

    public void RotateCylinderToFireReady(float timeInSeconds)
    {
        if(RotationState == 1) return;
        var stepsToRotate = 7 - RotationState;
        var rotationAmount = CylinderRotation + 60f * stepsToRotate;
        rotationAmount = Mathf.DegToRad(rotationAmount);
        RotationTween = CreateTween();
        RotationTween.TweenProperty(this, "CylinderRotation", rotationAmount, timeInSeconds);
    }
}
