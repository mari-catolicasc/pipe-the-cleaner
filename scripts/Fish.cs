using Godot;

public partial class Fish : Area2D
{
    [Export] public float Speed = 100.0f;
    [Export] public float PatrolDistance = 200.0f;

    private Vector2 _startPosition;
    private float _direction = 1.0f;
    private Sprite2D _sprite;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        _sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 position = GlobalPosition;
        position.X += Speed * _direction * (float)delta;

        // Inverte direção ao atingir limite do patrol
        if (Mathf.Abs(position.X - _startPosition.X) >= PatrolDistance)
        {
            _direction *= -1;
            _sprite.FlipH = _direction < 0;
        }

        GlobalPosition = position;
    }
}
