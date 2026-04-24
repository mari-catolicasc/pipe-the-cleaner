using Godot;

public partial class MachinePiece : Area2D
{
    [Export] public string PartId = "part_1";
    [Export] public string NextScenePath;

    private CollisionShape2D _collision;
    private bool _isActive = false;
    private bool _wasCollected = false;

    public override void _Ready()
    {
        AddToGroup("machine_piece");

        _collision = GetNode<CollisionShape2D>("CollisionShape2D");

        AreaEntered += OnAreaEntered;

        Visible = false;
        _collision.Disabled = true;
        Monitoring = false;
        Monitorable = false;

        GD.Print("MachinePiece Ready!");

        CallDeferred(nameof(DeferredInit));
    }

    private void DeferredInit()
    {
        if (GameManager.Instance == null)
            return;

        // Initially the piece should be invisible/disabled until all trash collected
        Visible = false;
        _collision.Disabled = true;
        Monitoring = false;
        Monitorable = false;

        // If part already collected in saved state, remove it
        if (GameManager.Instance.HasPart(PartId))
        {
            _wasCollected = true;
            QueueFree();
            return;
        }

        // Subscribe to notification when all trash is collected
        GameManager.Instance.OnAllTrashCollected += OnAllTrashCollected;
    }

    public void Activate()
    {
        if (_isActive)
            return;

        _isActive = true;

        GD.Print("MachinePiece ACTIVATED!");

        Visible = true;
        ZIndex = 100;

        _collision.Disabled = false;
        Monitoring = true;
        Monitorable = true;
    }

    private void OnAllTrashCollected()
    {
        // Make piece available to be collected
        if (_wasCollected)
            return;

        GD.Print($"MachinePiece {PartId} is now enabled because all trash collected.");
        Activate();
    }

    public override void _ExitTree()
    {
        // Unsubscribe to avoid keeping a dangling reference in GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.OnAllTrashCollected -= OnAllTrashCollected;

        base._ExitTree();
    }

    private void OnAreaEntered(Area2D area)
    {
        if (!_isActive)
            return;

        Node current = area;

        while (current != null && !(current is Player))
            current = current.GetParent();

        if (current is Player)
        {
            CallDeferred(nameof(OnCollected));
        }
    }

    private void OnCollected()
    {
        GD.Print("MachinePiece COLLECTED!");

        GameManager.Instance.AddPart(PartId);

        QueueFree();

        GameManager.Instance.AddPart(PartId);
        GameManager.Instance.EndLevel(NextScenePath);
        QueueFree();
    }
}