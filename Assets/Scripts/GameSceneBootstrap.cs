using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class GameSceneBootstrap : MonoBehaviour
{
    private void Awake()
    {
        if (Object.FindFirstObjectByType<GameManager>() != null)
        {
            Destroy(gameObject);
            return;
        }

        MobileScreenSetup.Apply();
        SetupCamera();
        CreateEventSystemIfNeeded();
        CreateSpawnPoints(out Transform echoSpawn, out Transform[] enemySpawns);
        CreateGameManager(echoSpawn, enemySpawns);
        CreateUI();
        Destroy(gameObject);
    }

    private static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        cam.orthographic = false;
        cam.fieldOfView = 50f;
        cam.transform.position = new Vector3(0.4f, 6.5f, -6.6f);
        cam.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
    }

    private static void CreateEventSystemIfNeeded()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        InputSystemUIInputModule inputModule = es.AddComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();
        es.AddComponent<UiInputModuleFix>();
    }

    private static void CreateSpawnPoints(out Transform echoSpawn, out Transform[] enemySpawns)
    {
        echoSpawn = CreateSpawn("EchoSpawn", BattleGround.SpawnPosition(-2.5f, 0f));
        enemySpawns = new[]
        {
            CreateSpawn("EnemySpawn0", BattleGround.SpawnPosition(3.2f, -2.7f)),
            CreateSpawn("EnemySpawn1", BattleGround.SpawnPosition(3.2f, -0.9f)),
            CreateSpawn("EnemySpawn2", BattleGround.SpawnPosition(3.2f, 0.9f)),
            CreateSpawn("EnemySpawn3", BattleGround.SpawnPosition(3.2f, 2.7f)),
            CreateSpawn("EnemySpawn4", BattleGround.SpawnPosition(4.55f, -1.8f)),
            CreateSpawn("EnemySpawn5", BattleGround.SpawnPosition(4.55f, 0f)),
            CreateSpawn("EnemySpawn6", BattleGround.SpawnPosition(4.55f, 1.8f)),
            CreateSpawn("EnemySpawn7", BattleGround.SpawnPosition(5.9f, 0f))
        };
    }

    private static Transform CreateSpawn(string name, Vector3 position)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        return go.transform;
    }

    private void CreateGameManager(Transform echoSpawn, Transform[] enemySpawns)
    {
        GameObject systems = new GameObject("GameSystems");
        GameManager gm = systems.AddComponent<GameManager>();
        gm.SetSpawnPoints(echoSpawn, enemySpawns);
        systems.AddComponent<PlayerWallet>();
        systems.AddComponent<GachaManager>();
    }

    private void CreateUI()
    {
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
        GameObject safeAreaGo = CreateRect("SafeArea", canvasRect);
        Stretch(safeAreaGo.GetComponent<RectTransform>());
        safeAreaGo.AddComponent<SafeArea>();
        RectTransform safeRect = safeAreaGo.GetComponent<RectTransform>();

        UIManager ui = Object.FindFirstObjectByType<GameManager>().gameObject.AddComponent<UIManager>();

        TextMeshProUGUI waveText = CreateText("WaveText", safeRect, "Wave: 1", 42f, TextAlignmentOptions.Left);
        SetAnchored(waveText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -60f), new Vector2(400f, 80f));

        TextMeshProUGUI currencyText = CreateText("CurrencyText", safeRect, "Crystals: 500", 36f, TextAlignmentOptions.Center);
        SetAnchored(currencyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(500f, 60f));

        Button pauseButton = CreateButton("PauseButton", safeRect, "||", new Vector2(120f, 60f));
        SetAnchored(pauseButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -60f), new Vector2(120f, 60f));

        Button gachaButton = CreateButton("BannerButton", safeRect, "Banner", new Vector2(160f, 60f));
        SetAnchored(gachaButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -150f), new Vector2(160f, 60f));
        gachaButton.GetComponent<Image>().color = new Color(0.45f, 0.25f, 0.65f, 0.95f);

        Slider hpSlider = CreateSlider(safeRect);
        SetAnchored(hpSlider.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(700f, 40f));

        GameObject pausePanel = CreateModal(safeRect, "PausePanel", "PAUSED", out _, out RectTransform pauseRoot);
        Button resumeButton = CreateButton("ResumeButton", pauseRoot, "Resume", new Vector2(420f, 80f));
        SetAnchored(resumeButton.GetComponent<RectTransform>(), Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, -40f), new Vector2(420f, 80f));
        Button pauseMenuButton = CreateButton("MainMenuButton", pauseRoot, "Main Menu", new Vector2(420f, 80f));
        SetAnchored(pauseMenuButton.GetComponent<RectTransform>(), Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, -140f), new Vector2(420f, 80f));
        pausePanel.SetActive(false);

        GameObject gachaPanel = CreateGachaPanel(safeRect, out TextMeshProUGUI gachaResult, out TextMeshProUGUI gachaCost, out Button pullButton, out Button closeGachaButton);
        gachaPanel.SetActive(false);

        GameObject winPanel = CreateWinPanel(safeRect, out TextMeshProUGUI winTitle, out TextMeshProUGUI winSubtitle, out RectTransform winRoot, out CanvasGroup winTitleGroup, out Button winContinue, out Button winRestart, out Button winMenu);
        winPanel.SetActive(false);

        GameObject losePanel = CreateModal(safeRect, "LosePanel", "DEFEATED", out TextMeshProUGUI loseTitle, out RectTransform loseRoot);
        CanvasGroup loseTitleGroup = loseTitle.gameObject.AddComponent<CanvasGroup>();
        Button loseRestart = CreateButton("RestartButton", loseRoot, "Restart", new Vector2(420f, 80f));
        SetAnchored(loseRestart.GetComponent<RectTransform>(), Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, -40f), new Vector2(420f, 80f));
        Button loseMenu = CreateButton("MainMenuButton", loseRoot, "Main Menu", new Vector2(420f, 80f));
        SetAnchored(loseMenu.GetComponent<RectTransform>(), Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, -140f), new Vector2(420f, 80f));
        losePanel.SetActive(false);

        ui.Configure(waveText, hpSlider, pausePanel, winPanel, losePanel, winRoot, loseRoot, winTitle, winSubtitle, winTitleGroup, loseTitleGroup,
            winContinue, new[] { winRestart, winMenu }, new[] { loseRestart, loseMenu });
        ui.ConfigureEchoHPText(hpSlider.GetComponentInChildren<TextMeshProUGUI>());
        ui.ConfigureGacha(currencyText, gachaPanel, gachaResult, gachaCost, pullButton);

        winContinue.onClick.AddListener(ui.OnContinueToLevel2Pressed);
        gachaButton.onClick.AddListener(ui.OnGachaPressed);
        pullButton.onClick.AddListener(ui.OnPullPressed);
        closeGachaButton.onClick.AddListener(ui.OnCloseGachaPressed);
        pauseButton.onClick.AddListener(ui.OnPausePressed);
        resumeButton.onClick.AddListener(ui.OnResumePressed);
        pauseMenuButton.onClick.AddListener(ui.OnMainMenuPressed);
        winRestart.onClick.AddListener(ui.OnRestartPressed);
        winMenu.onClick.AddListener(ui.OnMainMenuPressed);
        loseRestart.onClick.AddListener(ui.OnRestartPressed);
        loseMenu.onClick.AddListener(ui.OnMainMenuPressed);
    }

    private static GameObject CreateRect(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.GetComponent<RectTransform>().SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetAnchored(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = pivot;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
    }

    private static TextMeshProUGUI CreateText(string name, RectTransform parent, string text, float size, TextAlignmentOptions align)
    {
        GameObject go = CreateRect(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        return tmp;
    }

    private static Button CreateButton(string name, RectTransform parent, string label, Vector2 size)
    {
        GameObject go = CreateRect(name, parent);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.25f, 0.35f, 0.95f);
        Button button = go.AddComponent<Button>();
        go.GetComponent<RectTransform>().sizeDelta = size;

        TextMeshProUGUI tmp = CreateText("Text", go.GetComponent<RectTransform>(), label, 32f, TextAlignmentOptions.Center);
        Stretch(tmp.rectTransform);
        return button;
    }

    private static Slider CreateSlider(RectTransform parent)
    {
        GameObject sliderGo = CreateRect("EchoHPBar", parent);
        Slider slider = sliderGo.AddComponent<Slider>();
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        GameObject bg = CreateRect("Background", sliderGo.GetComponent<RectTransform>());
        bg.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        Stretch(bg.GetComponent<RectTransform>());

        GameObject fillArea = CreateRect("Fill Area", sliderGo.GetComponent<RectTransform>());
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        Stretch(fillAreaRect);
        fillAreaRect.offsetMin = new Vector2(8f, 8f);
        fillAreaRect.offsetMax = new Vector2(-8f, -8f);

        GameObject fill = CreateRect("Fill", fillAreaRect);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.7f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        Stretch(fillRect);
        slider.fillRect = fillRect;

        TextMeshProUGUI hpText = CreateText("HPText", sliderGo.GetComponent<RectTransform>(), "100 / 100", 24f, TextAlignmentOptions.Center);
        hpText.raycastTarget = false;
        Stretch(hpText.rectTransform);

        return slider;
    }

    private static GameObject CreateModal(RectTransform parent, string name, string title, out TextMeshProUGUI titleText, out RectTransform panelRoot)
    {
        GameObject panel = CreateRect(name, parent);
        Stretch(panel.GetComponent<RectTransform>());
        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        GameObject root = CreateRect("PanelRoot", panel.GetComponent<RectTransform>());
        panelRoot = root.GetComponent<RectTransform>();
        panelRoot.anchorMin = panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.sizeDelta = new Vector2(700f, 500f);
        panelRoot.localScale = Vector3.zero;
        root.AddComponent<Image>().color = new Color(0.15f, 0.2f, 0.3f, 0.98f);

        titleText = CreateText("Title", panelRoot, title, 64f, TextAlignmentOptions.Center);
        SetAnchored(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(600f, 100f));
        return panel;
    }

    private static GameObject CreateWinPanel(RectTransform parent, out TextMeshProUGUI titleText, out TextMeshProUGUI subtitleText, out RectTransform panelRoot, out CanvasGroup titleGroup, out Button continueButton, out Button restartButton, out Button menuButton)
    {
        GameObject panel = CreateRect("WinPanel", parent);
        Stretch(panel.GetComponent<RectTransform>());
        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        GameObject root = CreateRect("PanelRoot", panel.GetComponent<RectTransform>());
        panelRoot = root.GetComponent<RectTransform>();
        panelRoot.anchorMin = panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.sizeDelta = new Vector2(750f, 580f);
        panelRoot.localScale = Vector3.zero;
        root.AddComponent<Image>().color = new Color(0.15f, 0.2f, 0.3f, 0.98f);

        titleText = CreateText("Title", panelRoot, "VICTORY", 56f, TextAlignmentOptions.Center);
        SetAnchored(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -35f), new Vector2(650f, 70f));

        subtitleText = CreateText("Subtitle", panelRoot, "", 26f, TextAlignmentOptions.Center);
        SetAnchored(subtitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(650f, 100f));
        subtitleText.color = new Color(0.75f, 0.85f, 1f);

        titleGroup = titleText.gameObject.AddComponent<CanvasGroup>();

        continueButton = CreateButton("ContinueLevel2Button", panelRoot, "Level 2", new Vector2(420f, 75f));
        SetAnchored(continueButton.GetComponent<RectTransform>(), Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, -30f), new Vector2(420f, 75f));
        continueButton.GetComponent<Image>().color = new Color(0.25f, 0.55f, 0.35f, 1f);

        restartButton = CreateButton("RestartButton", panelRoot, "Restart", new Vector2(420f, 75f));
        SetAnchored(restartButton.GetComponent<RectTransform>(), Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, -120f), new Vector2(420f, 75f));

        menuButton = CreateButton("MainMenuButton", panelRoot, "Main Menu", new Vector2(420f, 75f));
        SetAnchored(menuButton.GetComponent<RectTransform>(), Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, -210f), new Vector2(420f, 75f));

        return panel;
    }

    private static GameObject CreateGachaPanel(RectTransform parent, out TextMeshProUGUI resultText, out TextMeshProUGUI costText, out Button pullButton, out Button closeButton)
    {
        GameObject panel = CreateRect("GachaPanel", parent);
        Stretch(panel.GetComponent<RectTransform>());
        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        GameObject root = CreateRect("PanelRoot", panel.GetComponent<RectTransform>());
        RectTransform panelRoot = root.GetComponent<RectTransform>();
        panelRoot.anchorMin = panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.sizeDelta = new Vector2(750f, 620f);
        root.AddComponent<Image>().color = new Color(0.12f, 0.1f, 0.22f, 0.98f);

        TextMeshProUGUI title = CreateText("Title", panelRoot, "BANNER", 56f, TextAlignmentOptions.Center);
        SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -35f), new Vector2(600f, 80f));
        title.color = new Color(1f, 0.85f, 0.4f);

        costText = CreateText("CostText", panelRoot, "Single Pull — 100 crystals", 30f, TextAlignmentOptions.Center);
        SetAnchored(costText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(600f, 50f));

        resultText = CreateText("ResultText", panelRoot, "Pull to recruit allies or upgrade Echo!", 28f, TextAlignmentOptions.Center);
        SetAnchored(resultText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(650f, 120f));

        pullButton = CreateButton("PullButton", panelRoot, "Pull x1", new Vector2(420f, 80f));
        SetAnchored(pullButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(420f, 80f));
        pullButton.GetComponent<Image>().color = new Color(0.55f, 0.3f, 0.85f, 1f);

        closeButton = CreateButton("CloseButton", panelRoot, "Close", new Vector2(420f, 80f));
        SetAnchored(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -200f), new Vector2(420f, 80f));

        return panel;
    }
}
