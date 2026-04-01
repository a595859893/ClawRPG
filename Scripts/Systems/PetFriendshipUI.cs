using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物友谊界面 - 显示宠物友谊关系和互动的UI
/// </summary>
public partial class PetFriendshipUI : Control
{
    private PetManager petManager;
    private PetFriendshipSystem friendshipSystem;

    private Label titleLabel;
    private HBoxContainer petListContainer;
    private VBoxContainer friendshipDetailsContainer;
    private Label statsLabel;
    private Button closeButton;

    private int selectedPetId = -1;
    private int selectedFriendId = -1;

    public override void _Ready()
    {
        petManager = GetNode<PetManager>("/root/Main/PetManager");
        friendshipSystem = GetNode<PetFriendshipSystem>("/root/Main/PetFriendshipSystem");

        SetupUI();
        RefreshPetList();
    }

    private void SetupUI()
    {
        var mainPanel = new PanelContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 100,
            OffsetTop = 50,
            OffsetRight = -100,
            OffsetBottom = -50
        };
        AddChild(mainPanel);

        var mainVBox = new VBoxContainer { MarginLeft = 10, MarginTop = 10, MarginRight = -10, MarginBottom = -10 };
        mainPanel.AddChild(mainVBox);

        titleLabel = new Label
        {
            Text = "🐾 Pet Friendship System",
            Align = Label.AlignEnum.Center,
            CustomMinimumSize = new Vector2(0, 40)
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);

        var hsplit = new HSplitContainer { SizeFlagsVertical = Control.SizeFlags.ExpandAndFill };
        mainVBox.AddChild(hsplit);

        var leftPanel = new ScrollContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandAndFill, CustomMinimumSize = new Vector2(300, 0) };
        hsplit.AddChild(leftPanel);

        petListContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandAndFill };
        leftPanel.AddChild(petListContainer);

        var rightPanel = new ScrollContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandAndFill, CustomMinimumSize = new Vector2(400, 0) };
        hsplit.AddChild(rightPanel);

        friendshipDetailsContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandAndFill };
        rightPanel.AddChild(friendshipDetailsContainer);

        var separator = new HSeparator();
        mainVBox.AddChild(separator);

        statsLabel = new Label { Text = "Statistics: Loading..." };
        mainVBox.AddChild(statsLabel);

        closeButton = new Button { Text = "Close (ESC)", SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd, CustomMinimumSize = new Vector2(120, 40) };
        closeButton.Pressed += () => Hide();
        mainVBox.AddChild(closeButton);

        UpdateStats();
    }

    private void RefreshPetList()
    {
        foreach (var child in petListContainer.GetChildren())
            child.QueueFree();

        if (petManager == null || petManager.PlayerPets == null) return;

        var headerLabel = new Label { Text = "Your Pets:", MarginTop = 10 };
        headerLabel.AddThemeFontSizeOverride("font_size", 18);
        petListContainer.AddChild(headerLabel);

        foreach (var pet in petManager.PlayerPets)
        {
            var petButton = new Button
            {
                Text = $"🐾 {pet.PetName} (ID: {pet.PetId})",
                CustomMinimumSize = new Vector2(0, 50),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandAndFill
            };
            petButton.Pressed += () => OnPetSelected(pet.PetId);
            petListContainer.AddChild(petButton);
        }

        var friendHeader = new Label { Text = "\nFriendships:", MarginTop = 20 };
        friendHeader.AddThemeFontSizeOverride("font_size", 18);
        petListContainer.AddChild(friendHeader);

        var friendships = friendshipSystem.GetAllFriendships();
        int count = 0;
        foreach (var outer in friendships)
        {
            foreach (var kvp in outer.Value)
            {
                var friendButton = new Button
                {
                    Text = $"❤️ Pet {kvp.Value.PetId} ↔ Pet {kvp.Value.FriendPetId}",
                    CustomMinimumSize = new Vector2(0, 40)
                };
                int pet1 = kvp.Value.PetId;
                int pet2 = kvp.Value.FriendPetId;
                friendButton.Pressed += () => ShowFriendshipDetails(pet1, pet2);
                petListContainer.AddChild(friendButton);
                count++;
            }
        }

        if (count == 0)
        {
            var noFriendsLabel = new Label { Text = "No friendships yet!", Modulate = new Color(1, 1, 0, 1) };
            petListContainer.AddChild(noFriendsLabel);
        }
    }

    private void OnPetSelected(int petId)
    {
        selectedPetId = petId;
        UpdateFriendshipDetails();
    }

    private void ShowFriendshipDetails(int petId1, int petId2)
    {
        selectedPetId = petId1;
        selectedFriendId = petId2;
        UpdateFriendshipDetails();
    }

    private void UpdateFriendshipDetails()
    {
        foreach (var child in friendshipDetailsContainer.GetChildren())
            child.QueueFree();

        if (selectedPetId < 0) return;

        var headerLabel = new Label
        {
            Text = $"Pet {selectedPetId} Friendship Details",
            Align = Label.AlignEnum.Center
        };
        headerLabel.AddThemeFontSizeOverride("font_size", 20);
        friendshipDetailsContainer.AddChild(headerLabel);

        friendshipDetailsContainer.AddChild(new VBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandAndFill });

        if (selectedFriendId >= 0)
        {
            var friendship = friendshipSystem.GetFriendship(selectedPetId, selectedFriendId);
            if (friendship != null)
            {
                var details = new VBoxContainer { MarginTop = 20 };
                friendshipDetailsContainer.AddChild(details);

                string tier = PetFriendshipDatabase.GetFriendshipTier(friendship.FriendshipLevel);
                int expNeeded = PetFriendshipDatabase.GetExpForLevel(friendship.FriendshipLevel + 1);
                float bonus = friendshipSystem.GetCombatBonus(selectedPetId, selectedFriendId);

                details.AddChild(new Label { Text = $"Tier: {tier}" });
                details.AddChild(new Label { Text = $"Level: {friendship.FriendshipLevel}" });
                details.AddChild(new Label { Text = $"Experience: {friendship.Experience}/{expNeeded}" });
                details.AddChild(new Label { Text = $"Combat Bonus: {(bonus - 1) * 100:F1}%" });
                details.AddChild(new Label { Text = $"Bonds of War: {(friendship.IsBondsOfWar ? "✅ Active" : "❌ Inactive")}" });

                var equippedSkill = friendshipSystem.GetEquippedSkill(selectedPetId, selectedFriendId);
                details.AddChild(new Label { Text = $"Equipped Skill: {(string.IsNullOrEmpty(equippedSkill) ? "None" : equippedSkill)}" });

                if (friendship.FriendshipLevel >= 5)
                {
                    var skillLabel = new Label { Text = "\nAvailable Skills:" };
                    skillLabel.AddThemeFontSizeOverride("font_size", 16);
                    details.AddChild(skillLabel);

                    var skills = PetFriendshipDatabase.GetFriendshipSkills();
                    foreach (var skill in skills)
                    {
                        var skillButton = new Button { Text = skill, SizeFlagsHorizontal = Control.SizeFlags.ExpandAndFill };
                        int skillLevel = friendship.FriendshipLevel;
                        skillButton.Pressed += () => friendshipSystem.EquipSkill(selectedPetId, selectedFriendId, skill);
                        details.AddChild(skillButton);
                    }
                }

                var bondsButton = new Button { Text = "Toggle Bonds of War", SizeFlagsHorizontal = Control.SizeFlags.ExpandAndFill };
                bondsButton.Pressed += () =>
                {
                    friendshipSystem.SetBondsOfWar(selectedPetId, selectedFriendId, !friendship.IsBondsOfWar);
                    UpdateFriendshipDetails();
                };
                details.AddChild(bondsButton);
            }
        }
        else
        {
            var noSelectionLabel = new Label { Text = "Select a friend to view details", Modulate = new Color(1, 1, 0, 1) };
            friendshipDetailsContainer.AddChild(noSelectionLabel);

            var addFriendLabel = new Label { Text = "\nForm New Friendship:" };
            addFriendLabel.AddThemeFontSizeOverride("font_size", 16);
            friendshipDetailsContainer.AddChild(addFriendLabel);

            var addButton = new Button { Text = "Select Two Pets to Connect", SizeFlagsHorizontal = Control.SizeFlags.ExpandAndFill };
            addButton.Pressed += () => ShowFriendshipOptions(selectedPetId);
            friendshipDetailsContainer.AddChild(addButton);
        }
    }

    private void ShowFriendshipOptions(int petId)
    {
        if (petManager == null || petManager.PlayerPets == null) return;

        var optionsLabel = new Label { Text = "\nSelect second pet:" };
        friendshipDetailsContainer.AddChild(optionsLabel);

        foreach (var pet in petManager.PlayerPets)
        {
            if (pet.PetId != petId)
            {
                var petButton = new Button
                {
                    Text = $"Connect with {pet.PetName}",
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandAndFill
                };
                int targetPetId = pet.PetId;
                petButton.Pressed += () =>
                {
                    friendshipSystem.AddFriendship(petId, targetPetId);
                    RefreshPetList();
                    UpdateFriendshipDetails();
                };
                friendshipDetailsContainer.AddChild(petButton);
            }
        }
    }

    private void UpdateStats()
    {
        var stats = friendshipSystem.GetStatistics();
        statsLabel.Text = $"Total Bonds: {stats["total_bonds"]} | Max Level: {stats["max_level_bonds"]} | " +
            $"Friends: {stats["friend_bonds"]} | Close: {stats["close_friend_bonds"]} | Best: {stats["best_friend_bonds"]} | Soulmates: {stats["soulmate_bonds"]}";
    }

    public override void _Input(InputEvent ev)
    {
        if (ev.IsActionPressed("ui_cancel"))
            Hide();
    }

    public void Show()
    {
        Visible = true;
        RefreshPetList();
    }
}
