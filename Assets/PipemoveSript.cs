using UnityEngine;

public class PipemoveSript : MonoBehaviour
{
    // Horizontal speed (units per second)
    public float moveSpeed = 3.6f;

    void Update()
    {
        // Don't move pipes if waiting on start screen or game over
        if (LogicScript.Instance != null && !LogicScript.Instance.IsPlaying()) return;

        transform.position += (Vector3.left * moveSpeed) * Time.deltaTime;

        if (transform.position.x < -30f)
        {
            Destroy(gameObject);
        }
    }
}
