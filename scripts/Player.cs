using Godot;
using System;

public partial class Player : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float SwimGravity = 80.0f;
    private AnimatedSprite2D _animatedSprite;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Vector2 velocity = Velocity;

        // Movimentação horizontal
        velocity.X = direction.X * Speed;

        // Movimentação vertical
        if (direction.Y != 0)
        {
            velocity.Y = direction.Y * Speed;
        }
        else
        {
            // Apply slow downward drift when not pressing up/down
            velocity.Y += SwimGravity * (float)delta;
        }

        velocity *= 0.98f;

        Velocity = velocity;

        DefineAnimation(direction);
        MoveAndSlide();
    }

    public void DefineAnimation(Vector2 direction)
    {
        if (direction == Vector2.Zero)
        {
			_animatedSprite.Play("idle");
            return;
        }

        if (direction.X < 0) // esquerda
        {
            if (direction.Y < 0) _animatedSprite.Play("left-up");
            else if (direction.Y > 0) _animatedSprite.Play("left-down");
            else _animatedSprite.Play("left");
        }
        else if (direction.X > 0) // direita
        {
            if (direction.Y < 0) _animatedSprite.Play("right-up");
            else if (direction.Y > 0) _animatedSprite.Play("right-down");
            else _animatedSprite.Play("right");
        }
        else // Cima e baixo - mesma animação
        {
            _animatedSprite.Play("up");
        }
    }
}