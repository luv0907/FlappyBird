using UnityEngine;

public class pipeSpawner : MonoBehaviour
{
    public GameObject pipe; // single pipe prefab (pointing up). If using a pair prefab, it should contain children.
    public float spawnRate = 1f;
    private float timer = 0f;

    public float spawnX = 10f;
    public float minY = -1f;
    public float maxY = 1f;
    public float gap = 2f;
    public float heightOffset = 0f; // fine-tune vertical placement if needed
    public float deadZone = -45f; // X position at which spawned pipes are force-deleted

    // container to hold spawned pipes so we can clean them up centrally
    private Transform pipeContainer;

    void Start()
    {
        // create a container to parent spawned pipes for easier cleanup
        pipeContainer = new GameObject("PipeContainer").transform;
        pipeContainer.parent = this.transform;
    }

    void Update()
    {
        // Don't spawn pipes if the game is waiting on the start screen or is game over
        if (LogicScript.Instance != null && !LogicScript.Instance.IsPlaying())
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            SpawnPipePair();
            timer = 0f;
        }

        // cleanup any spawned pipes that moved past the deadZone
        if (pipeContainer != null)
        {
            for (int i = pipeContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = pipeContainer.GetChild(i);
                if (child.position.x <= deadZone)
                {
                    Debug.Log($"pipeSpawner: Deleting pipe '{child.name}' at x={child.position.x} (deadZone={deadZone})", child.gameObject);
                    Destroy(child.gameObject);
                }
            }
        }
    }

    void SpawnPipePair()
    {
        // Clamp heightOffset to camera view to avoid accidentally moving pipes off-screen
        if (Camera.main != null)
        {
            float camHalf = Camera.main.orthographicSize;
            float maxOffset = camHalf;
            float minOffset = -camHalf;
            if (heightOffset > maxOffset || heightOffset < minOffset)
            {
                Debug.LogWarning($"pipeSpawner: heightOffset ({heightOffset}) is outside camera bounds. Clamping to [{minOffset},{maxOffset}].");
                heightOffset = Mathf.Clamp(heightOffset, minOffset, maxOffset);
            }
        }

        if (pipe == null)
        {
            Debug.LogWarning("pipeSpawner: pipe prefab is not assigned in the Inspector.");
            return;
        }

        float centerY = Random.Range(minY, maxY);
        float halfGap = gap * 0.5f;

        // Measure pipe prefab visual height (world units) by instantiating a temporary copy
        float pipeHeight = 0f;
        GameObject temp = Instantiate(pipe, new Vector3(0f, -1000f, 0f), Quaternion.identity);
        SpriteRenderer sr = temp.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            pipeHeight = sr.bounds.size.y;
        }
        else
        {
            Collider2D col = temp.GetComponentInChildren<Collider2D>();
            if (col != null) pipeHeight = col.bounds.size.y;
        }
        Destroy(temp);

        // Determine a sensible target visual pipe height based on camera view (so pipes are visible)
        float defaultTarget = 3.0f;
        float targetVisualPipeHeight = defaultTarget;
        if (Camera.main != null)
        {
            // Make pipes about 60% of the camera height by default
            targetVisualPipeHeight = Camera.main.orthographicSize * 2f * 0.6f;
        }

        float scaleFactor = 1f;
        if (pipeHeight > 0.01f && pipeHeight > targetVisualPipeHeight)
        {
            scaleFactor = targetVisualPipeHeight / pipeHeight;
        }
        // Clamp scale so pipes don't become invisibly small
        scaleFactor = Mathf.Clamp(scaleFactor, 0.5f, 1f);

        float halfPipe = pipeHeight * 0.5f;
        Vector3 bottomPos = new Vector3(spawnX, centerY - halfGap - halfPipe + heightOffset, 0f);
        Vector3 topPos = new Vector3(spawnX, centerY + halfGap + halfPipe + heightOffset, 0f);

        // If the prefab looks like a pipe pair (has children), instantiate it once and place its center at centerY
        GameObject prefabGO = pipe as GameObject;
        if (prefabGO != null && prefabGO.transform.childCount > 0)
        {
            GameObject pair = Instantiate(pipe, new Vector3(spawnX, centerY + heightOffset, 0f), Quaternion.identity);
            if (Mathf.Abs(scaleFactor - 1f) > 0.001f)
            {
                pair.transform.localScale *= scaleFactor;
                Debug.Log("pipeSpawner: scaled pair by " + scaleFactor);
            }
            // parent to container for cleanup
            if (pipeContainer != null) pair.transform.parent = pipeContainer;

            // create scoring trigger and parent to the pair so it moves with the pipes
            var scoreZone = new GameObject("ScoreZone");
            scoreZone.transform.position = new Vector3(spawnX, centerY, 0f);
            var bc = scoreZone.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            bc.size = new Vector2(0.2f, gap);
            scoreZone.AddComponent<ScoringTrigger>();
            scoreZone.transform.parent = pair.transform;

            // Try to find two visible children (top and bottom) and adjust their local positions
            if (pair.transform.childCount >= 2)
            {
                Transform a = pair.transform.GetChild(0);
                Transform b = pair.transform.GetChild(1);
                // determine top and bottom by localPosition.y
                Transform top = (a.localPosition.y > b.localPosition.y) ? a : b;
                Transform bottom = (a.localPosition.y > b.localPosition.y) ? b : a;

                // get their sprite heights (in local units) using SpriteRenderer bounds in world then convert to local scale
                SpriteRenderer srTop = top.GetComponentInChildren<SpriteRenderer>();
                SpriteRenderer srBottom = bottom.GetComponentInChildren<SpriteRenderer>();
                float topHalf = 0f, bottomHalf = 0f;
                if (srTop != null) topHalf = srTop.bounds.size.y * 0.5f;
                if (srBottom != null) bottomHalf = srBottom.bounds.size.y * 0.5f;

                // current center distance between children (local Y)
                float currentDist = top.localPosition.y - bottom.localPosition.y;
                // desired center distance so inner edges are `gap` apart
                float desiredDist = topHalf + bottomHalf + gap;
                float delta = desiredDist - currentDist;

                // move them apart/together evenly around their current center
                top.localPosition += new Vector3(0f, delta * 0.5f, 0f);
                bottom.localPosition -= new Vector3(0f, delta * 0.5f, 0f);
            }
        }
        else
        {
            GameObject bottom = Instantiate(pipe, bottomPos, Quaternion.identity);
            GameObject top = Instantiate(pipe, topPos, Quaternion.Euler(0f, 0f, 180f));
            // parent to container for cleanup
            if (pipeContainer != null)
            {
                bottom.transform.parent = pipeContainer;
                top.transform.parent = pipeContainer;
            }

            // create scoring trigger and parent to bottom so it moves with the pipes
            var scoreZone = new GameObject("ScoreZone");
            scoreZone.transform.position = new Vector3(spawnX, centerY, 0f);
            var bc = scoreZone.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            bc.size = new Vector2(0.2f, gap);
            scoreZone.AddComponent<ScoringTrigger>();
            scoreZone.transform.parent = bottom.transform;
            if (Mathf.Abs(scaleFactor - 1f) > 0.001f)
            {
                bottom.transform.localScale *= scaleFactor;
                top.transform.localScale *= scaleFactor;
                Debug.Log("pipeSpawner: scaled single pipe instances by " + scaleFactor);
            }
        }
    }
}
