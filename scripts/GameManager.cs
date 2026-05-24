using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static GameManager Instance;

    public const int CorrectDiscardPoints = 100;
    public const int IncorrectDiscardPoints = -50;

    public const int TotalLevels = 5;
    private const string ProgressPath = "user://progress.cfg";

    private HashSet<string> _collectedParts = new();
    private int _levelTotalTrash = 0;
    private int _collectedTrashCount = 0;

    public int CurrentPoints { get; private set; }
    public int MaxPossiblePoints { get; private set; }

    public int CurrentLevelIndex { get; set; } = 1;
    public int HighestCompletedLevel { get; private set; } = 0;
    public int HighestUnlockedLevel => Math.Min(HighestCompletedLevel + 1, TotalLevels);

    private readonly Dictionary<int, int> _bestStars = new();

    private string _pendingNextScene;

    public event Action OnAllTrashCollected;
    public event Action<int> OnPointsChanged;
    public event Action<int, int, int> OnLevelEnded; // points, maxPoints, stars

    public override void _Ready()
    {
        Instance = this;
        LoadProgress();
    }

    private void LoadProgress()
    {
        var cfg = new ConfigFile();
        if (cfg.Load(ProgressPath) == Error.Ok)
        {
            int completed = (int)cfg.GetValue("progress", "highest_completed_level", 0);
            if (completed < 0) completed = 0;
            if (completed > TotalLevels) completed = TotalLevels;
            HighestCompletedLevel = completed;

            _bestStars.Clear();
            for (int i = 1; i <= TotalLevels; i++)
            {
                int stars = (int)cfg.GetValue("stars", $"level_{i}", 0);
                if (stars > 0) _bestStars[i] = Math.Clamp(stars, 0, 3);
            }
        }
    }

    private void SaveProgress()
    {
        var cfg = new ConfigFile();
        cfg.SetValue("progress", "highest_completed_level", HighestCompletedLevel);
        foreach (var kv in _bestStars)
        {
            cfg.SetValue("stars", $"level_{kv.Key}", kv.Value);
        }
        cfg.Save(ProgressPath);
    }

    public int GetBestStars(int levelIndex)
    {
        return _bestStars.TryGetValue(levelIndex, out var s) ? s : 0;
    }

    private void RecordStars(int levelIndex, int stars)
    {
        if (stars <= 0) return;
        if (!_bestStars.TryGetValue(levelIndex, out var current) || stars > current)
        {
            _bestStars[levelIndex] = stars;
        }
    }

    private void UnlockNextLevel()
    {
        if (CurrentLevelIndex > HighestCompletedLevel)
        {
            HighestCompletedLevel = Math.Min(CurrentLevelIndex, TotalLevels);
            SaveProgress();
            GD.Print($"Fase {CurrentLevelIndex} completada — destravado até {HighestUnlockedLevel}");
        }
    }

    public void ResetProgress()
    {
        HighestCompletedLevel = 0;
        SaveProgress();
    }

    public void AddPart(string partId)
    {
        _collectedParts.Add(partId);
        GD.Print("Peça coletada: " + partId);
    }

    public void SetTotalTrash(int total)
    {
        _levelTotalTrash = total;
        _collectedTrashCount = 0;
        CurrentPoints = 0;
        MaxPossiblePoints = total * CorrectDiscardPoints;
        OnPointsChanged?.Invoke(CurrentPoints);
    }

    public void AddPoints(int delta)
    {
        CurrentPoints = Math.Max(0, CurrentPoints + delta);
        OnPointsChanged?.Invoke(CurrentPoints);
    }

    public int GetStars()
    {
        if (MaxPossiblePoints <= 0) return 0;

        float pct = (float)CurrentPoints / MaxPossiblePoints;
        if (pct >= 1.0f) return 3;
        if (pct >= 0.6f) return 2;
        if (CurrentPoints > 0) return 1;
        return 0;
    }

    public void NotifyTrashCollected()
    {
        // If level total wasn't set, try to compute it now from the scene
        if (_levelTotalTrash == 0)
        {
            var nodes = GetTree().GetNodesInGroup("trash");
            int total = 0;
            foreach (var n in nodes)
            {
                if (n is Node node && node.IsInsideTree() && node is Trash) total++;
            }
            _levelTotalTrash = total;
            MaxPossiblePoints = total * CorrectDiscardPoints;
            GD.Print($"GameManager: inferred level total trash = {_levelTotalTrash}");
        }

        _collectedTrashCount++;
        GD.Print($"GameManager: trash collected {_collectedTrashCount}/{_levelTotalTrash}");

        if (_levelTotalTrash > 0 && _collectedTrashCount >= _levelTotalTrash)
        {
            GD.Print("All trash collected for this level.");
            OnAllTrashCollected?.Invoke();
        }
    }

    public bool IsAllTrashCollected()
    {
        return _levelTotalTrash > 0 && _collectedTrashCount >= _levelTotalTrash;
    }

    public void EndLevel(string nextScenePath)
    {
        int stars = GetStars();
        GD.Print($"Ending level {CurrentLevelIndex}, points={CurrentPoints}/{MaxPossiblePoints}, stars={stars}");
        _pendingNextScene = nextScenePath;
        RecordStars(CurrentLevelIndex, stars);
        UnlockNextLevel();
        SaveProgress();
        OnLevelEnded?.Invoke(CurrentPoints, MaxPossiblePoints, stars);
    }

    public void AdvanceToNextLevel()
    {
        _pendingNextScene = null;

        CurrentPoints = 0;
        MaxPossiblePoints = 0;
        _levelTotalTrash = 0;
        _collectedTrashCount = 0;

        GetTree().Paused = false;

        GetTree().ChangeSceneToFile("res://scenes/WorldMap.tscn");
    }

    public bool HasPart(string partId)
    {
        return _collectedParts.Contains(partId);
    }

    public int TotalParts()
    {
        return _collectedParts.Count;
    }
}
