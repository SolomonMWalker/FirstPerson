using FirstPerson.CustomTypes.StateMachine;
using Godot;

public partial class DogIsMovingState : IsMovingState
{
    private Dog Dog { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        Dog = (Dog)CombatAgent;
    }
    
    public override void StateEntered()
    {
        base.StateEntered();
        Dog.CustomAnimationTree.TrySetParam("moving", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Dog.CustomAnimationTree.TrySetParam("moving", false);
    }
}
