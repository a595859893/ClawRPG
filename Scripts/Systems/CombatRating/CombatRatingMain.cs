using Godot;

public class CombatRatingMain : BaseSystem
{
	private CombatRatingSystem combatRatingSystem;
	private CombatRatingUI combatRatingUI;
	
	public override void _Ready()
	{
		// Initialize system
		combatRatingSystem = new CombatRatingSystem();
		combatRatingSystem.Name = "CombatRatingSystem";
		GetTree().Root.AddChild(combatRatingSystem);
		
		// Initialize UI
		combatRatingUI = new CombatRatingUI();
		combatRatingUI.Name = "CombatRatingUI";
		combatRatingUI.Visible = false;
		GetTree().Root.AddChild(combatRatingUI);
		
		GD.Print("Combat Rating System initialized");
	}
	
	public void ToggleCombatRatingUI()
	{
		if (combatRatingUI != null)
		{
			combatRatingUI.Toggle();
		}
	}
	
	/// <summary>
	/// 导出保存数据
	/// </summary>
	public override Dictionary ExportSaveData() {
		var data = new Dictionary();
		// CombatRatingMain 是容器系统，无持久化状态
		return data;
	}

	/// <summary>
	/// 导入保存数据
	/// </summary>
	public override void ImportSaveData(Dictionary data) {
		if (data == null) return;
		// CombatRatingMain 是容器系统，无持久化状态
	}
}
