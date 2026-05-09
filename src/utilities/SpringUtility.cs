namespace FirstPerson.utilities;

public static class SpringUtility
{
    public static (float value, float velocity) ApplySpring(float value, float velocity, float target, float stiffness, 
        float damping, float delta)
    {
        velocity += (target - value) * stiffness * delta;
        velocity *= 1.0f - damping * delta;
        value += velocity * delta;
        return (value, velocity);
    }
}