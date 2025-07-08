namespace BrokenProjectileCollection.Grenades;

using BrokenSigilCollection.Interface;
using BrokenSigilCollection.Utility;
using Godot;
using Godot.Collections;

/// <summary>
/// Base 3D grenades node.
/// </summary>
[Icon("uid://dev3elj4sc2gq")]
public partial class Grenade3D : RigidBody3D, IDuration, IReset
{

    #region Signals
    /// <summary>
    /// Signal emitted when the grenade explodes.
    /// </summary>
    [Signal]
    public delegate void OnExplodeEventHandler(Array<Node3D> colliders);
    [Signal]
    public delegate void ActiveChangedEventHandler(bool active);
    #endregion

    [ExportCategory("Fuse Duration")]
    private float _defaultDuration = 4.0f;
    [Export]
    public float DefaultDuration
    {
        get => _defaultDuration;
        private set
        {
            float newValue = Mathf.Abs(value);
            _defaultDuration = newValue;
            Duration = newValue;
        }
    }

    private float _duration = 4.0f;
    /// <summary>
    /// The grenade's fuse duration.
    /// </summary>
    public float Duration
    {
        get => _duration;
        set
        {
            float newValue = Mathf.Abs(value);
            _duration = newValue;

            if (fuseTimer is not null)
                fuseTimer.WaitTime = newValue;
        }
    }

    [Export]
    protected Timer fuseTimer;

    [ExportCategory("Explosion Radius")]
    private float _explosionRadius = 3f;
    [Export]
    public float DefaultRadius
    {
        get => _explosionRadius;
        private set
        {
            float newValue = Mathf.Abs(value);
            _explosionRadius = newValue;
            Radius = newValue;
        }
    }

    private float _radius = 3f;
    public float Radius
    {
        get => _radius;
        set
        {
            float newValue = Mathf.Abs(value);
            _radius = newValue;

            _setRadius(newValue);
        }
    }

    [Export]
    public bool CoverCulling { get; private set; } = false;

    [Export]
    protected Area3D explosionArea3D;
    [Export]
    protected CollisionShape3D explosionShape3D;

    [ExportGroup("Process")]
    private bool _active = true;
    /// <summary>
    /// Indicates whether the projectile is active.
    /// </summary>
    [Export]
    public bool Active
    {
        get => _active;
        set
        {
            if (_active != value && value == true)
                fuseTimer.Start();

            _active = value;

            Visible = value;
            SetDeferred(PropertyName.ProcessMode, (int)(value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled));

            EmitSignal(SignalName.ActiveChanged, value);
        }
    }

    [Export]
    protected bool freeOnUse { get; private set; } = false;

    public bool Performing => _active;

    public override void _EnterTree()
    {
        if (fuseTimer is null)
        {
            fuseTimer = GetNodeOrNull<Timer>("FuseTimer");
            // Create the fuse timer if it doesn't exist
            if (fuseTimer is null)
            {
                fuseTimer = SigilFactory.CreateTimer(Duration, autoStart: true, name: "FuseTimer");

                fuseTimer.ProcessCallback = Timer.TimerProcessCallback.Physics;

                fuseTimer.Timeout += fuseTimeout;
                CallDeferred(MethodName.AddChild, fuseTimer);
            }
        }

        if (explosionArea3D is null)
        {
            explosionArea3D = GetNodeOrNull<Area3D>("ExplosionArea3D");
            // Create the explosion area if it doesn't exist
            if (explosionArea3D is null)
            {
                explosionArea3D = new Area3D();
                explosionArea3D.Name = "ExplosionArea3D";
                explosionArea3D.CollisionLayer = 1;
                explosionArea3D.CollisionMask = 1;
                CallDeferred(MethodName.AddChild, explosionArea3D);
            }

        }

        if (explosionShape3D is null)
        {
            explosionShape3D = explosionArea3D.GetNodeOrNull<CollisionShape3D>("ExplosionShape3D");
            // Create the explosion shape if it doesn't exist
            if (explosionShape3D is null)
            {
                explosionShape3D = new CollisionShape3D();
                explosionShape3D.Name = "ExplosionShape3D";

                SphereShape3D shape = new SphereShape3D();
                shape.Radius = Radius;
                explosionShape3D.Shape = shape;

                explosionArea3D.AddChild(explosionShape3D);
            }
        }
        else
            _setRadius(Radius);

    }

    /// <summary>
    /// Handles the fuse timeout of the grenade.
    /// </summary>
    protected virtual void fuseTimeout()
    {

        var colliders = explosionArea3D.GetOverlappingBodies();

        if ((explosionArea3D.CollisionMask & CollisionLayer) != 0)
        {
            if (colliders.Contains(this))
                colliders.Remove(this);
        }

        if (colliders.Count == 0)
        {
            Finished();
            return;
        }

        if (CoverCulling)
        {
            PhysicsDirectSpaceState3D spaceState3D = GetWorld3D().DirectSpaceState;

            for (int i = 0, length = colliders.Count; i < length;)
            {
                PhysicsRayQueryParameters3D rayParams = PhysicsRayQueryParameters3D.Create(from: GlobalTransform.Origin, to: colliders[i].GlobalTransform.Origin, collisionMask: explosionArea3D.CollisionMask);

                var result = spaceState3D.IntersectRay(rayParams);
                if (!result["collider"].As<Node3D>().Equals(colliders[i]))
                {
                    colliders.Remove(colliders[i]);
                    length--;
                    continue;
                }

                i++;

            }

        }

        EmitSignal(SignalName.OnExplode, colliders);

#if DEBUG
        GD.Print($"{Name} explodsion detected {colliders.Count} colliders");
#endif

        Finished();

    }

    protected void Finished()
    {
        if (!freeOnUse)
            Active = false;
        else
            QueueFree();
    }

    /// <summary>
    /// Resets the grenade to its default state.
    /// </summary>
    public void Reset()
    {
        Duration = DefaultDuration;
    }

    private void _setRadius(float radius)
    {
        if (explosionShape3D is null)
            return;

        Shape3D shape = explosionShape3D.Shape;

        switch (shape)
        {
            case SphereShape3D sphereShape:
                sphereShape.Radius = radius;
                break;
            case BoxShape3D boxShape:
                boxShape.Size = new Vector3(radius, radius, radius);
                break;
            case CapsuleShape3D capsuleShape:
                capsuleShape.Radius = radius;
                break;
            case CylinderShape3D cylinderShape:
                cylinderShape.Radius = radius;
                break;
            default:
                GD.PushError($"Unsupported shape type: {explosionShape3D.GetType()}");
                break;
        }
    }

}
