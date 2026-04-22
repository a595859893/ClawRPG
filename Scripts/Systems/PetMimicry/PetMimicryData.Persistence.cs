using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// PetMimicryData — 持久化
    /// </summary>
    public partial class PetMimicryData
    {
        public Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            var imprintList = new List<Dictionary>();
            foreach (var imprint in _imprints)
            {
                imprintList.Add(new Dictionary
                {
                    { "envType", (int)imprint.EnvironmentType },
                    { "behaviorType", (int)imprint.BehaviorType },
                    { "level", imprint.ImprintLevel },
                    { "xp", imprint.Xp },
                    { "lastRecorded", imprint.LastRecordedAt.ToString("o") },
                    { "totalTriggers", imprint.TotalTriggers },
                    { "fidelity", imprint.Fidelity },
                    { "decayTimer", imprint.DecayTimer }
                });
            }
            data["imprints"] = imprintList;
            return data;
        }

        public void ImportSaveData(Dictionary<string, object> data)
        {
            _imprints.Clear();
            _highestBehaviorLevel.Clear();
            _eventDrivenBonus.Clear();

            if (data == null || !data.ContainsKey("imprints")) return;

            var imprintList = (Godot.Collections.Array)data["imprints"];
            foreach (Dictionary imprintData in imprintList)
            {
                var imprint = new BehaviorImprint
                {
                    EnvironmentType = (RoomEnvironmentType)(int)imprintData["envType"],
                    BehaviorType = (PlayerBehaviorType)(int)imprintData["behaviorType"],
                    ImprintLevel = (int)imprintData["level"],
                    Xp = (float)(double)imprintData["xp"],
                    LastRecordedAt = DateTime.Parse((string)imprintData["lastRecorded"]),
                    TotalTriggers = (int)imprintData["totalTriggers"],
                    Fidelity = imprintData.ContainsKey("fidelity") ? (float)(double)imprintData["fidelity"] : 0.5f,
                    DecayTimer = imprintData.ContainsKey("decayTimer") ? (float)(double)imprintData["decayTimer"] : 0f
                };
                _imprints.Add(imprint);
                UpdateHighestLevel(imprint.BehaviorType, imprint.ImprintLevel);
                if (imprint.LastRecordedAt > _mostRecentRecordTime)
                    _mostRecentRecordTime = imprint.LastRecordedAt;
            }

            GD.Print($"[PetMimicryData] Loaded {_imprints.Count} behavior imprints from save");
        }
    }
}
