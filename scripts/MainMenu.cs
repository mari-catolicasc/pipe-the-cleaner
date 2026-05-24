using Godot;

public partial class MainMenu : Node2D
{
    private Button _buttonJogar;
    private Button _buttonInstrucoes;
    private Button _buttonCreditos;
    private Button _buttonSair;

    private Control _panelInstrucoes;
    private Control _panelCreditos;

    private Button _buttonFecharInstrucoes;
    private Button _buttonFecharCreditos;

    private Label _progressLabel;

    public override void _Ready()
    {
        _progressLabel = GetNode<Label>("UI/ProgressLabel");
        UpdateProgressLabel();

        _buttonJogar = GetNode<Button>("UI/MenuButtons/ButtonJogar");
        _buttonInstrucoes = GetNode<Button>("UI/MenuButtons/ButtonInstrucoes");
        _buttonCreditos = GetNode<Button>("UI/MenuButtons/ButtonCreditos");
        _buttonSair = GetNode<Button>("UI/MenuButtons/ButtonSair");

        _panelInstrucoes = GetNode<Control>("UI/PanelInstrucoes");
        _panelCreditos = GetNode<Control>("UI/PanelCreditos");

        _buttonFecharInstrucoes = GetNode<Button>("UI/PanelInstrucoes/ButtonFecharInstrucoes");
        _buttonFecharCreditos = GetNode<Button>("UI/PanelCreditos/ButtonFecharCreditos");

        _panelInstrucoes.Visible = false;
        _panelCreditos.Visible = false;

        _buttonJogar.Pressed += OnJogarPressed;
        _buttonInstrucoes.Pressed += OnInstrucoesPressed;
        _buttonCreditos.Pressed += OnCreditosPressed;
        _buttonSair.Pressed += OnSairPressed;

        _buttonFecharInstrucoes.Pressed += OnFecharInstrucoesPressed;
        _buttonFecharCreditos.Pressed += OnFecharCreditosPressed;
    }

    private void OnJogarPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
    }

    private void OnInstrucoesPressed()
    {
        _panelInstrucoes.Visible = true;
        _panelCreditos.Visible = false;
    }

    private void OnCreditosPressed()
    {
        _panelCreditos.Visible = true;
        _panelInstrucoes.Visible = false;
    }

    private void OnSairPressed()
    {
        GetTree().Quit();
    }

    private void OnFecharInstrucoesPressed()
    {
        _panelInstrucoes.Visible = false;
    }

    private void OnFecharCreditosPressed()
    {
        _panelCreditos.Visible = false;
    }

    private void UpdateProgressLabel()
    {
        if (_progressLabel == null) return;

        int completed = GameManager.Instance?.HighestCompletedLevel ?? 0;
        int total = GameManager.TotalLevels;
        _progressLabel.Text = $"Progresso: {completed} / {total} fases";
    }
}
