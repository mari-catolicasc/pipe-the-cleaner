using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
    public override void _Ready()
    {
        Layer = 10; // maior que o HUD

        ProcessMode = Node.ProcessModeEnum.Always;
        GD.Print("PauseMenu carregado");

        // Conectar sinais dos botões
        GetNode<Button>("UI/PauseButtons/ButtonContinuar").Pressed += OnContinuarPressed;
        GetNode<Button>("UI/PauseButtons/ButtonReiniciar").Pressed += OnReiniciarPressed;
        GetNode<Button>("UI/PauseButtons/ButtonMapa").Pressed += OnMapaPressed;
        GetNode<Button>("UI/PauseButtons/ButtonMenuPrincipal").Pressed += OnMenuPrincipalPressed;
        GetNode<Button>("UI/PauseButtons/ButtonInstrucoes").Pressed += OnInstrucoesPressed;
        GetNode<Button>("UI/PauseButtons/ButtonSair").Pressed += OnSairPressed;

        GetNode<Button>("UI/PanelInstrucoes/ButtonFecharInstrucoes").Pressed += OnFecharInstrucoesPressed;
        GetNode<Button>("UI/PanelCreditos/ButtonFecharCreditos").Pressed += OnFecharCreditosPressed;
    }

    private void OnContinuarPressed()
    {
        GetTree().Paused = false;
        Visible = false;
    }

    private void OnReiniciarPressed()
    {
        GetTree().ReloadCurrentScene();
    }

    private void OnMapaPressed()
    {
        GetTree().Paused = false;
        Visible = false; // esconde o PauseMenu
        GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
    }

    private void OnMenuPrincipalPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
    }

    private void OnInstrucoesPressed()
    {
        GetNode<Panel>("UI/PanelInstrucoes").Visible = true;
    }

    private void OnFecharInstrucoesPressed()
    {
        GetNode<Panel>("UI/PanelInstrucoes").Visible = false;
    }

    private void OnFecharCreditosPressed()
    {
        GetNode<Panel>("UI/PanelCreditos").Visible = false;
    }

    private void OnSairPressed()
    {
        GetTree().Quit();
    }
}
