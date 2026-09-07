using UnityEngine;

public class PipeMove : MonoBehaviour
{
    // Horizontal speed (units per second) the pipes move left
    public float speed = 3.6f;
    // X position at which the pipe gets destroyed
    public float destroyX = -15f;

    void Update()
    {
        if (LogicScript.Instance != null && !LogicScript.Instance.IsPlaying()) return;

        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x <= destroyX)
        {
            Destroy(gameObject);
        }
    }
}
