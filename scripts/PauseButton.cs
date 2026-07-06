using Godot;
using System;

public partial class PauseButton : TextureButton
{
    // Guarda referência do PauseMenu
    private PauseMenu pauseMenu;

    public override void _Ready()
    {
        Pressed += OnPausePressed;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            TogglePause();
        }
    }

    private void OnPausePressed()
    {
        TogglePause();
    }

    private void TogglePause()
    {
        GetTree().Paused = !GetTree().Paused;

        if (pauseMenu == null)
        {
            var pauseMenuScene = GD.Load<PackedScene>("res://scenes/PauseMenu.tscn");
            pauseMenu = pauseMenuScene.Instantiate<PauseMenu>();

            GetTree().Root.AddChild(pauseMenu);

            // Move para o topo da pilha de desenho
            GetTree().Root.MoveChild(pauseMenu, GetTree().Root.GetChildCount() - 1);

            pauseMenu.ProcessMode = Node.ProcessModeEnum.Always;

            GD.Print("PauseMenu instanciado e adicionado ao Root");
        }

        if (GetTree().Paused)
            pauseMenu.Show();
        else
            pauseMenu.Hide();
    }
}
