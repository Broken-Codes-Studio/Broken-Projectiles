namespace BrokenProjectileCollection.Projectiles;

using Godot;

/// <summary>
/// A 3D projectile with bounce capabilities.
/// </summary>
public partial class BouncyProjectile3D : Projectile3D
{
    #region Signals
    /// <summary>
    /// Signal emitted when the projectile bounces off a collider.
    /// </summary>
    [Signal]
    public delegate void OnBounceEventHandler(Node3D collider);
    #endregion

    [Export(PropertyHint.Layers3DPhysics)]
    public uint BounceMask { get; set; } = 1;
    [Export]
    public uint MaxBounce { get; set; } = 2;
    [Export(PropertyHint.Range, "0,180,radians")]
    public float BounceAngle { get; set; } = Mathf.Pi / 2;

    protected uint bounced { get; private set; } = 0;

    public override bool Active
    {
        get => base.Active;
        set
        {
            if (base.Active != value && value == true)
                bounced = 0;
            base.Active = value;
        }
    }

    /// <summary>
    /// Handles the movement and bouncing of the projectile.
    /// </summary>
    /// <param name="delta">Time since the last frame.</param>
    protected override bool move(double delta)
    {

        KinematicCollision3D collision = MoveAndCollide(Velocity * (float)delta);

        if (collision is null)
            return false;

        CollisionObject3D collider = collision.GetCollider() as CollisionObject3D;

        if ((collider.CollisionLayer & BounceMask) != 0)
        {
            Vector3 colliderNormal = collision.GetNormal();
            Vector3 direction = GlobalTransform.Basis.Z.Normalized();
            Vector3 bouncedDirection = direction.Bounce(colliderNormal).Normalized();
            float angle = direction.AngleTo(bouncedDirection);

            if (angle > BounceAngle)
            {
                bounced = 0;
                onHit(collider);
                return true;
            }
            if (bounced > MaxBounce)
            {
                bounced = 0;
                onHit(collider);
                return true;
            }

            bounce(colliderNormal, collider);
            return true;
        }

        bounced = 0;
        onHit(collider);

        return true;
    }

    /// <summary>
    /// Bounces the projectile based on the collision normal.
    /// </summary>
    /// <param name="normal">The normal of the collision.</param>
    /// <param name="Collider">The collider node.</param>
    public virtual void bounce(Vector3 normal, Node3D Collider)
    {
        LookAt(GlobalTransform.Origin - GlobalBasis.Z.Bounce(normal), GlobalBasis.Y);
        //Velocity = Velocity.Bounce(normal);

        bounced++;

#if DEBUG
        GD.Print($"{Name} bounced {bounced} times");
#endif

        EmitSignal(SignalName.OnBounce, Collider);
    }
}
