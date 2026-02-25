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
        if (Grunt.CombatTriggerArea.HasOverlappingAreas())
        {
            var overlapArea = Grunt.CombatTriggerArea.GetOverlappingAreas().First();
            if (overlapArea is Hitbox hitbox)
            {
                Grunt.NavAgentMovementTargetNode = hitbox.Parent;
                OnStateChangeRequired(new ChangeStateEventArgs("FollowState"));
                return;
            }
        }
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
        
        Grunt.HandleJustGravity(delta);
    }
}
