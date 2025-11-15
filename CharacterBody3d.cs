using System;
using Godot;

namespace FirstPerson;

public partial class CharacterBody3d : CharacterBody3D
{
    public Camera3D camera;
    public CapsuleShape3D collisionCapsuleShape;
    public float cameraSensitivity = 0.01f;
    public float speed = 10;
    public float jumpVelocity = 4.5f;
    public float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    public float crouchCameraHeightMult = 0.5f;
    public float crouchCollisionShapeHeightMult = 0.5f;

    private float defaultCameraHeight;
    private float defaultColliderShapeHeight;

    private bool isCrouching = false;

    public override void _Ready()
    {
        base._Ready();
        camera = GetNode<Camera3D>("Camera3D");
        defaultCameraHeight = camera.Position.Y;
        collisionCapsuleShape = (CapsuleShape3D)GetNode<CollisionShape3D>("CollisionShape3D").Shape;
        defaultColliderShapeHeight = collisionCapsuleShape.Height;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleCrouch();
        var movementInput = Vector2.Zero;
        if (Input.IsKeyPressed(Key.W)) //forward
        {
            movementInput += Vector2.Up;
        }
        if (Input.IsKeyPressed(Key.S)) //backward
        {
            movementInput += Vector2.Down;
        }
        if (Input.IsKeyPressed(Key.D)) //right
        {
            movementInput += Vector2.Right;
        }
        if (Input.IsKeyPressed(Key.A)) //left
        {
            movementInput += Vector2.Left;
        }
        
        var tempVelocity = Vector3.Zero;

        if (IsOnFloor())
        {
            if (Input.IsKeyPressed(Key.Space))
            {
                tempVelocity.Y = jumpVelocity;
            }
        }
        else
        {
            tempVelocity.Y = (float) (Velocity.Y - gravity * delta);
        }
        
        //awesome reference https://git.colormatic.org/ColormaticStudios/quality-godot-first-person/src/branch/main/addons/fpc/character.gd

        var directionV2 = movementInput.Rotated(-camera.Rotation.Y);
        tempVelocity.X = directionV2.X * speed;
        tempVelocity.Z = directionV2.Y * speed;
        Velocity = tempVelocity;
        MoveAndSlide();
    }

    public void HandleCrouch()
    {
        //this needs to be "action just pressed"
        if (Input.IsActionJustPressed("Crouch") && IsOnFloor())
        {
            if (isCrouching)
            {
                camera.Position = new Vector3(camera.Position.X, defaultCameraHeight, camera.Position.Z);
                collisionCapsuleShape.Height = defaultColliderShapeHeight;
                isCrouching = false;
            }
            else
            {
                camera.Position = new Vector3(camera.Position.X, defaultCameraHeight * crouchCameraHeightMult, camera.Position.Z);
                collisionCapsuleShape.Height = defaultColliderShapeHeight * crouchCollisionShapeHeightMult;
                isCrouching = true;
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            var lookDir = mouseMotionEvent.Relative;
            var rotationY = camera.Rotation.Y - lookDir.X * cameraSensitivity;
            var rotationX = Math.Clamp(camera.Rotation.X - lookDir.Y * cameraSensitivity, 
                Mathf.DegToRad(-90), Mathf.DegToRad(90));
            camera.SetRotation(new Vector3(rotationX, rotationY, 0));
        }
    }
}