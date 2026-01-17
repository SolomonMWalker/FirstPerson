using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FirstPerson.CustomTypes.StateMachine;

//Runs processing for itself and all of its children in parallel
[GlobalClass]
public partial class ParallelState : State
{
    public List<State> ChildrenStates { get; private set; } = [];
    
    public override List<State> GetAllStates()
    {
        List<State> states = [this];
        foreach (var child in ChildrenStates)
        {
            states.AddRange(child.GetAllStates());
        }

        return states;
    }
    
    public override void Enable()
    {
        base.Enable();
        ChildrenStates.ForEach(cs => cs.Enable());
    }

    public override void Disable()
    {
        base.Disable();
        ChildrenStates.ForEach(cs => cs.Disable());
    }

    public override string GetFullStateString()
    {
        var s = $"{Name}(";
        s = ChildrenStates
            .Aggregate(s, (current, childState) => current + $"{childState.GetFullStateString()}, ");
        s += ")";
        return s;
    }

    public override void _Ready()
    {
        ChildrenStates = GetChildren().OfType<State>().ToList();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (Enabled) StatePhysicsProcessing(delta);
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Enabled) StateProcessing(delta);
    }
}