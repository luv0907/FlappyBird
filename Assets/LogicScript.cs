using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogicScript : MonoBehaviour
{
    public static LogicScript Instance { get; private set; }

    public Text scoreText; // assign the UI Text (Legacy) in Inspector
    public GameObject gameOverPanel; // assign a UI panel (or button) to show on game over

    private int score = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        score = PlayerPrefs.GetInt("score", 0);
        // auto-find a UI Text if not assigned
        if (scoreText == null)
        {
            var t = FindObjectOfType<UnityEngine.UI.Text>();
            if (t != null) scoreText = t;
        }
        UpdateUI();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            CreateDefaultGameOverUI();
        }
    }

    public void GameOver()
    {
        Debug.Log("LogicScript: Game Over");
        // pause the game
        Time.timeScale = 0f;
        // disable bird control if present
        var bird = FindObjectOfType<BirdScript>();
        if (bird != null) bird.enabled = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void AddScore(int amount = 1)
    {
        score += amount;
        PlayerPrefs.SetInt("score", score);
        UpdateUI();
    }

    public void ResetScore()
    {
        score = 0;
        PlayerPrefs.SetInt("score", score);
        UpdateUI();
    }

    // Called by UI Button to restart the scene
    public void RestartGame()
    {
        // reset time scale and score, then reload the current scene
        Time.timeScale = 1f;
        ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Create a simple Game Over UI at runtime if none is assigned in the Inspector
    void CreateDefaultGameOverUI()
    {
        // try to find or create a Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // create panel
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(canvas.transform, false);
        var img = panel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0f, 0f, 0f, 0.6f);
        RectTransform prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0f, 0f);
        prt.anchorMax = new Vector2(1f, 1f);
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        // title text
        GameObject title = new GameObject("GameOverText");
        title.transform.SetParent(panel.transform, false);
        var txt = title.AddComponent<UnityEngine.UI.Text>();
        txt.text = "Game Over";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontSize = 64;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform trt = title.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.25f, 0.6f);
        trt.anchorMax = new Vector2(0.75f, 0.9f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        // restart button
        GameObject btnGO = new GameObject("RestartButton");
        btnGO.transform.SetParent(panel.transform, false);
        var btnImage = btnGO.AddComponent<UnityEngine.UI.Image>();
        btnImage.color = new Color(1f, 1f, 1f, 0.9f);
        var button = btnGO.AddComponent<UnityEngine.UI.Button>();
        RectTransform brt = btnGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.35f, 0.3f);
        brt.anchorMax = new Vector2(0.65f, 0.5f);
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;

        // button label
        GameObject btnText = new GameObject("Text");
        btnText.transform.SetParent(btnGO.transform, false);
        var btxt = btnText.AddComponent<UnityEngine.UI.Text>();
        btxt.text = "Restart";
        btxt.alignment = TextAnchor.MiddleCenter;
        btxt.fontSize = 32;
        btxt.color = Color.black;
        btxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform btrt = btnText.GetComponent<RectTransform>();
        btrt.anchorMin = new Vector2(0f, 0f);
        btrt.anchorMax = new Vector2(1f, 1f);
        btrt.offsetMin = Vector2.zero;
        btrt.offsetMax = Vector2.zero;

        // wire up button to RestartGame
        button.onClick.AddListener(RestartGame);

        // assign and hide
        gameOverPanel = panel;
        gameOverPanel.SetActive(false);
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        else
        {
            Debug.LogWarning("LogicScript: scoreText not assigned in Inspector.");
        }
    }
}

