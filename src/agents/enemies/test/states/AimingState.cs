using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class AimingState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        if (Grunt is null) return;
        Grunt.readyToFire = false;
        Grunt.firing = true;
        Grunt.aimingOver = false;
        Grunt.CustomAnimationTree.TrySetParam("doneFiring", false);
        Grunt.CustomAnimationTree.TrySetParam("aiming", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.CustomAnimationTree.TrySetParam("aiming", false);
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Grunt is null) return;
        
        if (Grunt.ragdoll || Grunt.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RagdollState"));
            return;
        }
        
        if(!Grunt.IsFloorRaycastColliding())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FallingState"));
            return;
        }
        
        if (Grunt.aimingOver)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FiringState"));
            return;
        }
    }
}
