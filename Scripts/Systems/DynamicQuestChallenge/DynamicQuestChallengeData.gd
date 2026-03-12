# DynamicQuestChallengeData - 动态任务挑战数据

var active_challenges: Array = []
var completed_challenges: Array = []
var statistics: Dictionary = {
	"total_generated": 0,
	"total_completed": 0,
	"total_abandoned": 0,
	"current_streak": 0,
	"longest_streak": 0,
	"total_gold_earned": 0,
	"total_experience_earned": 0
}

func to_dict() -> Dictionary:
	return {
		"active_challenges": active_challenges,
		"completed_challenges": completed_challenges,
		"statistics": statistics
	}

func from_dict(dict: Dictionary):
	if dict.has("active_challenges"):
		active_challenges = dict["active_challenges"]
	if dict.has("completed_challenges"):
		completed_challenges = dict["completed_challenges"]
	if dict.has("statistics"):
		statistics = dict["statistics"]
