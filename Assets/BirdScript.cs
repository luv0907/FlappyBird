using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class BirdScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody2D;
    public float flapStrength = 10;

    void Awake()
    {
        if (myRigidbody2D == null) myRigidbody2D = GetComponent<Rigidbody2D>();

        // Ensure a legacy Animation component plays the 'Bird Flap' clip if present
        Animation anim = GetComponent<Animation>();
        if (anim == null) anim = gameObject.AddComponent<Animation>();

        // try to find the clip named "Bird Flap" in loaded assets
        AnimationClip flap = null;
        var clips = Resources.FindObjectsOfTypeAll<AnimationClip>();
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name == "Bird Flap")
            {
                flap = clips[i];
                break;
            }
        }

        if (flap != null)
        {
            if (anim.GetClip(flap.name) == null) anim.AddClip(flap, flap.name);
            anim.clip = flap;
            anim.playAutomatically = true;
            anim.wrapMode = flap.isLooping ? WrapMode.Loop : WrapMode.Once;
            anim.Play(flap.name);
        }
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool pressed = false;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) pressed = true;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) pressed = true;
        if (pressed)
        {
            Vector2 v = myRigidbody2D.linearVelocity;
            v.y = flapStrength;
            myRigidbody2D.linearVelocity = v;
        }
#else
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 v = myRigidbody2D.velocity;
            v.y = flapStrength;
            myRigidbody2D.velocity = v;
        }
#endif
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // If we hit something (pipe, ground), trigger game over
        if (LogicScript.Instance != null)
        {
            LogicScript.Instance.GameOver();
        }
    }
}
