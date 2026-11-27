// Car Exit · sits on the car's child object Exit and keeps it on a free spot next to the car.
//
// When you get out, the car's FSM "Car" puts the Player where Exit is. This script looks for
// that spot all the time: first to the left of the car (the driver's door), then to the right,
// then behind it, then in front of it. A spot counts as free when there is floor under it and
// nothing solid stands in it: no wall, no other car. If no spot is free, Exit goes onto the roof.
using UnityEngine;

[AddComponentMenu("IMA/Car Exit")]
public class CarExit : MonoBehaviour
{
    [Tooltip("How far from the car's middle the spots to the left and right are, in metres.")]
    public float beside = 1.55f;
    [Tooltip("How far from the car's middle the spots behind and in front are, in metres.")]
    public float behind = 3.2f;
    [Tooltip("The size of the Player: the spot has to be free for a body this wide and this tall.")]
    public float bodyRadius = 0.4f;
    public float bodyHeight = 1.8f;
    [Tooltip("What counts as solid.")]
    public LayerMask solid = ~0;

    void FixedUpdate()
    {
        var car = transform.parent;
        if (car == null) return;
        var spots = new[]
        {
            new Vector3(-beside, 0f, 0f), new Vector3(beside, 0f, 0f),
            new Vector3(0f, 0f, -behind), new Vector3(0f, 0f, behind),
        };
        foreach (var spot in spots)
        {
            var at = car.TransformPoint(spot);
            // Floor under the spot, not much higher or lower than the car stands.
            if (!Physics.Raycast(at + Vector3.up * 1.2f, Vector3.down, out var floor, 2.4f, solid, QueryTriggerInteraction.Ignore)) continue;
            at = floor.point;
            var low = at + Vector3.up * (bodyRadius + 0.1f);
            var high = at + Vector3.up * (bodyHeight - bodyRadius);
            if (Physics.CheckCapsule(low, high, bodyRadius, solid, QueryTriggerInteraction.Ignore)) continue;
            transform.SetPositionAndRotation(at + Vector3.up * 0.05f, car.rotation);
            return;
        }
        transform.SetPositionAndRotation(car.position + Vector3.up * 2f, car.rotation);
    }
}
