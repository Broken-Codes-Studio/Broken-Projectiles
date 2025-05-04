namespace BrokenProjectileCollection.Projectiles;

using Godot;
using BrokenSigilCollection.Interface;

/// <summary>
/// A 3D projectile that follows a target.
/// </summary>
public partial class TrackingProjectile3D : Projectile3D, ITarget<Node3D>
{
    /// <summary>
    /// The target node that the projectile will track.
    /// </summary>
    [Export]
    public Node3D Target { get; protected set; }

    // Default rotation speed for tracking the target.
    private float _defaultRotationSpeed = 2.5f;

    [Export]
    public float DefaultRotationSpeed
    {
        get => _defaultRotationSpeed;
        private set
        {
            _defaultRotationSpeed = value;
            RotationSpeed = value;
        }
    }

    // Current rotation speed of the projectile.
    public float RotationSpeed { get; set; } = 2.5f;

    /// <summary>
    /// Sets the velocity of the projectile, adjusting its direction to track the target.
    /// </summary>
    /// <param name="delta">Time since the last frame.</param>
    protected override void setVelocity(double delta)
    {
        if (Target is null)
        {
            base.setVelocity(delta);
            return;
        }

        // Calculate the transform to look at the target.
        Transform3D lookatTransform = GlobalTransform.LookingAt(Target.GlobalPosition, useModelFront: true);

        // Smoothly rotate towards the target.
        GlobalBasis = GlobalBasis.Slerp(lookatTransform.Basis, (float)delta * RotationSpeed);

        base.setVelocity(delta);
    }

    /// <summary>
    /// Sets the target for the projectile to track.
    /// </summary>
    /// <param name="target">The target node.</param>
    public void SetTarget(Node3D target)
    {
        Target = target;
    }
}
