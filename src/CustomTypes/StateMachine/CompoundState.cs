using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FirstPerson.CustomTypes.StateMachine;

//runs processing for itself and its chosen child
[GlobalClass]
public partial class CompoundState : State
{
    [Export] public string DefaultStateName;

    public List<State> ChildrenStates { get; private set; } = [];
    public State ActiveState;
    public State NextState;

    public override void _Ready()
    {
        ChildrenStates = GetChildren().OfType<State>().ToList();
        if (ChildrenStates.Count == 0)
        {
            throw new Exception("Compound state has no children states");
        }

        if (String.IsNullOrWhiteSpace(DefaultStateName))
        {
            DefaultStateName = ChildrenStates.First().Name;
        }

        if (!TryGetStateByName(DefaultStateName, out var state))
        {
            throw new Exception($"Default state name of {DefaultStateName} doesn't match with children states");
        }

        ActiveState = state;
        ActiveState.Enable();
    }

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
        ActiveState.Enable();
    }

    public override void Disable()
    {
        base.Disable();
        ChildrenStates.ForEach(cs => cs.Disable());
    }

    public bool TryGetStateByName(string stateName, out State state)
    {
        state = ChildrenStates.FirstOrDefault(s => s.Name.ToString().Equals(stateName));
        return state is not null;
    }

    public void ChangeState(string stateName = null)
    {
        stateName ??= NextState?.Name;
        NextState = null;

        if(TryGetStateByName(stateName, out var state))
        {
            if (state.Name == ActiveState.Name) return;
            
            ActiveState.StateExited();
            ActiveState = state;
            ActiveState.StateEntered();
        }
        else
        {
            throw new Exception($"State with name {stateName} is not found in children states");
        }
    }

    public override string GetFullStateString()
    {
        return $"{Name}({ActiveState.GetFullStateString()})";
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if(Enabled) StatePhysicsProcessing(delta);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(Enabled) StateProcessing(delta);
    }
}