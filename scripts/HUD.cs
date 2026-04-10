using Godot;
using System.Collections.Generic;

public partial class HUD : CanvasLayer
{
    private HBoxContainer _heartsContainer;
    private Label _trashLabel;

    private static ImageTexture _heartTexture;
    private static ImageTexture _trashIconTexture;

    [Export] public Texture2D PlasticIconTexture;
    [Export] public Texture2D PaperIconTexture;
    [Export] public Texture2D GlassIconTexture;
    [Export] public Texture2D MetalIconTexture;
    [Export] public Texture2D OrganicIconTexture;

    private TextureRect _plasticIconNode;
    private Label _plasticCount;

    private TextureRect _paperIconNode;
    private Label _paperCount;

    private TextureRect _glassIconNode;
    private Label _glassCount;

    private TextureRect _metalIconNode;
    private Label _metalCount;

    private TextureRect _organicIconNode;
    private Label _organicCount;

    public override void _Ready()
    {
        _heartsContainer = GetNodeOrNull<HBoxContainer>("HeartsContainer");
        _trashLabel = GetNodeOrNull<Label>("TrashContainer/TrashLabel");

        _plasticIconNode = GetNode<TextureRect>("Control/InventoryContainer/PlasticSlot/Icon");
        _plasticCount    = GetNode<Label>("Control/InventoryContainer/PlasticSlot/Count");

        _paperIconNode = GetNode<TextureRect>("Control/InventoryContainer/PaperSlot/Icon");
        _paperCount    = GetNode<Label>("Control/InventoryContainer/PaperSlot/Count");

        _glassIconNode = GetNode<TextureRect>("Control/InventoryContainer/GlassSlot/Icon");
        _glassCount    = GetNode<Label>("Control/InventoryContainer/GlassSlot/Count");

        _metalIconNode = GetNode<TextureRect>("Control/InventoryContainer/MetalSlot/Icon");
        _metalCount    = GetNode<Label>("Control/InventoryContainer/MetalSlot/Count");

        _organicIconNode = GetNode<TextureRect>("Control/InventoryContainer/OrganicSlot/Icon");
        _organicCount    = GetNode<Label>("Control/InventoryContainer/OrganicSlot/Count");

        if (_heartsContainer == null || _trashLabel == null ||
            _plasticIconNode == null || _plasticCount == null ||
            _paperIconNode == null || _paperCount == null ||
            _glassIconNode == null || _glassCount == null ||
            _metalIconNode == null || _metalCount == null ||
            _organicIconNode == null || _organicCount == null)
        {
            GD.PrintErr("HUD: estrutura da cena incorreta. Verifique os caminhos dos nós.");
            return;
        }

        ApplyInventoryIcons();

        CreateHeartTexture();
        CreateTrashIconTexture();
        ApplyHeartTextures();
        ApplyTrashIcon();

        UpdateInventory(new List<Trash.TrashType>());
    }

    private void ApplyInventoryIcons()
    {
        _plasticIconNode.Texture = PlasticIconTexture ?? GD.Load<Texture2D>("res://assets/trash/plastic/plastic_bag_red.png");
        _paperIconNode.Texture   = PaperIconTexture   ?? GD.Load<Texture2D>("res://assets/trash/paper/newspaper_blue.png");
        _glassIconNode.Texture   = GlassIconTexture   ?? GD.Load<Texture2D>("res://assets/trash/glass/perfume_bottle_green.png");
        _metalIconNode.Texture   = MetalIconTexture   ?? GD.Load<Texture2D>("res://assets/trash/metal/soda_can_yellow.png");
        _organicIconNode.Texture = OrganicIconTexture ?? GD.Load<Texture2D>("res://assets/trash/organic/apple_brown.png");
    }

    public void UpdateInventory(List<Trash.TrashType> inventory)
    {
        int plastic = 0;
        int paper = 0;
        int glass = 0;
        int metal = 0;
        int organic = 0;

        foreach (var item in inventory)
        {
            switch (item)
            {
                case Trash.TrashType.Plastic: plastic++; break;
                case Trash.TrashType.Paper: paper++; break;
                case Trash.TrashType.Glass: glass++; break;
                case Trash.TrashType.Metal: metal++; break;
                case Trash.TrashType.Organic: organic++; break;
            }
        }

        SetSlot(_plasticIconNode, _plasticCount, plastic);
        SetSlot(_paperIconNode, _paperCount, paper);
        SetSlot(_glassIconNode, _glassCount, glass);
        SetSlot(_metalIconNode, _metalCount, metal);
        SetSlot(_organicIconNode, _organicCount, organic);
    }

    public void UpdateSelection(Trash.TrashType? selected)
    {
        HighlightSlot(_plasticIconNode, selected == Trash.TrashType.Plastic);
        HighlightSlot(_paperIconNode, selected == Trash.TrashType.Paper);
        HighlightSlot(_glassIconNode, selected == Trash.TrashType.Glass);
        HighlightSlot(_metalIconNode, selected == Trash.TrashType.Metal);
        HighlightSlot(_organicIconNode, selected == Trash.TrashType.Organic);
    }

    private void HighlightSlot(TextureRect icon, bool selected)
    {
        if (icon == null) return;

        icon.Scale = selected ? new Vector2(1.3f, 1.3f) : new Vector2(1f, 1f);
        icon.Modulate = selected ? Colors.White : new Color(1,1,1,0.5f);
    }

    private static void SetSlot(TextureRect icon, Label countLabel, int value)
    {
        if (icon != null)
        {
            icon.Modulate = value > 0 ? Colors.White : new Color(1, 1, 1, 0.35f);
        }

        if (countLabel != null)
        {
            countLabel.Text = value.ToString();
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
            GD.PrintErr($"Heart pattern size inválido: {trashPattern.Length}, esperado: {size * size}");
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

    public void UpdateTrash(int collected, int total)
    {
        _trashLabel.Text = $"{collected} / {total}";
    }
}