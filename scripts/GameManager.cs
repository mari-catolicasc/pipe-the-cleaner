using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static GameManager Instance;

    private HashSet<string> _collectedParts = new();
    private int _levelTotalTrash = 0;
    private int _collectedTrashCount = 0;

    public event Action OnAllTrashCollected;

    public override void _Ready()
    {
        Instance = this;
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
        GD.Print($"Ending level, next={nextScenePath}");
        // run any cleanup, analytics, UI, etc. here
        if (!string.IsNullOrEmpty(nextScenePath))
            GetTree().ChangeSceneToFile(nextScenePath);
        else
            GetTree().ChangeSceneToFile("res://scenes/level_complete.tscn"); // fallback
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