namespace BrokenProjectileCollection.Projectiles;

using BrokenSigilCollection.Interface;
using BrokenSigilCollection.Utility;
using Godot;

/// <summary>
/// Base 3D projectile node.
/// </summary>
[Icon("uid://dfav5iadt30c4")]
public partial class Projectile3D : CharacterBody3D, IDuration, IReset
{
    #region Signals
    /// <summary>
    /// Signal emitted when the projectile hits a collider.
    /// </summary>
    [Signal]
    public delegate void OnHitEventHandler(Node3D collider);
    [Signal]
    public delegate void ActiveChangedEventHandler(bool active);
    #endregion

    // Default speed of the projectile.
    private float _defaultSpeed = 7.5f;

    [Export]
    public float DefaultSpeed
    {
        get => _defaultSpeed;
        protected set
        {
            _defaultSpeed = value;
            Speed = value;
        }
    }

    // Current speed of the projectile.
    public float Speed { get; set; } = 7.5f;

    [ExportGroup("Timer")]
    [Export]
    public float Duration { get; set; } = 3.5f;

    [Export]
    protected Timer selfDestructionTimer;

    [ExportGroup("Process")]
    private bool _active = true;

    /// <summary>
    /// Indicates whether the projectile is active.
    /// </summary>
    [Export]
    public virtual bool Active
    {
        get => _active;
        set
        {
            if (_active != value && value == true)
                selfDestructionTimer.Start();

            _active = value;

            Visible = value;
            SetDeferred(PropertyName.ProcessMode, (int)(value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled));

            EmitSignal(SignalName.ActiveChanged, value);
        }
    }

    [Export]
    protected bool freeOnUse { get; private set; } = false;

    // Node that the projectile should not collide with.
    private Node3D _collisionException = null;

    public Node3D CollisionException
    {
        get => _collisionException;
        set
        {
            if (_collisionException is not null && IsInstanceValid(_collisionException))
                if (_collisionException != value)
                    RemoveCollisionExceptionWith(_collisionException);

            _collisionException = value;
            AddCollisionExceptionWith(_collisionException);
        }
    }

    public bool Performing => _active;

    /// <summary>
    /// Called when the node enters the scene tree.
    /// Initializes the projectile and sets up the self-destruction timer.
    /// </summary>
    public override void _EnterTree()
    {
        Reset();

        if (selfDestructionTimer is null)
        {
            selfDestructionTimer = SigilFactory.CreateTimer(Duration, autoStart: true, name: "SelfDestructionTimer");

            selfDestructionTimer.Timeout += timeout;
            CallDeferred(MethodName.AddChild, selfDestructionTimer);
        }
    }

    /// <summary>
    /// Called every physics frame to update the projectile's movement and handle collisions.
    /// </summary>
    /// <param name="delta">Time since the last frame.</param>
    public override void _PhysicsProcess(double delta)
    {
        if (!Active)
            return;

        setVelocity(delta);
        move(delta);
    }

    // Sets the velocity of the projectile.
    protected virtual void setVelocity(double delta)
    {
        Vector3 forwardDir = GlobalBasis.Z.Normalized();
        Velocity = forwardDir * Speed;
    }

    // Moves the projectile and handles collisions.
    protected virtual bool move(double delta)
    {
        var collision = MoveAndCollide(Velocity * (float)delta);

        if (collision is null)
            return false;

        onHit(collision.GetCollider() as Node3D);

        return true;
    }

    // Handles the projectile hitting a collider.
    protected virtual void onHit(Node3D collider)
    {
        EmitSignal(SignalName.OnHit, collider);

#if DEBUG
        GD.Print($"Hit {collider.Name} with {Name}");
#endif

        if (!freeOnUse)
            Active = false;
        else
            QueueFree();
    }

    /// <summary>
    /// Handles the timeout of the projectile.
    /// </summary>
    protected virtual void timeout()
    {
        if (!freeOnUse)
            Active = false;
        else
            QueueFree();
    }

    /// <summary>
    /// Resets the projectile to its default state.
    /// </summary>
    public virtual void Reset()
    {
        Speed = DefaultSpeed;
    }
}