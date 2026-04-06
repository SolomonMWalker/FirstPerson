using FirstPerson.agents.AiComponents;
using Godot;

namespace FirstPerson.agents.enemies.testDog;

[GlobalClass]
public partial class LeapAttackAiComponent : BaseAiNavComponent
{
    [Export] public Dog Dog { get; set; }
    [Export] public Timer CheckGroundedTimer { get; set; }

    private float Gravity { get; } = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    private bool _jumped, _canCheckGrounded;
    private Vector3 _leapAttackVelocity;

    public override void _Ready()
    {
        base._Ready();
        CheckGroundedTimer.Timeout += () =>
        {
            _canCheckGrounded = true;
        };
    }

    public void StartLeapAttack()
    {
        _jumped = false;
        _canCheckGrounded = false;
    }

    public override void HandleNavigation(double delta)
    {
        if (!Dog.leapAttackInProgress)
        {
            Dog.AgentStopComponent.HandleNavigation(delta);
            return;
        }

        if (_canCheckGrounded && Dog.IsOnFloor())
        {
            Dog.CustomAnimationTree.TrySetParam("leapAttack", false);
            Dog.CustomAnimationTree.TrySetParam("leapAttackGrounded", true);
            Dog.leapAttackInProgress = false;
            Dog.leapAttacking = false;
            Dog.SetCurrentNavComponent(Dog.AgentStopComponent);
            Dog.AgentStopComponent.HandleNavigation(delta);
            return;
        }

        if (!_jumped)
        {
            _leapAttackVelocity = Dog.GlobalPosition.DirectionTo(Dog.MovementTarget.GlobalPosition) * Dog.LeapAttackSpeed;
            _leapAttackVelocity = _leapAttackVelocity with { Y = 0 };
            _leapAttackVelocity.Y = Dog.LeapAttackJumpForce;
            _jumped = true;
        }
        else
        {
            _leapAttackVelocity.Y = Dog.Velocity.Y;
        }

        _leapAttackVelocity.Y -= Gravity * (float)delta;
        _leapAttackVelocity.Y = Mathf.Clamp(_leapAttackVelocity.Y, -50f, 50f);

        Dog.Velocity = _leapAttackVelocity;
        Dog.MoveAndSlide();
    }
}