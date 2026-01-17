using System;
using System.Collections.Generic;
using Godot;

namespace FirstPerson.CustomTypes.StateMachine;

[GlobalClass]
public partial class State : Node
{
    public event EventHandler<ChangeStateEventArgs> StateChangeRequired;
    protected void OnStateChangeRequired(ChangeStateEventArgs e)
    {
        var handler = StateChangeRequired;
        handler?.Invoke(this, e);
    }

    public virtual List<State> GetAllStates()
    {
        return [this];
    }
    
    private bool _enabled { get; set; }
    public bool Enabled => _enabled;

    public virtual void Enable()
    {
        _enabled = true;
    }

    public virtual void Disable()
    {
        _enabled = false;
    }

    public virtual void StateEntered()
    {
        Enable();
    }

    public virtual void StateExited()
    {
        Disable();
    }
    public virtual void StatePhysicsProcessing(double delta) {}
    public virtual void StateProcessing(double delta) {}

    public virtual string GetFullStateString() => "";
}