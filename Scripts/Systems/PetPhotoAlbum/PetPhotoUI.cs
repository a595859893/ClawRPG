using Godot;
using System;
using System.Collections.Generic;

public partial class PetPhotoUI : Control
{
    private TabContainer _tabContainer;
    private OptionButton _locationSelector;
    private VBoxContainer _photoList;
    private Label _statsLabel;
    
    // Colors
    private Color GoldColor = new Color(1f, 0.84f, 0f);
    private Color FavoriteColor = new Color(1f, 0.4f, 0.4f);
    private Color LocationColor = new Color(0.4f, 0.8f, 0.4f);

    public override void _Ready()
    {
        SetupUI();
        UpdatePhotoList();
        GD.Print("[PetPhotoUI] Ready");
    }

    private void SetupUI()
    {
        // Main container
        VBoxContainer mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(650, 550);
        AddChild(mainContainer);

        // Title
        Label title = new Label();
        title.Text = " 📸 Pet Photo Album";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(title);

        // Quick actions
        HBoxContainer actionContainer = new HBoxContainer();
        mainContainer.AddChild(actionContainer);

        Button takePhotoBtn = new Button();
        takePhotoBtn.Text = "Take Photo 📷";
        takePhotoBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        takePhotoBtn.Pressed += _on_take_photo_pressed;
        actionContainer.AddChild(takePhotoBtn);

        Button unlockBtn = new Button();
        unlockBtn.Text = "Unlock Location 🌍";
        unlockBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        unlockBtn.Pressed += _on_unlock_location_pressed;
        actionContainer.AddChild(unlockBtn);

        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);

        // Photos tab
        Control photosTab = new Control();
        photosTab.Name = "Photos";
        _tabContainer.AddChild(photosTab);

        _photoList = new VBoxContainer();
        _photoList.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _photoList.MarginLeft = 10;
        _photoList.MarginTop = 10;
        _photoList.MarginRight = -10;
        _photoList.MarginBottom = -10;
        photosTab.AddChild(_photoList);

        // Locations tab
        Control locationsTab = new Control();
        locationsTab.Name = "Locations";
        _tabContainer.AddChild(locationsTab);

        VBoxContainer locationsContainer = new VBoxContainer();
        locationsContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        locationsContainer.MarginLeft = 10;
        locationsContainer.MarginTop = 10;
        locationsContainer.MarginRight = -10;
        locationsContainer.MarginBottom = -10;
        locationsTab.AddChild(locationsContainer);

        Label locationsTitle = new Label();
        locationsTitle.Text = "Unlocked Locations";
        locationsTitle.AddThemeFontSizeOverride("font_size", 18);
        locationsContainer.AddChild(locationsTitle);

        ScrollContainer locationsScroll = new ScrollContainer();
        locationsScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        locationsContainer.AddChild(locationsScroll);

        VBoxContainer locationsList = new VBoxContainer();
        locationsScroll.AddChild(locationsList);

        // Populate locations
        int locationCount = PetPhotoDatabase.GetLocationCount();
        for (int i = 0; i < locationCount; i++)
        {
            bool unlocked = PetPhotoSystem.Instance.IsLocationUnlocked(i);
            string locationName = PetPhotoDatabase.GetLocationName(i);
            int unlockLevel = PetPhotoDatabase.GetLocationUnlockLevel(i);

            HBoxContainer locRow = new HBoxContainer();

            Label statusIcon = new Label();
            statusIcon.Text = unlocked ? "✅" : "🔒";
            statusIcon.CustomMinimumSize = new Vector2(30, 0);
            locRow.AddChild(statusIcon);

            Label locLabel = new Label();
            locLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            locLabel.Text = $"{locationName}";
            if (!unlocked)
            {
                locLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                locLabel.Text += $" (Lv. {unlockLevel})";
            }
            else
            {
                locLabel.AddThemeColorOverride("font_color", LocationColor);
            }
            locRow.AddChild(locLabel);

            // Photo count
            int photoCount = PetPhotoSystem.Instance.GetPhotosByLocation(i).Count;
            Label countLabel = new Label();
            countLabel.Text = $"{photoCount} 📷";
            countLabel.CustomMinimumSize = new Vector2(60, 0);
            locRow.AddChild(countLabel);

            locationsList.AddChild(locRow);
        }

        // Albums tab
        Control albumsTab = new Control();
        albumsTab.Name = "Albums";
        _tabContainer.AddChild(albumsTab);

        VBoxContainer albumsContainer = new VBoxContainer();
        albumsContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        albumsContainer.MarginLeft = 10;
        albumsContainer.MarginTop = 10;
        albumsContainer.MarginRight = -10;
        albumsContainer.MarginBottom = -10;
        albumsTab.AddChild(albumsContainer);

        // Create album button
        Button createAlbumBtn = new Button();
        createAlbumBtn.Text = "Create New Album +";
        createAlbumBtn.Pressed += _on_create_album_pressed;
        albumsContainer.AddChild(createAlbumBtn);

        // Statistics tab
        Control statsTab = new Control();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);

        VBoxContainer statsContainer = new VBoxContainer();
        statsContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        statsContainer.MarginLeft = 10;
        statsContainer.MarginTop = 10;
        statsContainer.MarginRight = -10;
        statsContainer.MarginBottom = -10;
        statsTab.AddChild(statsContainer);

        _statsLabel = new Label();
        _statsLabel.Text = "Loading statistics...";
        statsContainer.AddChild(_statsLabel);

        // Close button
        Button closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Align = Button.AlignEnum.Center;
        closeButton.Pressed += _on_close_pressed;
        mainContainer.AddChild(closeButton);

        // Initial stats update
        UpdateStatistics();
    }

    private void UpdatePhotoList()
    {
        // Clear existing entries
        foreach (Node child in _photoList.GetChildren())
        {
            child.QueueFree();
        }

        var photos = PetPhotoSystem.Instance.GetAllPhotos();

        if (photos.Count == 0)
        {
            Label noDataLabel = new Label();
            noDataLabel.Text = "No photos yet! Take a photo with your pet.";
            noDataLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _photoList.AddChild(noDataLabel);
            return;
        }

        // Sort by timestamp (newest first)
        photos.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

        foreach (var photo in photos)
        {
            VBoxContainer photoCard = new VBoxContainer();
            photoCard.AddThemeStyleBoxOverride("panel", CreateCardStyle());

            // Header
            HBoxContainer header = new HBoxContainer();

            // Favorite icon
            Label favIcon = new Label();
            favIcon.Text = photo.IsFavorite ? "❤️" : "🤍";
            favIcon.CustomMinimumSize = new Vector2(30, 0);
            header.AddChild(favIcon);

            // Pet name
            Label petLabel = new Label();
            petLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            petLabel.Text = photo.PetName;
            petLabel.AddThemeFontSizeOverride("font_size", 16);
            header.AddChild(petLabel);

            // Quality stars
            Label qualityLabel = new Label();
            string stars = "";
            for (int i = 0; i < photo.PhotoQuality; i++) stars += "⭐";
            qualityLabel.Text = stars;
            header.AddChild(qualityLabel);

            photoCard.AddChild(header);

            // Location
            Label locationLabel = new Label();
            locationLabel.Text = $"📍 {photo.LocationName}";
            locationLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            photoCard.AddChild(locationLabel);

            // Timestamp
            Label timeLabel = new Label();
            timeLabel.Text = $"🕐 {photo.Timestamp:yyyy-MM-dd HH:mm}";
            timeLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            photoCard.AddChild(timeLabel);

            // Notes (if any)
            if (!string.IsNullOrEmpty(photo.Notes))
            {
                Label notesLabel = new Label();
                notesLabel.Text = $"📝 {photo.Notes}";
                notesLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
                photoCard.AddChild(notesLabel);
            }

            // Actions
            HBoxContainer actions = new HBoxContainer();

            Button favBtn = new Button();
            favBtn.Text = photo.IsFavorite ? "Unfavorite" : "❤️ Favorite";
            favBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            favBtn.Pressed += () => _on_favorite_pressed(photo.PhotoId);
            actions.AddChild(favBtn);

            Button deleteBtn = new Button();
            deleteBtn.Text = "🗑️ Delete";
            deleteBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            deleteBtn.Pressed += () => _on_delete_pressed(photo.PhotoId);
            actions.AddChild(deleteBtn);

            photoCard.AddChild(actions);

            _photoList.AddChild(photoCard);
        }
    }

    private StyleBoxFlat CreateCardStyle()
    {
        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.15f, 0.2f);
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.ContentMarginLeft = 10;
        style.ContentMarginTop = 8;
        style.ContentMarginRight = 10;
        style.ContentMarginBottom = 8;
        return style;
    }

    private void UpdateStatistics()
    {
        var stats = PetPhotoSystem.Instance.GetStatistics();

        string statsText = "📸 Photo Album Statistics\n\n";
        statsText += $"Total Photos: {stats["total_photos"]}\n";
        statsText += $"Favorite Photos: {stats["favorite_photos"]}\n";
        statsText += $"Albums Created: {stats["total_albums"]}\n";
        statsText += $"Locations Unlocked: {stats["locations_unlocked"]}/{PetPhotoDatabase.GetLocationCount()}\n";

        if (_statsLabel != null)
            _statsLabel.Text = statsText;
    }

    private void _on_take_photo_pressed()
    {
        // Get first unlocked location (or random from unlocked)
        var unlocked = PetPhotoSystem.Instance.GetUnlockedLocations();
        if (unlocked.Count == 0)
        {
            GD.Print("[PetPhotoUI] No locations unlocked!");
            return;
        }

        // Take photo with default pet (for demo)
        var photo = PetPhotoSystem.Instance.TakePhoto("pet_1", "My Pet", unlocked[0], 30);
        
        if (photo != null)
        {
            UpdatePhotoList();
            UpdateStatistics();
        }
    }

    private void _on_unlock_location_pressed()
    {
        // Try to unlock next available location
        var unlocked = PetPhotoSystem.Instance.GetUnlockedLocations();
        int nextLocation = unlocked.Count; // Next location index

        if (nextLocation >= PetPhotoDatabase.GetLocationCount())
        {
            GD.Print("[PetPhotoUI] All locations already unlocked!");
            return;
        }

        // Unlock with level 999 (for demo, unlock all)
        if (PetPhotoSystem.Instance.UnlockLocation(nextLocation, 999))
        {
            UpdateStatistics();
            // Refresh locations tab
            _tabContainer.CurrentTab = 1;
        }
    }

    private void _on_favorite_pressed(string photoId)
    {
        PetPhotoSystem.Instance.ToggleFavorite(photoId);
        UpdatePhotoList();
        UpdateStatistics();
    }

    private void _on_delete_pressed(string photoId)
    {
        // For now, just refresh - delete not implemented
        GD.Print($"[PetPhotoUI] Delete photo: {photoId}");
        UpdatePhotoList();
    }

    private void _on_create_album_pressed()
    {
        string albumName = $"Album {_data.Albums.Count + 1}";
        PetPhotoSystem.Instance.CreateAlbum(albumName);
        UpdateStatistics();
    }

    private void _on_close_pressed()
    {
        Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Scancode == KeyList.Escape)
            {
                Visible = false;
            }
        }
    }

    // Reference to data for album count
    private PetPhotoData _data = new PetPhotoData();
}
