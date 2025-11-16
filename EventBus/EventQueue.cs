using System;
using System.Collections.Generic;


namespace FirstPerson.EventBus;

public class EventQueue
{
    public readonly Guid Id = new Guid();
    public readonly string EventName;
    public readonly int DequeueMaxPerCall;
    private readonly Queue<object> _queue = [];

    public EventQueue(string eventName, int dequeueMaxPerCall)
    {
        EventName = eventName;
        DequeueMaxPerCall = dequeueMaxPerCall;
    }

    public void Enqueue(object parameters)
    {
        _queue.Enqueue(parameters);
    }

    public List<object> Dequeue(int? dequeueAmount = null)
    {
        if (!HasMessages) return [];
        dequeueAmount ??= DequeueMaxPerCall;
        if (_queue.Count < DequeueMaxPerCall)
        {
            dequeueAmount = _queue.Count;
        }
        List<object> dqList = [];
        for (int i = 0; i < dequeueAmount; i++)
        {
            dqList.Add(_queue.Dequeue());
        }

        return dqList;
    }

    public bool HasMessages => _queue.Count > 0;
}