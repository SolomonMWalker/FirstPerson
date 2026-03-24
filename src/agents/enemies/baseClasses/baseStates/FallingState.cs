using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class FallingState : EnemyAtomicState
{
    private bool _doneFalling;
    
    public override void StateEntered()
    {
        base.StateEntered();
        _doneFalling = false;
        CombatAgent.falling = true;
        //CombatAgent.CustomAnimationTree.TrySetParam("notFalling", false);        
        //CombatAgent.CustomAnimationTree.TrySetParam("falling", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        CombatAgent.falling = false;
        CombatAgent.ApplyFloorSnap();
        //CombatAgent.CustomAnimationTree.TrySetParam("notFalling", true);        
        //CombatAgent.CustomAnimationTree.TrySetParam("falling", false);
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (CombatAgent.IsFloorRaycastColliding())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
    }
}
