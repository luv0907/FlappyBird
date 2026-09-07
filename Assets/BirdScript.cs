using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class BirdScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody2D;
    public float flapStrength = 10f;
    public float maxTiltAngle = 30f;
    public float minTiltAngle = -75f;
    public float tiltSpeed = 8f;
    public float topBoundary = 5.5f;

    private Vector3 startPosition;
    private float initialGravityScale = 2.5f;
    private float currentAngle = 0f;
    private bool isAlive = true;

    void Awake()
    {
        if (myRigidbody2D == null) myRigidbody2D = GetComponent<Rigidbody2D>();

        // Store or set appropriate gravity scale
        if (myRigidbody2D.gravityScale > 0.1f)
        {
            initialGravityScale = myRigidbody2D.gravityScale;
        }
        else
        {
            initialGravityScale = 2.5f;
            myRigidbody2D.gravityScale = initialGravityScale;
        }

        // Setup legacy Animation component if present
        Animation anim = GetComponent<Animation>();
        if (anim == null) anim = gameObject.AddComponent<Animation>();

        var clips = Resources.FindObjectsOfTypeAll<AnimationClip>();
        AnimationClip flap = null;
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

    void Start()
    {
        startPosition = transform.position;
        // In ready state, disable gravity until the player starts the game
        if (LogicScript.Instance != null && !LogicScript.Instance.IsPlaying())
        {
            myRigidbody2D.gravityScale = 0f;
            myRigidbody2D.linearVelocity = Vector2.zero;
        }
    }

    void Update()
    {
        if (!isAlive) return;

        bool isPlaying = LogicScript.Instance == null || LogicScript.Instance.IsPlaying();

        if (!isPlaying)
        {
            // Bob peacefully up and down while waiting on the Start / Hero screen
            float bobY = startPosition.y + Mathf.Sin(Time.time * 4f) * 0.25f;
            transform.position = new Vector3(startPosition.x, bobY, startPosition.z);
            transform.rotation = Quaternion.identity;

            // If the user taps anywhere on screen, start the game!
            if (CheckInputPressed())
            {
                if (LogicScript.Instance != null)
                {
                    LogicScript.Instance.StartGame();
                }
                StartPlaying();
                Flap();
            }
            return;
        }

        // When playing: check for jump inputs (touch, click, spacebar)
        if (CheckInputPressed())
        {
            Flap();
        }

        // Smooth pitch tilt based on vertical velocity
        float targetAngle = Mathf.Clamp(myRigidbody2D.linearVelocity.y * 3.5f, minTiltAngle, maxTiltAngle);
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * tiltSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

        // Ceiling clamp: prevent bird from flying infinitely off the top
        if (transform.position.y > topBoundary)
        {
            transform.position = new Vector3(transform.position.x, topBoundary, transform.position.z);
            if (myRigidbody2D.linearVelocity.y > 0f)
            {
                myRigidbody2D.linearVelocity = new Vector2(myRigidbody2D.linearVelocity.x, 0f);
            }
        }
    }

    public void StartPlaying()
    {
        myRigidbody2D.gravityScale = initialGravityScale;
    }

    public void Flap()
    {
        if (!isAlive) return;
        Vector2 v = myRigidbody2D.linearVelocity;
        v.y = flapStrength;
        myRigidbody2D.linearVelocity = v;
        currentAngle = maxTiltAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    bool CheckInputPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // 1. Check Touchscreen on Android / mobile devices
        if (Touchscreen.current != null)
        {
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                return true;

            // Check any active touch that just began
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].press.wasPressedThisFrame)
                    return true;
            }
        }

        // 2. Check Pointer (unified handler for Touch, Mouse, Pen)
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            return true;

        // 3. Check Mouse left click (editor / desktop)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        // 4. Check Keyboard space key
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            return true;

        return false;
#else
        // Legacy input manager support
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            return true;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                    return true;
            }
        }

        return false;
#endif
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // Check if pointer is over any UI element
        return EventSystem.current.IsPointerOverGameObject();
#else
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        return EventSystem.current.IsPointerOverGameObject();
#endif
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isAlive) return;

        // If game is not yet playing, ignore accidental premature collision
        if (LogicScript.Instance != null && !LogicScript.Instance.IsPlaying()) return;

        isAlive = false;
        if (LogicScript.Instance != null)
        {
            LogicScript.Instance.GameOver();
        }
    }
}
