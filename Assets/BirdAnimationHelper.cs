using UnityEngine;

[RequireComponent(typeof(Transform))]
public class BirdAnimationHelper : MonoBehaviour
{
    public AnimationClip flapClip; // assign Bird Flap.anim here in Inspector

    void Awake()
    {
        if (flapClip == null) return;

        // Ensure legacy Animation component exists and the clip is assigned
        Animation anim = GetComponent<Animation>();
        if (anim == null)
        {
            anim = gameObject.AddComponent<Animation>();
        }

        // Add the clip as legacy (will use clip.name as the key)
        if (anim.GetClip(flapClip.name) == null)
        {
            anim.AddClip(flapClip, flapClip.name);
        }

        anim.clip = flapClip;
        anim.playAutomatically = true;
        anim.wrapMode = flapClip.isLooping ? WrapMode.Loop : WrapMode.Once;
        anim.Play(flapClip.name);
    }
}
