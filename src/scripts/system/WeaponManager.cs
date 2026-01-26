using Godot;
using System;
using System.Collections.Generic;
using FirstPerson.Scenes.Player;
using Range = Godot.Range;

public partial class WeaponManager : Node
{
    [Export] public Godot.Collections.Dictionary<int, WeaponData> Weapons = new();
    [Export] public PlayerController PlayerController;

    public int CurrentSlot { get; set; } = 1;

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("WeaponManager");
        
        //Add weapon input actions
        for (int i = 1; i < 10; i++)
        {
            var actionName = $"weapon_{i}";
            if (!InputMap.HasAction(actionName))
            {
                InputMap.AddAction(actionName);
                var eventKey = new InputEventKey();
                eventKey.Keycode = Key.Key1 + (i - 1);
                InputMap.ActionAddEvent(actionName, eventKey);
            }
        }

        CallDeferred("InitializeStartingWeapon");
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        base._UnhandledKeyInput(@event);
        for (int i = 1; i < 10; i++)
        {
            if (@event.IsActionPressed($"weapon_{i}"))
            {
                GD.Print($"switching to slot {i}");
                SwitchToSlot(i);
            }
        }
    }

    private void SwitchToSlot(int slot)
    {
        if (!Weapons.TryGetValue(slot, out var weaponData))
        {
            GD.PrintErr($"Weapon not found at slot {slot}.");
        }

        if (weaponData is not null && weaponData.Unlocked)
        {
            CurrentSlot = slot;
            PlayerController.WeaponController.SwitchWeapon(weaponData);
        }
    }

    public void UseAmmo(int slot, int amount = 1)
    {
        if (Weapons.TryGetValue(slot, out var weaponData))
        {
            weaponData.Ammo = Mathf.Max(0, weaponData.Ammo - amount);
        }
    }

    public int GetCurrentAmmo() => Weapons[CurrentSlot].Ammo;

    private void InitializeStartingWeapon()
    {
        for (int i = 1; i < 10; i++)
        {
            if (Weapons.ContainsKey(i))
            {
                SwitchToSlot(i);
                return;
            }
        }
    }
}
