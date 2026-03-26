using Godot;
using System;
using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class GruntNotInCombatState : NotInCombatState
{
    private Grunt Grunt { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        Grunt = (Grunt)CombatAgent;
    }

    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.CustomAnimationTree.TrySetParam("notInCombat", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.CustomAnimationTree.TrySetParam("notInCombat", false);
    }
}
