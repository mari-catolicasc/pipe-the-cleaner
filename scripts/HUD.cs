using Godot;
using System.Collections.Generic;

public partial class HUD : CanvasLayer
{
    private HBoxContainer _heartsContainer;
    private Label _trashLabel;
    private Label _pointsLabel;

    private Panel _levelCompletePanel;
    private Label _levelCompletePoints;
    private Label _star1;
    private Label _star2;
    private Label _star3;
    private Button _nextButton;

    private static ImageTexture _heartTexture;
    private static ImageTexture _trashIconTexture;

    [Export] public Texture2D PlasticIconTexture;
    [Export] public Texture2D PaperIconTexture;
    [Export] public Texture2D GlassIconTexture;
    [Export] public Texture2D MetalIconTexture;
    [Export] public Texture2D OrganicIconTexture;

    [Export] public Texture2D SlotFrameEmptyTexture;

    private const int InventorySlotCount = 3;

    private struct InventorySlotNodes
    {
        public TextureRect Frame;
        public TextureRect Icon;
        public Control SelectionOverlay;
    }

    private readonly InventorySlotNodes[] _slots = new InventorySlotNodes[InventorySlotCount];
    private readonly Dictionary<Trash.TrashType, Texture2D> _iconByType = new();

    public override void _Ready()
    {
        _heartsContainer = GetNodeOrNull<HBoxContainer>("HeartsContainer");
        _trashLabel = GetNodeOrNull<Label>("TrashContainer/TrashLabel");
        _pointsLabel = GetNodeOrNull<Label>("PointsContainer/PointsLabel");

        _levelCompletePanel = GetNodeOrNull<Panel>("LevelCompletePanel");
        _levelCompletePoints = GetNodeOrNull<Label>("LevelCompletePanel/VBox/PointsResult");
        _star1 = GetNodeOrNull<Label>("LevelCompletePanel/VBox/StarsContainer/Star1");
        _star2 = GetNodeOrNull<Label>("LevelCompletePanel/VBox/StarsContainer/Star2");
        _star3 = GetNodeOrNull<Label>("LevelCompletePanel/VBox/StarsContainer/Star3");
        _nextButton = GetNodeOrNull<Button>("LevelCompletePanel/VBox/NextButton");

        if (_levelCompletePanel != null)
            _levelCompletePanel.Visible = false;

        if (_nextButton != null)
            _nextButton.Pressed += OnNextLevelPressed;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelEnded += OnLevelEnded;
            GameManager.Instance.OnPointsChanged += UpdatePoints;
        }

        for (int i = 0; i < InventorySlotCount; i++)
        {
            int slotNumber = i + 1;
            _slots[i] = new InventorySlotNodes
            {
                Frame = GetNodeOrNull<TextureRect>($"InventoryContainer/Slot{slotNumber}/Frame"),
                Icon = GetNodeOrNull<TextureRect>($"InventoryContainer/Slot{slotNumber}/Icon"),
                SelectionOverlay = GetNodeOrNull<Control>($"InventoryContainer/Slot{slotNumber}/SelectionOverlay"),
            };
        }

        BuildIconLookup();

        if (_heartsContainer == null || _trashLabel == null ||
            _slots[0].Frame == null || _slots[1].Frame == null || _slots[2].Frame == null)
        {
            GD.PrintErr("HUD: estrutura da cena incorreta. Verifique os caminhos dos nós.");
            return;
        }

        CreateHeartTexture();
        CreateTrashIconTexture();
        ApplyHeartTextures();
        ApplyTrashIcon();

        UpdateInventory(new List<Trash.TrashType>(), -1, InventorySlotCount);
    }

    private void BuildIconLookup()
    {
        _iconByType[Trash.TrashType.Plastic] = PlasticIconTexture
            ?? GD.Load<Texture2D>("res://assets/trash/plastic/plastic_bag_red.png");
        _iconByType[Trash.TrashType.Paper] = PaperIconTexture
            ?? GD.Load<Texture2D>("res://assets/trash/paper/newspaper_blue.png");
        _iconByType[Trash.TrashType.Glass] = GlassIconTexture
            ?? GD.Load<Texture2D>("res://assets/trash/glass/glass_bottle_green.png");
        _iconByType[Trash.TrashType.Metal] = MetalIconTexture
            ?? GD.Load<Texture2D>("res://assets/trash/metal/soda_can_yellow.png");
        _iconByType[Trash.TrashType.Organic] = OrganicIconTexture
            ?? GD.Load<Texture2D>("res://assets/trash/organic/apple_brown.png");
    }

    public void UpdateInventory(IReadOnlyList<Trash.TrashType> slots, int selectedIndex, int maxCapacity = 3)
    {
        var emptyFrame = SlotFrameEmptyTexture
            ?? GD.Load<Texture2D>("res://assets/ui/inventory_slot_empty.png");

        for (int i = 0; i < InventorySlotCount; i++)
        {
            bool hasItem = slots != null && i < slots.Count;
            bool isSelected = hasItem && i == selectedIndex;

            var frame = _slots[i].Frame;
            if (frame != null)
            {
                frame.Texture = emptyFrame;
                frame.Modulate = hasItem ? Colors.White : new Color(1, 1, 1, 0.55f);
            }

            var icon = _slots[i].Icon;
            if (icon != null)
            {
                if (hasItem && _iconByType.TryGetValue(slots[i], out var tex))
                {
                    icon.Texture = tex;
                    icon.Visible = true;
                    icon.Modulate = isSelected
                        ? new Color(1f, 1f, 0.85f, 1f)
                        : new Color(1f, 1f, 1f, 0.9f);
                    icon.Scale = isSelected ? new Vector2(1.08f, 1.08f) : Vector2.One;
                }
                else
                {
                    icon.Texture = null;
                    icon.Visible = false;
                }
            }

            var overlay = _slots[i].SelectionOverlay;
            if (overlay != null)
                overlay.Visible = isSelected;
        }
    }

    private void CreateHeartTexture()
    {
        if (_heartTexture != null) return;

        int size = 32;
        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);

        var red = new Color(1, 0.15f, 0.2f, 1);
        var darkRed = new Color(0.7f, 0.1f, 0.15f, 1);
        var transparent = new Color(0, 0, 0, 0);

        image.Fill(transparent);

        int[] heartPattern = {
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,
            0,0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0,
            0,0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,
            0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,
            0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,
            0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,
            0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,
            0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,
            0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,
            0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        };

        if (heartPattern.Length != size * size)
        {
            GD.PrintErr($"Heart pattern size inválido: {heartPattern.Length}, esperado: {size * size}");
            return;
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (heartPattern[y * size + x] == 1)
                {
                    bool isBorder = false;
                    if (x > 0 && heartPattern[y * size + (x - 1)] == 0) isBorder = true;
                    if (x < size - 1 && heartPattern[y * size + (x + 1)] == 0) isBorder = true;
                    if (y > 0 && heartPattern[(y - 1) * size + x] == 0) isBorder = true;
                    if (y < size - 1 && heartPattern[(y + 1) * size + x] == 0) isBorder = true;

                    image.SetPixel(x, y, isBorder ? darkRed : red);
                }
            }
        }

        _heartTexture = ImageTexture.CreateFromImage(image);
    }

    private void CreateTrashIconTexture()
    {
        if (_trashIconTexture != null) return;

        int size = 32;
        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        var green = new Color(0.3f, 0.75f, 0.3f, 1);
        var darkGreen = new Color(0.2f, 0.5f, 0.2f, 1);
        var transparent = new Color(0, 0, 0, 0);

        image.Fill(transparent);

        int[] trashPattern = {
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        };

        if (trashPattern.Length != size * size)
        {
            GD.PrintErr($"Trash pattern size inválido: {trashPattern.Length}, esperado: {size * size}");
            return;
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (trashPattern[y * size + x] == 1)
                {
                    bool isBorder = false;
                    if (x > 0 && trashPattern[y * size + (x - 1)] == 0) isBorder = true;
                    if (x < size - 1 && trashPattern[y * size + (x + 1)] == 0) isBorder = true;
                    if (y > 0 && trashPattern[(y - 1) * size + x] == 0) isBorder = true;
                    if (y < size - 1 && trashPattern[(y + 1) * size + x] == 0) isBorder = true;

                    image.SetPixel(x, y, isBorder ? darkGreen : green);
                }
            }
        }

        _trashIconTexture = ImageTexture.CreateFromImage(image);
    }

    private void ApplyHeartTextures()
    {
        for (int i = 0; i < _heartsContainer.GetChildCount(); i++)
        {
            var heart = _heartsContainer.GetChild<TextureRect>(i);
            heart.Texture = _heartTexture;
        }
    }

    private void ApplyTrashIcon()
    {
        var icon = GetNodeOrNull<TextureRect>("TrashContainer/TrashIcon");
        if (icon != null)
        {
            icon.Texture = _trashIconTexture;
        }
    }

    public void UpdateHearts(int currentHealth, int maxHealth)
    {
        for (int i = 0; i < _heartsContainer.GetChildCount(); i++)
        {
            var heart = _heartsContainer.GetChild<TextureRect>(i);
            heart.Modulate = i < currentHealth
                ? new Color(1, 1, 1, 1)
                : new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
    }

    public void UpdateTrash(int discarded, int total)
    {
        _trashLabel.Text = $"{discarded} / {total}";
    }

    public void UpdatePoints(int points)
    {
        if (_pointsLabel != null)
            _pointsLabel.Text = $"Pontos: {points}";
    }

    private void OnLevelEnded(int points, int maxPoints, int stars)
    {
        if (_levelCompletePanel == null)
            return;

        if (_levelCompletePoints != null)
            _levelCompletePoints.Text = maxPoints > 0
                ? $"Pontuação: {points} / {maxPoints}"
                : $"Pontuação: {points}";

        ResetStar(_star1);
        ResetStar(_star2);
        ResetStar(_star3);

        _levelCompletePanel.Visible = true;
        GetTree().Paused = true;

        AnimateStar(_star1, stars >= 1, 0.10f);
        AnimateStar(_star2, stars >= 2, 0.35f);
        AnimateStar(_star3, stars >= 3, 0.60f);
    }

    private static void ResetStar(Label star)
    {
        if (star == null) return;
        star.Text = "☆";
        star.Modulate = new Color(0.4f, 0.4f, 0.45f, 1f);
        star.Scale = new Vector2(0.1f, 0.1f);
    }

    private void AnimateStar(Label star, bool earned, float delay)
    {
        if (star == null) return;

        var gold = new Color(1f, 0.85f, 0.2f, 1f);
        var dim = new Color(0.35f, 0.35f, 0.4f, 1f);

        var tween = star.CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle);
        tween.TweenInterval(delay);

        tween.TweenCallback(Callable.From(() =>
        {
            star.Text = earned ? "★" : "☆";
            star.Modulate = earned ? gold : dim;
        }));

        tween.TweenProperty(star, "scale", new Vector2(1.4f, 1.4f), 0.18f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(star, "scale", Vector2.One, 0.12f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
    }

    private void OnNextLevelPressed()
    {
        if (_levelCompletePanel != null)
            _levelCompletePanel.Visible = false;

        GetTree().Paused = false;
        GameManager.Instance?.AdvanceToNextLevel();
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelEnded -= OnLevelEnded;
            GameManager.Instance.OnPointsChanged -= UpdatePoints;
        }

        base._ExitTree();
    }
}
