// This script defines a Godot EditorPlugin for managing custom projectile types.
#if TOOLS
namespace BrokenProjectileCollection.Editor;

using Godot;

/// <summary>
/// Editor plugin for managing custom projectile types in the Godot editor.
/// </summary>
[Tool]
public partial class BrokenProjectilesPlugin : EditorPlugin
{
    #region Constants
    /// <summary>
    /// Constant for the 3D projectile type.
    /// </summary>
    private const string PROJECTILE_3D = "Projectile3D";

    /// <summary>
    /// Constant for the 3D beam type.
    /// </summary>
    private const string BEAM_3D = "Beam3D";

    /// <summary>
    /// Constant for the 3D grenade type.
    /// </summary>
    private const string GRENADE_3D = "Grenade3D";
    #endregion

    /// <summary>
    /// Called when the plugin is added to the editor.
    /// Registers custom projectile types.
    /// </summary>
    public override void _EnterTree()
    {
        // Register custom types with their respective base types, scripts, and icons.
        AddCustomType(PROJECTILE_3D, "CharacterBody3D", GD.Load<Script>("uid://bqb5hw64y3jjt"), GD.Load<Texture2D>("uid://dfav5iadt30c4"));
        AddCustomType(BEAM_3D, "RayCast3D", GD.Load<Script>("uid://8s36v28e2cen"), GD.Load<Texture2D>("uid://b8we12in2x8y7"));
        AddCustomType(GRENADE_3D, "RigidBody3D", GD.Load<Script>("uid://lmd28u3f8i02"), GD.Load<Texture2D>("uid://dev3elj4sc2gq"));
    }

    /// <summary>
    /// Called when the plugin is removed from the editor.
    /// Unregisters custom projectile types.
    /// </summary>
    public override void _ExitTree()
    {
        // Unregister custom types.
        RemoveCustomType(PROJECTILE_3D);
        RemoveCustomType(BEAM_3D);
        RemoveCustomType(GRENADE_3D);
    }
}
#endif
