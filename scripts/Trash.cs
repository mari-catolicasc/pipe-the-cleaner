using Godot;

public partial class Trash : Area2D
{
    public enum TrashType
    {
        Plastic,
        Paper,
        Glass,
        Metal,
        Organic
    }

    [Export] public TrashType Type = TrashType.Plastic;
    [Export] public bool Floats = false;
    [Export] public float FloatAmplitude = 5.0f;
    [Export] public float FloatSpeed = 2.0f;

    private float _time = 0.0f;
    private Vector2 _startPosition;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        AddToGroup("trash");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Floats)
            return;

        _time += (float)delta;

        Vector2 pos = _startPosition;
        pos.Y += Mathf.Sin(_time * FloatSpeed) * FloatAmplitude;
        GlobalPosition = pos;
    }

    public void Collect()
    {
        QueueFree();
    }
}