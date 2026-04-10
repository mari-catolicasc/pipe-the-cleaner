using Godot;
using System.Linq;

public partial class TrashCan : StaticBody2D
{
    [Export] public Trash.TrashType AcceptedType;
	private Player _playerInside;

    private Area2D _area;

    public override void _Ready()
    {
        _area = GetNode<Area2D>("Area2D");
        _area.AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
	{
		_playerInside = area.GetParent<Player>();
	}

	private void OnAreaExited(Area2D area)
	{
		if (area.GetParent<Player>() != null)
			_playerInside = null;
	}

    private void DepositTrash(Player player)
	{
		var selected = player.GetSelectedTrash();

		if (selected == null)
		{
			GD.Print("Nenhum lixo selecionado.");
			return;
		}

		if (selected != AcceptedType)
		{
			// TODO: adicionar lógica de dedução de pontos e retorno do lixo ao local inicial
			GD.Print("Tipo errado!");
			return;
		}

		bool removed = player.RemoveOneTrash(selected.Value);

		if (removed)
		{
			GD.Print("Lixo descartado corretamente!");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_playerInside != null && Input.IsActionJustPressed("interact"))
		{
			DepositTrash(_playerInside);
		}
	}
}