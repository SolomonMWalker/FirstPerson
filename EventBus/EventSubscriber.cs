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

    public bool TryListenToEvent(string eventName, Action<object> eventAction)
    {
        if (_eventQueues.Any(q => q.EventName == eventName))
        {
            return false;
        }

        var eq = new EventQueue(eventName, _dequeueMax);
        _eventQueues.Add(eq);
        EventBus.AddEventQueue(eq);

        if (!_eventActions.TryAdd(eventName, [eventAction]))
        {
            _eventActions[eventName].Add(eventAction);
        }
        
        return true;
    }
}