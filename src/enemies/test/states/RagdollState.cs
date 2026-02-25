using Godot;
using System;
using System.Linq;
using FirstPerson.scenes.enemies.test.states;

public partial class RagdollState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.CollisionShape3D.Disabled = true;
        foreach (var cShape in Grunt.BoneCollisionShapes)
        {
            cShape.Disabled = false;
        }
        Grunt.PhysicalBoneSimulator3D.PhysicalBonesStartSimulation();
    }
}
