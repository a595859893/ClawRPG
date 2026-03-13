using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    public partial class MountRacingUI : Control {
        private MountRacingSystem _system;
        private MountRacingDatabase _database;
        private MountRacingData _data;
        
        private TabContainer _tabContainer;
        private VBoxContainer _tracksTab;
        private VBoxContainer _raceTab;
        private VBoxContainer _statsTab;
        
        private OptionButton _trackSelector;
        private OptionButton _mountSelector;
        private Label _trackInfoLabel;
        private Label _resultLabel;
        private Label _statsLabel;
        
        private int _selectedTrackIndex = -1;
        
        public void Initialize(MountRacingSystem system, MountRacingDatabase database, MountRacingData data) {
            _system = system;
            _database = database;
            _data = data;
            
            SetupUI();
            RefreshTracks();
            RefreshStats();
        }
        
        private void SetupUI() {
            // Main container
            var mainVBox = new VBoxContainer();
            mainVBox.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainVBox.CustomMinimumSize = new Vector2(600, 500);
            AddChild(mainVBox);
            
            // Title
            var title = new Label();
            title.Text = "🐎 Mount Racing";
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 24);
            mainVBox.AddChild(title);
            
            // Tab container
            _tabContainer = new TabContainer();
            _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            mainVBox.AddChild(_tabContainer);
            
            // Tracks tab
            _tracksTab = new VBoxContainer();
            _tracksTab.Name = "Tracks";
            _tabContainer.AddChild(_tracksTab);
            SetupTracksTab();
            
            // Race tab
            _raceTab = new VBoxContainer();
            _raceTab.Name = "Race";
            _tabContainer.AddChild(_raceTab);
            SetupRaceTab();
            
            // Stats tab
            _statsTab = new VBoxContainer();
            _statsTab.Name = "Statistics";
            _tabContainer.AddChild(_statsTab);
            SetupStatsTab();
            
            // Close button
            var closeBtn = new Button();
            closeBtn.Text = "Close (ESC)";
            closeBtn.Pressed += () => Visible = false;
            mainVBox.AddChild(closeBtn);
        }
        
        private void SetupTracksTab() {
            var scroll = new ScrollContainer();
            scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _tracksTab.AddChild(scroll);
            
            var list = new VBoxContainer();
            list.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            scroll.AddChild(list);
            
            var tracks = _system.GetAllTracks();
            foreach (var trackId in tracks) {
                var config = _system.GetTrackConfig(trackId);
                if (config == null) continue;
                
                var trackCard = CreateTrackCard(trackId, config);
                list.AddChild(trackCard);
            }
        }
        
        private Control CreateTrackCard(string trackId, TrackConfig config) {
            var card = new PanelContainer();
            card.CustomMinimumSize = new Vector2(500, 80);
            
            var hbox = new HBoxContainer();
            card.AddChild(hbox);
            
            // Track info
            var info = new VBoxContainer();
            info.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hbox.AddChild(info);
            
            var nameLabel = new Label();
            nameLabel.Text = $"🏁 {config.Name}";
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            info.AddChild(nameLabel);
            
            var descLabel = new Label();
            descLabel.Text = config.Description;
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            info.AddChild(descLabel);
            
            var statsLabel = new Label();
            var isUnlocked = _data.UnlockedTracks.Contains(trackId);
            var bestTime = _system.GetBestTime(trackId);
            var totalRaces = _data.TotalRaces.ContainsKey(trackId) ? _data.TotalRaces[trackId] : 0;
            var wins = _system.GetWinCount(trackId);
            
            string status = isUnlocked 
                ? $"✅ Unlocked | Length: {config.Length}m | ⏱ Best: {(bestTime > 0 ? bestTime + "s" : "N/A")} | 🏆 {wins}/{totalRaces} wins"
                : $"🔒 Locked | Unlock: Complete previous track";
            statsLabel.Text = status;
            statsLabel.AddThemeFontSizeOverride("font_size", 11);
            info.AddChild(statsLabel);
            
            // Difficulty badge
            var diffLabel = new Label();
            diffLabel.Text = config.Difficulty.ToString();
            diffLabel.AddThemeFontSizeOverride("font_size", 14);
            hbox.AddChild(diffLabel);
            
            return card;
        }
        
        private void SetupRaceTab() {
            var raceVBox = new VBoxContainer();
            raceVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            raceVBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _raceTab.AddChild(raceVBox);
            
            // Track selection
            var trackLabel = new Label();
            trackLabel.Text = "Select Track:";
            trackLabel.AddThemeFontSizeOverride("font_size", 16);
            raceVBox.AddChild(trackLabel);
            
            _trackSelector = new OptionButton();
            _trackSelector.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _trackSelector.ItemSelected += OnTrackSelected;
            raceVBox.AddChild(_trackSelector);
            
            // Mount selection  
            var mountLabel = new Label();
            mountLabel.Text = "Select Mount:";
            mountLabel.AddThemeFontSizeOverride("font_size", 16);
            raceVBox.AddChild(mountLabel);
            
            _mountSelector = new OptionButton();
            _mountSelector.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            // Add dummy mount options
            _mountSelector.AddItem("Thunder Hoof (Speed: 80, Stamina: 70)", 0);
            _mountSelector.AddItem("Swift Wind (Speed: 90, Stamina: 50)", 1);
            _mountSelector.AddItem("Iron Steed (Speed: 60, Stamina: 90)", 2);
            _mountSelector.AddItem("Mystic Wings (Speed: 85, Stamina: 75)", 3);
            raceVBox.AddChild(_mountSelector);
            
            // Track info
            _trackInfoLabel = new Label();
            _trackInfoLabel.Text = "Select a track to see details";
            _trackInfoLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            raceVBox.AddChild(_trackInfoLabel);
            
            // Race button
            var raceBtn = new Button();
            raceBtn.Text = "🐎 START RACE";
            raceBtn.CustomMinimumSize = new Vector2(200, 50);
            raceBtn.Pressed += OnRacePressed;
            raceVBox.AddChild(raceBtn);
            
            // Result display
            _resultLabel = new Label();
            _resultLabel.Text = "";
            _resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _resultLabel.AddThemeFontSizeOverride("font_size", 20);
            raceVBox.AddChild(_resultLabel);
        }
        
        private void SetupStatsTab() {
            _statsLabel = new Label();
            _statsLabel.Text = "Loading statistics...";
            _statsLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _statsTab.AddChild(_statsLabel);
        }
        
        private void RefreshTracks() {
            // Refresh is handled by recreating the tab
        }
        
        private void RefreshStats() {
            var stats = _system.GetStatistics();
            
            string statsText = "📊 Racing Statistics\n\n";
            statsText += $"Total Races: {stats.TotalRaces}\n";
            statsText += $"Total Wins: {stats.TotalWins}\n";
            statsText += $"Win Rate: {stats.WinRate:F1}%\n";
            statsText += $"Tracks Unlocked: {stats.TracksUnlocked}/8\n\n";
            statsText += $"💰 Total Gold Earned: {stats.TotalGoldEarned}\n";
            statsText += $"⭐ Total Experience: {stats.TotalExpEarned}\n\n";
            
            if (stats.BestTimes.Count > 0) {
                statsText += "🏆 Best Times:\n";
                foreach (var time in stats.BestTimes) {
                    statsText += $"  {time.Key}: {time.Value}s\n";
                }
            }
            
            _statsLabel.Text = statsText;
        }
        
        private void OnTrackSelected(int index) {
            _selectedTrackIndex = index;
            var trackName = _trackSelector.GetItemText(index);
            var config = _system.GetTrackConfig(trackName);
            
            if (config != null) {
                string info = $"📍 {config.Name}\n";
                info += $"   {config.Description}\n\n";
                info += $"   Length: {config.Length}m\n";
                info += $"   Difficulty: {config.Difficulty}\n";
                info += $"   Base Reward: {config.BaseReward} gold\n";
                info += $"   Players: {config.MinPlayers}-{config.MaxPlayers}\n";
                info += $"   Terrain: {config.Terrain}";
                
                _trackInfoLabel.Text = info;
            }
        }
        
        private void OnRacePressed() {
            if (_selectedTrackIndex < 0) {
                _resultLabel.Text = "Please select a track!";
                return;
            }
            
            var trackName = _trackSelector.GetItemText(_selectedTrackIndex);
            if (!_data.UnlockedTracks.Contains(trackName)) {
                _resultLabel.Text = "This track is locked!";
                return;
            }
            
            var mountIndex = _mountSelector.GetSelectedId();
            int[] mountStats = { (80, 70), (90, 50), (60, 90), (85, 75) };
            var (speed, stamina) = mountStats[Math.Clamp(mountIndex, 0, 3)];
            
            var result = _system.SimulateRace(trackName, $"Mount_{mountIndex}", speed, stamina);
            
            if (result != null) {
                string resultText = "";
                if (result.Rank == 1) {
                    resultText = $"🥇 1ST PLACE!\n";
                } else if (result.Rank == 2) {
                    resultText = $"🥈 2ND PLACE!\n";
                } else if (result.Rank == 3) {
                    resultText = $"🥉 3RD PLACE!\n";
                } else {
                    resultText = $"🏅 Rank #{result.Rank}\n";
                }
                
                resultText += $"⏱ Time: {result.Time}s\n";
                resultText += $"💰 +{result.GoldReward} gold\n";
                resultText += $"⭐ +{result.ExpReward} exp\n";
                
                if (result.IsNewBestTime) {
                    resultText += "🎉 NEW BEST TIME!";
                }
                
                _resultLabel.Text = resultText;
                RefreshStats();
            }
        }
        
        public override void _Input(InputEvent e) {
            if (e is InputEventKey key && key.Pressed && key.Keycode == Key.Escape) {
                Visible = false;
            }
        }
    }
}
