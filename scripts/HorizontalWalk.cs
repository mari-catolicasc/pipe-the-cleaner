using Godot;

public partial class HorizontalWalk : Area2D
{
    [Export] public float Speed = 80.0f;
    [Export] public float PatrolDistance = 150.0f;

    private Vector2 _startPosition;
    private float _direction = 1.0f;
    private AnimatedSprite2D _animatedSprite;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animatedSprite.Play("walk");

        // Garante que o caranguejo cause dano ao Pipe
        AddToGroup("enemy");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 position = GlobalPosition;
        position.X += Speed * _direction * (float)delta;

        // Inverte direção ao atingir o limite da patrulha
        if (Mathf.Abs(position.X - _startPosition.X) >= PatrolDistance)
        {
            _direction *= -1;
            _animatedSprite.FlipH = _direction < 0;
        }

        GlobalPosition = position;
    }
}
