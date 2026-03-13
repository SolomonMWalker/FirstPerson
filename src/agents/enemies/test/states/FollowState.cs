using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class FollowState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.FuzzyStartTimer.FuzzyStart();
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt is null) return;

        if (Grunt.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("DeadState"));
            return;
        }

        if (Grunt.EnemyStateMachine.CurrentCombatState == "NotInCombatState"
            || Grunt.CombatTarget is null)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("IdleState"));
            return;
        }

        if (Input.IsKeyLabelPressed(Key.H))
        {
            OnStateChangeRequired(new ChangeStateEventArgs("StaggeredState"));
            return;
        }
        
        Grunt.AgentFollowComponent.HandleNavigation(delta);
    }
}
