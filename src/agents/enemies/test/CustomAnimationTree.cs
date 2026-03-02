using Godot;
using System;

public partial class CustomAnimationTree : AnimationTree
{
    [Export] public Godot.Collections.Dictionary<StringName, Godot.Collections.Array<StringName>> ParamToPath;
    
    public bool TrySetParam(string param, bool value)
    {
        if (!ParamToPath.ContainsKey(param)) return false;
        foreach (var path in ParamToPath[param])
        {
            Set(path, value);
        }
        return true;
    }

    public bool TrySetParam(string param, float value)
    {
        if (!ParamToPath.ContainsKey(param)) return false;
        foreach (var path in ParamToPath[param])
        {
            Set(path, value);
        }
        return true;
    }
    
    public bool TrySetParam(string param, int value)
    {
        if (!ParamToPath.ContainsKey(param)) return false;
        foreach (var path in ParamToPath[param])
        {
            Set(path, value);
        }
        return true;
    }
    
    public bool TrySetParam(string param, Vector2 value)
    {
        if (!ParamToPath.ContainsKey(param)) return false;
        foreach (var path in ParamToPath[param])
        {
            Set(path, value);
        }
        return true;
    }
    
    public bool TrySetParam(string param, string value)
    {
        if (!ParamToPath.ContainsKey(param)) return false;
        foreach (var path in ParamToPath[param])
        {
            Set(path, value);
        }
        return true;
    }
}
