using Godot;
using System;

public partial class CharacterBody3d : CharacterBody3D
{
    public Camera3D camera;
    public float cameraSensitivity = 0.01f;
    public float speed = 10;
    public float jumpVelocity = 4.5f;
    public float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public override void _Ready()
    {
        base._Ready();
        camera = GetNode<Camera3D>("Camera3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
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

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            var lookDir = mouseMotionEvent.Relative;
            var rotationY = camera.Rotation.Y - lookDir.X * cameraSensitivity;
            var rotationX = Math.Clamp(camera.Rotation.X - lookDir.Y * cameraSensitivity, Mathf.DegToRad(-90), Mathf.DegToRad(90));
            camera.SetRotation(new Vector3(rotationX, rotationY, 0));
        }
    }
}
