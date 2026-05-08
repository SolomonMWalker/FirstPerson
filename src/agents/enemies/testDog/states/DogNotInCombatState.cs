using Godot;
using System;
using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class DogNotInCombatState : NotInCombatState
{
    private Dog Dog { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        Dog = (Dog)CombatAgent;
    }

    public override void StateEntered()
    {
        base.StateEntered();
        Dog.combatStateSwitchStateMachine.Travel("NotInCombatStateMachine");
    }
}
