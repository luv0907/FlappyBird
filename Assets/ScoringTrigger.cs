using UnityEngine;

public class ScoringTrigger : MonoBehaviour
{
    private Collider2D col;
    private bool triggered = false;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other == null) return;

        // detect the bird by BirdScript component
        var bird = other.GetComponent< BirdScript >();
        if (bird != null)
        {
            triggered = true;
            if (LogicScript.Instance != null) LogicScript.Instance.AddScore(1);
            Debug.Log($"ScoringTrigger: bird passed score zone at x={transform.position.x}");
            // optionally destroy this trigger so it doesn't fire again
            Destroy(gameObject);
        }
    }
}
