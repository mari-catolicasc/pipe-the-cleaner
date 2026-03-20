using Godot;

public partial class Crab : Area2D
{
    [Export] public float Speed = 60.0f;
    [Export] public float PatrolDistance = 150.0f;

    private Vector2 _startPosition;
    private float _direction = 1.0f;
    private AnimatedSprite2D _animatedSprite;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animatedSprite.Play("walk");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 position = GlobalPosition;
        position.X += Speed * _direction * (float)delta;

        if (Mathf.Abs(position.X - _startPosition.X) >= PatrolDistance)
        {
            _direction *= -1;
            _animatedSprite.FlipH = _direction < 0;
        }

        GlobalPosition = position;
    }
}
