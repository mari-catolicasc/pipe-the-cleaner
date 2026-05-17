using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float SwimGravity = 80.0f;

    [Export] public int MaxHealth = 3;
    [Export] public int MaxInventory = 3;

    private AnimatedSprite2D _animatedSprite;
    private Area2D _hurtbox;
    private HUD _hud;

    private int _currentHealth;

    private int _trashCollected = 0;
    private int _trashDisposed = 0;
    private int _trashTotal = 0;

    private bool _isInvincible = false;
    private float _invincibleTimer = 0.0f;
    private const float InvincibleDuration = 1.5f;

    private readonly Dictionary<Trash.TrashType, int> _inventory = new();
    private Trash.TrashType? _selectedType = null;

    private bool _levelCompleted = false;
    private bool _controlsEnabled = true;

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
        _hud = GetTree().Root.FindChild("HUD", true, false) as HUD;

        if (_hud == null)
        {
            GD.PrintErr("HUD não encontrado.");
            return;
        }

        _hud.UpdateHearts(_currentHealth, MaxHealth);
        _hud.UpdateTrash(_trashDisposed, _trashTotal);
        _hud.UpdateInventory(_inventory, MaxInventory);
    }

    private void CollectTrash(Trash trash)
    {
        var totalItems = _inventory.Values.Sum();

        if (totalItems >= MaxInventory)
        {
            GD.Print("Inventário cheio.");
            return;
        }

        if (!_inventory.ContainsKey(trash.Type))
            _inventory[trash.Type] = 0;

        _inventory[trash.Type]++;
        _trashCollected++;

        trash.Collect();

        _hud?.UpdateInventory(_inventory, MaxInventory);

        if (_selectedType == null)
        {
            _selectedType = trash.Type;
            _hud?.UpdateSelection(_selectedType);
        }

        GD.Print($"Coletado: {trash.Type}");
    }

    public void OnTrashDisposed(Trash.TrashType type)
    {
        _trashDisposed++;

        _hud?.UpdateTrash(_trashDisposed, _trashTotal);

        GD.Print($"Descartado: {type} ({_trashDisposed}/{_trashTotal})");

        // Notify GameManager that one trash item was disposed for this level
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyTrashCollected();
    }

    public List<Trash.TrashType> GetInventory()
    {
        var list = new List<Trash.TrashType>();

        foreach (var kv in _inventory)
        {
            for (int i = 0; i < kv.Value; i++)
                list.Add(kv.Key);
        }

        return list;
    }

    public void ClearInventory()
    {
        _inventory.Clear();
        _hud?.UpdateInventory(_inventory, MaxInventory);
    }

    private void CountTrash()
    {
        _trashTotal = GetTree().GetNodesInGroup("trash")
            .Where(n => n is Trash && n.IsInsideTree())
            .Count();

        _hud?.UpdateTrash(_trashDisposed, _trashTotal);

        // Inform GameManager about total trash for this level
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetTotalTrash(_trashTotal);
        }

        if (_trashTotal == 0)
        {
            CheckLevelCompletion();
        }
    }

    private void SelectTrash(Trash.TrashType type)
    {
        if (!_inventory.TryGetValue(type, out var count) || count <= 0)
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

        var type = _selectedType.Value;

        if (!_inventory.TryGetValue(type, out var count) || count <= 0)
            return;

        if (count <= 1)
        {
            _inventory.Remove(type);
            _selectedType = null;
        }
        else
        {
            _inventory[type] = count - 1;
        }

        if (!_inventory.ContainsKey(type))
            _selectedType = null;

        _hud?.UpdateInventory(_inventory, MaxInventory);
        _hud?.UpdateSelection(_selectedType);
    }

    public bool RemoveOneTrash(Trash.TrashType type)
    {
        if (!_inventory.TryGetValue(type, out var count) || count <= 0)
            return false;

        if (count <= 1)
        {
            _inventory.Remove(type);
            _selectedType = null;
        }
        else
        {
            _inventory[type] = count - 1;
        }

        if (!_inventory.ContainsKey(type))
            _selectedType = null;

        _hud?.UpdateInventory(_inventory, MaxInventory);
        _hud?.UpdateSelection(_selectedType);

        return true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_controlsEnabled)
            return;

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
        // Coletar lixo
        if (area is Trash areaTrash)
        {
            CollectTrash(areaTrash);
            return;
        }

        Node current = area;
        while (current != null && !(current is Trash))
            current = current.GetParent();

        if (current is Trash parentTrash)
        {
            CollectTrash(parentTrash);
            return;
        }

        if (_isInvincible)
            return;

        // Dano
        if (area.IsInGroup("enemy") || area.GetParent()?.IsInGroup("enemy") == true)
            TakeDamage();
    }

    private void CheckLevelCompletion()
    {
        if (_levelCompleted)
            return;

        if (_trashDisposed >= _trashTotal)
        {
            _levelCompleted = true;

            GD.Print("Todo o lixo foi descartado corretamente!");

            var pieces = GetTree().GetNodesInGroup("machine_piece");

            GD.Print($"Pedaços de máquina encontrados: {pieces.Count}");

            foreach (Node node in pieces)
            {
                if (node is MachinePiece piece)
                {
                    piece.CallDeferred(nameof(MachinePiece.Activate));
                }
            }
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
            Die();
    }

    private void Die()
    {
        GD.Print("Pipe morreu!");
        GetTree().CallDeferred("reload_current_scene");
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