// Global using directives for ClawRPG
// This file ensures commonly-used namespaces are available across all compilation units
// without requiring individual using statements in each file.

global using ClawRPG.Scripts.Framework;
global using Godot;
global using System;
global using System.Collections.Generic;
global using System.Collections;
global using System.Linq;

// Disambiguate: prefer non-generic Dictionary from System.Collections
// Files needing generic Dictionary should use Dictionary<TKey, TValue> explicitly
global using Dictionary = System.Collections.Dictionary;
