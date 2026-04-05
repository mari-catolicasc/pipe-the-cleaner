using Godot;
using System;

public partial class Player : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float SwimGravity = 80.0f;

    [Export] public int MaxHealth = 3;

    private AnimatedSprite2D _animatedSprite;
    private Area2D _hurtbox;
    private HUD _hud;
    private int _currentHealth;
    private int _trashCollected = 0;
    private int _trashTotal = 0;
    private bool _isInvincible = false;
    private float _invincibleTimer = 0.0f;
    private const float InvincibleDuration = 1.5f;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _hurtbox = GetNode<Area2D>("Hurtbox");
        _hurtbox.AreaEntered += OnHurtboxAreaEntered;
        _currentHealth = MaxHealth;

        // Busca o HUD na árvore de cena
        _hud = GetTree().Root.GetNodeOrNull<HUD>("Main/HUD");
        _hud?.UpdateHearts(_currentHealth, MaxHealth);

        // Conta total de lixos após todos os nós serem carregados
        CallDeferred(MethodName.CountTrash);
    }

    private void CountTrash()
    {
        _trashTotal = GetTree().GetNodesInGroup("trash").Count;
        _hud?.UpdateTrash(_trashCollected, _trashTotal);
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
            velocity.Y *= 0.98f;
        }

        Velocity = velocity;

        DefineAnimation(direction);
        MoveAndSlide();

        // Timer de invencibilidade
        if (_isInvincible)
        {
            _invincibleTimer -= (float)delta;
            // Piscar o sprite durante invencibilidade
            _animatedSprite.Visible = ((int)(_invincibleTimer * 10) % 2) == 0;

            if (_invincibleTimer <= 0)
            {
                _isInvincible = false;
                _animatedSprite.Visible = true;
            }
        }
    }

    private void OnHurtboxAreaEntered(Area2D area)
    {
        // Coleta lixo (layer 8)
        if (area is Trash trash)
        {
            CollectTrash(trash);
            return;
        }

        if (_isInvincible) return;

        // Criaturas estão na collision layer 2
        if (area.CollisionLayer == 2)
        {
            TakeDamage();
        }
    }

    private void CollectTrash(Trash trash)
    {
        _trashCollected++;
        trash.Collect();
        _hud?.UpdateTrash(_trashCollected, _trashTotal);
        GD.Print($"Lixo coletado! {_trashCollected}/{_trashTotal}");
    }

    private void TakeDamage()
    {
        _currentHealth--;
        _isInvincible = true;
        _invincibleTimer = InvincibleDuration;

        // Knockback - empurra o player para trás
        Vector2 knockback = Velocity.Normalized() * -200;
        Velocity = knockback;

        // Atualiza os corações no HUD
        _hud?.UpdateHearts(_currentHealth, MaxHealth);

        GD.Print($"Pipe levou dano! Vida: {_currentHealth}/{MaxHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GD.Print("Pipe morreu!");
        // Reinicia a cena
        GetTree().ReloadCurrentScene();
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
