# Combo Skill UI - 连击技能系统界面
## 连击技能系统用户界面

extends Control
class_name ComboSkillUI

## 关联的连击系统
var combo_system: ComboSkillSystem

var combo_list: GridContainer
var detail_panel: VBoxContainer
var equipped_panel: HBoxContainer
var selected_combo_id: String = ""

var toggle_key: Key = Key.J

func _ready():
	combo_system = ComboSkillSystem.get_instance()
	if combo_system:
		combo_system.combo_unlocked.connect(_on_combo_unlocked)
		combo_system.combo_executed.connect(_on_combo_executed)
		combo_system.cooldown_updated.connect(_on_cooldown_updated)
	
	visible = false
	_setup_ui()

func _input(event):
	if event is InputEventKey and event.pressed:
		if event.keycode == toggle_key:
			toggle()
		elif event.keycode == Key.ESCAPE and visible:
			visible = false

func toggle():
	visible = not visible
	if visible:
		refresh()

func refresh():
	if combo_system == null:
		return
	_update_combo_list()
	_update_equipped_panel()
	_update_detail_panel()

# ============ UI构建 ============

func _setup_ui():
	# 主容器
	var main_vbox = VBoxContainer.new()
	main_vbox.set_anchors_preset(Control.PRESET_FULL_RECT)
	main_vbox.add_theme_constant_override("separation", 10)
	add_child(main_vbox)
	
	# 标题栏
	var title_bar = HBoxContainer.new()
	main_vbox.add_child(title_bar)
	
	var title = Label.new()
	title.text = "连击技能系统"
	title.add_theme_font_size_override("font_size", 24)
	title_bar.add_child(title)
	
	title_bar.add_child(Control.new())
	
	var close_btn = Button.new()
	close_btn.text = "×"
	close_btn.pressed.connect(func(): visible = false)
	title_bar.add_child(close_btn)
	
	# 装备栏
	var equipped_label = Label.new()
	equipped_label.text = "已装备连击 (按快捷键触发)"
	equipped_label.add_theme_font_size_override("font_size", 16)
	main_vbox.add_child(equipped_label)
	
	equipped_panel = HBoxContainer.new()
	equipped_panel.add_theme_constant_override("separation", 10)
	main_vbox.add_child(equipped_panel)
	
	# 内容区域
	var content_hbox = HBoxContainer.new()
	content_hbox.set_v_size_flags(Control.SIZE_EXPAND_FILL)
	main_vbox.add_child(content_hbox)
	
	# 左侧：连击列表
	var list_panel = PanelContainer.new()
	list_panel.custom_minimum_size.x = 350
	content_hbox.add_child(list_panel)
	
	var list_scroll = ScrollContainer.new()
	list_panel.add_child(list_scroll)
	
	combo_list = GridContainer.new()
	combo_list.columns = 2
	combo_list.add_theme_constant_override("hseparation", 5)
	combo_list.add_theme_constant_override("vseparation", 5)
	list_scroll.add_child(combo_list)
	
	# 右侧：详情面板
	detail_panel = VBoxContainer.new()
	detail_panel.set_h_size_flags(Control.SIZE_EXPAND_FILL)
	content_hbox.add_child(detail_panel)
	_setup_detail_panel()

func _setup_detail_panel():
	var title = Label.new()
	title.text = "连击详情"
	title.add_theme_font_size_override("font_size", 18)
	detail_panel.add_child(title)
	
	var name_label = Label.new()
	name_label.name = "name_label"
	name_label.add_theme_font_size_override("font_size", 20)
	detail_panel.add_child(name_label)
	
	var type_label = Label.new()
	type_label.name = "type_label"
	detail_panel.add_child(type_label)
	
	var desc_label = Label.new()
	desc_label.name = "desc_label"
	desc_label.autowrap_mode = TextServer.AUTOWRAP_WORD
	detail_panel.add_child(desc_label)
	
	var stats_label = Label.new()
	stats_label.name = "stats_label"
	detail_panel.add_child(stats_label)
	
	var steps_label = Label.new()
	steps_label.name = "steps_label"
	steps_label.autowrap_mode = TextServer.AUTOWRAP_WORD
	detail_panel.add_child(steps_label)
	
	var button_hbox = HBoxContainer.new()
	detail_panel.add_child(button_hbox)
	
	var equip_btn = Button.new()
	equip_btn.name = "equip_btn"
	equip_btn.text = "装备"
	equip_btn.pressed.connect(_on_equip_pressed)
	button_hbox.add_child(equip_btn)
	
	var execute_btn = Button.new()
	execute_btn.name = "execute_btn"
	execute_btn.text = "执行"
	execute_btn.pressed.connect(_on_execute_pressed)
	button_hbox.add_child(execute_btn)

# ============ 数据更新 ============

func _update_combo_list():
	# 清除旧项目
	for child in combo_list.get_children():
		child.queue_free()
	
	var unlocked = combo_system.get_unlocked_combos()
	
	for combo_id in unlocked:
		var combo = ComboSkillDatabase.get_instance().get_combo(combo_id)
		if combo == null:
			continue
		
		var btn = Button.new()
		btn.custom_minimum_size = Vector2(160, 50)
		btn.text = combo.name
		btn.tooltip_text = "%s\n%s" % [combo.description, _get_combo_type_name(combo.combo_type)]
		
		# 稀有度颜色
		var color = ComboSkillDatabase.get_instance().get_rarity_color(combo.rarity)
		var style = StyleBoxFlat.new()
		style.bg_color = color.darkened(0.7)
		style.border_color = color
		style.border_width_left = 2
		style.border_width_top = 2
		style.border_width_right = 2
		style.border_width_bottom = 2
		btn.add_theme_stylebox_override("normal", style)
		
		# 选中状态
		if combo_id == selected_combo_id:
			btn.modulate = Color.YELLOW
		
		# 冷却状态
		if combo_system.is_on_cooldown(combo_id):
			btn.disabled = true
		
		btn.pressed.connect(func():
			selected_combo_id = combo_id
			_update_combo_list()
			_update_detail_panel()
		)
		
		combo_list.add_child(btn)
	
	# 显示未解锁提示
	if unlocked.size() == 0:
		var empty_label = Label.new()
		empty_label.text = "暂无解锁的连击技能\n通过升级或完成任务解锁"
		empty_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		combo_list.add_child(empty_label)

func _update_equipped_panel():
	for child in equipped_panel.get_children():
		child.queue_free()
	
	var equipped = combo_system.get_equipped_combos()
	
	for i in range(5):
		var slot = VBoxContainer.new()
		slot.custom_minimum_size = Vector2(80, 80)
		
		var btn = Button.new()
		btn.custom_minimum_size = Vector2(70, 70)
		btn.text = ""
		
		if i < equipped.size():
			var combo_id = equipped[i].combo_id
			var combo = ComboSkillDatabase.get_instance().get_combo(combo_id)
			if combo != null:
				btn.text = combo.name.substr(0, 2)
				btn.tooltip_text = "%s\n冷却: %.1fs" % [combo.name, combo_system.get_cooldown(combo_id)]
				
				# 冷却显示
				var cooldown = combo_system.get_cooldown(combo_id)
				if cooldown > 0:
					var progress = StyleBoxFlat.new()
					progress.bg_color = Color(0, 0, 0, 0.5)
					var height = 70 * (cooldown / combo.cooldown)
					progress.content_margin_top = 70 - height
					btn.add_theme_stylebox_override("normal", progress)
				
				# 快捷键提示
				btn.shortcut_in_tooltip = false
			
			btn.pressed.connect(func(): _execute_combo_index(i))
		else:
			btn.text = "+"
			btn.disabled = true
		
		slot.add_child(btn)
		
		var key_label = Label.new()
		key_label.text = "J+%d" % (i + 1)
		key_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		slot.add_child(key_label)
		
		equipped_panel.add_child(slot)

func _update_detail_panel():
	if selected_combo_id == "":
		_clear_detail_panel()
		return
	
	var combo = ComboSkillDatabase.get_instance().get_combo(selected_combo_id)
	if combo == null:
		_clear_detail_panel()
		return
	
	var name_label = detail_panel.find_child("name_label", true, false)
	var type_label = detail_panel.find_child("type_label", true, false)
	var desc_label = detail_panel.find_child("desc_label", true, false)
	var stats_label = detail_panel.find_child("stats_label", true, false)
	var steps_label = detail_panel.find_child("steps_label", true, false)
	var equip_btn = detail_panel.find_child("equip_btn", true, false)
	var execute_btn = detail_panel.find_child("execute_btn", true, false)
	
	if name_label:
		name_label.text = combo.name
		name_label.add_theme_color_override("font_color", ComboSkillDatabase.get_instance().get_rarity_color(combo.rarity))
	
	if type_label:
		type_label.text = "类型: %s | 稀有度: %s" % [_get_combo_type_name(combo.combo_type), ComboSkillDatabase.get_instance().get_rarity_name(combo.rarity)]
	
	if desc_label:
		desc_label.text = combo.description
	
	if stats_label:
		var cooldown = combo_system.get_cooldown(selected_combo_id)
		var is_equipped = combo_system.is_equipped(selected_combo_id)
		stats_label.text = "冷却: %.1fs | 法力: %.0f\n等级需求: %d\n已装备: %s\n执行次数: %d" % [
			combo.cooldown - cooldown if cooldown > 0 else combo.cooldown,
			combo.mana_cost,
			combo.level_required,
			"是" if is_equipped else "否",
			0  # use_count from player combo
		]
	
	if steps_label:
		var steps_text = "连击步骤 (%d 步):\n" % combo.steps.size()
		for i in range(combo.steps.size()):
			var step = combo.steps[i]
			steps_text += "%d. %s - 延迟: %.1fs\n" % [i + 1, step.skill_id, step.delay]
			for effect in step.effects:
				steps_text += "   → %s\n" % effect.description
		steps_label.text = steps_text
	
	if equip_btn:
		equip_btn.text = "卸下" if combo_system.is_equipped(selected_combo_id) else "装备"
		equip_btn.disabled = combo_system.is_on_cooldown(selected_combo_id)
	
	if execute_btn:
		execute_btn.disabled = combo_system.is_on_cooldown(selected_combo_id)

func _clear_detail_panel():
	var labels = ["name_label", "type_label", "desc_label", "stats_label", "steps_label"]
	for label_name in labels:
		var label = detail_panel.find_child(label_name, true, false)
		if label:
			label.text = ""

func _get_combo_type_name(type: ComboType) -> String:
	match type:
		ComboType.Sequential: return "顺序"
		ComboType.Parallel: return "并行"
		ComboType.Chain: return "链式"
		ComboType.Conditional: return "条件"
	return "未知"

# ============ 信号处理 ============

func _on_combo_unlocked(combo_id: String):
	refresh()

func _on_combo_executed(combo_id: String):
	refresh()

func _on_cooldown_updated(combo_id: String, remaining: float):
	refresh()

func _on_equip_pressed():
	if selected_combo_id == "":
		return
	
	if combo_system.is_equipped(selected_combo_id):
		combo_system.unequip_combo(selected_combo_id)
	else:
		combo_system.equip_combo(selected_combo_id)
	
	refresh()

func _on_execute_pressed():
	if selected_combo_id == "":
		return
	
	combo_system.execute_combo(selected_combo_id)

func _execute_combo_index(index: int):
	var equipped = combo_system.get_equipped_combos()
	if index < equipped.size():
		combo_system.execute_combo(equipped[index].combo_id)
