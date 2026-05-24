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

    // Ordered slots: each entry is one piece of trash the pipe is carrying.
    // Slot index in the list maps 1:1 to the visual slot in the HUD.
    private readonly List<Trash.TrashType> _inventorySlots = new();
    private int _selectedSlotIndex = -1;

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
        RefreshInventoryHUD();
        _hud.UpdatePoints(GameManager.Instance?.CurrentPoints ?? 0);
    }

    private void RefreshInventoryHUD()
    {
        _hud?.UpdateInventory(_inventorySlots, _selectedSlotIndex, MaxInventory);
    }

    private void CollectTrash(Trash trash)
    {
        if (_inventorySlots.Count >= MaxInventory)
        {
            GD.Print("Inventário cheio.");
            return;
        }

        _inventorySlots.Add(trash.Type);
        _trashCollected++;

        if (_selectedSlotIndex < 0)
            _selectedSlotIndex = _inventorySlots.Count - 1;

        trash.Collect();

        RefreshInventoryHUD();

        GD.Print($"Coletado: {trash.Type} (slot {_inventorySlots.Count - 1})");
    }

    public void OnTrashDisposed(Trash.TrashType type, bool correct)
    {
        _trashDisposed++;

        int delta = correct ? GameManager.CorrectDiscardPoints : GameManager.IncorrectDiscardPoints;

        if (GameManager.Instance != null)
            GameManager.Instance.AddPoints(delta);

        _hud?.UpdateTrash(_trashDisposed, _trashTotal);
        _hud?.UpdatePoints(GameManager.Instance?.CurrentPoints ?? 0);

        SpawnFloatingPoints(delta, correct);

        GD.Print($"Descartado: {type} ({_trashDisposed}/{_trashTotal}) - {(correct ? "correto" : "incorreto")}");

        if (GameManager.Instance != null)
            GameManager.Instance.NotifyTrashCollected();
    }

    private void SpawnFloatingPoints(int delta, bool correct)
    {
        var label = new Label();
        label.Text = (delta >= 0 ? "+" : "") + delta;
        label.AddThemeFontSizeOverride("font_size", 28);
        label.AddThemeColorOverride("font_color", correct
            ? new Color(0.3f, 1f, 0.4f, 1f)
            : new Color(1f, 0.35f, 0.35f, 1f));
        label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f));
        label.AddThemeConstantOverride("shadow_offset_x", 2);
        label.AddThemeConstantOverride("shadow_offset_y", 2);
        label.ZIndex = 100;
        label.TopLevel = true;
        label.Position = GlobalPosition + new Vector2(-20, -55);
        label.MouseFilter = Control.MouseFilterEnum.Ignore;

        AddChild(label);

        var tween = label.CreateTween().SetParallel();
        tween.TweenProperty(label, "position", label.Position + new Vector2(0, -50), 0.9f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(label, "modulate:a", 0.0f, 0.9f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
        tween.Chain().TweenCallback(Callable.From(label.QueueFree));
    }

    public List<Trash.TrashType> GetInventory()
    {
        return new List<Trash.TrashType>(_inventorySlots);
    }

    public void ClearInventory()
    {
        _inventorySlots.Clear();
        _selectedSlotIndex = -1;
        RefreshInventoryHUD();
    }

    private void CountTrash()
    {
        _trashTotal = GetTree().GetNodesInGroup("trash")
            .Where(n => n is Trash && n.IsInsideTree())
            .Count();

        _hud?.UpdateTrash(_trashDisposed, _trashTotal);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetTotalTrash(_trashTotal);
        }

        if (_trashTotal == 0)
        {
            CheckLevelCompletion();
        }
    }

    private void CycleSelectedTrash()
    {
        if (_inventorySlots.Count == 0)
        {
            _selectedSlotIndex = -1;
            RefreshInventoryHUD();
            GD.Print("Inventário vazio.");
            return;
        }

        int start = _selectedSlotIndex < 0 ? -1 : _selectedSlotIndex;
        _selectedSlotIndex = (start + 1) % _inventorySlots.Count;
        RefreshInventoryHUD();
        GD.Print($"Selecionado: slot {_selectedSlotIndex} ({_inventorySlots[_selectedSlotIndex]})");
    }

    public Trash.TrashType? GetSelectedTrash()
    {
        if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _inventorySlots.Count)
            return null;
        return _inventorySlots[_selectedSlotIndex];
    }

    public bool DepositSelectedTrash(out Trash.TrashType depositedType)
    {
        depositedType = default;

        if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _inventorySlots.Count)
            return false;

        depositedType = _inventorySlots[_selectedSlotIndex];
        _inventorySlots.RemoveAt(_selectedSlotIndex);

        if (_inventorySlots.Count == 0)
            _selectedSlotIndex = -1;
        else if (_selectedSlotIndex >= _inventorySlots.Count)
            _selectedSlotIndex = _inventorySlots.Count - 1;

        RefreshInventoryHUD();
        return true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_controlsEnabled)
            return;

        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
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

        if (Input.IsActionJustPressed("cycle_inventory"))
            CycleSelectedTrash();

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
