using Godot;

public class CombatRatingMain : Node
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
}
