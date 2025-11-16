using System.Collections.Generic;

namespace FirstPerson.EventBus;

public static class EventBus
{
    private static readonly Dictionary<string, List<EventQueue>> EventNameToQueue = [];

    public static void AddEventQueue(EventQueue queue)
    {
        TryCreateEvent(queue.EventName);
        EventNameToQueue[queue.EventName].Add(queue);
    }

    public static bool TryCreateEvent(string eventName) => EventNameToQueue.TryAdd(eventName, []);

    public static bool TryPublishEvent(string eventName, object parameters)
    {
        if (!EventNameToQueue.TryGetValue(eventName, out var queues))
        {
            return false;
        }

        foreach (var queue in queues)
        {
            queue.Enqueue(parameters);
        }

        return true;
    }
}