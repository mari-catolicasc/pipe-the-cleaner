using Godot;
using System.Collections.Generic;

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
    private readonly List<Trash.TrashType> _inventory = new();
    private Trash.TrashType? _selectedType = null;
    [Export] public int MaxInventory = 3;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _hurtbox = GetNode<Area2D>("Hurtbox");
        _hurtbox.AreaEntered += OnHurtboxAreaEntered;

        _currentHealth = MaxHealth;

        CallDeferred(nameof(InitHUD));
        CallDeferred(nameof(CountTrash));
    }

    private void InitHUD()
    {
        _hud = GetTree().CurrentScene.GetNodeOrNull<HUD>("HUD");
        if (_hud == null)
        {
            _hud = GetTree().Root.GetNodeOrNull<HUD>("Main/HUD");
        }

        if (_hud == null)
        {
            GD.PrintErr("HUD não encontrado.");
            return;
        }

        _hud.UpdateHearts(_currentHealth, MaxHealth);
        _hud.UpdateTrash(_trashCollected, _trashTotal);
        _hud.UpdateInventory(_inventory);
    }

    private void CollectTrash(Trash trash)
    {
        if (_inventory.Count >= MaxInventory)
        {
            GD.Print("Inventário cheio.");
            return;
        }

        _inventory.Add(trash.Type);
        _trashCollected++;

        trash.Collect();

        _hud?.UpdateTrash(_trashCollected, _trashTotal);
        _hud?.UpdateInventory(_inventory);

        if (_selectedType == null)
        {
            _selectedType = trash.Type;
            _hud?.UpdateSelection(_selectedType);
        }

        GD.Print($"Coletado: {trash.Type}");
    }

    public List<Trash.TrashType> GetInventory()
    {
        return _inventory;
    }

    public void ClearInventory()
    {
        _inventory.Clear();
        _hud?.UpdateInventory(_inventory);
    }

    private void CountTrash()
    {
        int total = 0;

        var trashNodes = GetTree().GetNodesInGroup("trash");
        if (trashNodes.Count > 0)
        {
            total = trashNodes.Count;
        }
        else
        {
            var container = GetTree().CurrentScene.GetNodeOrNull<Node>("TrashItems");
            if (container != null)
            {
                foreach (Node child in container.GetChildren())
                {
                    if (child is Trash)
                        total++;
                }
            }
        }

        _trashTotal = total;
        _hud?.UpdateTrash(_trashCollected, _trashTotal);
    }

    private void SelectTrash(Trash.TrashType type)
    {
        // Só pode selecionar se o player tiver lixo
        if (!_inventory.Contains(type))
        {
            GD.Print($"Não possui {type}");
            return;
        }

        _selectedType = type;

        _hud?.UpdateSelection(_selectedType);
    }

    public Trash.TrashType? GetSelectedTrash()
    {
        return _selectedType;
    }

    public void UseSelectedTrash()
    {
        if (_selectedType == null) return;

        var index = _inventory.IndexOf(_selectedType.Value);

        if (index == -1) return;

        _inventory.RemoveAt(index);

        // If no more of this type, deselect
        if (!_inventory.Contains(_selectedType.Value))
        {
            _selectedType = null;
        }

        _hud?.UpdateInventory(_inventory);
        _hud?.UpdateSelection(_selectedType);
    }

    public bool RemoveOneTrash(Trash.TrashType type)
    {
        int index = _inventory.IndexOf(type);

        if (index == -1)
            return false;

        _inventory.RemoveAt(index);

        // Update selection
        if (!_inventory.Contains(type))
            _selectedType = null;

        _hud?.UpdateInventory(_inventory);
        _hud?.UpdateSelection(_selectedType);

        return true;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Vector2 velocity = Velocity;

        velocity.X = direction.X * Speed;

        if (direction.Y != 0)
        {
            velocity.Y = direction.Y * Speed;
        }
        else
        {
            if (!IsOnFloor())
            {
                velocity.Y += SwimGravity * (float)delta;
                velocity.Y *= 0.98f;
            }
            else
            {
                velocity.Y = 0;
            }
        }
        if (Input.IsActionJustPressed("select_plastic"))
            SelectTrash(Trash.TrashType.Plastic);

        if (Input.IsActionJustPressed("select_paper"))
            SelectTrash(Trash.TrashType.Paper);

        if (Input.IsActionJustPressed("select_glass"))
            SelectTrash(Trash.TrashType.Glass);

        if (Input.IsActionJustPressed("select_metal"))
            SelectTrash(Trash.TrashType.Metal);

        if (Input.IsActionJustPressed("select_organic"))
            SelectTrash(Trash.TrashType.Organic);

        Velocity = velocity;

        DefineAnimation(direction);
        MoveAndSlide();

        if (_isInvincible)
        {
            _invincibleTimer -= (float)delta;
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
        if (area is Trash trash)
        {
            CollectTrash(trash);
            return;
        }

        if (_isInvincible)
            return;

        if (area.IsInGroup("enemy") || area.GetParent()?.IsInGroup("enemy") == true)
        {
            TakeDamage();
        }
    }

    private void TakeDamage()
    {
        _currentHealth--;
        _isInvincible = true;
        _invincibleTimer = InvincibleDuration;

        Vector2 knockback = Velocity.Normalized() * -200;
        Velocity = knockback;

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
        GetTree().ReloadCurrentScene();
    }

    public void DefineAnimation(Vector2 direction)
    {
        if (direction == Vector2.Zero)
        {
            _animatedSprite.Play("idle");
            return;
        }

        if (direction.X < 0)
        {
            if (direction.Y < 0) _animatedSprite.Play("left-up");
            else if (direction.Y > 0) _animatedSprite.Play("left-down");
            else _animatedSprite.Play("left");
        }
        else if (direction.X > 0)
        {
            if (direction.Y < 0) _animatedSprite.Play("right-up");
            else if (direction.Y > 0) _animatedSprite.Play("right-down");
            else _animatedSprite.Play("right");
        }
        else
        {
            _animatedSprite.Play("up");
        }
    }
}