using Godot;
using System;
using FirstPerson.Scenes.Player;

namespace FirstPerson.Scenes.Player;
public partial class Idle : PlayerState
{
    public void OnIdleStatePhysicsProcessing(float delta)
    {
        if (PlayerController is not null && PlayerController.InputDirections.LengthSquared() > 0)
        {
            PlayerController.StateChart.SendEvent("onMoving");
        }
    }
}
