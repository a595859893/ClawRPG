using Godot;
using System;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

/// <summary>
/// 坐骑竞速系统 - 坐骑竞速比赛管理
/// 支持计时赛、障碍赛等多种模式
/// </summary>
public partial class MountRaceSystem : BaseSystem
{
	private static MountRaceSystem _instance;
	public static MountRaceSystem Instance
	{
		get
		{
			if (_instance == null)
			{
				GD.PrintErr("MountRaceSystem not found! Make sure it's in the scene.");
			}
			return _instance;
		}
	}

	public static SignalContainer.Signals Signals { get; } = new SignalContainer.Signals();
	public Action RaceStarted;
	public Action RaceFinished;
	public Action CheckpointReached;
	public Action RacePositionUpdate;

	private MountRaceData.RaceState _currentState = MountRaceData.RaceState.Waiting;
	private MountRaceData.MountRace _currentRace;
	private List<MountRaceData.PlayerRaceData> _racers = new List<MountRaceData.PlayerRaceData>();
	private MountRaceData.PlayerRaceProgress _playerProgress;

	private float _raceTimer = 0f;
	private bool _isRacing = false; 

	// Race configuration
	private float _baseSpeed = 10f;
	private float _speedVariance = 3f;
	private float _acceleration = 2f;
	private float _deceleration = 1f;
	private float _boostChance = 0.1f;
	private float _boostMultiplier = 1.5f;

	public override void _Ready()
	{
		_instance = this;
		_playerProgress = new MountRaceData.PlayerRaceProgress();
	}

	public override void _Process(double delta)
	{
		if (!_isRacing || _currentRace == null)
			return;

		_raceTimer += delta;

		// Update player racer
		var playerRacer = _racers.Find(r => r.IsPlayer);
		if (playerRacer != null)
		{
			UpdateRacerMovement(playerRacer, delta);

			// Check checkpoints
			if (playerRacer.CurrentCheckpoint < _currentRace.Checkpoints.Count)
			{
				float checkpointPosition = (playerRacer.CurrentCheckpoint + 1) * 
					(_currentRace.Distance / _currentRace.Checkpoints.Count);
				
				if (playerRacer.CurrentPosition >= checkpointPosition)
				{
					playerRacer.CurrentCheckpoint++;
					EmitSignal(SignalName.CheckpointReached);
				}
			}

			// Check finish
			if (playerRacer.CurrentPosition >= _currentRace.Distance)
			{
				FinishRace();
			}

			EmitSignal(SignalName.RacePositionUpdate);
		}

		// Update AI racers
		foreach (var racer in _racers)
		{
			if (!racer.IsPlayer)
			{
				UpdateAIRacer(racer, delta);
			}
		}
	}

	private void UpdateRacerMovement(MountRaceData.PlayerRaceData racer, float delta)
	{
		// Calculate target speed based on race progress
		float progressPercent = racer.CurrentPosition / _currentRace.Distance;
		float targetSpeed = _baseSpeed + (progressPercent * _acceleration * 10f);

		// Random speed variation
		float randomFactor = (float)GD.RandRange(-_speedVariance, _speedVariance);
		
		// Boost chance
		if (GD.Randf() < _boostChance)
		{
			targetSpeed *= _boostMultiplier;
		}

		// Smoothly adjust speed
		if (racer.CurrentSpeed < targetSpeed)
			racer.CurrentSpeed += _acceleration * delta;
		else
			racer.CurrentSpeed -= _deceleration * delta;

		racer.CurrentSpeed += randomFactor * delta;
		racer.CurrentSpeed = Mathf.Max(0, racer.CurrentSpeed);

		// Update position
		racer.CurrentPosition += racer.CurrentSpeed * delta;
		racer.ElapsedTime = _raceTimer;
	}

	private void UpdateAIRacer(MountRaceData.PlayerRaceData racer, float delta)
	{
		// AI uses a simulated skill level based on race difficulty
		float aiSkill = 0.7f + (GD.Randf() * 0.3f);
		float difficultyFactor = 1f - (_currentRace.Difficulty * 0.05f);
		
		float targetSpeed = _baseSpeed * aiSkill * difficultyFactor;
		
		// AI acceleration
		if (racer.CurrentSpeed < targetSpeed)
			racer.CurrentSpeed += _acceleration * delta * 0.8f;
		else
			racer.CurrentSpeed -= _deceleration * delta;

		racer.CurrentPosition += racer.CurrentSpeed * delta;
		racer.ElapsedTime = _raceTimer;

		// AI checkpoint logic
		float checkpointPosition = (racer.CurrentCheckpoint + 1) * 
			(_currentRace.Distance / _currentRace.Checkpoints.Count);
		if (racer.CurrentPosition >= checkpointPosition && racer.CurrentCheckpoint < _currentRace.Checkpoints.Count - 1)
		{
			racer.CurrentCheckpoint++;
		}
	}

	public bool CanStartRace(string raceId)
	{
		var race = MountRaceDatabase.Instance.GetRace(raceId);
		if (race == null)
			return false;

		var player = GetTree().Root.GetNode<Player>("Player");
		if (player == null)
			return false;

		// Check if player has enough gold
		if (player.Gold < race.EntryFee)
		{
			GD.Print("Not enough gold to enter race");
			return false;
		}

		// Check if player has a mount
		if (string.IsNullOrEmpty(player.CurrentMountId))
		{
			GD.Print("Need a mount to race");
			return false;
		}

		return true;
	}

	public bool StartRace(string raceId)
	{
		if (!CanStartRace(raceId))
			return false;

		var race = MountRaceDatabase.Instance.GetRace(raceId);
		if (race == null)
			return false;

		var player = GetTree().Root.GetNode<Player>("Player");
		if (player == null)
			return false;

		// Deduct entry fee
		player.Gold -= race.EntryFee;

		// Setup race
		_currentRace = race;
		_currentState = MountRaceData.RaceState.Racing;
		_racers.Clear();
		_raceTimer = 0f;

		// Add player
		var playerRacer = new MountRaceData.PlayerRaceData
		{
			RaceId = raceId,
			IsPlayer = true,
			MountId = player.CurrentMountId,
			DisplayName = "玩家",
			Status = MountRaceData.RaceStatus.InProgress
		};
		_racers.Add(playerRacer);

		// Add AI opponents (3-5 based on difficulty)
		int aiCount = 3 + _currentRace.Difficulty;
		string[] aiNames = { "风速", "闪电", "火焰", "冰霜", "雷霆", "追风", "逐日", "奔月", "凌云", "踏雪" };
		
		for (int i = 0; i < aiCount; i++)
		{
			var aiRacer = new MountRaceData.PlayerRaceData
			{
				RaceId = raceId,
				IsPlayer = false,
				MountId = "mount_horse", // Default mount
				DisplayName = aiNames[i % aiNames.Length],
				Status = MountRaceData.RaceStatus.InProgress,
				CurrentSpeed = _baseSpeed * 0.5f // Start behind
			};
			_racers.Add(aiRacer);
		}

		_isRacing = true;
		EmitSignal(SignalName.RaceStarted);
		
		GD.Print($"Race started: {race.Name}");
		return true;
	}

	private void FinishRace()
	{
		if (_currentRace == null)
			return;

		_isRacing = false; 
		_currentState = MountRaceData.RaceState.Finished;

		// Sort racers by position
		_racers.Sort((a, b) => b.CurrentPosition.CompareTo(a.CurrentPosition));

		// Find player position
		var playerRacer = _racers.Find(r => r.IsPlayer);
		if (playerRacer != null)
		{
			int place = _racers.IndexOf(playerRacer) + 1;
			playerRacer.Status = MountRaceData.RaceStatus.Completed;

			// Calculate rewards
			int reward = 0;
			int expReward = _currentRace.RewardExp;
			
			if (place == 1)
			{
				reward = _currentRace.RewardGold;
				_playerProgress.FirstPlaces++;
				GD.Print($"🏆 You won 1st place!");
			}
			else if (place == 2)
			{
				reward = (int)(_currentRace.RewardGold * 0.6f);
				_playerProgress.SecondPlaces++;
				GD.Print($"🥈 You got 2nd place!");
			}
			else if (place == 3)
			{
				reward = (int)(_currentRace.RewardGold * 0.3f);
				_playerProgress.ThirdPlaces++;
				GD.Print($"🥉 You got 3rd place!");
			}
			else
			{
				reward = (int)(_currentRace.RewardGold * 0.1f);
				GD.Print($"You finished {place}th place");
			}

			// Award rewards
			var player = GetTree().Root.GetNode<Player>("Player");
			if (player != null)
			{
				player.Gold += reward;
				player.AddExp(expReward);
			}

			// Update progress
			_playerProgress.TotalRaces++;
			_playerProgress.TotalEarnings += reward;

			// Record best time
			if (!_playerProgress.BestTimes.ContainsKey(_currentRace.Id) || 
				playerRacer.ElapsedTime < _playerProgress.BestTimes[_currentRace.Id])
			{
				_playerProgress.BestTimes[_currentRace.Id] = playerRacer.ElapsedTime;
			}

			// Add race record
			if (!_playerProgress.CompletedRaces.ContainsKey(_currentRace.Id))
			{
				_playerProgress.CompletedRaces[_currentRace.Id] = new List<MountRaceData.RaceRecord>();
			}
			_playerProgress.CompletedRaces[_currentRace.Id].Add(new MountRaceData.RaceRecord
			{
				RaceId = _currentRace.Id,
				RaceName = _currentRace.Name,
				Time = playerRacer.ElapsedTime,
				Place = place,
				Reward = reward
			});
		}

		EmitSignal(SignalName.RaceFinished);
		
		// Auto-save
		SaveRaceProgress();
	}

	public void CancelRace()
	{
		_isRacing = false; 
		_currentState = MountRaceData.RaceState.Waiting;
		_currentRace = null;
		_racers.Clear();
		_raceTimer = 0f;
	}

	public MountRaceData.MountRace GetCurrentRace()
	{
		return _currentRace;
	}

	public List<MountRaceData.PlayerRaceData> GetRacers()
	{
		return new List<MountRaceData.PlayerRaceData>(_racers);
	}

	public MountRaceData.PlayerRaceData GetPlayerRacer()
	{
		return _racers.Find(r => r.IsPlayer);
	}

	public int GetPlayerPosition()
	{
		if (_racers.Count == 0)
			return 0;
		
		var sorted = new List<MountRaceData.PlayerRaceData>(_racers);
		sorted.Sort((a, b) => b.CurrentPosition.CompareTo(a.CurrentPosition));
		return sorted.FindIndex(r => r.IsPlayer) + 1;
	}

	public float GetRaceTime()
	{
		return _raceTimer;
	}

	public MountRaceData.RaceState GetRaceState()
	{
		return _currentState;
	}

	public MountRaceData.PlayerRaceProgress GetProgress()
	{
		return _playerProgress;
	}

	public bool IsRacing()
	{
		return _isRacing;
	}

	// Save/Load
	public Dictionary GetSaveData()
	{
		var data = new Dictionary<string, object>();
		
		// Save best times
		var bestTimesArray = new Godot.Collections.Array();
		foreach (var kvp in _playerProgress.BestTimes)
		{
			bestTimesArray.Add(new Dictionary { { "race_id", kvp.Key }, { "time", kvp.Value } });
		}
		data["best_times"] = bestTimesArray;
		
		data["total_races"] = _playerProgress.TotalRaces;
		data["first_places"] = _playerProgress.FirstPlaces;
		data["second_places"] = _playerProgress.SecondPlaces;
		data["third_places"] = _playerProgress.ThirdPlaces;
		data["total_earnings"] = _playerProgress.TotalEarnings;

		return data;
	}

	public void LoadSaveData(Dictionary data)
	{
		if (data == null)
			return;

		_playerProgress = new MountRaceData.PlayerRaceProgress();

		if (data.ContainsKey("best_times"))
		{
			var bestTimesArray = (Godot.Collections.Array)data["best_times"];
			foreach (Dictionary entry in bestTimesArray)
			{
				string raceId = (string)entry["race_id"];
				float time = (float)entry["time"];
				_playerProgress.BestTimes[raceId] = time;
			}
		}

		if (data.ContainsKey("total_races"))
			_playerProgress.TotalRaces = (int)data["total_races"];
		if (data.ContainsKey("first_places"))
			_playerProgress.FirstPlaces = (int)data["first_places"];
		if (data.ContainsKey("second_places"))
			_playerProgress.SecondPlaces = (int)data["second_places"];
		if (data.ContainsKey("third_places"))
			_playerProgress.ThirdPlaces = (int)data["third_places"];
		if (data.ContainsKey("total_earnings"))
			_playerProgress.TotalEarnings = (int)data["total_earnings"];
	}

	private void SaveRaceProgress()
	{
		var saveSystem = GetTree().Root.GetNode<SaveSystem>("SaveSystem");
		if (saveSystem != null)
		{
			saveSystem.RequestSave();
		}
	}

	// ===== 持久化方法 =====

	public override Dictionary<string, object> ExportSaveData()
	{
		var data = new Dictionary<string, object>();
		
		// 玩家竞速进度
		data["best_times"] = _playerProgress.BestTimes;
		data["total_races"] = _playerProgress.TotalRaces;
		data["first_places"] = _playerProgress.FirstPlaces;
		data["second_places"] = _playerProgress.SecondPlaces;
		data["third_places"] = _playerProgress.ThirdPlaces;
		data["total_earnings"] = _playerProgress.TotalEarnings;
		
		// 当前比赛状态
		data["currentState"] = (int)_currentState;
		
		return data;
	}

	public override void ImportSaveData(Dictionary<string, object> data)
	{
		if (data == null) return;
		
		// 加载最佳时间
		if (data.ContainsKey("best_times"))
		{
			var bestTimesArray = (Array)data["best_times"];
			_playerProgress.BestTimes.Clear();
			foreach (Dictionary entry in bestTimesArray)
			{
				string raceId = entry["race_id"].ToString();
				float time = (float)entry["time"];
				_playerProgress.BestTimes[raceId] = time;
			}
		}
		
		if (data.ContainsKey("total_races"))
			_playerProgress.TotalRaces = (int)data["total_races"];
		if (data.ContainsKey("first_places"))
			_playerProgress.FirstPlaces = (int)data["first_places"];
		if (data.ContainsKey("second_places"))
			_playerProgress.SecondPlaces = (int)data["second_places"];
		if (data.ContainsKey("third_places"))
			_playerProgress.ThirdPlaces = (int)data["third_places"];
		if (data.ContainsKey("total_earnings"))
			_playerProgress.TotalEarnings = (int)data["total_earnings"];
		
		// 加载当前状态
		if (data.ContainsKey("currentState"))
			_currentState = (MountRaceData.RaceState)(int)data["currentState"];
	}
}
