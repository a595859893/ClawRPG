extends Control

var system: Node

# UI Elements
var title_label: Label
var active_tab: Button
var completed_tab: Button
var stats_tab: Button
var challenge_container: ScrollContainer
var stats_container: VBoxContainer

# Colors
var color_active = Color(0.2, 0.8, 0.2)
var color_completed = Color(0.2, 0.5, 0.9)
var color_stats = Color(0.9, 0.7, 0.2)
var color_easy = Color(0.5, 0.8, 0.5)
var color_medium = Color(0.8, 0.8, 0.2)
var color_hard = Color(0.9, 0.6, 0.2)
var color_epic = Color(0.8, 0.3, 0.8)
var color_legendary = Color(1.0, 0.8, 0.0)

func _ready():
	system = get_node("/root/Main/DynamicQuestChallengeSystem")
	if not system:
		system = load("res://Scripts/Systems/DynamicQuestChallenge/DynamicQuestChallengeSystem.gd").new()
		get_tree().current_scene.add_child(system)
	
	setup_ui()

func setup_ui():
	# Main panel
	var panel = PanelContainer.new()
	panel.anchor_right = 1.0
	panel.anchor_bottom = 1.0
	panel.offset_left = 200
	panel.offset_top = 100
	panel.offset_right = -200
	panel.offset_bottom = -100
	add_child(panel)
	
	var vbox = VBoxContainer.new()
	panel.add_child(vbox)
	
	# Title
	title_label = Label.new()
	title_label.text = "Dynamic Quest Challenge"
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_label.add_theme_font_size_override("font_size", 24)
	vbox.add_child(title_label)
	
	# Tab buttons
	var tab_hbox = HBoxContainer.new()
	vbox.add_child(tab_hbox)
	
	active_tab = Button.new()
	active_tab.text = "Active"
	active_tab.pressed.connect(_on_active_tab_pressed)
	tab_hbox.add_child(active_tab)
	
	completed_tab = Button.new()
	completed_tab.text = "Completed"
	completed_tab.pressed.connect(_on_completed_tab_pressed)
	tab_hbox.add_child(completed_tab)
	
	stats_tab = Button.new()
	stats_tab.text = "Statistics"
	stats_tab.pressed.connect(_on_stats_tab_pressed)
	tab_hbox.add_child(stats_tab)
	
	# Challenge container
	challenge_container = ScrollContainer.new()
	challenge_container.anchor_right = 1.0
	challenge_container.anchor_bottom = 1.0
	challenge_container.offset_top = 100
	vbox.add_child(challenge_container)
	
	var challenge_vbox = VBoxContainer.new()
	challenge_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	challenge_container.add_child(challenge_vbox)
	
	# Stats container
	stats_container = VBoxContainer.new()
	stats_container.visible = false
	vbox.add_child(stats_container)
	
	# Generate button
	var generate_btn = Button.new()
	generate_btn.text = "Generate New Challenge"
	generate_btn.pressed.connect(_on_generate_pressed)
	vbox.add_child(generate_btn)
	
	# Close button
	var close_btn = Button.new()
	close_btn.text = "Close (ESC)"
	close_btn.pressed.connect(_on_close_pressed)
	vbox.add_child(close_btn)
	
	show_active_challenges()

func _process(_delta):
	if system:
		system.check_expired()

func show_active_challenges():
	clear_challenge_container()
	stats_container.visible = false
	challenge_container.visible = true
	
	if not system:
		return
	
	var challenges = system.get_active_challenges()
	
	if challenges.is_empty():
		var empty_label = Label.new()
		empty_label.text = "No active challenges"
		empty_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		challenge_container.add_child(empty_label)
		return
	
	for challenge in challenges:
		var card = create_challenge_card(challenge, true)
		challenge_container.add_child(card)

func show_completed_challenges():
	clear_challenge_container()
	stats_container.visible = false
	challenge_container.visible = true
	
	if not system:
		return
	
	var challenges = system.get_completed_challenges()
	
	if challenges.is_empty():
		var empty_label = Label.new()
		empty_label.text = "No completed challenges"
		empty_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		challenge_container.add_child(empty_label)
		return
	
	for challenge in challenges:
		var card = create_challenge_card(challenge, false)
		challenge_container.add_child(card)

func show_statistics():
	clear_challenge_container()
	challenge_container.visible = false
	stats_container.visible = true
	
	if not system:
		return
	
	var stats = system.get_statistics()
	
	# Clear and rebuild stats
	for child in stats_container.get_children():
		child.queue_free()
	
	var stats_title = Label.new()
	stats_title.text = "Challenge Statistics"
	stats_title.add_theme_font_size_override("font_size", 20)
	stats_container.add_child(stats_title)
	
	var stats_items = [
		["Total Generated", str(stats.get("total_generated", 0))],
		["Total Completed", str(stats.get("total_completed", 0))],
		["Total Abandoned", str(stats.get("total_abandoned", 0))],
		["Current Streak", str(stats.get("current_streak", 0))],
		["Longest Streak", str(stats.get("longest_streak", 0))],
		["Total Gold Earned", str(stats.get("total_gold_earned", 0))],
		["Total Experience Earned", str(stats.get("total_experience_earned", 0))]
	]
	
	for item in stats_items:
		var label = Label.new()
		label.text = item[0] + ": " + item[1]
		stats_container.add_child(label)

func create_challenge_card(challenge: Dictionary, is_active: bool) -> Control:
	var card = PanelContainer.new()
	card.custom_minimum_size = Vector2(0, 80)
	
	var hbox = HBoxContainer.new()
	card.add_child(hbox)
	
	# Info section
	var info_vbox = VBoxContainer.new()
	info_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	hbox.add_child(info_vbox)
	
	var name_label = Label.new()
	name_label.text = challenge.get("name", "Unknown")
	name_label.add_theme_font_size_override("font_size", 16)
	info_vbox.add_child(name_label)
	
	var desc_label = Label.new()
	desc_label.text = challenge.get("description", "")
	info_vbox.add_child(desc_label)
	
	# Progress
	var progress_label = Label.new()
	if is_active:
		var progress = challenge.get("progress", 0)
		var target = challenge.get("target_amount", 0)
		progress_label.text = "Progress: " + str(progress) + "/" + str(target)
	else:
		progress_label.text = "Completed!"
	info_vbox.add_child(progress_label)
	
	# Difficulty badge
	var difficulty = challenge.get("difficulty", "Easy")
	var difficulty_label = Label.new()
	difficulty_label.text = "[" + difficulty + "]"
	difficulty_label.modulate = get_difficulty_color(difficulty)
	hbox.add_child(difficulty_label)
	
	# Abandon button for active challenges
	if is_active:
		var abandon_btn = Button.new()
		abandon_btn.text = "Abandon"
		abandon_btn.pressed.connect(func(): _on_abandon_pressed(challenge.get("id", "")))
		hbox.add_child(abandon_btn)
	
	return card

func get_difficulty_color(difficulty: String) -> Color:
	match difficulty:
		"Easy": return color_easy
		"Medium": return color_medium
		"Hard": return color_hard
		"Epic": return color_epic
		"Legendary": return color_legendary
		_: return Color.WHITE

func clear_challenge_container():
	for child in challenge_container.get_children():
		child.queue_free()

func _on_active_tab_pressed():
	show_active_challenges()

func _on_completed_tab_pressed():
	show_completed_challenges()

func _on_stats_tab_pressed():
	show_statistics()

func _on_generate_pressed():
	if system:
		# 简化的玩家数据
		var challenge = system.generate_challenge(10, "Warrior", [])
		print("Generated challenge: ", challenge.get("name", ""))
		show_active_challenges()

func _on_abandon_pressed(challenge_id: String):
	if system:
		system.abandon_challenge(challenge_id)
		show_active_challenges()

func _on_close_pressed():
	queue_free()

func _input(event):
	if event.is_action_pressed("ui_cancel"):
		_on_close_pressed()
