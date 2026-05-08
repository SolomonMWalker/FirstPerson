using Godot;
using System;
using System.Collections.Generic;
using FirstPerson.scripts.system;

public partial class DebugMenu : Control
{
    [Export] public VBoxContainer ValuesContainer { get; set; }

    private static DebugMenu _instance;
    private static readonly Dictionary<string, Func<object>> _registry = new();
    private readonly Dictionary<string, Label> _labels = new();

    public static void Register(string name, Func<object> getter)
    {
        _registry[name] = getter;
        _instance?.AddWatchedLabel(name, getter);
    }

    public static void Unregister(string name)
    {
        _registry.Remove(name);
        if (_instance != null && _instance._labels.TryGetValue(name, out var label))
        {
            label.QueueFree();
            _instance._labels.Remove(name);
        }
    }

    public override void _Ready()
    {
        base._Ready();
        _instance = this;
        foreach (var (name, getter) in _registry)
            AddWatchedLabel(name, getter);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        foreach (var (name, getter) in _registry)
        {
            if (_labels.TryGetValue(name, out var label))
                label.Text = FormatItem(name, getter);
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (_instance == this) _instance = null;
    }

    private void AddWatchedLabel(string name, Func<object> getter)
    {
        if (_labels.ContainsKey(name)) return;
        var label = new Label { Text = FormatItem(name, getter), AutowrapMode = TextServer.AutowrapMode.Word };
        ValuesContainer.AddChild(label);
        _labels[name] = label;
    }

    private static string FormatItem(string name, Func<object> getter) =>
        $"{name}: {getter()}";
}
