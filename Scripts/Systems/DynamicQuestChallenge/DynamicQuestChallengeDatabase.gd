extends BaseSystem

# DynamicQuestChallengeDatabase - 动态任务挑战数据库
# 提供挑战模板和生成逻辑

func _ready():
	super._ready()
	system_name = "DynamicQuestChallengeDatabase"

func initialize():
	super.initialize()

func export_save_data() -> Dictionary:
	# 数据库类通常不需要保存数据，返回空字典
	return {}

func import_save_data(data: Dictionary):
	# 数据库类通常不需要加载数据
	pass

var challenge_templates = {
	"Combat": [
		{
			"template_id": "combat_kill_enemies",
			"name": "Enemy Slayer",
			"description": "Defeat %d enemies",
			"type": "Combat",
			"category": "Battle",
			"target_type": "kill_count",
			"base_target": 20,
			"duration": 300,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 100,
				"experience": 50
			}
		},
		{
			"template_id": "combat_boss_defeat",
			"name": "Boss Hunter",
			"description": "Defeat %d boss enemies",
			"type": "Combat",
			"category": "Boss",
			"target_type": "boss_kill",
			"base_target": 3,
			"duration": 600,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 500,
				"experience": 200
			}
		},
		{
			"template_id": "combat_damage_dealt",
			"name": "Damage Dealer",
			"description": "Deal %d total damage",
			"type": "Combat",
			"category": "Damage",
			"target_type": "damage_dealt",
			"base_target": 1000,
			"duration": 300,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 150,
				"experience": 75
			}
		}
	],
	"Collection": [
		{
			"template_id": "collect_items",
			"name": "Collector",
			"description": "Collect %d items",
			"type": "Collection",
			"category": "Gathering",
			"target_type": "item_collect",
			"base_target": 15,
			"duration": 400,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 80,
				"experience": 40
			}
		},
		{
			"template_id": "collect_gold",
			"name": "Treasure Hunter",
			"description": "Collect %d gold from loot",
			"type": "Collection",
			"category": "Wealth",
			"target_type": "gold_collect",
			"base_target": 500,
			"duration": 300,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 50,
				"experience": 25
			}
		}
	],
	"Exploration": [
		{
			"template_id": "explore_dungeons",
			"name": "Dungeon Explorer",
			"description": "Complete %d dungeon floors",
			"type": "Exploration",
			"category": "Dungeon",
			"target_type": "dungeon_floor",
			"base_target": 5,
			"duration": 600,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 200,
				"experience": 100
			}
		},
		{
			"template_id": "explore_areas",
			"name": "World Traveler",
			"description": "Visit %d new areas",
			"type": "Exploration",
			"category": "World",
			"target_type": "area_visit",
			"base_target": 10,
			"duration": 500,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 120,
				"experience": 60
			}
		}
	],
	"Social": [
		{
			"template_id": "social_friends",
			"name": "Social Butterfly",
			"description": "Add %d new friends",
			"type": "Social",
			"category": "Friends",
			"target_type": "friend_add",
			"base_target": 3,
			"duration": 600,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 100,
				"experience": 50
			}
		},
		{
			"template_id": "social_guild",
			"name": "Guild Member",
			"description": "Complete %d guild quests",
			"type": "Social",
			"category": "Guild",
			"target_type": "guild_quest",
			"base_target": 5,
			"duration": 600,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 150,
				"experience": 75
			}
		}
	],
	"Economy": [
		{
			"template_id": "economy_trade",
			"name": "Merchant",
			"description": "Complete %d trades in auction house",
			"type": "Economy",
			"category": "Trading",
			"target_type": "trade_complete",
			"base_target": 10,
			"duration": 600,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 200,
				"experience": 50
			}
		},
		{
			"template_id": "economy_earn",
			"name": "Wealth Builder",
			"description": "Earn %d total gold",
			"type": "Economy",
			"category": "Wealth",
			"target_type": "gold_earn",
			"base_target": 1000,
			"duration": 500,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 100,
				"experience": 30
			}
		}
	],
	"Pet": [
		{
			"template_id": "pet_battle",
			"name": "Pet Battler",
			"description": "Win %d pet battles",
			"type": "Pet",
			"category": "Battle",
			"target_type": "pet_battle_win",
			"base_target": 10,
			"duration": 400,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 100,
				"experience": 50
			}
		},
		{
			"template_id": "pet_interact",
			"name": "Pet Lover",
			"description": "Interact with your pet %d times",
			"type": "Pet",
			"category": "Interaction",
			"target_type": "pet_interact",
			"base_target": 20,
			"duration": 400,
			"difficulty_scales": {
				"Easy": 1.0,
				"Medium": 1.5,
				"Hard": 2.0,
				"Epic": 3.0,
				"Legendary": 5.0
			},
			"rewards": {
				"gold": 50,
				"experience": 30
			}
		}
	]
}

func get_challenge_types() -> Array:
	return challenge_templates.keys()

func get_challenges_by_type(challenge_type: String) -> Array:
	return challenge_templates.get(challenge_type, [])

func generate_challenge(challenge_type: String, difficulty: String, player_level: int, player_class: String) -> Dictionary:
	var challenges = get_challenges_by_type(challenge_type)
	if challenges.is_empty():
		return {}
	
	var template = challenges[randi() % challenges.size()]
	var scale = template["difficulty_scales"].get(difficulty, 1.0)
	var target = int(template["base_target"] * scale)
	
	# 根据玩家等级调整
	target = int(target * (1.0 + player_level * 0.05))
	
	# 计算奖励
	var rewards = template["rewards"].duplicate()
	rewards["gold"] = int(rewards["gold"] * scale * (1.0 + player_level * 0.02))
	rewards["experience"] = int(rewards["experience"] * scale * (1.0 + player_level * 0.03))
	
	var challenge = {
		"template_id": template["template_id"],
		"name": template["name"],
		"description": template["description"] % target,
		"type": template["type"],
		"category": template["category"],
		"target_type": template["target_type"],
		"target_amount": target,
		"difficulty": difficulty,
		"duration": int(template["duration"] * (1.0 if difficulty == "Easy" else (0.8 if difficulty == "Legendary" else 1.0))),
		"rewards": rewards,
		"player_level": player_level,
		"player_class": player_class
	}
	
	return challenge
