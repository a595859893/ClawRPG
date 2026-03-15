extends Node
class_name BaseSystem

# BaseSystem - 所有游戏系统的基类
# 提供统一的生命周期管理和数据持久化接口

## 系统是否已初始化
var is_initialized: bool = false

## 系统名称（用于日志）
var system_name: String = ""

func _ready():
	# 自动调用初始化
	initialize()
	if not system_name:
		system_name = get_class_name()

# 获取类名
func get_class_name() -> String:
	var script = get_script()
	if script:
		return script.get_global_name().get_basename()
	return "BaseSystem"

## 初始化系统 - 子类重写此方法进行初始化
func initialize() -> void:
	is_initialized = true
	print("[BaseSystem] %s initialized" % system_name)

## 关闭系统 - 子类重写此方法进行清理
func shutdown() -> void:
	is_initialized = false
	print("[BaseSystem] %s shutdown" % system_name)

## 导出保存数据 - 子类重写此方法实现数据持久化
## @return 可序列化的字典数据
func export_save_data() -> Dictionary:
	return {}

## 导入保存数据 - 子类重写此方法实现数据加载
## @param data 保存的字典数据
func import_save_data(data: Dictionary) -> void:
	if data == null or data.is_empty():
		return

## 重置系统数据
func reset() -> void:
	is_initialized = false

## 获取系统唯一ID
func get_id() -> String:
	return system_name

## 系统间通信：发送消息到指定系统
## @param target_system 目标系统名称
## @param message 消息内容
## @param data 附加数据
func send_message(target_system: String, message: String, data: Dictionary = {}) -> void:
	var target = get_tree().get_root().get_node_or_null(target_system)
	if target and target.has_method("receive_message"):
		target.receive_message(system_name, message, data)

## 系统间通信：接收消息
## @param from_system 来源系统名称
## @param message 消息内容
## @param data 附加数据
func receive_message(from_system: String, message: String, data: Dictionary = {}) -> void:
	print("[BaseSystem] %s received message from %s: %s" % [system_name, from_system, message])
