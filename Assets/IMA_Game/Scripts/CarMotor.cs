// Car Motor · what makes a car of the kit drive (Car_Taxi, Car_Police, ...).
//
// The car's FSM "Car" decides WHO drives: it switches the child object Driver on when you get in
// and off when you get out. This script does the driving itself, because it is sums on every
// physics step: while Driver is on it reads the same keys the Player walks with (W A S D, the
// arrow keys, the left stick) and moves the car's Rigidbody.
//
//   W            faster, up to Top Speed
//   S            brakes; when the car stands, it drives backwards up to Reverse Speed
//   A and D      steer. A car only turns while it rolls, and backwards it turns the other way
//   no key       the car rolls out
//   nobody in it the car brakes by itself and then stands where it is; nobody can push it away
//
// The car cannot tip over: it only ever turns around its vertical axis.
// Change Top Speed, Acceleration and Steering in the Inspector and try the car again.
using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("IMA/Car Motor")]
[RequireComponent(typeof(Rigidbody))]
public class CarMotor : MonoBehaviour
{
    [Header("How fast")]
    [Tooltip("Metres per second forwards. 16 is about 58 km/h.")]
    public float topSpeed = 16f;
    [Tooltip("Metres per second backwards.")]
    public float reverseSpeed = 5f;
    [Tooltip("How quickly it gets faster: metres per second, every second.")]
    public float acceleration = 10f;
    [Tooltip("How quickly it brakes: metres per second, every second.")]
    public float braking = 20f;
    [Tooltip("How quickly it slows down when no key is held.")]
    public float rollingOut = 4f;

    [Header("How it turns")]
    [Tooltip("Degrees per second with A or D held.")]
    public float steering = 100f;
    [Tooltip("Below this speed the car turns less; at 0 it does not turn at all.")]
    public float fullSteeringAt = 5f;

    [Header("Who drives")]
    [Tooltip("The car is driven while this object is active. The FSM Car switches it on and off.")]
    public GameObject driver;
    [Tooltip("The Input Action the keys come from: Player/Move, the one the Player walks with.")]
    public InputActionReference move;

    [Header("For others to read")]
    [Tooltip("A trigger at the front that is only active while the car is faster than Bumper Speed.")]
    public GameObject bumper;
    public float bumperSpeed = 3f;
    [Tooltip("How fast the car is right now, in metres per second. Backwards it is below 0. Read only.")]
    public float speed;

    Rigidbody _body;

    /// Somebody sits in the car.
    public bool Driving => driver != null && driver.activeInHierarchy;

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;
        var yaw = transform.eulerAngles.y;
        var forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        var velocity = _body.linearVelocity;
        // What the car really did in the last step: a wall that stopped it has taken its speed.
        speed = Vector3.Dot(velocity, forward);

        float gas = 0f, steer = 0f;
        var driving = Driving;
        if (driving && move != null && move.action != null)
        {
            if (!move.action.enabled) move.action.Enable();
            var keys = move.action.ReadValue<Vector2>();
            gas = keys.y;
            steer = keys.x;
        }

        if (!driving) speed = Mathf.MoveTowards(speed, 0f, braking * dt);
        else if (gas > 0.1f)
        {
            if (speed < -0.5f) speed = Mathf.MoveTowards(speed, 0f, braking * dt);
            else speed = Mathf.MoveTowards(speed, topSpeed, acceleration * gas * dt);
        }
        else if (gas < -0.1f)
        {
            if (speed > 0.5f) speed = Mathf.MoveTowards(speed, 0f, braking * dt);
            else speed = Mathf.MoveTowards(speed, -reverseSpeed, acceleration * -gas * dt);
        }
        else speed = Mathf.MoveTowards(speed, 0f, rollingOut * dt);

        // Steering counts by how fast the car rolls: none at a standstill, mirrored backwards.
        var rolling = Mathf.Clamp(speed / Mathf.Max(0.1f, fullSteeringAt), -1f, 1f);
        yaw += steer * steering * rolling * dt;
        var turned = Quaternion.Euler(0f, yaw, 0f);
        _body.MoveRotation(turned);

        // A parked car that has stopped is held in place: the Player cannot shove it.
        var resting = !driving && Mathf.Abs(speed) < 0.05f;
        _body.constraints = resting
            ? RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ
            : RigidbodyConstraints.FreezeRotation;
        if (resting) speed = 0f;

        // Gravity stays as it is; going up is capped, so a kerb lifts the car and does not throw it.
        var rise = Mathf.Min(velocity.y, 1.5f);
        _body.linearVelocity = turned * Vector3.forward * speed + Vector3.up * rise;

        if (bumper != null)
        {
            var fast = Mathf.Abs(speed) >= bumperSpeed;
            if (bumper.activeSelf != fast) bumper.SetActive(fast);
        }
    }
}
