namespace FirstPerson.EventBus;

public class EventPublisher
{
    public bool TryPublishEvent(string eventName, object parameters) =>
        EventBus.TryPublishEvent(eventName, parameters);

    public bool TryCreateEvent(string eventName) => EventBus.TryCreateEvent(eventName);
}