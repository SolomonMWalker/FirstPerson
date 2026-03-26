using Godot;
using System;
using System.Collections.Generic;

public partial class CustomAnimationTree : AnimationTree
{
    [Export] public bool IsDebug { get; set; }
    private Dictionary<string, List<string>> _parameterMap = [];

    //Thanks for the help, Claude
    public override void _Ready()
    {
        base._Ready();
        _parameterMap = BuildParameterMap();

        if (IsDebug)
        {
            foreach (var kvp in _parameterMap)
            {
                GD.Print($"Parameter: {kvp.Key}");
                foreach (var path in kvp.Value)
                    GD.Print($"  -> {path}");
            }
        }
    }

    private Dictionary<string, List<string>> BuildParameterMap()
    {
        var map = new Dictionary<string, List<string>>();

        foreach (Godot.Collections.Dictionary prop in GetPropertyList())
        {
            string fullPath = prop["name"].AsString();
            if (!fullPath.StartsWith("parameters/"))
                continue;

            // Extract just the parameter name — the last segment of the path
            // e.g. "parameters/StateMachineA/playback" -> "playback"
            string paramName = fullPath.Split('/')[^1];

            if (!map.ContainsKey(paramName))
                map[paramName] = new List<string>();

            map[paramName].Add(fullPath);
        }

        return map;
    }

    private void SetAllByName(string paramName, Variant value)
    {
        if (!_parameterMap.TryGetValue(paramName, out var paths)) return;
        foreach (var path in paths)
            Set(path, value);
    }

    public bool TrySetParam(string paramName, Variant value)
    {
        if (!_parameterMap.ContainsKey(paramName)) return false;
        SetAllByName(paramName, value);
        return true;
    }
}
