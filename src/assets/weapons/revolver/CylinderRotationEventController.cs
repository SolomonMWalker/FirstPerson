using Godot;
using System;

public partial class CylinderRotationEventController : Node
{
    private Node3D _nodeToRotate;
    private Basis _startBasis, _endBasis;
    private bool _active;
    private float _timeSinceStart;
    private Tween _interpolateTween;

    public void StartRotation(float time, Basis destination, Node3D node)
    {
        if (_interpolateTween is not null && _interpolateTween.IsRunning())
        {
            _nodeToRotate.Basis = destination;
            _interpolateTween.Kill();
        }
        
        _nodeToRotate = node;
        _startBasis = node.Basis;
        _endBasis = destination;
        _active = true;
        
        _interpolateTween = CreateTween();
        var callable = Callable.From((float weight) => Interpolate(weight));
        _interpolateTween.TweenMethod(callable, 0.0, 1.0, time)
            .SetTrans(Tween.TransitionType.Expo);
        _interpolateTween.Finished += TweenDone;
    }

    public void StartRotation(float time, float rotAmount, Vector3 rotAxis, Node3D node)
    {        
        var rotationAmount = rotAmount;
        var rotationAxis = rotAxis;
        var rotationNode = node;
        _endBasis = rotationNode.Basis.Rotated(rotationAxis, rotationAmount);
        _nodeToRotate = node;
        _startBasis = node.Basis;
        _active = true;
        
        _interpolateTween?.Kill();
        _interpolateTween = CreateTween();
        var callable = Callable.From((float weight) => Interpolate(weight));
        _interpolateTween.TweenMethod(callable, 0.0, 1.0, time)
            .SetTrans(Tween.TransitionType.Expo);
        _interpolateTween.Finished += TweenDone;
    }

    private void Interpolate(float weight)
    {
        _nodeToRotate.Basis = _startBasis.Slerp(_endBasis, weight);
    }

    private void TweenDone()
    {
        _active = false;
    }
}
