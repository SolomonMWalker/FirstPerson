using Godot;
using System;
using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class NotInCombatState : EnemyAtomicState
{
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
    
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Grunt is null || Grunt.firing || Grunt.dead) return;
        if (Grunt.CombatTriggerArea.HasOverlappingAreas())
        {
            var overlapArea = Grunt.CombatTriggerArea.GetOverlappingAreas().First();
            if (overlapArea is Hitbox hitbox)
            {
                Grunt.NavAgentMovementTargetNode = hitbox.Parent;
                OnStateChangeRequired(new ChangeStateEventArgs("InCombatState"));
                return;
            }
        }
    }
}
