using Godot;

public partial class Trash : Area2D
{
    [Export] public bool Floats = false;

    private float _time = 0.0f;
    private Vector2 _startPosition;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        CollisionLayer = 8;
        CollisionMask = 1;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Floats) return;

        _time += (float)delta;
        Vector2 pos = _startPosition;
        pos.Y += Mathf.Sin(_time * 2.0f) * 5.0f;
        GlobalPosition = pos;
    }

    public void Collect()
    {
        QueueFree();
    }
}
