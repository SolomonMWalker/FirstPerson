using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FirstPerson.CustomTypes.StateMachine;

public class ChangeStateEventArgs(string stateName) : EventArgs
{
    public string StateName { get; set; } = stateName;
}

[GlobalClass]
public partial class StateMachine: Node
{
    [Export] public State RootState { get; set; }
    public List<State> States { get; set; }
    public List<PathToAtomicState> PathsToAtomicStates { get; set; } = [];

    public override void _Ready()
    {
        base._Ready();
        States = GetAllStates(RootState);
        RootState = States.FirstOrDefault(s => s.Name == "RootState");
        if (RootState is null)
        {
            throw new Exception("No Root state present as a child of state machine");
        }
        RootState.Enable();
        AddChangeStateDelegatesToEventHandler();
        BuildPaths();
    }

    private List<State> GetAllStates(State rootState)
    {
        return rootState.GetAllStates();
    }

    public void BuildPaths()
    {
        var atomicStates = States.OfType<AtomicState>().ToList();
        foreach (var aState in atomicStates)
        {
            PathsToAtomicStates.Add(new PathToAtomicState(aState));
        }

        foreach (var path in PathsToAtomicStates)
        {
            var s = "Path: ";
            path.Path.ForEach(state => s += $"{state.Name}, ");
            GD.Print(s);
        }
    }

    public void AddChangeStateDelegatesToEventHandler()
    {
        States.ForEach(s => s.StateChangeRequired += HandleChangeStateEvent);
    }
    
    public void HandleChangeStateEvent(object sender, ChangeStateEventArgs args)
    {
        var path = PathsToAtomicStates.FirstOrDefault(p => p.AtomicState.Name == args.StateName);
        if (path is null)
        {
            throw new Exception($"Path to state {args.StateName} not in this state machine");
        }

        for (int i = 0; i < path.Path.Count; i++)
        {
            if (path.Path[i] is CompoundState cState)
            {
                cState.NextState = path.Path[i + 1];
            }
        }

        foreach (var cState in path.Path.OfType<CompoundState>())
        {
            cState.ChangeState();
        }
    }

    public string GetStateMachineString()
    {
        return RootState.GetFullStateString();
    }
}