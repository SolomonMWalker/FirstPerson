using FirstPerson;
using FirstPerson.Configuration;
using Godot;

public partial class TestAgent : Agent
{
    public override void _Ready()
    {
        base._Ready();
        AllowedGoals.Add(Goal.MoveToCover);
        CurrentGoal = Goal.MoveToCover;
        
        Target = GetNode<HittableCharacterBody3D>("/root/Test/EnemyTarget");
        //Target = GetNode<ShootableCharacterBody3D>(Configuration.GetConfigValues().PlayerSceneTreePath);
    }

    protected override void CalculateNavigation(double delta)
    {
        switch (CurrentGoal)
        {
            case Goal.MoveToCover:
                MoveToCover(delta);
                break;
            case Goal.MoveToTargetMedium:
                MoveToTarget(delta);
                break;
        }
    }
}
