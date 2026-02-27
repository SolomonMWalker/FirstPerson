using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class FallingState : EnemyAtomicState
{
    private bool _doneFalling;
    
    public override void StateEntered()
    {
        base.StateEntered();
        _doneFalling = false;
        Grunt.falling = true;
        Grunt.CustomAnimationTree.TrySetParam("notFalling", false);        
        Grunt.CustomAnimationTree.TrySetParam("falling", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.falling = false;
        Grunt.ApplyFloorSnap();
        Grunt.CustomAnimationTree.TrySetParam("notFalling", true);        
        Grunt.CustomAnimationTree.TrySetParam("falling", false);
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt.IsFloorRaycastColliding())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
    }
}
