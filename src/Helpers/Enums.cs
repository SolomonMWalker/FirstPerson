namespace FirstPerson.Helpers;

public enum Goal
{
    MoveToCover,
    MoveToTarget,
    Patrol,
    Standby
}

public enum AgentMovementState
{
    Still,
    DefaultMoving
}

public enum PlayerMovementState
{
    Default,
    Crouching,
    Sprinting
}

public enum PlayerActionState
{
    OnFloor,
    InAir,
    Clambering,
    CoyoteTime
}