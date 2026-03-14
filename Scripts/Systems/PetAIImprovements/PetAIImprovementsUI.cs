# Pet AI Improvements UI
# Display pet AI personality, learning, emotions and stats

const AI_UI_TOGGLED = "ai_ui_toggled"

class_name PetAIImprovementsUI extends Control

# UI Elements
var main_panel: PanelContainer
var tab_container: TabContainer
var personality_panel: VBoxContainer
var behavior_panel: VBoxContainer
var learning_panel: VBoxContainer
var emotion_panel: VBoxContainer
var stats_panel: VBoxContainer

# Labels
var ai_level_label: Label
var personality_label: Label
var emotion_label: Label
var state_label: Label
var adaptation_label: Label
var win_rate_label: Label

# System reference
var ai_system: PetAIImprovementsSystem = null

func _ready():
	_setup_ui()
	visible = false

func _setup_ui():
	# Main panel
	main_panel = PanelContainer.new()
	main_panel.anchor_right = 1.0
	main_panel.anchor_bottom = 1.0
	main_panel.offset_left = 200
	main_panel.offset_top = 100
	main_panel.offset_right = -200
	main_panel.offset_bottom = -100
	main_panel.set_meta("ui_type", "pet_ai_improvements")
	add_child(main_panel)
	
	# Style
	var style = StyleBoxFlat.new()
	style.bg_color = Color(0.1, 0.1, 0.15, 0.95)
	style.border_color = Color(0.3, 0.6, 0.9, 1.0)
	style.set_border_width_all(2)
	style.set_corner_radius_all(8)
	main_panel.add_theme_stylebox_override("panel", style)
	
	# VBox container
	var vbox = VBoxContainer.new()
	main_panel.add_child(vbox)
	vbox.set_anchors_preset(Control.PRESET_FULL_RECT)
	vbox.add_theme_constant_override("separation", 10)
	
	# Title
	var title_label = Label.new()
	title_label.text = "🐾 Pet AI Companion"
	title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title_label.add_theme_font_size_override("font_size", 24)
	vbox.add_child(title_label)
	
	# AI Level display
	ai_level_label = Label.new()
	ai_level_label.text = "AI Level: 1"
	ai_level_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	ai_level_label.add_theme_font_size_override("font_size", 18)
	vbox.add_child(ai_level_label)
	
	# Tab container
	tab_container = TabContainer.new()
	tab_container.size_flags_vertical = Control.SIZE_EXPAND_FILL
	vbox.add_child(tab_container)
	
	# Create tabs
	_setup_personality_tab()
	_setup_behavior_tab()
	_setup_learning_tab()
	_setup_emotion_tab()
	_setup_stats_tab()
	
	# Close button
	var close_button = Button.new()
	close_button.text = "Close"
	close_button.pressed.connect(_on_close_pressed)
	vbox.add_child(close_button)

func _setup_personality_tab():
	personality_panel = VBoxContainer.new()
	personality_panel.name = "Personality"
	tab_container.add_child(personality_panel)
	
	var title = Label.new()
	title.text = "🐶 Personality Traits"
	title.add_theme_font_size_override("font_size", 18)
	personality_panel.add_child(title)
	
	personality_label = Label.new()
	personality_label.text = "Type: Aggressive"
	personality_panel.add_child(personality_label)
	
	var curiosity_label = Label.new()
	curiosity_label.name = "curiosity"
	curiosity_label.text = "Curiosity: 50%"
	personality_panel.add_child(curiosity_label)
	
	var energy_label = Label.new()
	energy_label.name = "energy"
	energy_label.text = "Energy: 100%"
	personality_panel.add_child(energy_label)
	
	var loyalty_label = Label.new()
	loyalty_label.name = "loyalty"
	loyalty_label.text = "Loyalty: 50%"
	personality_panel.add_child(loyalty_label)
	
	# Personality type selector
	var selector_label = Label.new()
	selector_label.text = "\nChange Personality:"
	personality_panel.add_child(selector_label)
	
	var type_names = ["Aggressive", "Defensive", "Supportive", "Curious", "Lazy"]
	for i in range(type_names.size()):
		var btn = Button.new()
		btn.text = type_names[i]
		btn.pressed.connect(_on_personality_selected.bind(i))
		personality_panel.add_child(btn)

func _setup_behavior_tab():
	behavior_panel = VBoxContainer.new()
	behavior_panel.name = "Behavior"
	tab_container.add_child(behavior_panel)
	
	var title = Label.new()
	title.text = "🎯 Current Behavior"
	title.add_theme_font_size_override("font_size", 18)
	behavior_panel.add_child(title)
	
	state_label = Label.new()
	state_label.text = "State: Idle"
	behavior_panel.add_child(state_label)
	
	var priority_label = Label.new()
	priority_label.name = "priority"
	priority_label.text = "Priority: 0"
	behavior_panel.add_child(priority_label)
	
	var target_label = Label.new()
	target_label.name = "target"
	target_label.text = "Target: None"
	behavior_panel.add_child(target_label)

func _setup_learning_tab():
	learning_panel = VBoxContainer.new()
	learning_panel.name = "Learning"
	tab_container.add_child(learning_panel)
	
	var title = Label.new()
	title.text = "📚 Learning Progress"
	title.add_theme_font_size_override("font_size", 18)
	learning_panel.add_child(title)
	
	adaptation_label = Label.new()
	adaptation_label.text = "Adaptation: 0%"
	learning_panel.add_child(adaptation_label)
	
	win_rate_label = Label.new()
	win_rate_label.text = "Win Rate: 0%"
	learning_panel.add_child(win_rate_label)
	
	var battles_label = Label.new()
	battles_label.name = "battles"
	battles_label.text = "Total Battles: 0"
	learning_panel.add_child(battles_label)
	
	var best_combo_label = Label.new()
	best_combo_label.name = "best_combo"
	best_combo_label.text = "Best Combo: 0"
	learning_panel.add_child(best_combo_label)
	
	var enemy_label = Label.new()
	enemy_label.name = "enemy"
	enemy_label.text = "Most Killed: None"
	learning_panel.add_child(enemy_label)

func _setup_emotion_tab():
	emotion_panel = VBoxContainer.new()
	emotion_panel.name = "Emotion"
	tab_container.add_child(emotion_panel)
	
	var title = Label.new()
	title.text = "😊 Emotional State"
	title.add_theme_font_size_override("font_size", 18)
	emotion_panel.add_child(title)
	
	emotion_label = Label.new()
	emotion_label.text = "Current: Happy"
	emotion_panel.add_child(emotion_label)
	
	var intensity_label = Label.new()
	intensity_label.name = "intensity"
	intensity_label.text = "Intensity: 50%"
	emotion_panel.add_child(intensity_label)
	
	var history_label = Label.new()
	history_label.name = "history"
	history_label.text = "Recent Emotions: None"
	emotion_panel.add_child(history_label)

func _setup_stats_tab():
	stats_panel = VBoxContainer.new()
	stats_panel.name = "Combat Stats"
	tab_container.add_child(stats_panel)
	
	var title = Label.new()
	title.text = "⚔️ Combat Statistics"
	title.add_theme_font_size_override("font_size", 18)
	stats_panel.add_child(title)
	
	var damage_label = Label.new()
	damage_label.name = "damage"
	damage_label.text = "Damage Dealt: 0"
	stats_panel.add_child(damage_label)
	
	var prevented_label = Label.new()
	prevented_label.name = "prevented"
	prevented_label.text = "Damage Prevented: 0"
	stats_panel.add_child(prevented_label)
	
	var healing_label = Label.new()
	healing_label.name = "healing"
	healing_label.text = "Healing Done: 0"
	stats_panel.add_child(healing_label)
	
	var crit_label = Label.new()
	crit_label.name = "crits"
	crit_label.text = "Critical Hits: 0"
	stats_panel.add_child(crit_label)
	
	var dodge_label = Label.new()
	dodge_label.name = "dodges"
	dodge_label.text = "Perfect Dodges: 0"
	stats_panel.add_child(dodge_label)

func set_ai_system(system: PetAIImprovementsSystem):
	ai_system = system
	update_display()

func update_display():
	if ai_system == null:
		return
	
	# Update personality
	if ai_system.data and ai_system.data.personality:
		personality_label.text = "Type: " + ai_system.get_personality_type()
	
	# Update AI level
	ai_level_label.text = "AI Level: " + str(ai_system.get_ai_level())
	
	# Update behavior state
	state_label.text = "State: " + ai_system.get_ai_state()
	
	# Update emotion
	emotion_label.text = "Current: " + ai_system.get_current_emotion()
	
	# Update learning stats
	var learning_stats = ai_system.get_learning_stats()
	adaptation_label.text = "Adaptation: " + str(round(learning_stats["adaptation_level"] * 100)) + "%"
	win_rate_label.text = "Win Rate: " + str(round(learning_stats["win_rate"] * 100)) + "%"
	
	# Update combat stats
	var combat_stats = ai_system.get_combat_stats()
	var damage_label = stats_panel.get_node("damage")
	if damage_label:
		damage_label.text = "Damage Dealt: " + str(round(combat_stats["total_damage_dealt"]))
	var prevented_label = stats_panel.get_node("prevented")
	if prevented_label:
		prevented_label.text = "Damage Prevented: " + str(round(combat_stats["total_damage_prevented"]))
	var healing_label = stats_panel.get_node("healing")
	if healing_label:
		healing_label.text = "Healing Done: " + str(round(combat_stats["total_healing_done"]))
	var crit_label = stats_panel.get_node("crits")
	if crit_label:
		crit_label.text = "Critical Hits: " + str(combat_stats["critical_hits"])
	var dodge_label = stats_panel.get_node("dodges")
	if dodge_label:
		dodge_label.text = "Perfect Dodges: " + str(combat_stats["perfect_dodges"])

func _input(event):
	if event.is_action_pressed("pet_ai_toggle"):
		toggle()
		get_viewport().set_input_as_handled()

func toggle():
	visible = not visible
	emit_signal(AI_UI_TOGGLED, visible)
	if visible and ai_system:
		update_display()

func _on_close_pressed():
	visible = false

func _on_personality_selected(type: int):
	if ai_system:
		ai_system.set_personality_type(type)
		personality_label.text = "Type: " + ai_system.get_personality_type()
