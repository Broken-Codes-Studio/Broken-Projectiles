namespace BrokenProjectileCollection.Beams;

using BrokenSigilCollection.Interface;
using BrokenSigilCollection.Utility;
using Godot;
using Godot.Collections;

/// <summary>
/// Base 3D beam node.
/// </summary>
[Icon("uid://b8we12in2x8y7")]
public partial class Beam3D : RayCast3D, ITick, IReset
{
    #region Signals
    /// <summary>
    /// Signal emitted on each tick.
    /// </summary>
    [Signal]
    public delegate void OnTickEventHandler();
    [Signal]
    public delegate void ActiveChangedEventHandler(bool active);
    #endregion

    private float _defaultLength = 1f;

    /// <summary>
    /// Gets or sets the default length of the beam.
    /// </summary>
    [Export]
    public float DefaultLength
    {
        get => _defaultLength;
        private set
        {
            float absValue = Mathf.Abs(value);
            _defaultLength = absValue;
            Length = absValue;
        }
    }

    private float _length = 1f;

    /// <summary>
    /// Gets or sets the current length of the beam.
    /// </summary>
    public float Length
    {
        get => _length;
        set
        {
            float absValue = Mathf.Abs(value);
            _length = absValue;
            changeLength(absValue);
        }
    }

    private bool _active = true;

    /// <summary>
    /// Gets or sets the active state of the beam.
    /// </summary>
    [Export]
    public virtual bool Active
    {
        get => _active;
        set
        {
            // Set the process mode based on the active state
            SetDeferred(PropertyName.ProcessMode, (int)(value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled));
            Enabled = value;
            Visible = value;
            _active = value;

            EmitSignal(SignalName.ActiveChanged, value);
        }
    }

    private float _defaultTick = 0.5f;

    /// <summary>
    /// Gets or sets the default tick interval of the beam.
    /// </summary>
    [Export]
    public float DefaultTick
    {
        get => _defaultTick;
        private set
        {
            float absValue = Mathf.Abs(value);
            _defaultTick = absValue;
            TickDuration = absValue;

            if (_tickTimer is not null)
                _tickTimer.WaitTime = absValue;
        }
    }
    public float TickDuration { get; set; }

    [ExportGroup("Visualize")]
    [Export]
    protected Array<MeshInstance3D> beamMeshs = new();

    private Timer _tickTimer;

    public CollisionObject3D CollisionObject { get; protected set; }

    /// <summary>
    /// Called when the node is ready.
    /// Initializes the beam and sets up the tick timer.
    /// </summary>
    public override void _Ready()
    {
        Reset();

        if (_tickTimer is null)
        {
            _tickTimer = SigilFactory.CreateTimer(TickDuration, oneShot: false, autoStart: true, name: "TickTimer");

            _tickTimer.Timeout += Tick;
            CallDeferred(MethodName.AddChild, _tickTimer);
        }

        changeLength(Length);
        changeVisualLength(Length);
    }

    /// <summary>
    /// Called every physics frame to update the beam's collision and visual length.
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        ForceRaycastUpdate();

        if (!IsColliding())
        {
            CollisionObject = null;
            changeVisualLength(Length);
            return;
        }

        CollisionObject = GetCollider() as CollisionObject3D;

        Vector3 CollisionPoint = GetCollisionPoint();
        float distance = CollisionPoint.DistanceTo(GlobalPosition);
        changeVisualLength(distance);
    }

    /// <summary>
    /// Handles the tick event, emitting a signal if the beam is colliding with an object.
    /// </summary>
    public virtual void Tick()
    {
        if (CollisionObject is not null)
            EmitSignal(SignalName.OnTick);
    }

    /// <summary>
    /// Changes the length of the beam.
    /// </summary>
    /// <param name="length">The new length of the beam.</param>
    private void changeLength(float length)
    {
        TargetPosition = Vector3.Back * length;
    }

    /// <summary>
    /// Changes the visual representation of the beam's length.
    /// </summary>
    /// <param name="length">The new visual length of the beam.</param>
    private void changeVisualLength(float length)
    {
        float height = Mathf.Abs(length);

        foreach (var beemMesh in beamMeshs)
        {

            var mesh = beemMesh.Mesh;

            switch (mesh)
            {
                case CylinderMesh cylinderMesh:
                    cylinderMesh.Height = height;
                    break;
                case CapsuleMesh capsuleMesh:
                    capsuleMesh.Height = height;
                    break;
                case BoxMesh boxMesh:
                    var size = boxMesh.Size;
                    size.Y = height;
                    boxMesh.Size = size;
                    break;
                case SphereMesh sphereMesh:
                    sphereMesh.Radius = height;
                    break;
                default:
                    GD.PushError($"Mesh type {mesh.GetType()} is not supported for visual length change.");
                    break;
            }
            beemMesh.Position = new(0, 0, (height / 2));
        }
    }

    /// <summary>
    /// Resets the beam to its default state.
    /// </summary>
    public void Reset()
    {
        Length = DefaultLength;
        TickDuration = DefaultTick;
    }
}
