extends BaseSystem

# DynamicQuestChallengeSystem - 动态任务挑战系统
# 基于玩家状态动态生成个性化挑战任务
# 应用 Procedural Quest Generation 学习成果

var data: DynamicQuestChallengeData
var database: DynamicQuestChallengeDatabase

func _ready():
	super._ready()
	system_name = "DynamicQuestChallengeSystem"
	data = DynamicQuestChallengeData.new()
	database = DynamicQuestChallengeDatabase.new()
	load_data()

func _exit_tree():
	save_data()
	shutdown()

func initialize():
	super.initialize()
	is_initialized = true

func shutdown():
	save_data()
	super.shutdown()

func export_save_data() -> Dictionary:
	if data:
		return {
			"data": data.to_dict(),
			"system_name": system_name
		}
	return {
		"data": {},
		"system_name": system_name
	}

func import_save_data(save_data: Dictionary):
	super.import_save_data(save_data)
	if save_data.has("data") and save_data["data"] is Dictionary:
		data.from_dict(save_data["data"])

# 生成挑战任务
func generate_challenge(player_level: int, player_class: String, current_quests: Array) -> Dictionary:
	var challenge_types = database.get_challenge_types()
	var selected_type = challenge_types[randi() % challenge_types.size()]
	
	var difficulty = calculate_difficulty(player_level)
	var challenge = database.generate_challenge(selected_type, difficulty, player_level, player_class)
	
	# 添加到活跃挑战
	data.active_challenges.append(challenge)
	
	# 生成唯一ID
	challenge["id"] = generate_unique_id()
	challenge["generated_time"] = Time.get_unix_time_from_system()
	challenge["expires_time"] = challenge["generated_time"] + challenge["duration"]
	challenge["progress"] = 0
	challenge["completed"] = false
	
	save_data()
	return challenge

# 计算难度
func calculate_difficulty(player_level: int) -> String:
	var rand = randi() % 100
	
	if player_level < 10:
		return "Easy" if rand < 70 else "Medium"
	elif player_level < 30:
		return "Easy" if rand < 30 else ( "Medium" if rand < 70 else "Hard")
	elif player_level < 50:
		return "Medium" if rand < 40 else ( "Hard" if rand < 80 else "Epic")
	else:
		return "Hard" if rand < 30 else ( "Epic" if rand < 70 else "Legendary")

# 更新挑战进度
func update_progress(challenge_id: String, progress_delta: int) -> Dictionary:
	for challenge in data.active_challenges:
		if challenge["id"] == challenge_id:
			challenge["progress"] += progress_delta
			
			# 检查完成
			if challenge["progress"] >= challenge["target_amount"]:
				challenge["completed"] = true
				challenge["completion_time"] = Time.get_unix_time_from_system()
				data.completed_challenges.append(challenge)
				data.active_challenges.erase(challenge)
				
				# 统计
				data.statistics["total_completed"] += 1
				data.statistics["current_streak"] += 1
				if data.statistics["current_streak"] > data.statistics["longest_streak"]:
					data.statistics["longest_streak"] = data.statistics["current_streak"]
			
			save_data()
			return challenge
	
	return {}

# 放弃挑战
func abandon_challenge(challenge_id: String) -> bool:
	for challenge in data.active_challenges:
		if challenge["id"] == challenge_id:
			data.active_challenges.erase(challenge)
			data.statistics["total_abandoned"] += 1
			data.statistics["current_streak"] = 0
			save_data()
			return true
	return false

# 获取活跃挑战
func get_active_challenges() -> Array:
	return data.active_challenges

# 获取已完成挑战
func get_completed_challenges() -> Array:
	return data.completed_challenges

# 获取挑战统计
func get_statistics() -> Dictionary:
	return data.statistics

# 检查过期挑战
func check_expired():
	var current_time = Time.get_unix_time_from_system()
	var expired = []
	
	for challenge in data.active_challenges:
		if current_time > challenge["expires_time"]:
			expired.append(challenge["id"])
	
	for challenge_id in expired:
		abandon_challenge(challenge_id)

# 生成唯一ID
func generate_unique_id() -> String:
	return "challenge_" + str(Time.get_unix_time_from_system()) + "_" + str(randi() % 10000)

# 保存数据
func save_data():
	var save_path = "user://dynamic_quest_challenge.save"
	var file = FileAccess.open(save_path, FileAccess.WRITE)
	if file:
		var json_string = JSON.stringify(data.to_dict())
		file.store_string(json_string)
		file.close()

# 加载数据
func load_data():
	var save_path = "user://dynamic_quest_challenge.save"
	if FileAccess.file_exists(save_path):
		var file = FileAccess.open(save_path, FileAccess.READ)
		if file:
			var json_string = file.get_as_text()
			file.close()
			var json = JSON.new()
			var parse_result = json.parse(json_string)
			if parse_result == OK:
				data.from_dict(json.get_data())
