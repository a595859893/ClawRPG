using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Pets;
using ClawRPG.Scripts.Systems.Pets.AI;
using ClawRPG.Scripts.Systems.PetMimicry;
using ClawRPG.Systems.Pets.AI;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// PetCombatCompanionUI - Event Handlers partial class
    /// Contains all event handlers, refresh methods, and lifecycle methods
    /// </summary>
    public partial class PetCombatCompanionUI
    {
        // ── Tactical Event Handlers ────────────────────────────────────────

        private void OnRefreshDecisionPressed()
        {
            RefreshDecisionTab();
        }

        private void OnReplayFinished()
        {
            // Refresh the decision list when replay finishes (battle ends)
            RefreshDecisionTab();
        }

        private void OnDecisionRecorded(PetDecisionRecord record)
        {
            // Optionally update in real-time, but for now just refresh on battle end
        }

        /// <summary>
        /// Handle tactical mode button press - REQ-112-03
        /// </summary>
        private void OnTacticalModePressed(PetTacticalAI.PetTacticalMode mode)
        {
            var tacticalAI = PetTacticalAI.Instance;
            if (tacticalAI != null)
            {
                tacticalAI.SetTacticalMode(mode);
                AppendDecisionLog($"[玩家] 切换至 {GetModeName(mode)}");
            }
            else
            {
                GD.PushWarning("[PetCombatCompanionUI] PetTacticalAI.Instance is null");
            }
        }

        /// <summary>
        /// Handle tactical mode changes from PetTacticalAI
        /// </summary>
        private void OnPetTacticalModeChanged(PetTacticalAI.PetTacticalMode oldMode, PetTacticalAI.PetTacticalMode newMode)
        {
            _tacticalModeLabel.Text = $"战术模式: {GetModeName(newMode)}";
            UpdateModeButtonHighlight(newMode);
        }

        /// <summary>
        /// Handle tactical decision events - REQ-112-04 Readable Failure
        /// </summary>
        private void OnTacticalDecision(string reason)
        {
            AppendDecisionLog(reason);
        }

        /// <summary>
        /// Append a line to the decision log
        /// </summary>
        private void AppendDecisionLog(string line)
        {
            if (_decisionLogLabel == null) return;

            string existing = _decisionLogLabel.Text;
            if (existing == "等待决策..." || existing == "")
            {
                _decisionLogLabel.Text = line;
            }
            else
            {
                // Keep last 5 lines
                string[] lines = existing.Split('\n');
                if (lines.Length >= 5)
                {
                    var trimmed = new List<string>(lines);
                    while (trimmed.Count >= 5) trimmed.RemoveAt(0);
                    _decisionLogLabel.Text = string.Join("\n", trimmed) + "\n" + line;
                }
                else
                {
                    _decisionLogLabel.Text = existing + "\n" + line;
                }
            }
        }

        /// <summary>
        /// Highlight the active mode button
        /// </summary>
        private void UpdateModeButtonHighlight(PetTacticalAI.PetTacticalMode activeMode)
        {
            _btnFollow.ButtonDisabled = activeMode != PetTacticalAI.PetTacticalMode.Follow;
            _btnProtect.ButtonDisabled = activeMode != PetTacticalAI.PetTacticalMode.Protect;
            _btnAttack.ButtonDisabled = activeMode != PetTacticalAI.PetTacticalMode.Attack;
        }

        private string GetModeName(PetTacticalAI.PetTacticalMode mode)
        {
            return mode switch
            {
                PetTacticalAI.PetTacticalMode.Follow => "跟随",
                PetTacticalAI.PetTacticalMode.Protect => "保护",
                PetTacticalAI.PetTacticalMode.Attack => "进攻",
                _ => "未知"
            };
        }

        // ── Observer Event Handlers ────────────────────────────────────────

        private void OnToggleObserverPressed()
        {
            var observerSystem = AdversarialObserverSystem.Instance;
            if (observerSystem == null) return;

            var state = observerSystem.GetObserverState();
            bool newDisabled = !state.IsDisabled;
            observerSystem.SetEnabled(!newDisabled);

            _btnToggleObserver.Text = newDisabled ? "▶️ 开启" : "🛑 关闭";
            _observerConfidenceLabel.Text = newDisabled ? "已关闭" : "观测中";
        }

        private void OnObserverConfidenceChanged(float confidence)
        {
            RefreshObserverTab();
        }

        private void RefreshObserverTab()
        {
            var observerSystem = AdversarialObserverSystem.Instance;
            if (observerSystem == null || _observerWorldLabel == null) return;

            var assessment = observerSystem.GetCurrentAssessment();
            var goalInference = observerSystem.GetCurrentGoalInference();
            var state = observerSystem.GetObserverState();

            // Update world label
            if (_observerWorldLabel != null && _narrativeModule != null)
            {
                _observerWorldLabel.Text = _narrativeModule.DescribeWorldAssessment(assessment);
            }

            // Update goal label
            if (_observerGoalLabel != null && _narrativeModule != null)
            {
                _observerGoalLabel.Text = _narrativeModule.DescribeGoalInference(goalInference);
            }

            // Update confidence
            if (_observerConfidenceLabel != null)
            {
                float conf = state.PersistentState.Confidence;
                string confStr = conf > 0.75f ? "◆◆◆ 高" : (conf > 0.5f ? "◆◆ 中" : (conf > 0.25f ? "◆ 低" : "◇ 迷茫"));
                _observerConfidenceLabel.Text = $"{confStr} ({conf:P0})";
            }
        }

        // ── Companion System Event Handlers ────────────────────────────────

        private void OnComboChainChanged(string petId, int chain)
        {
            _comboLabel.Text = $"连击链: {chain}";
        }

        private void OnRoleChanged(string petId, string role)
        {
            if (petId == _selectedPetId)
            {
                _roleLabel.Text = $"当前角色: {role}";
            }
        }

        private void OnSyncLevelChanged(string petId, float level)
        {
            if (petId == _selectedPetId || string.IsNullOrEmpty(_selectedPetId))
            {
                int percentage = (int)(level * 100);
                _syncLabel.Text = $"同步率: {percentage}%";
                _syncBar.Value = percentage;
            }
        }

        private void OnComboExecuted(string petId, ComboType type, float damage)
        {
            GD.Print($"Combo executed: {type} for {damage} damage");
        }

        private void OnLearningUpdated(string petId, string updateType)
        {
            RefreshLearningTab();
        }

        // ── Refresh Methods ───────────────────────────────────────────────

        private void RefreshUI()
        {
            if (_companionSystem == null || string.IsNullOrEmpty(_selectedPetId))
            {
                // Show overall stats
                var stats = _companionSystem.GetStatistics();
                _syncLabel.Text = "同步率: -";
                _syncBar.Value = 0;
                _comboLabel.Text = $"总连击数: {stats["total_combos"]}";
                _roleLabel.Text = "宠物数量: " + stats["pet_count"];
            }
            else
            {
                // Show pet-specific stats
                var stats = _companionSystem.GetPetStatistics(_selectedPetId);

                if (stats.ContainsKey("sync_level"))
                {
                    float sync = (float)stats["sync_level"];
                    int percentage = (int)(sync * 100);
                    _syncLabel.Text = $"同步率: {percentage}%";
                    _syncBar.Value = percentage;
                }

                if (stats.ContainsKey("current_combo_chain"))
                {
                    _comboLabel.Text = $"连击链: {stats["current_combo_chain"]}";
                }

                if (stats.ContainsKey("role"))
                {
                    _roleLabel.Text = $"当前角色: {stats["role"]}";
                }
            }

            RefreshStatsTab();
            RefreshLearningTab();
            RefreshPersonalityTab();
            RefreshPerformanceTab();
        }

        private void RefreshTacticalUI()
        {
            var tacticalAI = PetTacticalAI.Instance;
            if (tacticalAI != null)
            {
                var mode = tacticalAI.GetCurrentMode();
                _tacticalModeLabel.Text = $"战术模式: {GetModeName(mode)}";
                UpdateModeButtonHighlight(mode);
            }
        }

        private void RefreshStatsTab()
        {
            if (_companionSystem == null) return;

            string text = "=== 战斗统计 ===\n\n";

            var overallStats = _companionSystem.GetStatistics();
            text += $"总连击次数: {overallStats["total_combos"]}\n";
            text += $"总连击伤害: {overallStats["total_combo_damage"]:F1}\n";
            text += $"最高连击链: {overallStats["highest_combo_chain"]}\n";
            text += $"激活宠物数: {overallStats["pet_count"]}\n";

            _statsLabel.Text = text;
        }

        private void RefreshLearningTab()
        {
            if (_companionSystem == null || string.IsNullOrEmpty(_selectedPetId)) return;

            string text = "=== 学习数据 ===\n\n";

            var learning = _companionSystem.GetLearningReport(_selectedPetId);

            foreach (var kvp in learning)
            {
                text += $"{kvp.Key}: {kvp.Value}\n";
            }

            _learningLabel.Text = text;
        }

        private void RefreshPersonalityTab()
        {
            if (_personalityTab == null) return;

            var mimicryData = PetMimicryData.Instance;
            if (mimicryData == null) return;

            // Determine personality card type
            var dominant = mimicryData.GetDominantBehavior();
            string typeName;
            string description;

            if (dominant.HasValue)
            {
                typeName = GetPersonalityTypeName(dominant.Value);
                description = GetPersonalityDescription(dominant.Value);
            }
            else
            {
                typeName = "尚未形成个性";
                description = "宠物正在观察你的行为...";
            }

            _personalityTypeLabel.Text = typeName;
            _personalityDescLabel.Text = description;

            // Rebuild imprint list
            foreach (var child in _imprintListContainer.GetChildren())
            {
                child.QueueFree();
            }

            var ranking = mimicryData.GetBehaviorRanking();
            if (ranking.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "暂无印记记录",
                    Modulate = new Color(0.5f, 0.5f, 0.5f)
                };
                _imprintListContainer.AddChild(emptyLabel);
                return;
            }

            foreach (var (behavior, level) in ranking)
            {
                var row = CreateImprintRow(behavior, level);
                _imprintListContainer.AddChild(row);
            }
        }

        private void RefreshDecisionTab()
        {
            // Clear existing entries
            foreach (var child in _decisionList.GetChildren())
            {
                child.QueueFree();
            }

            var replaySystem = PetReplayTraceSystem.Instance;
            if (replaySystem == null)
            {
                _decisionList.AddChild(new Label { Text = "PetReplayTraceSystem 不可用" });
                return;
            }

            var records = replaySystem.GetCurrentBattleRecords();
            if (records.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "暂无决策记录\n请先进行一场战斗",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                _decisionList.AddChild(emptyLabel);
                return;
            }

            // Add each decision as a card
            foreach (var record in records)
            {
                AddDecisionCard(record);
            }
        }

        private void RefreshPerformanceTab()
        {
            if (_performanceTab == null || PetPerformanceData.Instance == null)
                return;

            var perfData = PetPerformanceData.Instance;
            var comparison = perfData.GetComparison();

            int petCount = perfData.GetPetAssistedCount();
            int soloCount = perfData.GetSoloCount();

            if (!comparison.HasEnoughData)
            {
                _performanceSummaryLabel.Text = "数据收集中...\n\n使用宠物参与战斗，我会记录通关数据。\n收集足够的样本后，我会展示宠物对我战斗表现的帮助。";
                _performanceSummaryLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
                _performanceTimeLabel.Text = "⏱ 平均时间: 待收集";
                _performanceHpLabel.Text = "❤️ 平均HP损耗: 待收集";
                _performanceWinRateLabel.Text = "🏆 胜率对比: 待收集";
                _performanceSampleLabel.Text = $"样本数: {petCount}宠物 / {soloCount}独战 (需要各5个样本)";
                return;
            }

            // 有足够数据，显示对比
            _performanceSummaryLabel.Modulate = new Color(1f, 0.9f, 0.5f);

            string timeStr = comparison.TimeSavedPerRoom >= 0
                ? $"宠物帮我平均节省 {comparison.TimeSavedPerRoom:F1}秒/房间"
                : $"宠物参战时平均多花 {-comparison.TimeSavedPerRoom:F1}秒/房间";

            string hpStr = comparison.HpSavedPerRoom >= 0
                ? $"宠物帮我平均节省 {comparison.HpSavedPerRoom}HP/房间"
                : $"宠物参战时多损耗 {-comparison.HpSavedPerRoom}HP/房间";

            string winStr = $"宠物模式胜率 {comparison.WinRatePetAssisted:P0} vs 独战胜率 {comparison.WinRateSolo:P0}";

            _performanceSummaryLabel.Text = "=== 宠物价值报告 ===\n\n" +
                $"📊 基于 {petCount}次宠物参战 vs {soloCount}次独战数据";

            _performanceTimeLabel.Text = $"⏱ {timeStr}";
            _performanceTimeLabel.Modulate = comparison.TimeSavedPerRoom >= 0
                ? new Color(0.5f, 1f, 0.5f)
                : new Color(1f, 0.5f, 0.5f);

            _performanceHpLabel.Text = $"❤️ {hpStr}";
            _performanceHpLabel.Modulate = comparison.HpSavedPerRoom >= 0
                ? new Color(0.5f, 1f, 0.5f)
                : new Color(1f, 0.5f, 0.5f);

            _performanceWinRateLabel.Text = $"🏆 {winStr}";
            _performanceSampleLabel.Text = $"样本数: {petCount}宠物 / {soloCount}独战 ✓";
            _performanceSampleLabel.Modulate = new Color(0.5f, 1f, 0.5f);
        }

        // ── Decision Card Helpers ──────────────────────────────────────────

        private void AddDecisionCard(PetDecisionRecord record)
        {
            var card = new PanelContainer();
            var cardStyle = new StyleBoxFlat
            {
                BgColor = GetDecisionColor(record.Outcome).WithAlpha(0.15f)
            };
            cardStyle.SetBorderWidthAll(1);
            cardStyle.BorderColor = GetDecisionColor(record.Outcome).WithAlpha(0.4f);
            cardStyle.SetCornerRadiusAll(4);
            card.AddThemeStyleboxOverride("panel", cardStyle);
            _decisionList.AddChild(card);

            var cardVBox = new VBoxContainer();
            card.AddChild(cardVBox);

            // Header: tick + type + outcome icon
            var headerHBox = new HBoxContainer();
            cardVBox.AddChild(headerHBox);

            var tickLabel = new Label
            {
                Text = $"[Tick {record.TickId}]",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            tickLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
            tickLabel.AddThemeFontSizeOverride("font_size", 14);
            headerHBox.AddChild(tickLabel);

            var typeLabel = new Label
            {
                Text = GetDecisionTypeLabel(record.Type),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            typeLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            typeLabel.AddThemeFontSizeOverride("font_size", 13);
            headerHBox.AddChild(typeLabel);

            var outcomeLabel = new Label
            {
                Text = GetOutcomeIcon(record.Outcome),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            outcomeLabel.AddThemeColorOverride("font_color", GetDecisionColor(record.Outcome));
            headerHBox.AddChild(outcomeLabel);

            // State transition info
            if (record.Type == PetDecisionRecord.DecisionType.StateTransition)
            {
                var stateLabel = new Label
                {
                    Text = $"{record.StateBefore} → {record.StateAfter}",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                stateLabel.AddThemeFontSizeOverride("font_size", 13);
                stateLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.75f, 0.75f));
                cardVBox.AddChild(stateLabel);
            }

            // Target info
            if (!string.IsNullOrEmpty(record.TargetName) && record.TargetName != "null")
            {
                var targetLabel = new Label
                {
                    Text = $"目标: {record.TargetName} ({record.TargetDistance:F0}px)",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                targetLabel.AddThemeFontSizeOverride("font_size", 13);
                cardVBox.AddChild(targetLabel);
            }

            // Reason
            if (!string.IsNullOrEmpty(record.Reason))
            {
                var reasonLabel = new Label
                {
                    Text = record.Reason,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    AutowrapMode = TextServer.AutowrapMode.WordSmart
                };
                reasonLabel.AddThemeFontSizeOverride("font_size", 12);
                reasonLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.65f, 0.65f));
                cardVBox.AddChild(reasonLabel);
            }
        }

        private static string GetDecisionTypeLabel(PetDecisionRecord.DecisionType type)
        {
            return type switch
            {
                PetDecisionRecord.DecisionType.StateTransition => "🔄 状态切换",
                PetDecisionRecord.DecisionType.TargetSelection => "🎯 目标选择",
                PetDecisionRecord.DecisionType.BehaviorExecution => "⚡ 行为执行",
                _ => "❓ 未知"
            };
        }

        private static string GetOutcomeIcon(PetDecisionRecord.DecisionOutcome outcome)
        {
            return outcome switch
            {
                PetDecisionRecord.DecisionOutcome.Success => "✅",
                PetDecisionRecord.DecisionOutcome.Failure => "❌",
                PetDecisionRecord.DecisionOutcome.Cancelled => "⭕",
                _ => "⚪"
            };
        }

        private static Color GetDecisionColor(PetDecisionRecord.DecisionOutcome outcome)
        {
            return outcome switch
            {
                PetDecisionRecord.DecisionOutcome.Success => new Color(0.3f, 0.9f, 0.3f),
                PetDecisionRecord.DecisionOutcome.Failure => new Color(0.9f, 0.3f, 0.2f),
                PetDecisionRecord.DecisionOutcome.Cancelled => new Color(0.9f, 0.7f, 0.2f),
                _ => new Color(0.6f, 0.6f, 0.6f)
            };
        }

        // ── Personality Helpers ────────────────────────────────────────────

        private string GetPersonalityTypeName(PlayerBehaviorType behavior)
        {
            return behavior switch
            {
                PlayerBehaviorType.UseFireSkill => "🔥 火焰使者",
                PlayerBehaviorType.UseIceSkill => "❄️ 冰霜使者",
                PlayerBehaviorType.UseElectricSkill => "⚡ 雷电使者",
                PlayerBehaviorType.UseShadowSkill => "🌙 暗影使者",
                PlayerBehaviorType.UseHolySkill => "✨ 神圣使者",
                PlayerBehaviorType.UseNatureSkill => "🌿 自然使者",
                PlayerBehaviorType.FrequentDodge => "💨 闪避大师",
                PlayerBehaviorType.AggressiveAttack => "⚔️ 战斗狂人",
                PlayerBehaviorType.DefensiveStance => "🛡️ 守护者",
                PlayerBehaviorType.LowHPAggression => "💀 背水一战",
                PlayerBehaviorType.QuickRetreat => "🏃 撤退专家",
                PlayerBehaviorType.FocusElite => "🎯 精英猎手",
                PlayerBehaviorType.AvoidCombat => "🔍 规避战士",
                PlayerBehaviorType.TriggerTrap => "⚙️ 陷阱触发者",
                PlayerBehaviorType.SolvePuzzle => "🧩 解谜专家",
                PlayerBehaviorType.CollectLoot => "💰 收藏家",
                PlayerBehaviorType.UseHealing => "💚 治愈师",
                PlayerBehaviorType.PetSynergy => "🐾 协战伙伴",
                PlayerBehaviorType.SpecialInteraction => "🌟 特殊互动者",
                _ => "❓ 未知性格"
            };
        }

        private string GetPersonalityDescription(PlayerBehaviorType behavior)
        {
            return behavior switch
            {
                PlayerBehaviorType.UseFireSkill => "你的火系法术给宠物留下了灼烧的印象",
                PlayerBehaviorType.UseIceSkill => "你的冰系控制让宠物学会了冰霜护体",
                PlayerBehaviorType.UseElectricSkill => "你的闪电战术被宠物记在心里",
                PlayerBehaviorType.UseShadowSkill => "你的暗系能力让宠物学会了潜行",
                PlayerBehaviorType.UseHolySkill => "你的神圣力量启发了宠物",
                PlayerBehaviorType.UseNatureSkill => "你对自然的亲近感染了宠物",
                PlayerBehaviorType.FrequentDodge => "你灵活的走位是宠物的教材",
                PlayerBehaviorType.AggressiveAttack => "你的激进打法激励了宠物",
                PlayerBehaviorType.DefensiveStance => "宠物从你身上学到了防守",
                PlayerBehaviorType.LowHPAggression => "你在低血量时的勇敢震撼了宠物",
                PlayerBehaviorType.QuickRetreat => "你的战术撤退被宠物效仿",
                PlayerBehaviorType.FocusElite => "你优先击杀精英的策略被宠物观察",
                PlayerBehaviorType.AvoidCombat => "你规避战斗的方式影响了宠物",
                PlayerBehaviorType.TriggerTrap => "你触发陷阱的行为被宠物记住",
                PlayerBehaviorType.SolvePuzzle => "你解谜的能力启发了宠物",
                PlayerBehaviorType.CollectLoot => "你收集战利品的习惯传染给了宠物",
                PlayerBehaviorType.UseHealing => "你的治疗本能被宠物继承",
                PlayerBehaviorType.PetSynergy => "你经常与宠物协同作战，宠物更加信任你",
                PlayerBehaviorType.SpecialInteraction => "你的特殊互动方式被宠物铭记",
                _ => "宠物正在形成独特个性"
            };
        }

        private Control CreateImprintRow(PlayerBehaviorType behavior, int level)
        {
            var hbox = new HBoxContainer();
            hbox.CustomMinimumSize = new Vector2(380, 28);

            var nameLabel = new Label
            {
                Text = GetBehaviorDisplayName(behavior),
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 12);
            hbox.AddChild(nameLabel);

            // Level stars
            var starsLabel = new Label
            {
                Text = new string('★', level) + new string('☆', 5 - level),
                VerticalAlignment = VerticalAlignment.Center,
                Modulate = GetLevelColor(level)
            };
            starsLabel.AddThemeFontSizeOverride("font_size", 12);
            hbox.AddChild(starsLabel);

            return hbox;
        }

        private string GetBehaviorDisplayName(PlayerBehaviorType behavior)
        {
            return behavior switch
            {
                PlayerBehaviorType.UseFireSkill => "🔥 火系",
                PlayerBehaviorType.UseIceSkill => "❄️ 冰系",
                PlayerBehaviorType.UseElectricSkill => "⚡ 电系",
                PlayerBehaviorType.UseShadowSkill => "🌙 暗系",
                PlayerBehaviorType.UseHolySkill => "✨ 神圣",
                PlayerBehaviorType.UseNatureSkill => "🌿 自然",
                PlayerBehaviorType.FrequentDodge => "💨 闪避",
                PlayerBehaviorType.AggressiveAttack => "⚔️ 激进攻击",
                PlayerBehaviorType.DefensiveStance => "🛡️ 防守姿态",
                PlayerBehaviorType.LowHPAggression => "💀 背水一战",
                PlayerBehaviorType.QuickRetreat => "🏃 快速撤退",
                PlayerBehaviorType.FocusElite => "🎯 精英猎手",
                PlayerBehaviorType.AvoidCombat => "🔍 规避战斗",
                PlayerBehaviorType.TriggerTrap => "⚙️ 触发陷阱",
                PlayerBehaviorType.SolvePuzzle => "🧩 解谜",
                PlayerBehaviorType.CollectLoot => "💰 收集战利品",
                PlayerBehaviorType.UseHealing => "💚 治疗",
                PlayerBehaviorType.PetSynergy => "🐾 协战",
                PlayerBehaviorType.SpecialInteraction => "🌟 特殊互动",
                _ => behavior.ToString()
            };
        }

        private Color GetLevelColor(int level)
        {
            return level switch
            {
                0 => new Color(0.4f, 0.4f, 0.4f),
                1 => new Color(0.6f, 0.9f, 0.6f),
                2 => new Color(0.9f, 0.9f, 0.4f),
                3 => new Color(1f, 0.7f, 0.3f),
                4 => new Color(1f, 0.5f, 0.2f),
                5 => new Color(1f, 0.3f, 0.3f),
                _ => Colors.White
            };
        }

        // ── Health & Synergy UI ────────────────────────────────────────────

        public override void _Process(double delta)
        {
            // Update health bars from PetTacticalAI state
            RefreshHealthDisplay();
        }

        /// <summary>
        /// Refresh health display from PetTacticalAI
        /// </summary>
        private void RefreshHealthDisplay()
        {
            var tacticalAI = PetTacticalAI.Instance;
            if (tacticalAI == null) return;

            // Pet health
            float petHP = tacticalAI.GetPetHealthPercent();
            int petPercent = (int)(petHP * 100);
            _petHealthLabel.Text = $"宠物: {petPercent}%";
            _petHealthBar.Value = petPercent;

            // Player health
            float playerHP = tacticalAI.GetPlayerHealthPercent();
            int playerPercent = (int)(playerHP * 100);
            _playerHealthLabel.Text = $"玩家: {playerPercent}%";
            _playerHealthBar.Value = playerPercent;
        }

        /// <summary>
        /// 更新协同攻击计数器 UI（由 PetSynergyTracker 调用）
        /// </summary>
        public void UpdateSynergyCounter(int count, int threshold, bool active, float remaining)
        {
            if (_synergyCounterLabel == null) return;

            if (active)
            {
                _synergyCounterLabel.Text = $"⚡ 协同激活！剩余 {remaining:F0}s (+10%)";
                _synergyCounterLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.1f));
            }
            else
            {
                _synergyCounterLabel.Text = $"协同攻击: {count}/{threshold}";
                if (count >= threshold - 1)
                {
                    _synergyCounterLabel.AddThemeColorOverride("font_color", new Color(1f, 0.5f, 0.2f));
                }
                else
                {
                    _synergyCounterLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
                }
            }
        }

        /// <summary>
        /// 显示协同增益爆发特效（由 PetSynergyTracker 调用）
        /// </summary>
        public void ShowSynergyBurst()
        {
            if (_synergyBurstPanel == null) return;

            _synergyBurstPanel.Visible = true;

            // 2秒后自动隐藏
            var timer = new Timer { OneShot = true, WaitTime = 2f };
            timer.Timeout += () => {
                if (_synergyBurstPanel != null)
                {
                    _synergyBurstPanel.Visible = false;
                }
                timer.QueueFree();
            };
            AddChild(timer);
            timer.Start();

            // 淡出动画
            var tween = CreateTween();
            tween.TweenInterval(1.5f);
            tween.TweenProperty(_synergyBurstPanel, "modulate:a", 0f, 0.5f);
        }

        // ── Lifecycle ─────────────────────────────────────────────────────

        public override void _Notification(int what)
        {
            if (what == NotificationExitTree)
            {
                if (_companionSystem != null)
                {
                    _companionSystem.ComboChainChanged -= OnComboChainChanged;
                    _companionSystem.RoleChanged -= OnRoleChanged;
                    _companionSystem.SyncLevelChanged -= OnSyncLevelChanged;
                    _companionSystem.ComboExecuted -= OnComboExecuted;
                    _companionSystem.LearningUpdated -= OnLearningUpdated;
                }

                var tacticalAI = PetTacticalAI.Instance;
                if (tacticalAI != null)
                {
                    tacticalAI.OnTacticalModeChanged -= OnPetTacticalModeChanged;
                    tacticalAI.OnTacticalDecision -= OnTacticalDecision;
                }
            }
        }
    }
}
