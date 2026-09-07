using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogicScript : MonoBehaviour
{
    public static LogicScript Instance { get; private set; }

    public enum GameState { Ready, Playing, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Ready;

    public Text scoreText;
    public GameObject startPanel;
    public GameObject gameOverPanel;

    private int score = 0;
    private int bestScore = 0;
    private bool isNewBest = false;

    // UI References
    private Canvas mainCanvas;
    private Text liveScoreText;
    private Text gameOverScoreText;
    private Text gameOverBestText;
    private Text gameOverMedalText;
    private Text gameOverRecordBadge;
    private Font uiFont;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
        bestScore = PlayerPrefs.GetInt("HighScore", 0);
        score = 0;

        // Initialize UI
        SetupUI();

        // Start in Ready state
        SetGameState(GameState.Ready);
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public void StartGame()
    {
        if (CurrentState != GameState.Ready) return;

        SetGameState(GameState.Playing);

        var bird = FindObjectOfType<BirdScript>();
        if (bird != null)
        {
            bird.StartPlaying();
        }
    }

    public void AddScore(int amount = 1)
    {
        if (CurrentState != GameState.Playing) return;

        score += amount;

        if (score > bestScore)
        {
            bestScore = score;
            isNewBest = true;
            PlayerPrefs.SetInt("HighScore", bestScore);
            PlayerPrefs.Save();
        }

        UpdateScoreDisplay();
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        SetGameState(GameState.GameOver);

        if (score > bestScore)
        {
            bestScore = score;
            isNewBest = true;
            PlayerPrefs.SetInt("HighScore", bestScore);
            PlayerPrefs.Save();
        }

        UpdateGameOverDisplay();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SetGameState(GameState newState)
    {
        CurrentState = newState;

        if (startPanel != null)
        {
            startPanel.SetActive(CurrentState == GameState.Ready);
        }

        if (liveScoreText != null)
        {
            liveScoreText.gameObject.SetActive(CurrentState == GameState.Playing);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(CurrentState == GameState.GameOver);
        }
    }

    private void UpdateScoreDisplay()
    {
        if (liveScoreText != null)
        {
            liveScoreText.text = score.ToString();
        }
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void UpdateGameOverDisplay()
    {
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = $"SCORE:  {score}";
        }

        if (gameOverBestText != null)
        {
            gameOverBestText.text = $"BEST:  {bestScore}";
        }

        if (gameOverRecordBadge != null)
        {
            gameOverRecordBadge.gameObject.SetActive(isNewBest && score > 0);
        }

        if (gameOverMedalText != null)
        {
            if (score >= 30)
                gameOverMedalText.text = "MEDAL:  💎 PLATINUM";
            else if (score >= 20)
                gameOverMedalText.text = "MEDAL:  🥇 GOLD";
            else if (score >= 10)
                gameOverMedalText.text = "MEDAL:  🥈 SILVER";
            else if (score >= 5)
                gameOverMedalText.text = "MEDAL:  🥉 BRONZE";
            else
                gameOverMedalText.text = "MEDAL:  Keep Going!";
        }
    }

    // ==========================================
    // UI BUILDER (Dynamic & Clean)
    // ==========================================
    private void SetupUI()
    {
        // Load default UI font
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (uiFont == null) uiFont = Font.CreateDynamicFontFromOSFont("Arial", 32);

        // Ensure Canvas exists and scales properly for mobile portrait screens
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            GameObject canvasGO = new GameObject("MainCanvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Configure Canvas Scaler for consistent mobile resolution
        CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Clean up any existing generated panels to avoid duplicates
        Transform existingStart = mainCanvas.transform.Find("StartPanel");
        if (existingStart != null) Destroy(existingStart.gameObject);
        Transform existingOver = mainCanvas.transform.Find("GameOverPanel");
        if (existingOver != null) Destroy(existingOver.gameObject);
        Transform existingScore = mainCanvas.transform.Find("LiveScoreText");
        if (existingScore != null) Destroy(existingScore.gameObject);

        // Build Start / Hero Screen
        BuildStartScreen();

        // Build In-Game HUD (Score)
        BuildHUD();

        // Build Game Over Screen
        BuildGameOverScreen();
    }

    private void BuildStartScreen()
    {
        GameObject panel = new GameObject("StartPanel");
        panel.transform.SetParent(mainCanvas.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        // Subtle gradient backdrop
        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.4f);
        bg.raycastTarget = false;

        // Title: FLOPYBIRD
        GameObject titleGO = new GameObject("GameTitle");
        titleGO.transform.SetParent(panel.transform, false);
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "FLOPYBIRD";
        titleText.font = uiFont;
        titleText.fontSize = 96;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.82f, 0.2f); // Vibrant Gold
        titleText.raycastTarget = false;
        var titleShadow = titleGO.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        titleShadow.effectDistance = new Vector2(4, -4);
        var titleOutline = titleGO.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0.85f, 0.35f, 0.05f);
        titleOutline.effectDistance = new Vector2(3, -3);

        RectTransform trt = titleGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.1f, 0.65f);
        trt.anchorMax = new Vector2(0.9f, 0.85f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        // Subtitle / High score banner
        GameObject subGO = new GameObject("BestScoreBanner");
        subGO.transform.SetParent(panel.transform, false);
        var subText = subGO.AddComponent<Text>();
        subText.text = $"★ BEST SCORE: {bestScore} ★";
        subText.font = uiFont;
        subText.fontSize = 44;
        subText.fontStyle = FontStyle.Bold;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(1f, 1f, 1f, 0.95f);
        subText.raycastTarget = false;
        var subShadow = subGO.AddComponent<Shadow>();
        subShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        subShadow.effectDistance = new Vector2(2, -2);

        RectTransform srt = subGO.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.15f, 0.55f);
        srt.anchorMax = new Vector2(0.85f, 0.65f);
        srt.offsetMin = Vector2.zero;
        srt.offsetMax = Vector2.zero;

        // Big START Button
        GameObject btnGO = new GameObject("StartButton");
        btnGO.transform.SetParent(panel.transform, false);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.78f, 0.35f, 0.95f); // Vibrant Green
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(StartGame);

        RectTransform brt = btnGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.2f, 0.32f);
        brt.anchorMax = new Vector2(0.8f, 0.44f);
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;

        // Button Label
        GameObject btnTextGO = new GameObject("BtnLabel");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        var btnText = btnTextGO.AddComponent<Text>();
        btnText.text = "TAP TO FLY ▶";
        btnText.font = uiFont;
        btnText.fontSize = 52;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        var btnShadow = btnTextGO.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(0.08f, 0.45f, 0.18f, 0.9f);
        btnShadow.effectDistance = new Vector2(2, -2);

        RectTransform btrt = btnTextGO.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.offsetMin = Vector2.zero;
        btrt.offsetMax = Vector2.zero;

        // Hint: tap anywhere
        GameObject hintGO = new GameObject("HintText");
        hintGO.transform.SetParent(panel.transform, false);
        var hintText = hintGO.AddComponent<Text>();
        hintText.text = "Tap button or tap screen to start";
        hintText.font = uiFont;
        hintText.fontSize = 32;
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.color = new Color(1f, 1f, 1f, 0.7f);
        hintText.raycastTarget = false;

        RectTransform hrt = hintGO.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0.1f, 0.22f);
        hrt.anchorMax = new Vector2(0.9f, 0.30f);
        hrt.offsetMin = Vector2.zero;
        hrt.offsetMax = Vector2.zero;

        startPanel = panel;
    }

    private void BuildHUD()
    {
        GameObject scoreGO = new GameObject("LiveScoreText");
        scoreGO.transform.SetParent(mainCanvas.transform, false);
        liveScoreText = scoreGO.AddComponent<Text>();
        liveScoreText.text = "0";
        liveScoreText.font = uiFont;
        liveScoreText.fontSize = 110;
        liveScoreText.fontStyle = FontStyle.Bold;
        liveScoreText.alignment = TextAnchor.MiddleCenter;
        liveScoreText.color = Color.white;
        liveScoreText.raycastTarget = false;

        var shadow = scoreGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(4, -4);
        var outline = scoreGO.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(3, -3);

        RectTransform rt = scoreGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.3f, 0.82f);
        rt.anchorMax = new Vector2(0.7f, 0.96f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        liveScoreText.gameObject.SetActive(false);
    }

    private void BuildGameOverScreen()
    {
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(mainCanvas.transform, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f); // Dark tint

        // Game Over Card Box
        GameObject cardGO = new GameObject("ScoreCard");
        cardGO.transform.SetParent(panel.transform, false);
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.12f, 0.14f, 0.22f, 0.95f); // Sleek slate card
        RectTransform crt = cardGO.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.12f, 0.35f);
        crt.anchorMax = new Vector2(0.88f, 0.78f);
        crt.offsetMin = Vector2.zero;
        crt.offsetMax = Vector2.zero;

        // Card Border
        var cardOutline = cardGO.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0.98f, 0.75f, 0.18f, 0.8f);
        cardOutline.effectDistance = new Vector2(3, -3);

        // Header: GAME OVER
        GameObject headerGO = new GameObject("GameOverTitle");
        headerGO.transform.SetParent(cardGO.transform, false);
        var headerText = headerGO.AddComponent<Text>();
        headerText.text = "GAME OVER";
        headerText.font = uiFont;
        headerText.fontSize = 68;
        headerText.fontStyle = FontStyle.Bold;
        headerText.alignment = TextAnchor.MiddleCenter;
        headerText.color = new Color(0.95f, 0.25f, 0.25f);
        var hShadow = headerGO.AddComponent<Shadow>();
        hShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        hShadow.effectDistance = new Vector2(3, -3);

        RectTransform hrt = headerGO.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0.05f, 0.76f);
        hrt.anchorMax = new Vector2(0.95f, 0.95f);
        hrt.offsetMin = Vector2.zero;
        hrt.offsetMax = Vector2.zero;

        // New Record Badge
        GameObject badgeGO = new GameObject("NewRecordBadge");
        badgeGO.transform.SetParent(cardGO.transform, false);
        gameOverRecordBadge = badgeGO.AddComponent<Text>();
        gameOverRecordBadge.text = "★ NEW HIGH SCORE! ★";
        gameOverRecordBadge.font = uiFont;
        gameOverRecordBadge.fontSize = 36;
        gameOverRecordBadge.fontStyle = FontStyle.Bold;
        gameOverRecordBadge.alignment = TextAnchor.MiddleCenter;
        gameOverRecordBadge.color = new Color(1f, 0.85f, 0.2f);
        var bShadow = badgeGO.AddComponent<Shadow>();
        bShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        bShadow.effectDistance = new Vector2(2, -2);

        RectTransform bdrt = badgeGO.GetComponent<RectTransform>();
        bdrt.anchorMin = new Vector2(0.05f, 0.62f);
        bdrt.anchorMax = new Vector2(0.95f, 0.74f);
        bdrt.offsetMin = Vector2.zero;
        bdrt.offsetMax = Vector2.zero;

        // Current Score Text
        GameObject curScoreGO = new GameObject("CurrentScore");
        curScoreGO.transform.SetParent(cardGO.transform, false);
        gameOverScoreText = curScoreGO.AddComponent<Text>();
        gameOverScoreText.text = "SCORE:  0";
        gameOverScoreText.font = uiFont;
        gameOverScoreText.fontSize = 46;
        gameOverScoreText.fontStyle = FontStyle.Bold;
        gameOverScoreText.alignment = TextAnchor.MiddleCenter;
        gameOverScoreText.color = Color.white;

        RectTransform csrt = curScoreGO.GetComponent<RectTransform>();
        csrt.anchorMin = new Vector2(0.05f, 0.44f);
        csrt.anchorMax = new Vector2(0.95f, 0.60f);
        csrt.offsetMin = Vector2.zero;
        csrt.offsetMax = Vector2.zero;

        // Best Score Text
        GameObject bestScoreGO = new GameObject("BestScore");
        bestScoreGO.transform.SetParent(cardGO.transform, false);
        gameOverBestText = bestScoreGO.AddComponent<Text>();
        gameOverBestText.text = "BEST:  0";
        gameOverBestText.font = uiFont;
        gameOverBestText.fontSize = 42;
        gameOverBestText.fontStyle = FontStyle.Bold;
        gameOverBestText.alignment = TextAnchor.MiddleCenter;
        gameOverBestText.color = new Color(1f, 0.82f, 0.2f);

        RectTransform bsrt = bestScoreGO.GetComponent<RectTransform>();
        bsrt.anchorMin = new Vector2(0.05f, 0.28f);
        bsrt.anchorMax = new Vector2(0.95f, 0.44f);
        bsrt.offsetMin = Vector2.zero;
        bsrt.offsetMax = Vector2.zero;

        // Medal Text
        GameObject medalGO = new GameObject("MedalText");
        medalGO.transform.SetParent(cardGO.transform, false);
        gameOverMedalText = medalGO.AddComponent<Text>();
        gameOverMedalText.text = "MEDAL: None";
        gameOverMedalText.font = uiFont;
        gameOverMedalText.fontSize = 36;
        gameOverMedalText.fontStyle = FontStyle.Bold;
        gameOverMedalText.alignment = TextAnchor.MiddleCenter;
        gameOverMedalText.color = new Color(0.85f, 0.9f, 1f);

        RectTransform mrt = medalGO.GetComponent<RectTransform>();
        mrt.anchorMin = new Vector2(0.05f, 0.08f);
        mrt.anchorMax = new Vector2(0.95f, 0.26f);
        mrt.offsetMin = Vector2.zero;
        mrt.offsetMax = Vector2.zero;

        // Big PLAY AGAIN Button
        GameObject restartBtnGO = new GameObject("PlayAgainButton");
        restartBtnGO.transform.SetParent(panel.transform, false);
        var rBtnImg = restartBtnGO.AddComponent<Image>();
        rBtnImg.color = new Color(0.18f, 0.78f, 0.35f, 0.95f); // Vibrant Green
        var rBtn = restartBtnGO.AddComponent<Button>();
        rBtn.onClick.AddListener(RestartGame);

        RectTransform rbrt = restartBtnGO.GetComponent<RectTransform>();
        rbrt.anchorMin = new Vector2(0.2f, 0.20f);
        rbrt.anchorMax = new Vector2(0.8f, 0.30f);
        rbrt.offsetMin = Vector2.zero;
        rbrt.offsetMax = Vector2.zero;

        // Button Label
        GameObject rBtnTextGO = new GameObject("RBtnLabel");
        rBtnTextGO.transform.SetParent(restartBtnGO.transform, false);
        var rBtnText = rBtnTextGO.AddComponent<Text>();
        rBtnText.text = "PLAY AGAIN ↺";
        rBtnText.font = uiFont;
        rBtnText.fontSize = 50;
        rBtnText.fontStyle = FontStyle.Bold;
        rBtnText.alignment = TextAnchor.MiddleCenter;
        rBtnText.color = Color.white;
        var rShadow = rBtnTextGO.AddComponent<Shadow>();
        rShadow.effectColor = new Color(0.08f, 0.45f, 0.18f, 0.9f);
        rShadow.effectDistance = new Vector2(2, -2);

        RectTransform rtrt = rBtnTextGO.GetComponent<RectTransform>();
        rtrt.anchorMin = Vector2.zero;
        rtrt.anchorMax = Vector2.one;
        rtrt.offsetMin = Vector2.zero;
        rtrt.offsetMax = Vector2.zero;

        gameOverPanel = panel;
        gameOverPanel.SetActive(false);
    }
}
