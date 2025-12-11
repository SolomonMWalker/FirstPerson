using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FirstPerson.EventBus;

public class EventSubscriber
{
    private readonly int _dequeueMax;
    private List<EventQueue> _eventQueues = [];
    private Dictionary<string, List<Action<object>>> _eventActions = [];

    public EventSubscriber(int? dequeueMax = null) => _dequeueMax = dequeueMax ?? 5;

    public void DequeueAndPerformActions()
    {
        foreach (var q in _eventQueues.Where(q => q.HasMessages))
        {
            if (!q.HasMessages) continue;   
            foreach (var eventParameters in q.Dequeue())
            {
                foreach (var action in _eventActions[q.EventName])
                {
                    action(eventParameters);
                }
            }
        }
    }

    private bool TryCreateQueueAndAddToEventBus(string eventName)
    {
        var eq = new EventQueue(eventName, _dequeueMax);
        if (_eventQueues.Any(eventQueue => eventQueue.EventName == eventName))
        {
            return false;
        }
        _eventQueues.Add(eq);
        EventBus.AddEventQueue(eq);
        return true;
    }

    public bool SubscribeToEvent(string eventName, Action<object> eventAction)
    {
        if (!TryCreateQueueAndAddToEventBus(eventName)) return false;
        if (!_eventActions.TryAdd(eventName, [eventAction]))
        {
            _eventActions[eventName].Add(eventAction);
        }
        
        return true;

    }
}