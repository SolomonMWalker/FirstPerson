namespace FirstPerson.CustomTypes.StateMachine;

public class Transition(string toStateName)
{
    public string ToStateName = toStateName;
    public virtual void OnTransition() {}
}