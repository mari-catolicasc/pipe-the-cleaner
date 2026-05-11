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
        _area.AreaExited += OnAreaExited;
    }

    private void OnAreaEntered(Area2D area)
	{
       var player = area.GetParent() as Player;
		if (player != null)
		{
			_playerInside = player;
		}
	}

	private void OnAreaExited(Area2D area)
	{
       var player = area.GetParent() as Player;
		if (player != null && _playerInside == player)
		{
			_playerInside = null;
		}
	}

    private void DepositTrash(Player player)
	{
		bool removed = player.RemoveOneTrash(AcceptedType);

		if (removed)
		{
			GD.Print("Lixo descartado corretamente!");

            player.OnTrashDisposed(AcceptedType);
        }
		else
		{
			GD.Print($"Nenhum lixo do tipo {AcceptedType} para descartar.");
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