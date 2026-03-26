using Godot;
using System;

public partial class Dog : CombatAgent
{
    [ExportCategory("Components")]
    [Export] public AgentZigzagComponent AgentZigzagComponent { get; set; }
    [Export] public AgentFollowComponent AgentFollowComponent { get; set; }
    [Export] public AgentIdleComponent AgentIdleComponent { get; set; }
    
    [ExportCategory("References")]
    [Export] public CustomAnimationTree CustomAnimationTree { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    //[Export] public FuzzyStartTimer FireRateTimer { get; set; }
    
    public override void SetLastDamageDirection(Vector3 sourceGlobalPosition, Vector3 collisionGlobalPoint)
    {
        base.SetLastDamageDirection(sourceGlobalPosition, collisionGlobalPoint);
        CustomAnimationTree.TrySetParam("impact", dirLastDamageXz);
        CustomAnimationTree.TrySetParam("impactOneShot", 1);
    }
    
    public override void _Ready()
    {
        base._Ready();
        CurrentNavComponent = AgentIdleComponent;
    }
    
    public override void StartRagdoll()
    {
        base.StartRagdoll();
        AnimationPlayer.Free();
    }
}
