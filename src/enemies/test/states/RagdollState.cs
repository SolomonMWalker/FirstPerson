using Godot;
using System;
using System.Linq;
using FirstPerson.scenes.enemies.test.states;

public partial class RagdollState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        foreach (var cShape in Grunt.BoneCollisionShapes)
        {
            cShape.Disabled = false;
        }
        Grunt.CollisionShape3D.Disabled = true;
        Grunt.PhysicalBoneSimulator3D.Active = true;
        Grunt.PhysicalBoneSimulator3D.PhysicalBonesStartSimulation();
        if (Grunt.affectedBone is not null)
        {
            Grunt.affectedBone.LinearVelocity = Grunt.dirLastDamage * 20f;
        }
    }
}
