using Godot;
using FirstPerson.EventBus;

public partial class EventBusTest : Node3D
{
    public EventPublisher EventPublisher = new EventPublisher();
    public EventSubscriber EventSubscriber = new EventSubscriber();
    public ColorRect ColorRect;

    public override void _Ready()
    {
        base._Ready();
        ColorRect = GetNode<ColorRect>("ColorRect");
        EventPublisher.TryCreateEvent("testEvent");
        EventSubscriber.SubscribeToEvent("testEvent", SetColorRect);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        EventSubscriber.DequeueAndPerformActions();
        if (Input.IsKeyLabelPressed(Key.R))
        {
            if (EventPublisher.TryPublishEvent("testEvent", new EventBusTestParameters("Red")))
            {
                GD.Print("Red event sent");
            }
            else
            {
                GD.Print("Red event wasn't sent");
            }
        }
        else if (Input.IsKeyLabelPressed(Key.G))
        {
            GD.Print("Green event sent");
            EventPublisher.TryPublishEvent("testEvent", new EventBusTestParameters("Green"));
        }
    }

    public void SetColorRect(object colorObj)
    {
        var colorString = (EventBusTestParameters)colorObj;
        if (colorString.colorName == "Red")
        {
            GD.Print("Red event received");
            ColorRect.Color = Colors.Red;
        }
        else if (colorString.colorName == "Green")
        {
            GD.Print("Green event received");
            ColorRect.Color = Colors.Green;
        }
    }
}

public record EventBusTestParameters(string colorName);
