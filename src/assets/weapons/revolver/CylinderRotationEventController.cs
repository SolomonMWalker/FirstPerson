using Godot;
using System;

public partial class CylinderRotationEventController : Node
{
    public float timeInSeconds;
    public float rotationAmount;
    public Vector3 rotationAxis;
    public Node3D rotationNode;
    public Basis startingBasis;
    public Basis destinationBasis;

    private bool _active;
    private float _timeSinceStart;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(_active) SetFrameRotation(delta);
    }

    public void StartRotation(float time, float rotAmount, Vector3 rotAxis, Node3D node)
    {
        timeInSeconds = time;
        rotationAmount = rotAmount;
        rotationAxis = rotAxis;
        rotationNode = node;
        destinationBasis = rotationNode.Basis.Rotated(rotationAxis, rotationAmount);
        _active = true;
        _timeSinceStart = 0;
    }

    public void SetFrameRotation(double delta)
    {
        var fDelta = (float) delta;

        if(_timeSinceStart > timeInSeconds)
        {
            rotationNode.Basis = destinationBasis;
            _active = false;
            _timeSinceStart = 0;
            rotationAmount = 0;
            rotationAxis = Vector3.Zero;
            rotationNode = null;
            startingBasis = Basis.Identity;
            destinationBasis = Basis.Identity;
            return;
        }

        var percentageToRotate = fDelta / timeInSeconds;
        rotationNode.Basis = rotationNode.Basis.Rotated(rotationAxis, percentageToRotate * rotationAmount);
        _timeSinceStart += fDelta;
    }

}
