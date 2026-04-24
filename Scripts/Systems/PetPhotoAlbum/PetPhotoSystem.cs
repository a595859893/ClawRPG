using Godot;
using System;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

public class PetPhotoData
{
    // Photo entry
    public class PhotoEntry
    {
        public string PhotoId { get; set; }
        public string PetId { get; set; }
        public string PetName { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public DateTime Timestamp { get; set; }
        public int PhotoQuality { get; set; } // 1-5 stars
        public bool IsFavorite { get; set; }
        public string Notes { get; set; }
    }

    // Photo album
    public class PhotoAlbum
    {
        public string AlbumName { get; set; }
        public List<string> PhotoIds { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }

    // Unlocked locations
    public List<int> UnlockedLocations { get; set; } = new List<int>();

    // All photos
    public List<PhotoEntry> AllPhotos { get; set; } = new List<PhotoEntry>();

    // Albums
    public List<PhotoAlbum> Albums { get; set; } = new List<PhotoAlbum>();

    // Statistics
    public int TotalPhotos { get; set; }
    public int FavoritePhotos { get; set; }
    public int TotalAlbums { get; set; }
    public int LocationsUnlocked { get; set; }

    // Constructor
    public PetPhotoData()
    {
        // Add some starter unlocked locations
        UnlockedLocations.Add(0); // Starting location
    }
}

public class PetPhotoDatabase
{
    // Photo locations
    private static readonly string[] LocationNames = {
        "Pet Home",
        "Forest Clearing",
        "Mountain Peak",
        "Beach Shore",
        "City Park",
        "Ancient Ruins",
        "Enchanted Garden",
        "Volcano Base",
        "Frozen Lake",
        "Crystal Cave",
        "Sunset Valley",
        "Moonlight Grove",
        "Dragon's Lair",
        "Underwater Reef",
        "Sky Temple"
    };

    // Location unlock requirements
    private static readonly Dictionary<int, int> LocationUnlockLevels = new Dictionary<int, int>
    {
        { 0, 1 },   // Pet Home - Level 1
        { 1, 5 },   // Forest Clearing - Level 5
        { 2, 10 },  // Mountain Peak - Level 10
        { 3, 15 },  // Beach Shore - Level 15
        { 4, 20 },  // City Park - Level 20
        { 5, 25 },  // Ancient Ruins - Level 25
        { 6, 30 },  // Enchanted Garden - Level 30
        { 7, 35 },  // Volcano Base - Level 35
        { 8, 40 },  // Frozen Lake - Level 40
        { 9, 45 },  // Crystal Cave - Level 45
        { 10, 50 }, // Sunset Valley - Level 50
        { 11, 55 }, // Moonlight Grove - Level 55
        { 12, 60 }, // Dragon's Lair - Level 60
        { 13, 70 }, // Underwater Reef - Level 70
        { 14, 80 }  // Sky Temple - Level 80
    };

    // Photo quality bonuses
    private static readonly int[] QualityScoreRequirements = { 0, 100, 500, 2000, 5000, 10000 };

    public static string GetLocationName(int locationId)
    {
        if (locationId >= 0 && locationId < LocationNames.Length)
            return LocationNames[locationId];
        return "Unknown Location";
    }

    public static int GetLocationUnlockLevel(int locationId)
    {
        if (LocationUnlockLevels.ContainsKey(locationId))
            return LocationUnlockLevels[locationId];
        return 999;
    }

    public static bool CanUnlockLocation(int locationId, int petLevel)
    {
        return petLevel >= GetLocationUnlockLevel(locationId);
    }

    public static int GetLocationCount()
    {
        return LocationNames.Length;
    }

    public static int CalculatePhotoQuality(int petLevel, int locationId, bool isFavorite)
    {
        // Base quality on pet level
        int baseQuality = Math.Min(5, petLevel / 15 + 1);
        
        // Bonus for special locations
        if (locationId >= 10)
            baseQuality = Math.Min(5, baseQuality + 1);
        
        // Bonus for favorite
        if (isFavorite)
            baseQuality = Math.Min(5, baseQuality + 1);
        
        return baseQuality;
    }
}

public partial class PetPhotoSystem : BaseSystem
{
    private static PetPhotoSystem _instance;
    public static PetPhotoSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new PetPhotoSystem();
            return _instance;
        }
    }

    private PetPhotoData _data = new PetPhotoData();
    private int _nextPhotoId = 1;

    protected override void Initialize()
    {
        base.Initialize();
        GD.Print("[PetPhotoSystem] Initialized");
    }

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Godot.Collections.Dictionary();
        
        // 保存已解锁的位置
        var unlockedLocations = new Godot.Array();
        foreach (int locId in _data.UnlockedLocations)
        {
            unlockedLocations.Add(locId);
        }
        data["unlocked_locations"] = unlockedLocations;
        
        // 保存所有照片
        var photosArray = new Godot.Array();
        foreach (var photo in _data.AllPhotos)
        {
            var photoData = new Godot.Collections.Dictionary();
            photoData["photo_id"] = photo.PhotoId;
            photoData["pet_id"] = photo.PetId;
            photoData["pet_name"] = photo.PetName;
            photoData["location_id"] = photo.LocationId;
            photoData["location_name"] = photo.LocationName;
            photoData["timestamp"] = photo.Timestamp.ToString("o");
            photoData["quality"] = photo.PhotoQuality;
            photoData["is_favorite"] = photo.IsFavorite;
            photoData["notes"] = photo.Notes;
            photosArray.Add(photoData);
        }
        data["all_photos"] = photosArray;
        
        // 保存相册
        var albumsArray = new Godot.Array();
        foreach (var album in _data.Albums)
        {
            var albumData = new Godot.Collections.Dictionary();
            albumData["album_name"] = album.AlbumName;
            var photoIdsArray = new Godot.Array();
            foreach (string photoId in album.PhotoIds)
            {
                photoIdsArray.Add(photoId);
            }
            albumData["photo_ids"] = photoIdsArray;
            albumData["created_at"] = album.CreatedAt.ToString("o");
            albumsArray.Add(albumData);
        }
        data["albums"] = albumsArray;
        
        // 保存统计数据
        var stats = new Godot.Collections.Dictionary();
        stats["total_photos"] = _data.TotalPhotos;
        stats["favorite_photos"] = _data.FavoritePhotos;
        stats["total_albums"] = _data.TotalAlbums;
        stats["locations_unlocked"] = _data.LocationsUnlocked;
        stats["next_photo_id"] = _nextPhotoId;
        data["stats"] = stats;
        
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 加载已解锁的位置
        if (data.ContainsKey("unlocked_locations"))
        {
            _data.UnlockedLocations.Clear();
            var unlockedLocations = (Godot.Array)data["unlocked_locations"];
            foreach (int locId in unlockedLocations)
            {
                _data.UnlockedLocations.Add(locId);
            }
        }
        
        // 加载所有照片
        if (data.ContainsKey("all_photos"))
        {
            _data.AllPhotos.Clear();
            var photosArray = (Godot.Array)data["all_photos"];
            foreach (Godot.Collections.Dictionary photoData in photosArray)
            {
                var photo = new PetPhotoData.PhotoEntry
                {
                    PhotoId = (string)photoData["photo_id"],
                    PetId = (string)photoData["pet_id"],
                    PetName = (string)photoData["pet_name"],
                    LocationId = (int)photoData["location_id"],
                    LocationName = (string)photoData["location_name"],
                    Timestamp = DateTime.Parse((string)photoData["timestamp"]),
                    PhotoQuality = (int)photoData["quality"],
                    IsFavorite = (bool)photoData["is_favorite"],
                    Notes = (string)photoData["notes"]
                };
                _data.AllPhotos.Add(photo);
            }
        }
        
        // 加载相册
        if (data.ContainsKey("albums"))
        {
            _data.Albums.Clear();
            var albumsArray = (Godot.Array)data["albums"];
            foreach (Godot.Collections.Dictionary albumData in albumsArray)
            {
                var album = new PetPhotoData.PhotoAlbum
                {
                    AlbumName = (string)albumData["album_name"],
                    CreatedAt = DateTime.Parse((string)albumData["created_at"])
                };
                var photoIdsArray = (Godot.Array)albumData["photo_ids"];
                foreach (string photoId in photoIdsArray)
                {
                    album.PhotoIds.Add(photoId);
                }
                _data.Albums.Add(album);
            }
        }
        
        // 加载统计数据
        if (data.ContainsKey("stats"))
        {
            var stats = (Godot.Collections.Dictionary)data["stats"];
            _data.TotalPhotos = (int)stats["total_photos"];
            _data.FavoritePhotos = (int)stats["favorite_photos"];
            _data.TotalAlbums = (int)stats["total_albums"];
            _data.LocationsUnlocked = (int)stats["locations_unlocked"];
            _nextPhotoId = (int)stats["next_photo_id"];
        }
        
        GD.Print($"[PetPhotoSystem] Loaded {_data.AllPhotos.Count} photos, {_data.Albums.Count} albums");
    }

    // Take a photo
    public PetPhotoData.PhotoEntry TakePhoto(string petId, string petName, int locationId, int petLevel)
    {
        if (!IsLocationUnlocked(locationId))
        {
            GD.Print($"[PetPhotoSystem] Location {locationId} is not unlocked yet!");
            return null;
        }

        string locationName = PetPhotoDatabase.GetLocationName(locationId);
        
        var photo = new PetPhotoData.PhotoEntry
        {
            PhotoId = $"photo_{_nextPhotoId++}",
            PetId = petId,
            PetName = petName,
            LocationId = locationId,
            LocationName = locationName,
            Timestamp = DateTime.Now,
            PhotoQuality = PetPhotoDatabase.CalculatePhotoQuality(petLevel, locationId, false),
            IsFavorite = false,
            Notes = ""
        };

        _data.AllPhotos.Add(photo);
        _data.TotalPhotos++;

        // Update statistics
        UpdateStatistics();

        GD.Print($"[PetPhotoSystem] Photo taken: {photo.PhotoId} at {locationName} with quality {photo.PhotoQuality}");

        return photo;
    }

    // Unlock location
    public bool UnlockLocation(int locationId, int petLevel)
    {
        if (_data.UnlockedLocations.Contains(locationId))
            return true;

        if (PetPhotoDatabase.CanUnlockLocation(locationId, petLevel))
        {
            _data.UnlockedLocations.Add(locationId);
            _data.LocationsUnlocked = _data.UnlockedLocations.Count;
            GD.Print($"[PetPhotoSystem] Location unlocked: {PetPhotoDatabase.GetLocationName(locationId)}");
            return true;
        }

        return false;
    }

    // Check if location is unlocked
    public bool IsLocationUnlocked(int locationId)
    {
        return _data.UnlockedLocations.Contains(locationId);
    }

    // Get unlocked locations
    public List<int> GetUnlockedLocations()
    {
        return new List<int>(_data.UnlockedLocations);
    }

    // Toggle favorite
    public void ToggleFavorite(string photoId)
    {
        foreach (var photo in _data.AllPhotos)
        {
            if (photo.PhotoId == photoId)
            {
                photo.IsFavorite = !photo.IsFavorite;
                _data.FavoritePhotos = CountFavorites();
                break;
            }
        }
    }

    // Add notes to photo
    public void AddNotes(string photoId, string notes)
    {
        foreach (var photo in _data.AllPhotos)
        {
            if (photo.PhotoId == photoId)
            {
                photo.Notes = notes;
                break;
            }
        }
    }

    // Create album
    public void CreateAlbum(string albumName)
    {
        var album = new PetPhotoData.PhotoAlbum
        {
            AlbumName = albumName,
            CreatedAt = DateTime.Now
        };
        _data.Albums.Add(album);
        _data.TotalAlbums++;
    }

    // Add photo to album
    public bool AddPhotoToAlbum(string photoId, string albumName)
    {
        foreach (var album in _data.Albums)
        {
            if (album.AlbumName == albumName)
            {
                if (!album.PhotoIds.Contains(photoId))
                {
                    album.PhotoIds.Add(photoId);
                    return true;
                }
            }
        }
        return false;
    }

    // Get all photos
    public List<PetPhotoData.PhotoEntry> GetAllPhotos()
    {
        return new List<PetPhotoData.PhotoEntry>(_data.AllPhotos);
    }

    // Get favorite photos
    public List<PetPhotoData.PhotoEntry> GetFavoritePhotos()
    {
        List<PetPhotoData.PhotoEntry> favorites = new List<PetPhotoData.PhotoEntry>();
        foreach (var photo in _data.AllPhotos)
        {
            if (photo.IsFavorite)
                favorites.Add(photo);
        }
        return favorites;
    }

    // Get photos by location
    public List<PetPhotoData.PhotoEntry> GetPhotosByLocation(int locationId)
    {
        List<PetPhotoData.PhotoEntry> locationPhotos = new List<PetPhotoData.PhotoEntry>();
        foreach (var photo in _data.AllPhotos)
        {
            if (photo.LocationId == locationId)
                locationPhotos.Add(photo);
        }
        return locationPhotos;
    }

    // Get photos by pet
    public List<PetPhotoData.PhotoEntry> GetPhotosByPet(string petId)
    {
        List<PetPhotoData.PhotoEntry> petPhotos = new List<PetPhotoData.PhotoEntry>();
        foreach (var photo in _data.AllPhotos)
        {
            if (photo.PetId == petId)
                petPhotos.Add(photo);
        }
        return petPhotos;
    }

    // Count favorites
    private int CountFavorites()
    {
        int count = 0;
        foreach (var photo in _data.AllPhotos)
        {
            if (photo.IsFavorite) count++;
        }
        return count;
    }

    // Update statistics
    private void UpdateStatistics()
    {
        _data.TotalPhotos = _data.AllPhotos.Count;
        _data.FavoritePhotos = CountFavorites();
        _data.TotalAlbums = _data.Albums.Count;
        _data.LocationsUnlocked = _data.UnlockedLocations.Count;
    }

    // Get statistics
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "total_photos", _data.TotalPhotos },
            { "favorite_photos", _data.FavoritePhotos },
            { "total_albums", _data.TotalAlbums },
            { "locations_unlocked", _data.LocationsUnlocked }
        };
    }

    // Load from file
    public void LoadFromFile(string filePath)
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) return;

        // 加载已解锁地点
        if (data.ContainsKey("pet_photo_unlocked_locations"))
        {
            var locationsArray = (Godot.Array)data["pet_photo_unlocked_locations"];
            _data.UnlockedLocations.Clear();
            foreach (int locId in locationsArray)
            {
                _data.UnlockedLocations.Add(locId);
            }
        }

        // 加载照片数据
        if (data.ContainsKey("pet_photo_photos"))
        {
            var photosArray = (Godot.Array)data["pet_photo_photos"];
            _data.AllPhotos.Clear();
            foreach (Godot.Collections.Dictionary photoData in photosArray)
            {
                var photo = new PetPhotoData.PhotoEntry
                {
                    PhotoId = (string)photoData["photo_id"],
                    PetId = (string)photoData["pet_id"],
                    PetName = (string)photoData["pet_name"],
                    LocationId = (int)photoData["location_id"],
                    LocationName = (string)photoData["location_name"],
                    Timestamp = DateTime.Parse((string)photoData["timestamp"]),
                    PhotoQuality = (int)photoData["quality"],
                    IsFavorite = (bool)photoData["is_favorite"],
                    Notes = (string)photoData["notes"]
                };
                _data.AllPhotos.Add(photo);
                
                // 更新 nextPhotoId
                int photoNum = int.Parse(photo.PhotoId.Replace("photo_", ""));
                if (photoNum >= _nextPhotoId)
                    _nextPhotoId = photoNum + 1;
            }
        }

        // 加载相册数据
        if (data.ContainsKey("pet_photo_albums"))
        {
            var albumsArray = (Godot.Array)data["pet_photo_albums"];
            _data.Albums.Clear();
            foreach (Godot.Collections.Dictionary albumData in albumsArray)
            {
                var album = new PetPhotoData.PhotoAlbum
                {
                    AlbumName = (string)albumData["album_name"],
                    CreatedAt = DateTime.Parse((string)albumData["created_at"])
                };
                var photoIdsArray = (Godot.Array)albumData["photo_ids"];
                foreach (string photoId in photoIdsArray)
                {
                    album.PhotoIds.Add(photoId);
                }
                _data.Albums.Add(album);
            }
        }

        // 加载统计数据
        if (data.ContainsKey("pet_photo_stats"))
        {
            var stats = (Godot.Collections.Dictionary)data["pet_photo_stats"];
            _data.TotalPhotos = (int)stats["total_photos"];
            _data.FavoritePhotos = (int)stats["favorite_photos"];
            _data.TotalAlbums = (int)stats["total_albums"];
            _data.LocationsUnlocked = (int)stats["locations_unlocked"];
        }

        GD.Print($"[PetPhotoSystem] Loaded {_data.AllPhotos.Count} photos, {_data.Albums.Count} albums");
    }

    // Save to file
    public void SaveToFile(string filePath)
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Collections.Dictionary();

        // 保存已解锁地点
        var locationsArray = new Godot.Array();
        foreach (int locId in _data.UnlockedLocations)
        {
            locationsArray.Add(locId);
        }
        data["pet_photo_unlocked_locations"] = locationsArray;

        // 保存照片数据
        var photosArray = new Godot.Array();
        foreach (var photo in _data.AllPhotos)
        {
            var photoData = new Godot.Collections.Dictionary();
            photoData["photo_id"] = photo.PhotoId;
            photoData["pet_id"] = photo.PetId;
            photoData["pet_name"] = photo.PetName;
            photoData["location_id"] = photo.LocationId;
            photoData["location_name"] = photo.LocationName;
            photoData["timestamp"] = photo.Timestamp.ToString("o");
            photoData["quality"] = photo.PhotoQuality;
            photoData["is_favorite"] = photo.IsFavorite;
            photoData["notes"] = photo.Notes;
            photosArray.Add(photoData);
        }
        data["pet_photo_photos"] = photosArray;

        // 保存相册数据
        var albumsArray = new Godot.Array();
        foreach (var album in _data.Albums)
        {
            var albumData = new Godot.Collections.Dictionary();
            albumData["album_name"] = album.AlbumName;
            albumData["created_at"] = album.CreatedAt.ToString("o");
            var photoIdsArray = new Godot.Array();
            foreach (string photoId in album.PhotoIds)
            {
                photoIdsArray.Add(photoId);
            }
            albumData["photo_ids"] = photoIdsArray;
            albumsArray.Add(albumData);
        }
        data["pet_photo_albums"] = albumsArray;

        // 保存统计数据
        var stats = new Godot.Collections.Dictionary();
        stats["total_photos"] = _data.TotalPhotos;
        stats["favorite_photos"] = _data.FavoritePhotos;
        stats["total_albums"] = _data.TotalAlbums;
        stats["locations_unlocked"] = _data.LocationsUnlocked;
        data["pet_photo_stats"] = stats;

        saveSystem.SaveGame(data);
        GD.Print($"[PetPhotoSystem] Saved {_data.AllPhotos.Count} photos, {_data.Albums.Count} albums");
    }
}

public class PetPhotoManager
{
    private static PetPhotoManager _instance;
    public static PetPhotoManager Instance
    {
        get
        {
            if (_instance == null) _instance = new PetPhotoManager();
            return _instance;
        }
    }

    public PetPhotoSystem PetPhotoSystem { get; private set; }
    public PetPhotoUI PetPhotoUI { get; private set; }

    public PetPhotoManager()
    {
        PetPhotoSystem = new PetPhotoSystem();
    }

    public void Initialize(PetPhotoUI ui)
    {
        PetPhotoUI = ui;
        GD.Print("[PetPhotoManager] Initialized");
    }

    public void ToggleUI()
    {
        if (PetPhotoUI != null)
        {
            PetPhotoUI.Visible = !PetPhotoUI.Visible;
        }
    }
}
