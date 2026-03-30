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
