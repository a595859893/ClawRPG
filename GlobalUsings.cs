// Global using directives for ClawRPG
// This file ensures commonly-used namespaces are available across all compilation units
// without requiring individual using statements in each file.

global using ClawRPG.Scripts.Framework;
global using Godot;
global using System;
global using System.Collections.Generic;
global using System.Collections;
global using System.Linq;

// System.Collections.Generic.Dictionary<TKey, TValue> is available as Dictionary<TKey, TValue>
// For untyped Godot dictionaries, use Godot.Collections.Dictionary or the Dictionary alias below:
global using Dictionary = Godot.Collections.Dictionary;
global using System.Threading.Tasks;
global using ClawRPG.Scripts.Systems;
global using ClawRPG.Scripts.Systems.Pets;
global using ClawRPG.Scripts.Systems.PetMimicry;
global using ClawRPG.Scripts.Data;
global using ClawRPG.Scripts.Database;
global using ClawRPG.Scripts.Framework;
global using ClawRPG.Scripts.Managers;
global using ClawRPG.Systems;
global using ClawRPG.Systems.Pets;

// Nested type aliases — resolve nested types that are used unqualified
global using ItemData = ClawRPG.Scripts.UI.ItemData;
global using PetMimicryData = ClawRPG.Scripts.Systems.PetMimicry.PetMimicryData;
global using ClawRPG.Systems.Artifact;
global using ClawRPG.Systems.MultiplayerVote;
global using ClawRPG.Systems.Pets.AI;
