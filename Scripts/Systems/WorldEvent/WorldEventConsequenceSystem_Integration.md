// WorldEventConsequenceSystem_Integration.cs
// REQ-197: WorldEvent因果事件链 — WorldEventSystem.cs 集成说明
// 以下是需要手动添加到 WorldEventSystem.cs 的修改
// 最小侵入：只添加调用，不修改核心逻辑

// ============ 1. 在文件顶部添加 using（如果还没有） ============
// 确保有这行：
// using ClawRPG.Core.Systems;

// ============ 2. 添加信号声明（在 class 内添加）============
// 在现有信号声明区域添加：

// 因果系统集成信号
public Action<WorldEventType, EventOutcome> OnEventOutcomeRecorded;

// ============ 3. 添加公共属性（获取因果系统实例）============
// 在 class 内添加：
/*
        /// <summary>
        /// 获取因果系统实例（用于外部调用）
        /// </summary>
        public WorldEventConsequenceSystem ConsequenceSystem => WorldEventConsequenceSystem.Instance;
*/

// ============ 4. 修改 TrySpawnEvent() — 修复 SpawnChance Bug ============
// 找到 TrySpawnEvent() 中的这段错误代码：
/*
                if (!recentlySpawned && config.SpawnChance > _random.NextDouble())
                {
                    availableEvents.Add(config);
                }
*/
// 替换为：
/*
                if (!recentlySpawned)
                {
                    // 使用因果系统调整触发概率（怨念加成 + 印记加成）
                    float adjustedChance = config.SpawnChance;
                    if (ConsequenceSystem != null)
                    {
                        adjustedChance = ConsequenceSystem.GetAdjustedSpawnChance(config.Type, config.SpawnChance);
                        // 印记额外加成
                        adjustedChance += ConsequenceSystem.GetMarkSpawnBonus(config.Type);
                    }

                    if (adjustedChance > _random.NextDouble())
                    {
                        availableEvents.Add(config);
                    }
                }
*/

// ============ 5. 修改 CompleteEvent() — 记录成功结果 ============
// 在 CompleteEvent() 方法末尾（OnEventCompleted?.Invoke(evt) 之前）添加：
/*
            // REQ-197: 记录成功因果
            ConsequenceSystem?.RecordOutcome(evt.Type, EventOutcome.Success);

            // 触发叙事文字（如果有）
            var narrative = ConsequenceSystem?.GetConsequenceNarrative(evt.Type);
            if (!string.IsNullOrEmpty(narrative))
            {
                OnEventOutcomeRecorded?.Invoke(evt.Type, EventOutcome.Success);
            }
*/

// ============ 6. 修改 FailEvent() — 记录失败结果 ============
// 在 FailEvent() 方法末尾（OnEventFailed?.Invoke(evt) 之前）添加：
/*
            // REQ-197: 记录失败因果（怨念）
            ConsequenceSystem?.RecordOutcome(evt.Type, EventOutcome.Failed);

            // 触发叙事文字（怨念升级）
            var narrative = ConsequenceSystem?.GetConsequenceNarrative(evt.Type);
            if (!string.IsNullOrEmpty(narrative))
            {
                OnEventOutcomeRecorded?.Invoke(evt.Type, EventOutcome.Failed);
            }
*/

// ============ 7. 添加 SkipEvent() 方法 — 记录跳过结果 ============
// 在 FailEvent() 方法之后添加：
/*
        /// <summary>
        /// 玩家跳过世界事件（忽略 Merchant/拒绝 Blessing/逃离 Invasion）
        /// </summary>
        public void SkipEvent(string eventId)
        {
            var evt = _activeEvents.Find(e => e.EventId == eventId);
            if (evt == null) return;

            // 从活跃列表移除
            _activeEvents.Remove(evt);

            // 记录为跳过（产生债务）
            ConsequenceSystem?.RecordOutcome(evt.Type, EventOutcome.Skipped);

            var narrative = ConsequenceSystem?.GetConsequenceNarrative(evt.Type);
            if (!string.IsNullOrEmpty(narrative))
            {
                OnEventOutcomeRecorded?.Invoke(evt.Type, EventOutcome.Skipped);
            }

            // 触发债务警告信号
            OnEventFailed?.Invoke(evt);
        }
*/

// ============ 8. 修改 GetEventConfig() — 附加因果信息 ============
// 修改 GetEventConfig 返回值，添加怨念信息到描述：
/*
        /// <summary>
        /// 获取事件配置（附加因果信息）
        /// </summary>
        public WorldEventConfig GetEventConfig(string configId)
        {
            var config = _eventConfigs.ContainsKey(configId) ? _eventConfigs[configId] : null;
            if (config != null && ConsequenceSystem != null)
            {
                // 可以在这里修改 config.Description 添加因果叙事
                // 但由于 WorldEventConfig 是值对象，直接修改会影响缓存
                // 更好的方式是在 UI 层处理
            }
            return config;
        }
*/

// ============ 9. 添加 ExportSaveData/ImportSaveData 因果系统调用 ============
// 在 ExportSaveData() 中添加：
/*
            // REQ-197: 因果系统持久化
            if (ConsequenceSystem != null)
            {
                var consequenceData = ConsequenceSystem.ExportSaveData();
                data["consequenceSystem"] = consequenceData;
            }
*/
// 在 ImportSaveData() 中添加：
/*
            // REQ-197: 因果系统持久化恢复
            if (data.Contains("consequenceSystem") && ConsequenceSystem != null)
            {
                ConsequenceSystem.ImportSaveData(data["consequenceSystem"] as Dictionary<string, object>);
            }
*/

// ============ 10. 债务触发检查（SafeHouse 进入时）============
// 在进入 SafeHouse 的逻辑处（如果有独立方法）添加：
/*
            // 检查活跃债务
            if (ConsequenceSystem != null)
            {
                var debts = ConsequenceSystem.CheckActiveDebts(GetCurrentPlayerLevel());
                foreach (var debt in debts)
                {
                    // 在事件描述中添加债务叙事
                    var narrative = ConsequenceSystem.GetDebtNarrative(debt.EventType, GetCurrentPlayerLevel());
                    // 显示在事件描述中
                }
            }
*/

// ============ 修复 SpawnChance 逻辑 Bug ============
// 原始错误：
// if ((now - _lastEventCheck).TotalSeconds >= _baseEventInterval)
// {
//     _lastEventCheck = now;
//     if (_random.NextDouble() < _spawnChance && _activeEvents.Count < 5)  // ← BUG: AND 条件导致概率相乘
//     {
//         TrySpawnEvent();
//     }
// }
//
// 正确应该是：
// if ((now - _lastEventCheck).TotalSeconds >= _baseEventInterval)
// {
//     _lastEventCheck = now;
//     if (_random.NextDouble() < _spawnChance && _activeEvents.Count < 5)
//     {
//         TrySpawnEvent();  // TrySpawnEvent 内部已经检查 config.SpawnChance
//     }
// }
//
// 或者将外层判断移除，直接用内层：
// if ((now - _lastEventCheck).TotalSeconds >= _baseEventInterval)
// {
//     _lastEventCheck = now;
//     if (_activeEvents.Count < 5)
//     {
//         TrySpawnEvent();
//     }
// }
