using Godot;
using System;
using System.Linq;
using FirstPerson.scenes.enemies.test.states;

public partial class RagdollState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.StartRagdoll();
    }
}
