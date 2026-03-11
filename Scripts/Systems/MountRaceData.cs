using Godot;
using System;
using System.Collections.Generic;

public class MountRaceData
{
	public enum RaceState
	{
		Waiting,
		Racing,
		Finished
	}

	public enum RaceStatus
	{
		NotStarted,
		InProgress,
		Completed,
		Disqualified
	}

	public class MountRace
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Difficulty { get; set; } // 1-5
		public float Distance { get; set; } // meters
		public int EntryFee { get; set; }
		public int RewardGold { get; set; }
		public int RewardExp { get; set; }
		public List<string> Checkpoints { get; set; }
		public float RecordTime { get; set; } // best time in seconds

		public MountRace()
		{
			Checkpoints = new List<string>();
		}
	}

	public class PlayerRaceData
	{
		public string RaceId { get; set; }
		public RaceStatus Status { get; set; }
		public float CurrentPosition { get; set; }
		public float CurrentSpeed { get; set; }
		public int CurrentCheckpoint { get; set; }
		public float ElapsedTime { get; set; }
		public bool IsPlayer { get; set; }
		public string MountId { get; set; }
		public string DisplayName { get; set; }

		public PlayerRaceData()
		{
			Status = RaceStatus.NotStarted;
			CurrentPosition = 0f;
			CurrentSpeed = 0f;
			CurrentCheckpoint = 0;
			ElapsedTime = 0f;
		}
	}

	public class PlayerRaceProgress
	{
		public Dictionary<string, List<RaceRecord>> CompletedRaces { get; set; }
		public Dictionary<string, float> BestTimes { get; set; }
		public int TotalRaces { get; set; }
		public int FirstPlaces { get; set; }
		public int SecondPlaces { get; set; }
		public int ThirdPlaces { get; set; }
		public int TotalEarnings { get; set; }

		public PlayerRaceProgress()
		{
			CompletedRaces = new Dictionary<string, List<RaceRecord>>();
			BestTimes = new Dictionary<string, float>();
			TotalRaces = 0;
			FirstPlaces = 0;
			SecondPlaces = 0;
			ThirdPlaces = 0;
			TotalEarnings = 0;
		}
	}

	public class RaceRecord
	{
		public string RaceId { get; set; }
		public string RaceName { get; set; }
		public float Time { get; set; }
		public int Place { get; set; }
		public int Reward { get; set; }
		public long Timestamp { get; set; }

		public RaceRecord()
		{
			Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		}
	}
}
