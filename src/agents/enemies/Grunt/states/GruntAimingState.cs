using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class GruntAimingState : EnemyAtomicState
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
        if (CombatAgent is null) return;
        
        if (CombatAgent.ragdoll || CombatAgent.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RagdollState"));
            return;
        }
        
        if(!CombatAgent.IsFloorRaycastColliding())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FallingState"));
            return;
        }
        
        if (CombatAgent.Staggered)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("StaggeredState"));
            return;
        }
        
        if (Grunt.aimingOver)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FiringState"));
            return;
        }
    }
}
