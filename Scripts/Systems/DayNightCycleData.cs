using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Day/Night Cycle data for save/load
    /// </summary>
    [Serializable]
    public class DayNightCycleData {
        public float CurrentTime;
        public float TimeScale;
        public float DayDuration;
        public bool IsEnabled;
        
        public DayNightCycleData() {
            CurrentTime = 12f; // Start at noon
            TimeScale = 1f;
            DayDuration = 600f; // 10 minutes
            IsEnabled = true;
        }
    }
}
