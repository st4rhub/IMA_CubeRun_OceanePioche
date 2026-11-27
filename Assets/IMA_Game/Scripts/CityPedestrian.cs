// City Pedestrian · somebody a driving car can knock over, like a toy figure.
//
// A car of the kit has a child Bumper: a trigger in front of the car that is on only while the
// car is faster than 3 m/s. When that Bumper touches this person, the person stops what he is
// doing (his other FSMs and his Animator are switched off), gets a Rigidbody and a shove, falls
// over and stays down. Then this script sends the event Hit to the FSM "Knock" on the same
// object. What happens next is written there (it broadcasts the global event Knocked: the clock
// takes time off, the police start).
using HutongGames.PlayMaker;
using UnityEngine;

[AddComponentMenu("IMA/City Pedestrian")]
public class CityPedestrian : MonoBehaviour
{
    [Tooltip("How hard the car shoves: 1 = as fast as the car drives.")]
    public float shove = 0.5f;
    [Tooltip("How fast the person is thrown upwards, metres per second.")]
    public float lift = 3.5f;
    [Tooltip("How fast the person tumbles, turns per second.")]
    public float tumble = 1.2f;
    [Tooltip("How near the Bumper has to come, metres from the person's middle.")]
    public float hitDistance = 0.7f;
    [Tooltip("The name of the FSM on this object that hears the event.")]
    public string fsmName = "Knock";
    [Tooltip("The event that FSM gets when the person is knocked over.")]
    public string eventName = "Hit";
    [Tooltip("Ticked once the person lies on the floor.")]
    public bool down;

    void OnTriggerStay(Collider other)
    {
        if (down || other.name != "Bumper") return;
        var middle = transform.position + Vector3.up * 0.9f;
        var nearest = other.ClosestPoint(middle);
        nearest.y = middle.y;
        if (Vector3.Distance(nearest, middle) > hitDistance) return;
        Fall(other);
    }

    /// Falls over, pushed the way the car drives.
    public void Fall(Collider bumper)
    {
        down = true;
        FsmComponent knock = null;
        foreach (var fsm in GetComponents<FsmComponent>())
        {
            if (fsm.FsmName == fsmName) knock = fsm;
            else fsm.enabled = false;
        }
        foreach (var animator in GetComponentsInChildren<Animator>()) animator.enabled = false;
        var speech = transform.Find("Speech");
        if (speech != null) speech.gameObject.SetActive(false);
        Collider solid = null;
        foreach (var c in GetComponents<Collider>())
        {
            if (c.isTrigger) c.enabled = false;
            else solid = c;
        }

        var car = bumper != null ? bumper.attachedRigidbody : null;
        var push = car != null ? car.linearVelocity : transform.forward * -6f;
        push.y = 0f;
        // The car that hit does not bump over the person a second time.
        if (car != null && solid != null)
            foreach (var c in car.GetComponentsInChildren<Collider>(true)) Physics.IgnoreCollision(solid, c, true);

        var body = GetComponent<Rigidbody>();
        if (body == null) body = gameObject.AddComponent<Rigidbody>();
        body.mass = 60f;
        body.linearDamping = 0.6f;
        body.angularDamping = 1.5f;
        body.linearVelocity = push * shove + Vector3.up * lift;
        var axis = push.sqrMagnitude > 0.01f ? Vector3.Cross(Vector3.up, push.normalized) : transform.right;
        body.angularVelocity = axis * tumble * 6.28f;

        if (knock != null) knock.SendEvent(eventName);
    }
}
