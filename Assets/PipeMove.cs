using UnityEngine;

public class PipeMove : MonoBehaviour
{
    // Horizontal speed (units per second) the pipes move left
    public float speed = 2f;
    // X position at which the pipe gets destroyed
    public float destroyX = -15f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x <= destroyX)
        {
            Destroy(gameObject);
        }
    }
}
