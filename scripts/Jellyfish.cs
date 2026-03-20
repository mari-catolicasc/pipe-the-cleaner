using Godot;

public partial class Jellyfish : Area2D
{
    [Export] public float Speed = 40.0f;
    [Export] public float FloatRange = 100.0f;

    private Vector2 _startPosition;
    private float _time = 0.0f;
    private Sprite2D _sprite;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        _sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        _time += (float)delta;

        // Movimento senoidal vertical (flutuando)
        Vector2 position = _startPosition;
        position.Y += Mathf.Sin(_time * Speed * 0.05f) * FloatRange;

        // Leve movimento horizontal
        position.X += Mathf.Sin(_time * Speed * 0.02f) * (FloatRange * 0.3f);

        GlobalPosition = position;
    }
}
