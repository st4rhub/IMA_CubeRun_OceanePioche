// CMS26_IMA_Game · the slot a character stands in.
// Sits on the object that carries the Animator (the Player of HeroRig, every NPC). When the game
// starts it looks at its children: a Synty character that was dragged in becomes the body. The
// Animator takes over that character's Avatar, and every other body goes.
//
// To change a character: drag a Synty character prefab onto this object in the Hierarchy.
// Untick the old body so that you only see the new one while you build.
using UnityEngine;

[DefaultExecutionOrder(-1000)]
[AddComponentMenu("IMA/Character Slot")]
public class CharacterSlot : MonoBehaviour
{
    void Awake()
    {
        var animator = GetComponent<Animator>();
        if (animator == null) return;

        // A body that is switched off is not part of the game.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (!child.gameObject.activeSelf && IsBody(child)) DestroyImmediate(child.gameObject);
        }

        // The new body: the last child that brings a humanoid Avatar of its own.
        Transform body = null;
        Avatar avatar = null;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            var own = child.GetComponent<Animator>();
            if (own == null || own.avatar == null || !own.avatar.isHuman) continue;
            body = child;
            avatar = own.avatar;
            DestroyImmediate(own);
            break;
        }
        if (body == null) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child != body && IsBody(child)) DestroyImmediate(child.gameObject);
        }
        body.localPosition = Vector3.zero;
        body.localRotation = Quaternion.identity;
        animator.avatar = avatar;
        animator.Rebind();
    }

    /// A body is a mesh with a skeleton, or a skeleton alone (Synty's dummy keeps the two side
    /// by side). Something a camera hangs on or looks at is never a body.
    static bool IsBody(Transform t)
    {
        if (t.GetComponentInChildren<Camera>(true) != null) return false;
        var skinned = t.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
        var skeleton = false;
        foreach (var x in t.GetComponentsInChildren<Transform>(true))
        {
            if (x.name.EndsWith("_LookAt")) return false;
            if (x.name == "Hips") skeleton = true;
        }
        return skinned || skeleton;
    }
}
