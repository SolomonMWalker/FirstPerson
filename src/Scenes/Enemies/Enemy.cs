using FirstPerson;
using FirstPerson.Configuration;
using FirstPerson.Helpers;using Godot;

public partial class Enemy : Agent
{
    public override void _Ready()
    {
        base._Ready();
        AllowedGoals.Add(Goal.MoveToCover);
        CurrentGoal = Goal.MoveToCover;
        
        Target = GetNode<Player>(Configuration.GetConfigValues().PlayerSceneTreePath);
    }

    protected override void CalculateNavigation(double delta)
    {
        switch (CurrentGoal)
        {
            case Goal.MoveToCover:
                MoveToCover(delta);
                break;
            case Goal.MoveToTarget:
                MoveToTarget();
                break;
        }
    }

    protected override void MoveToTarget()
    {
        SetNavigationToTarget();
    }
}
