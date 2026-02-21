using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class IdleState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.behaviorState = Grunt.BehaviorState.Idle;
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Grunt is null) return;
        if (Grunt.CombatTriggerArea.HasOverlappingBodies())
        {
            Grunt.NavAgentMovementTargetNode = Grunt.CombatTriggerArea.GetOverlappingBodies().First();
            OnStateChangeRequired(new ChangeStateEventArgs("FollowState"));
        }
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt is null) return;
        Grunt.HandleJustGravity(delta);
        Grunt.CharacterBody3D.MoveAndSlide();
    }
}
