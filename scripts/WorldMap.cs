using Godot;

public partial class WorldMap : Node2D
{
    private struct LevelInfo
    {
        public string Name;
        public string ScenePath;
        public Vector2 Position;

        public LevelInfo(string name, string scenePath, Vector2 position)
        {
            Name = name;
            ScenePath = scenePath;
            Position = position;
        }
    }

    private static readonly LevelInfo[] Levels =
    {
        new("Fase 1 - Atlântico", "res://scenes/levels/level1.tscn", new Vector2(645, 82)),
        new("Fase 2 - Pacífico",  "res://scenes/levels/level2.tscn", new Vector2(511, 329)),
        new("Fase 3 - Ártico",    "res://scenes/levels/level3.tscn", new Vector2(152, 418)),
        new("Fase 4 - Índico",    "res://scenes/levels/level4.tscn", new Vector2(667, 602)),
        new("Fase 5 - Antártico", "res://scenes/levels/level5.tscn", new Vector2(948, 496)),
    };

    private const string MapTexturePath = "res://assets/backgrounds/world_map.jpg";
    private const string LockTexturePath = "res://assets/cadeado_fase.png";
    private const bool DebugClickPositions = true;
    private const float ClickThreshold = 60f;
    private const float HaloRadius = 48f;
    private const float LockSize = 56f;

    private int _currentIndex = 0;
    private Sprite2D _pipe;
    private Polygon2D _halo;
    private Label _levelLabel;
    private TextureRect _mapImage;
    private Tween _moveTween;
    private bool _entering = false;
    private Sprite2D[] _lockIcons;
    private Label[] _starLabels;
    private Label[] _nameLabels;

    public override void _Ready()
    {
        _pipe = GetNode<Sprite2D>("Pipe");
        _levelLabel = GetNode<Label>("UI/LevelLabel");
        _mapImage = GetNode<TextureRect>("MapImage");

        if (ResourceLoader.Exists(MapTexturePath))
        {
            _mapImage.Texture = GD.Load<Texture2D>(MapTexturePath);
        }
        else
        {
            GD.PrintErr($"WorldMap: imagem do mapa não encontrada em {MapTexturePath}.");
        }

        CreateHalo();
        CreateNameLabels();
        CreateLockIcons();
        CreateStarLabels();
        StartAtHighestUnlocked();
        UpdateLockVisibility();
        UpdateStarLabels();
        UpdateLabel();
        StartPipeBob();
        StartHaloPulse();
    }

    private Texture2D LoadLockTextureWithChromaKey()
    {
        if (!ResourceLoader.Exists(LockTexturePath)) return null;

        var src = GD.Load<Texture2D>(LockTexturePath);
        var img = src.GetImage();
        if (img == null) return src;

        if (img.GetFormat() != Image.Format.Rgba8)
            img.Convert(Image.Format.Rgba8);

        int w = img.GetWidth();
        int h = img.GetHeight();

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var c = img.GetPixel(x, y);
                int r = (int)(c.R * 255);
                int g = (int)(c.G * 255);
                int b = (int)(c.B * 255);

                bool grayish = Mathf.Abs(r - g) <= 14 && Mathf.Abs(g - b) <= 14 && Mathf.Abs(r - b) <= 14;
                bool isChecker = grayish && r >= 180;

                if (isChecker)
                    img.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }

        return ImageTexture.CreateFromImage(img);
    }

    private void CreateNameLabels()
    {
        _nameLabels = new Label[Levels.Length];
        const float box = 120f;
        for (int i = 0; i < Levels.Length; i++)
        {
            var lab = new Label();
            lab.Text = $"Fase {i + 1}";
            lab.AddThemeFontSizeOverride("font_size", 18);
            lab.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 1f));
            lab.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.9f));
            lab.AddThemeConstantOverride("shadow_offset_x", 2);
            lab.AddThemeConstantOverride("shadow_offset_y", 2);
            lab.ZIndex = 9;
            lab.Size = new Vector2(box, 24);
            lab.Position = Levels[i].Position - new Vector2(box / 2f, 58f);
            lab.HorizontalAlignment = HorizontalAlignment.Center;
            AddChild(lab);
            _nameLabels[i] = lab;
        }
    }

    private void CreateHalo()
    {
        _halo = new Polygon2D();
        var verts = new Vector2[36];
        for (int i = 0; i < verts.Length; i++)
        {
            float a = Mathf.Tau * i / verts.Length;
            verts[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * HaloRadius;
        }
        _halo.Polygon = verts;
        _halo.Color = new Color(1f, 0.9f, 0.3f, 0.45f);
        _halo.ZIndex = 1;
        AddChild(_halo);
        MoveChild(_halo, 0);
    }

    private void CreateLockIcons()
    {
        _lockIcons = new Sprite2D[Levels.Length];
        Texture2D lockTex = LoadLockTextureWithChromaKey();

        if (lockTex == null)
        {
            GD.PrintErr($"WorldMap: imagem do cadeado não encontrada em {LockTexturePath}.");
        }

        for (int i = 0; i < Levels.Length; i++)
        {
            var sp = new Sprite2D();
            sp.Texture = lockTex;
            sp.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
            sp.Position = Levels[i].Position;
            sp.ZIndex = 10;
            if (lockTex != null)
            {
                float scale = LockSize / Mathf.Max(lockTex.GetSize().X, lockTex.GetSize().Y);
                sp.Scale = new Vector2(scale, scale);
            }
            AddChild(sp);
            _lockIcons[i] = sp;
        }
    }

    private void CreateStarLabels()
    {
        _starLabels = new Label[Levels.Length];
        for (int i = 0; i < Levels.Length; i++)
        {
            var lab = new Label();
            lab.AddThemeFontSizeOverride("font_size", 22);
            lab.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f, 1f));
            lab.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f));
            lab.AddThemeConstantOverride("shadow_offset_x", 2);
            lab.AddThemeConstantOverride("shadow_offset_y", 2);
            lab.ZIndex = 11;
            lab.Position = Levels[i].Position + new Vector2(-32, 30);
            lab.Size = new Vector2(64, 24);
            lab.HorizontalAlignment = HorizontalAlignment.Center;
            AddChild(lab);
            _starLabels[i] = lab;
        }
    }

    private void UpdateStarLabels()
    {
        int unlocked = GameManager.Instance?.HighestUnlockedLevel ?? 1;
        for (int i = 0; i < _starLabels.Length; i++)
        {
            bool isUnlocked = (i + 1) <= unlocked;
            if (!isUnlocked)
            {
                _starLabels[i].Visible = false;
                continue;
            }

            int stars = GameManager.Instance?.GetBestStars(i + 1) ?? 0;
            _starLabels[i].Visible = true;
            _starLabels[i].Text = new string('★', stars) + new string('☆', 3 - stars);
        }
    }

    private void StartAtHighestUnlocked()
    {
        int unlocked = GameManager.Instance?.HighestUnlockedLevel ?? 1;
        int desired = GameManager.Instance?.CurrentLevelIndex ?? 1;
        desired = Mathf.Clamp(desired, 1, unlocked);
        _currentIndex = desired - 1;
        _pipe.Position = Levels[_currentIndex].Position;
        _halo.Position = Levels[_currentIndex].Position;
    }

    private void UpdateLockVisibility()
    {
        int unlocked = GameManager.Instance?.HighestUnlockedLevel ?? 1;
        for (int i = 0; i < _lockIcons.Length; i++)
        {
            _lockIcons[i].Visible = (i + 1) > unlocked;
        }
    }

    private void StartPipeBob()
    {
        var t = CreateTween().SetLoops().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        t.TweenProperty(_pipe, "offset", new Vector2(0, -10), 0.9f);
        t.TweenProperty(_pipe, "offset", new Vector2(0, 0), 0.9f);
    }

    private void StartHaloPulse()
    {
        var t = CreateTween().SetLoops().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        t.TweenProperty(_halo, "scale", new Vector2(1.25f, 1.25f), 0.7f);
        t.Parallel().TweenProperty(_halo, "modulate:a", 0.15f, 0.7f);
        t.TweenProperty(_halo, "scale", Vector2.One, 0.7f);
        t.Parallel().TweenProperty(_halo, "modulate:a", 0.55f, 0.7f);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_entering) return;

        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            var pos = GetGlobalMousePosition();
            if (DebugClickPositions)
                GD.Print($"[WorldMap debug] clique em ({(int)pos.X}, {(int)pos.Y})");
            HandleClick(pos);
            return;
        }

        if (@event.IsActionPressed("ui_right") || @event.IsActionPressed("ui_down"))
        {
            MoveByStep(+1);
        }
        else if (@event.IsActionPressed("ui_left") || @event.IsActionPressed("ui_up"))
        {
            MoveByStep(-1);
        }
        else if (@event.IsActionPressed("ui_accept"))
        {
            EnterLevel();
        }
        else if (@event.IsActionPressed("ui_cancel"))
        {
            GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
        }
    }

    private void HandleClick(Vector2 globalPos)
    {
        int unlocked = GameManager.Instance?.HighestUnlockedLevel ?? 1;
        int closest = -1;
        float closestDist = ClickThreshold;

        for (int i = 0; i < Levels.Length; i++)
        {
            if ((i + 1) > unlocked) continue;
            float d = Levels[i].Position.DistanceTo(globalPos);
            if (d < closestDist)
            {
                closestDist = d;
                closest = i;
            }
        }

        if (closest < 0) return;

        if (closest == _currentIndex)
            EnterLevel();
        else
            MoveToIndex(closest);
    }

    private void MoveByStep(int direction)
    {
        int unlocked = GameManager.Instance?.HighestUnlockedLevel ?? Levels.Length;
        int target = _currentIndex;

        for (int i = 0; i < Levels.Length; i++)
        {
            target = (target + direction + Levels.Length) % Levels.Length;
            if ((target + 1) <= unlocked) break;
        }

        if (target == _currentIndex) return;
        MoveToIndex(target);
    }

    private void MoveToIndex(int index)
    {
        _currentIndex = index;
        UpdateLabel();

        Vector2 target = Levels[_currentIndex].Position;

        _moveTween?.Kill();
        _moveTween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        _moveTween.TweenProperty(_pipe, "position", target, 0.4f);
        _moveTween.Parallel().TweenProperty(_halo, "position", target, 0.4f);
    }

    private void UpdateLabel()
    {
        if (_levelLabel != null)
            _levelLabel.Text = Levels[_currentIndex].Name;
    }

    private void EnterLevel()
    {
        int unlocked = GameManager.Instance?.HighestUnlockedLevel ?? 1;
        if ((_currentIndex + 1) > unlocked) return;

        _entering = true;
        if (GameManager.Instance != null)
            GameManager.Instance.CurrentLevelIndex = _currentIndex + 1;

        GetTree().ChangeSceneToFile(Levels[_currentIndex].ScenePath);
    }
}
