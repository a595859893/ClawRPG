using Godot;
using System;
using System.Collections.Generic;

public class TradeRouteDatabase
{
    private static TradeRouteDatabase _instance;
    public static TradeRouteDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new TradeRouteDatabase();
            return _instance;
        }
    }

    // 城市/地区配置
    public class City
    {
        public string CityId { get; set; }
        public string CityName { get; set; }
        public string Region { get; set; } // Eastern/Western/Southern/Northern/Central
        public List<string> AvailableGoods { get; set; }
    }

    // 贸易路线模板
    public class RouteTemplate
    {
        public string RouteId { get; set; }
        public string RouteName { get; set; }
        public string StartCity { get; set; }
        public string EndCity { get; set; }
        public int BaseDistance { get; set; }
        public int MinLevel { get; set; }
        public int BaseInvestment { get; set; }
        public string Description { get; set; }
    }

    // 商品模板
    public class GoodsTemplate
    {
        public string GoodsId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int BasePrice { get; set; }
        public int Volatility { get; set; }
        public List<string> SourceRegions { get; set; }
    }

    public List<City> Cities { get; set; }
    public List<RouteTemplate> RouteTemplates { get; set; }
    public List<GoodsTemplate> GoodsTemplates { get; set; }

    private TradeRouteDatabase()
    {
        InitializeCities();
        InitializeRoutes();
        InitializeGoods();
    }

    private void InitializeCities()
    {
        Cities = new List<City>
        {
            new City { CityId = "ironhold", CityName = "Iron Hold", Region = "Northern", 
                AvailableGoods = new List<string> { "iron_ore", "steel_ingot", "coal", "weapon" } },
            new City { CityId = "goldport", CityName = "Gold Port", Region = "Southern",
                AvailableGoods = new List<string> { "spice", "silk", "gem", "magic_crystal" } },
            new City { CityId = "mistvale", CityName = "Mist Vale", Region = "Western",
                AvailableGoods = new List<string> { "herb", "potion", "magic_fruit", "cloth" } },
            new City { CityId = "sunforge", CityName = "Sun Forge", Region = "Eastern",
                AvailableGoods = new List<string> { "gold_ingot", "jewelry", "art_object", "enchant_scroll" } },
            new City { CityId = "darkcairn", CityName = "Dark Cairn", Region = "Central",
                AvailableGoods = new List<string> { "dark_herb", "bone", "shadow_gem", "rare_shadow" } },
            new City { CityId = "crystalpeak", CityName = "Crystal Peak", Region = "Northern",
                AvailableGoods = new List<string> { "crystal", "diamond", "rare_gem", "ice_crystal" } },
            new City { CityId = "lavatown", CityName = "Lava Town", Region = "Eastern",
                AvailableGoods = new List<string> { "fire_ore", "obsidian", "fire_crystal", "magma_gem" } },
            new City { CityId = "forest_haven", CityName = "Forest Haven", Region = "Western",
                AvailableGoods = new List<string> { "wood", "herb", "animal_fur", "mushroom" } }
        };
    }

    private void InitializeRoutes()
    {
        RouteTemplates = new List<RouteTemplate>
        {
            new RouteTemplate { RouteId = "iron_gold", RouteName = "Iron-Gold Trade Route", 
                StartCity = "ironhold", EndCity = "goldport", BaseDistance = 500, MinLevel = 1, BaseInvestment = 1000,
                Description = "Iron goods from the north to southern ports" },
            new RouteTemplate { RouteId = "mist_sun", RouteName = "Mist-Sun Route", 
                StartCity = "mistvale", EndCity = "sunforge", BaseDistance = 400, MinLevel = 5, BaseInvestment = 2000,
                Description = "Magical goods trade between valleys" },
            new RouteTemplate { RouteId = "dark_crystal", RouteName = "Dark Crystal Route", 
                StartCity = "darkcairn", EndCity = "crystalpeak", BaseDistance = 600, MinLevel = 10, BaseInvestment = 3500,
                Description = "Rare gems from dark to crystal regions" },
            new RouteTemplate { RouteId = "lava_forest", RouteName = "Lava-Forest Route", 
                StartCity = "lavatown", EndCity = "forest_haven", BaseDistance = 350, MinLevel = 3, BaseInvestment = 1500,
                Description = "Fire materials for forest crafts" },
            new RouteTemplate { RouteId = "port_iron", RouteName = "Port-Iron Circuit", 
                StartCity = "goldport", EndCity = "ironhold", BaseDistance = 550, MinLevel = 8, BaseInvestment = 3000,
                Description = "Importing weapons and armor" },
            new RouteTemplate { RouteId = "central_loop", RouteName = "Central Loop", 
                StartCity = "darkcairn", EndCity = "sunforge", BaseDistance = 450, MinLevel = 12, BaseInvestment = 4000,
                Description = "Ancient art and magic trade" },
            new RouteTemplate { RouteId = "northern_pass", RouteName = "Northern Pass", 
                StartCity = "crystalpeak", EndCity = "ironhold", BaseDistance = 300, MinLevel = 6, BaseInvestment = 2500,
                Description = "Crystal and steel exchange" },
            new RouteTemplate { RouteId = "southern_express", RouteName = "Southern Express", 
                StartCity = "goldport", EndCity = "lavatown", BaseDistance = 700, MinLevel = 15, BaseInvestment = 5000,
                Description = "Exotic eastern goods delivery" }
        };
    }

    private void InitializeGoods()
    {
        GoodsTemplates = new List<GoodsTemplate>
        {
            // Materials
            new GoodsTemplate { GoodsId = "iron_ore", Name = "Iron Ore", Category = "Material", BasePrice = 50, Volatility = 2, 
                SourceRegions = new List<string> { "Northern" } },
            new GoodsTemplate { GoodsId = "coal", Name = "Coal", Category = "Material", BasePrice = 30, Volatility = 1, 
                SourceRegions = new List<string> { "Northern", "Eastern" } },
            new GoodsTemplate { GoodsId = "wood", Name = "Wood", Category = "Material", BasePrice = 25, Volatility = 1, 
                SourceRegions = new List<string> { "Western" } },
            new GoodsTemplate { GoodsId = "crystal", Name = "Crystal", Category = "Material", BasePrice = 200, Volatility = 4, 
                SourceRegions = new List<string> { "Northern" } },
            new GoodsTemplate { GoodsId = "herb", Name = "Herbs", Category = "Material", BasePrice = 40, Volatility = 2, 
                SourceRegions = new List<string> { "Western", "Central" } },
            
            // Food
            new GoodsTemplate { GoodsId = "spice", Name = "Spices", Category = "Food", BasePrice = 100, Volatility = 3, 
                SourceRegions = new List<string> { "Southern" } },
            new GoodsTemplate { GoodsId = "magic_fruit", Name = "Magic Fruit", Category = "Food", BasePrice = 150, Volatility = 5, 
                SourceRegions = new List<string> { "Western" } },
            new GoodsTemplate { GoodsId = "mushroom", Name = "Rare Mushroom", Category = "Food", BasePrice = 80, Volatility = 4, 
                SourceRegions = new List<string> { "Western" } },
            
            // Weapons
            new GoodsTemplate { GoodsId = "weapon", Name = "Weapons", Category = "Weapon", BasePrice = 300, Volatility = 3, 
                SourceRegions = new List<string> { "Northern" } },
            
            // Armor
            new GoodsTemplate { GoodsId = "steel_ingot", Name = "Steel Ingot", Category = "Armor", BasePrice = 120, Volatility = 2, 
                SourceRegions = new List<string> { "Northern" } },
            new GoodsTemplate { GoodsId = "cloth", Name = "Fine Cloth", Category = "Armor", BasePrice = 80, Volatility = 2, 
                SourceRegions = new List<string> { "Western", "Southern" } },
            
            // Magic
            new GoodsTemplate { GoodsId = "magic_crystal", Name = "Magic Crystal", Category = "Magic", BasePrice = 250, Volatility = 5, 
                SourceRegions = new List<string> { "Southern", "Western" } },
            new GoodsTemplate { GoodsId = "fire_crystal", Name = "Fire Crystal", Category = "Magic", BasePrice = 350, Volatility = 6, 
                SourceRegions = new List<string> { "Eastern" } },
            new GoodsTemplate { GoodsId = "ice_crystal", Name = "Ice Crystal", Category = "Magic", BasePrice = 320, Volatility = 6, 
                SourceRegions = new List<string> { "Northern" } },
            new GoodsTemplate { GoodsId = "shadow_gem", Name = "Shadow Gem", Category = "Magic", BasePrice = 500, Volatility = 8, 
                SourceRegions = new List<string> { "Central" } },
            new GoodsTemplate { GoodsId = "enchant_scroll", Name = "Enchant Scroll", Category = "Magic", BasePrice = 400, Volatility = 4, 
                SourceRegions = new List<string> { "Eastern" } },
            
            // Art
            new GoodsTemplate { GoodsId = "silk", Name = "Silk", Category = "Art", BasePrice = 150, Volatility = 3, 
                SourceRegions = new List<string> { "Southern" } },
            new GoodsTemplate { GoodsId = "gem", Name = "Gems", Category = "Art", BasePrice = 280, Volatility = 4, 
                SourceRegions = new List<string> { "Southern", "Northern" } },
            new GoodsTemplate { GoodsId = "jewelry", Name = "Jewelry", Category = "Art", BasePrice = 500, Volatility = 5, 
                SourceRegions = new List<string> { "Eastern" } },
            new GoodsTemplate { GoodsId = "art_object", Name = "Art Objects", Category = "Art", BasePrice = 600, Volatility = 7, 
                SourceRegions = new List<string> { "Eastern", "Central" } },
            new GoodsTemplate { GoodsId = "diamond", Name = "Diamond", Category = "Art", BasePrice = 800, Volatility = 8, 
                SourceRegions = new List<string> { "Northern" } },
            new GoodsTemplate { GoodsId = "rare_gem", Name = "Rare Gem", Category = "Art", BasePrice = 1000, Volatility = 9, 
                SourceRegions = new List<string> { "Northern" } },
            
            // Special
            new GoodsTemplate { GoodsId = "potion", Name = "Potions", Category = "Special", BasePrice = 60, Volatility = 2, 
                SourceRegions = new List<string> { "Western" } },
            new GoodsTemplate { GoodsId = "gold_ingot", Name = "Gold Ingot", Category = "Special", BasePrice = 500, Volatility = 1, 
                SourceRegions = new List<string> { "Eastern" } },
            new GoodsTemplate { GoodsId = "animal_fur", Name = "Animal Fur", Category = "Special", BasePrice = 90, Volatility = 3, 
                SourceRegions = new List<string> { "Western" } },
            new GoodsTemplate { GoodsId = "dark_herb", Name = "Dark Herb", Category = "Special", BasePrice = 180, Volatility = 5, 
                SourceRegions = new List<string> { "Central" } },
            new GoodsTemplate { GoodsId = "bone", Name = "Bone", Category = "Special", BasePrice = 45, Volatility = 2, 
                SourceRegions = new List<string> { "Central" } },
            new GoodsTemplate { GoodsId = "obsidian", Name = "Obsidian", Category = "Special", BasePrice = 140, Volatility = 4, 
                SourceRegions = new List<string> { "Eastern" } },
            new GoodsTemplate { GoodsId = "magma_gem", Name = "Magma Gem", Category = "Special", BasePrice = 450, Volatility = 7, 
                SourceRegions = new List<string> { "Eastern" } },
            new GoodsTemplate { GoodsId = "rare_shadow", Name = "Rare Shadow Material", Category = "Special", BasePrice = 700, Volatility = 8, 
                SourceRegions = new List<string> { "Central" } }
        };
    }

    public City GetCity(string cityId)
    {
        foreach (var city in Cities)
        {
            if (city.CityId == cityId) return city;
        }
        return null;
    }

    public GoodsTemplate GetGoods(string goodsId)
    {
        foreach (var goods in GoodsTemplates)
        {
            if (goods.GoodsId == goodsId) return goods;
        }
        return null;
    }

    public List<GoodsTemplate> GetGoodsByRegion(string region)
    {
        var result = new List<GoodsTemplate>();
        foreach (var goods in GoodsTemplates)
        {
            if (goods.SourceRegions.Contains(region))
            {
                result.Add(goods);
            }
        }
        return result;
    }
}
