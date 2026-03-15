using Godot;
using System;
using System.Collections.Generic;

public class TutorialSystem : BaseSystem
{
    private TutorialData _data;
    private TutorialDatabase _database;
    private string _currentTutorialId = "";
    private int _currentStepIndex = 0;
    private float _stepTimer = 0f;
    private bool _isTutorialActive = false;
    private Player _player;

    public override void _Ready()
    {
        _database = TutorialDatabase.Instance;
        _data = new TutorialData();
        LoadTutorialData();
        GD.Print("[TutorialSystem] 游戏教程系统已初始化");
    }

    public void SetPlayer(Player player)
    {
        _player = player;
        CheckAutoStartTutorials();
    }

    private void CheckAutoStartTutorials()
    {
        int playerLevel = 1;
        if (_player != null)
        {
            playerLevel = _player.GetLevel();
        }

        var availableTutorials = _database.GetAvailableTutorials(playerLevel);
        foreach (var tutorial in availableTutorials)
        {
            if (tutorial.AutoStart && !IsTutorialCompleted(tutorial.TutorialId) && !IsTutorialInProgress(tutorial.TutorialId))
            {
                StartTutorial(tutorial.TutorialId);
                break;
            }
        }
    }

    public bool StartTutorial(string tutorialId)
    {
        var tutorial = _database.GetTutorial(tutorialId);
        if (tutorial == null)
        {
            GD.PrintErr("[TutorialSystem] 教程不存在: " + tutorialId);
            return false;
        }

        if (IsTutorialCompleted(tutorialId))
        {
            GD.Print("[TutorialSystem] 教程已完成: " + tutorialId);
            return false;
        }

        _currentTutorialId = tutorialId;
        _currentStepIndex = 0;
        _isTutorialActive = true;
        _stepTimer = 0f;

        if (!_data.InProgressTutorials.Contains(tutorialId))
        {
            _data.InProgressTutorials.Add(tutorialId);
        }

        _data.TutorialProgress[tutorialId] = 0;
        SaveTutorialData();

        NotifyTutorialStarted(tutorial);
        GD.Print("[TutorialSystem] 开始教程: " + tutorial.Title);
        return true;
    }

    public void CompleteCurrentStep()
    {
        if (!_isTutorialActive) return;

        var tutorial = _database.GetTutorial(_currentTutorialId);
        if (tutorial == null || _currentStepIndex >= tutorial.Steps.Count) return;

        var currentStep = tutorial.Steps[_currentStepIndex];
        _data.TutorialProgress[_currentTutorialId] = _currentStepIndex + 1;
        
        GD.Print("[TutorialSystem] 完成步骤: " + currentStep.Title);
        
        _currentStepIndex++;
        _stepTimer = 0f;

        if (_currentStepIndex >= tutorial.Steps.Count)
        {
            CompleteTutorial(_currentTutorialId);
        }
        else
        {
            NotifyStepChanged(tutorial.Steps[_currentStepIndex]);
        }

        SaveTutorialData();
    }

    public void SkipStep()
    {
        if (!_isTutorialActive) return;

        var tutorial = _database.GetTutorial(_currentTutorialId);
        if (tutorial == null) return;

        string stepId = tutorial.Steps[_currentStepIndex].StepId;
        if (_data.StepSkips.ContainsKey(stepId))
            _data.StepSkips[stepId]++;
        else
            _data.StepSkips[stepId] = 1;

        _data.HintsUsed++;
        
        GD.Print("[TutorialSystem] 跳过步骤: " + tutorial.Steps[_currentStepIndex].Title);
        
        CompleteCurrentStep();
    }

    public void CompleteTutorial(string tutorialId)
    {
        if (!_data.CompletedTutorials.ContainsKey(tutorialId) || !_data.CompletedTutorials[tutorialId])
        {
            _data.CompletedTutorials[tutorialId] = true;
            _data.TotalTutorialsCompleted++;
            _data.TutorialCompletionTimes[tutorialId] = DateTime.Now;
        }

        if (_data.InProgressTutorials.Contains(tutorialId))
        {
            _data.InProgressTutorials.Remove(tutorialId);
        }

        if (_currentTutorialId == tutorialId)
        {
            _isTutorialActive = false;
            _currentTutorialId = "";
            _currentStepIndex = 0;
        }

        _data.LastTutorialTime = DateTime.Now;
        SaveTutorialData();

        var tutorial = _database.GetTutorial(tutorialId);
        if (tutorial != null)
        {
            NotifyTutorialCompleted(tutorial);
            GD.Print("[TutorialSystem] 教程完成: " + tutorial.Title);
        }
    }

    public bool IsTutorialCompleted(string tutorialId)
    {
        return _data.CompletedTutorials.ContainsKey(tutorialId) && _data.CompletedTutorials[tutorialId];
    }

    public bool IsTutorialInProgress(string tutorialId)
    {
        return _data.InProgressTutorials.Contains(tutorialId);
    }

    public bool IsAnyTutorialActive()
    {
        return _isTutorialActive;
    }

    public string GetCurrentTutorialId()
    {
        return _currentTutorialId;
    }

    public TutorialStep GetCurrentStep()
    {
        if (!_isTutorialActive) return null;

        var tutorial = _database.GetTutorial(_currentTutorialId);
        if (tutorial == null || _currentStepIndex >= tutorial.Steps.Count) return null;

        return tutorial.Steps[_currentStepIndex];
    }

    public int GetCurrentStepIndex()
    {
        return _currentStepIndex;
    }

    public int GetTotalSteps()
    {
        if (!_isTutorialActive) return 0;

        var tutorial = _database.GetTutorial(_currentTutorialId);
        return tutorial?.Steps.Count ?? 0;
    }

    public float GetStepProgress()
    {
        int total = GetTotalSteps();
        if (total == 0) return 0f;
        return (float)_currentStepIndex / total;
    }

    public Dictionary<string, bool> GetCompletedTutorials()
    {
        return _data.CompletedTutorials;
    }

    public List<TutorialDefinition> GetAllTutorials()
    {
        List<TutorialDefinition> result = new List<TutorialDefinition>();
        foreach (var tutorial in _database.Tutorials.Values)
        {
            result.Add(tutorial);
        }
        return result;
    }

    public List<TutorialDefinition> GetTutorialsByCategory(string category)
    {
        return _database.GetTutorialsByCategory(category);
    }

    public string[] GetCategories()
    {
        return _database.GetCategories();
    }

    public Dictionary<string, int> GetStatistics()
    {
        Dictionary<string, int> stats = new Dictionary<string, int>();
        stats["TotalCompleted"] = _data.TotalTutorialsCompleted;
        stats["TotalViewed"] = _data.TotalTutorialsViewed;
        stats["InProgress"] = _data.InProgressTutorials.Count;
        stats["HintsUsed"] = _data.HintsUsed;
        stats["TotalAvailable"] = _database.Tutorials.Count;
        
        // Category stats
        var categories = _database.GetCategories();
        foreach (var category in categories)
        {
            int completed = 0;
            int total = 0;
            var tutorials = _database.GetTutorialsByCategory(category);
            foreach (var t in tutorials)
            {
                total++;
                if (IsTutorialCompleted(t.TutorialId))
                    completed++;
            }
            stats["Category_" + category + "_Completed"] = completed;
            stats["Category_" + category + "_Total"] = total;
        }

        return stats;
    }

    public void ResetTutorial(string tutorialId)
    {
        if (_data.CompletedTutorials.ContainsKey(tutorialId))
        {
            _data.CompletedTutorials[tutorialId] = false;
            _data.TotalTutorialsCompleted = Math.Max(0, _data.TotalTutorialsCompleted - 1);
        }

        if (_data.TutorialCompletionTimes.ContainsKey(tutorialId))
        {
            _data.TutorialCompletionTimes.Remove(tutorialId);
        }

        if (_currentTutorialId == tutorialId)
        {
            _isTutorialActive = false;
            _currentTutorialId = "";
            _currentStepIndex = 0;
        }

        SaveTutorialData();
        GD.Print("[TutorialSystem] 重置教程: " + tutorialId);
    }

    public void ResetAllTutorials()
    {
        _data.CompletedTutorials.Clear();
        _data.InProgressTutorials.Clear();
        _data.TutorialProgress.Clear();
        _data.TutorialCompletionTimes.Clear();
        _data.TotalTutorialsCompleted = 0;
        _data.TotalTutorialsViewed = 0;
        _data.HintsUsed = 0;
        _data.StepSkips.Clear();

        _isTutorialActive = false;
        _currentTutorialId = "";
        _currentStepIndex = 0;

        SaveTutorialData();
        GD.Print("[TutorialSystem] 重置所有教程");
    }

    private void NotifyTutorialStarted(TutorialDefinition tutorial)
    {
        // Emit signal for UI to update
        // This would typically call a signal or event system
    }

    private void NotifyStepChanged(TutorialStep step)
    {
        // Emit signal for UI to update
    }

    private void NotifyTutorialCompleted(TutorialDefinition tutorial)
    {
        // Emit signal for UI to show completion
    }

    public override void _Process(float delta)
    {
        if (!_isTutorialActive) return;

        var currentStep = GetCurrentStep();
        if (currentStep == null) return;

        // Handle timed steps
        if (currentStep.Duration > 0)
        {
            _stepTimer += delta;
            if (_stepTimer >= currentStep.Duration)
            {
                if (!currentStep.RequireAction)
                {
                    CompleteCurrentStep();
                }
            }
        }
    }

    private void LoadTutorialData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) return;

        if (data.Contains("tutorial_data"))
        {
            var tutorialData = (Godot.Collections.Dictionary)data["tutorial_data"];
            
            if (tutorialData.Contains("completed"))
            {
                var completed = (Godot.Collections.Dictionary)tutorialData["completed"];
                foreach (var key in completed.Keys)
                {
                    _data.CompletedTutorials[key.ToString()] = (bool)completed[key];
                }
            }

            if (tutorialData.Contains("progress"))
            {
                var progress = (Godot.Collections.Dictionary)tutorialData["progress"];
                foreach (var key in progress.Keys)
                {
                    _data.TutorialProgress[key.ToString()] = (int)(long)progress[key];
                }
            }

            if (tutorialData.Contains("in_progress"))
            {
                var inProgress = (Godot.Collections.Array)tutorialData["in_progress"];
                foreach (var item in inProgress)
                {
                    _data.InProgressTutorials.Add(item.ToString());
                }
            }

            if (tutorialData.Contains("hints_used"))
                _data.HintsUsed = (int)(long)tutorialData["hints_used"];

            if (tutorialData.Contains("total_completed"))
                _data.TotalTutorialsCompleted = (int)(long)tutorialData["total_completed"];

            GD.Print("[TutorialSystem] 教程数据已加载");
        }
    }

    private void SaveTutorialData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Collections.Dictionary();

        var tutorialData = new Godot.Collections.Dictionary();
        
        var completed = new Godot.Collections.Dictionary();
        foreach (var kvp in _data.CompletedTutorials)
        {
            completed[kvp.Key] = kvp.Value;
        }
        tutorialData["completed"] = completed;

        var progress = new Godot.Collections.Dictionary();
        foreach (var kvp in _data.TutorialProgress)
        {
            progress[kvp.Key] = kvp.Value;
        }
        tutorialData["progress"] = progress;

        var inProgress = new Godot.Collections.Array();
        foreach (var item in _data.InProgressTutorials)
        {
            inProgress.Add(item);
        }
        tutorialData["in_progress"] = inProgress;

        tutorialData["hints_used"] = _data.HintsUsed;
        tutorialData["total_completed"] = _data.TotalTutorialsCompleted;

        data["tutorial_data"] = tutorialData;
        saveSystem.SaveGame(data);
    }
}
