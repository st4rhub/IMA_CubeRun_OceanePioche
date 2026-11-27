// City Loose Props · the street furniture a car can knock over (cones, bins, benches, signs,
// lamp posts, trees, fences ...).
//
// Each thing in the list has a Rigidbody, so a car pushes it away instead of stopping at it.
// When the level starts, this script puts all of them to sleep: they stand exactly where they
// were placed, even leaning or half inside each other, until something hits them.
//
// A thing a car has knocked over stays where it lands, but a moment later (Clear After) cars
// no longer bump into it: wreckage on the road never stops a car a second time.
//
// To make one more thing loose: give it a Rigidbody (and a collider that is Convex) and add it
// to the list. To fix one in place again: remove its Rigidbody.
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("IMA/City Loose Props")]
public class CityLooseProps : MonoBehaviour
{
    [Tooltip("The things a car can knock over. Each has a Rigidbody.")]
    public List<Rigidbody> props = new List<Rigidbody>();

    [Tooltip("Seconds after a car has hit a thing until cars drive through it. 0 = it stays in the way for ever.")]
    public float clearAfter = 1.5f;

    void Start()
    {
        foreach (var p in props)
        {
            if (p == null) continue;
            p.Sleep();
            if (clearAfter > 0f) p.gameObject.AddComponent<CityKnockedProp>().clearAfter = clearAfter;
        }
    }
}

/// Sits on every loose thing while the game runs (City Loose Props puts it there). Waits for a
/// car, then switches off the collisions between this thing and every car.
public class CityKnockedProp : MonoBehaviour
{
    public float clearAfter = 1.5f;
    bool _hit;

    void OnCollisionEnter(Collision collision)
    {
        if (_hit) return;
        var other = collision.rigidbody;
        if (other == null || other.GetComponent<CarMotor>() == null) return;
        _hit = true;
        Invoke(nameof(Clear), clearAfter);
    }

    void Clear()
    {
        var mine = GetComponentsInChildren<Collider>(true);
        foreach (var car in FindObjectsByType<CarMotor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            foreach (var theirs in car.GetComponentsInChildren<Collider>(true))
            {
                if (theirs.isTrigger) continue;
                foreach (var c in mine) Physics.IgnoreCollision(c, theirs, true);
            }
    }
}
