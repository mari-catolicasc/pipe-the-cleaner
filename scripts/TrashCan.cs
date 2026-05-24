using Godot;
using System.Linq;

public partial class TrashCan : StaticBody2D
{
    [Export] public Trash.TrashType AcceptedType;
	private Player _playerInside;

    private Area2D _area;
    private Label _prompt;

    public override void _Ready()
    {
        _area = GetNode<Area2D>("Area2D");
        _area.AreaEntered += OnAreaEntered;
        _area.AreaExited += OnAreaExited;

        CreatePrompt();
    }

    private void CreatePrompt()
    {
        _prompt = new Label();
        _prompt.Text = "[E]";
        _prompt.AddThemeFontSizeOverride("font_size", 22);
        _prompt.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 1f));
        _prompt.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.85f));
        _prompt.AddThemeConstantOverride("shadow_offset_x", 2);
        _prompt.AddThemeConstantOverride("shadow_offset_y", 2);
        _prompt.MouseFilter = Control.MouseFilterEnum.Ignore;
        _prompt.HorizontalAlignment = HorizontalAlignment.Center;
        _prompt.Size = new Vector2(48, 28);
        _prompt.Position = new Vector2(-24, -60);
        _prompt.ZIndex = 50;
        _prompt.Visible = false;
        AddChild(_prompt);
    }

    private void OnAreaEntered(Area2D area)
	{
       var player = area.GetParent() as Player;
		if (player != null)
		{
			_playerInside = player;
			if (_prompt != null) _prompt.Visible = true;
		}
	}

	private void OnAreaExited(Area2D area)
	{
       var player = area.GetParent() as Player;
		if (player != null && _playerInside == player)
		{
			_playerInside = null;
			if (_prompt != null) _prompt.Visible = false;
		}
	}

    private void DepositTrash(Player player)
	{
		if (!player.DepositSelectedTrash(out var type))
		{
			GD.Print("Nenhum lixo selecionado.");
			return;
		}

		bool correct = type == AcceptedType;

		if (correct)
			GD.Print($"Descarte correto de {type}!");
		else
			GD.Print($"Descarte INCORRETO: {type} na lixeira de {AcceptedType}.");

		player.OnTrashDisposed(type, correct);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_playerInside != null && Input.IsActionJustPressed("interact"))
		{
			DepositTrash(_playerInside);
		}
	}
}
