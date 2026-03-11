extends Control

# Treasure Hunt UI
onready var region_list = $VBoxContainer/RegionList
onready var info_panel = $VBoxContainer/HBoxContainer/InfoPanel
onready var stats_panel = $VBoxContainer/HBoxContainer/StatsPanel
onready var energy_label = $VBoxContainer/HBoxContainer/StatsPanel/EnergyLabel
onready var success_rate_label = $VBoxContainer/HBoxContainer/InfoPanel/SuccessRateLabel
onready var hunt_button = $VBoxContainer/HBoxContainer/InfoPanel/HuntButton
onready var close_button = $VBoxContainer/CloseButton

var selected_region = null

func _ready():
	# Connect signals
	if close_button:
		close_button.connect("pressed", self, "_on_close_pressed")
	
	if hunt_button:
		hunt_button.connect("pressed", self, "_on_hunt_pressed")
	
	if region_list:
		region_list.connect("item_selected", self, "_on_region_selected")

func load_regions():
	if not TreasureHuntManager or not TreasureHuntManager.Instance:
		return
	
	var regions = TreasureHuntManager.Instance.GetRegions()
	var player_level = 1
	
	if Player:
		player_level = Player.level
	
	if region_list:
		region_list.clear()
	
	for region in regions:
		if region_list:
			var item_idx = region_list.add_item(region.name)
			var item_data = {
				"region": region,
				"locked": player_level < region.requiredLevel
			}
			region_list.set_item_metadata(item_idx, item_data)
			
			if player_level < region.requiredLevel:
				region_list.set_item_custom_fg_color(item_idx, Color(0.5, 0.5, 0.5))

func _on_region_selected(index):
	if not region_list:
		return
		
	var item_data = region_list.get_item_metadata(index)
	if not item_data:
		return
		
	selected_region = item_data["region"]
	
	# Update info panel
	if info_panel:
		var name_label = info_panel.get_node_or_null("RegionNameLabel")
		var desc_label = info_panel.get_node_or_null("DescriptionLabel")
		var level_label = info_panel.get_node_or_null("LevelLabel")
		var energy_label_info = info_panel.get_node_or_null("EnergyCostLabel")
		
		if name_label:
			name_label.text = selected_region.name
		if desc_label:
			desc_label.text = selected_region.description
		if level_label:
			level_label.text = "Required Level: %d" % selected_region.requiredLevel
		if energy_label_info:
			energy_label_info.text = "Energy Cost: %d" % selected_region.energyCost
	
	if success_rate_label:
		success_rate_label.text = "Success Rate: %d%%" % int(selected_region.successRate * 100)
	
	# Update treasure preview
	update_treasure_preview()

func update_treasure_preview():
	if selected_region == null:
		return
	
	var treasure_container = $VBoxContainer/HBoxContainer/InfoPanel/TreasureContainer
	if not treasure_container:
		return
		
	treasure_container.clear()
	
	for treasure in selected_region.treasures:
		var item_idx = treasure_container.add_item("%s - %d Gold" % [treasure.name, treasure.goldReward])

func _on_hunt_pressed():
	if selected_region == null:
		return
	
	if not TreasureHuntManager or not TreasureHuntManager.Instance:
		return
	
	var player_id = 1  # Default player
	var success = TreasureHuntManager.Instance.StartHunt(player_id, selected_region.id)
	
	if success:
		update_stats()
		load_regions()  # Refresh lock states
		show_result(true)
	else:
		show_result(false)

func show_result(success):
	var result_panel = $VBoxContainer/HBoxContainer/ResultPanel
	if not result_panel:
		return
		
	result_panel.visible = true
	
	var result_label = result_panel.get_node_or_null("ResultLabel")
	if result_label:
		if success:
			result_label.text = "Treasure Found!"
			result_label.modulate = Color.GREEN
		else:
			result_label.text = "Hunt Failed..."
			result_label.modulate = Color.RED
	
	# Auto-hide after 2 seconds
	await get_tree().create_timer(2.0).timeout
	result_panel.visible = false

func update_stats():
	if not TreasureHuntManager or not TreasureHuntManager.Instance:
		return
	
	var player_id = 1
	var data = TreasureHuntManager.Instance.GetPlayerData(player_id)
	
	if energy_label:
		energy_label.text = "Energy: %d/%d" % [data.currentEnergy, data.maxEnergy]
	
	# Update stats panel
	var stats_text = "Total Hunts: %d\n" % data.totalHunts
	stats_text += "Successful: %d\n" % data.successfulHunts
	stats_text += "Total Gold: %d\n" % data.totalGoldEarned
	stats_text += "Total EXP: %d\n" % data.totalExpEarned
	stats_text += "Treasures Found: %d" % data.discoveredTreasures.size()
	
	var stats_label = $VBoxContainer/HBoxContainer/StatsPanel/StatsLabel
	if stats_label:
		stats_label.text = stats_text

func _on_close_pressed():
	visible = false

func _input(event):
	if event.is_action_pressed("ui_cancel"):
		if visible:
			visible = false
