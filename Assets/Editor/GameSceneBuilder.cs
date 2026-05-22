#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string EchoPrefabPath = "Assets/Prefabs/EchoPrefab.prefab";
    private const string EnemyPrefabPath = "Assets/Prefabs/EnemyPrefab.prefab";

    [MenuItem("Echo Protocol/Build Game Scene")]
    public static void BuildGameSceneMenu()
    {
        BuildGameScene();
    }

    public static void BuildFromCommandLine()
    {
        BuildGameScene();
        EditorApplication.Exit(0);
    }

    public static void BuildGameScene()
    {
        EnsureFolders();

        Material blueMat = CreateColorMaterial("Assets/Materials/EchoBlue.mat", new Color(0.2f, 0.45f, 1f));
        Material redMat = CreateColorMaterial("Assets/Materials/EnemyRed.mat", new Color(0.9f, 0.2f, 0.2f));

        Unit echoPrefab = BuildUnitPrefab("Echo", EchoPrefabPath, blueMat, UnitTeam.Player, 100f, 20f, 1.5f);
        Unit enemyPrefab = BuildUnitPrefab("Enemy", EnemyPrefabPath, redMat, UnitTeam.Enemy, 40f, 10f, 2f);

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ClearSceneExceptDefaults(scene);

        GameObject entry = new GameObject("GameSceneEntry");
        entry.AddComponent<GameSceneBootstrap>();

        SetupCamera();
        Transform echoSpawn = CreateSpawnPoint("EchoSpawn", BattleGround.SpawnPosition(-2.5f, 0f));
        Transform[] enemySpawns =
        {
            CreateSpawnPoint("EnemySpawn0", BattleGround.SpawnPosition(3f, -2f)),
            CreateSpawnPoint("EnemySpawn1", BattleGround.SpawnPosition(3f, 0f)),
            CreateSpawnPoint("EnemySpawn2", BattleGround.SpawnPosition(3f, 2f)),
            CreateSpawnPoint("EnemySpawn3", BattleGround.SpawnPosition(4.2f, -1f)),
            CreateSpawnPoint("EnemySpawn4", BattleGround.SpawnPosition(4.2f, 1f)),
            CreateSpawnPoint("EnemySpawn5", BattleGround.SpawnPosition(5.2f, 0f))
        };

        GameObject systems = new GameObject("GameSystems");
        GameManager gameManager = systems.AddComponent<GameManager>();
        SerializedObject gmSo = new SerializedObject(gameManager);
        gmSo.FindProperty("echoPrefab").objectReferenceValue = echoPrefab;
        gmSo.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
        gmSo.FindProperty("echoSpawnPoint").objectReferenceValue = echoSpawn;
        gmSo.FindProperty("enemySpawnPoints").arraySize = enemySpawns.Length;
        for (int i = 0; i < enemySpawns.Length; i++)
            gmSo.FindProperty("enemySpawnPoints").GetArrayElementAtIndex(i).objectReferenceValue = enemySpawns[i];
        gmSo.ApplyModifiedPropertiesWithoutUndo();

        systems.AddComponent<PlayerWallet>();
        systems.AddComponent<GachaManager>();

        BuildCanvasUI(systems);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Echo Protocol Game Scene built successfully.");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Prefabs"));
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Materials"));
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "Editor"));
    }

    private static Material CreateColorMaterial(string path, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            mat = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            EditorUtility.SetDirty(mat);
        }

        return mat;
    }

    private static Unit BuildUnitPrefab(string name, string path, Material material, UnitTeam team, float hp, float damage, float cooldown)
    {
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        root.name = name;
        Object.DestroyImmediate(root.GetComponent<Collider>());

        Renderer renderer = root.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        Unit unit = root.AddComponent<Unit>();
        SerializedObject unitSo = new SerializedObject(unit);
        unitSo.FindProperty("team").enumValueIndex = (int)team;
        unitSo.FindProperty("maxHP").floatValue = hp;
        unitSo.FindProperty("attackDamage").floatValue = damage;
        unitSo.FindProperty("attackCooldown").floatValue = cooldown;
        unitSo.ApplyModifiedPropertiesWithoutUndo();

        root.AddComponent<UnitFSM>();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return AssetDatabase.LoadAssetAtPath<Unit>(path);
    }

    private static void ClearSceneExceptDefaults(Scene scene)
    {
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (go.name == "Main Camera" || go.name == "Directional Light")
                continue;

            Object.DestroyImmediate(go);
        }

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = es.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
            es.AddComponent<UiInputModuleFix>();
        }
    }

    private static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        cam.transform.position = new Vector3(0f, 2.5f, -7f);
        cam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
    }

    private static Transform CreateSpawnPoint(string name, Vector3 position)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        return go.transform;
    }

    private static void BuildCanvasUI(GameObject systems)
    {
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>().Apply1080x1920();
        canvasGo.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

        GameObject safeAreaGo = CreateUIObject("SafeArea", canvasRect);
        RectTransform safeRect = safeAreaGo.GetComponent<RectTransform>();
        StretchFull(safeRect);
        safeAreaGo.AddComponent<SafeArea>();

        UIManager ui = systems.AddComponent<UIManager>();

        TextMeshProUGUI waveText = CreateTMP("WaveText", safeRect, "Wave: 1", 42, TextAlignmentOptions.Left);
        RectTransform waveRect = waveText.rectTransform;
        waveRect.anchorMin = new Vector2(0f, 1f);
        waveRect.anchorMax = new Vector2(0f, 1f);
        waveRect.pivot = new Vector2(0f, 1f);
        waveRect.anchoredPosition = new Vector2(40f, -60f);
        waveRect.sizeDelta = new Vector2(400f, 80f);

        TextMeshProUGUI currencyText = CreateTMP("CurrencyText", safeRect, "Crystals: 500", 36, TextAlignmentOptions.Center);
        RectTransform currencyRect = currencyText.rectTransform;
        currencyRect.anchorMin = new Vector2(0.5f, 1f);
        currencyRect.anchorMax = new Vector2(0.5f, 1f);
        currencyRect.pivot = new Vector2(0.5f, 1f);
        currencyRect.anchoredPosition = new Vector2(0f, -60f);
        currencyRect.sizeDelta = new Vector2(500f, 60f);

        Button pauseButton = CreateButton("PauseButton", safeRect, "||", new Vector2(120f, 60f));
        RectTransform pauseRect = pauseButton.GetComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(1f, 1f);
        pauseRect.anchorMax = new Vector2(1f, 1f);
        pauseRect.pivot = new Vector2(1f, 1f);
        pauseRect.anchoredPosition = new Vector2(-40f, -60f);

        Button gachaButton = CreateButton("BannerButton", safeRect, "Banner", new Vector2(160f, 60f));
        RectTransform gachaRect = gachaButton.GetComponent<RectTransform>();
        gachaRect.anchorMin = new Vector2(1f, 1f);
        gachaRect.anchorMax = new Vector2(1f, 1f);
        gachaRect.pivot = new Vector2(1f, 1f);
        gachaRect.anchoredPosition = new Vector2(-40f, -150f);
        gachaButton.GetComponent<Image>().color = new Color(0.45f, 0.25f, 0.65f, 0.95f);

        Slider hpSlider = CreateHPSlider(safeRect);
        RectTransform hpRect = hpSlider.GetComponent<RectTransform>();
        hpRect.anchorMin = new Vector2(0.5f, 0f);
        hpRect.anchorMax = new Vector2(0.5f, 0f);
        hpRect.pivot = new Vector2(0.5f, 0f);
        hpRect.anchoredPosition = new Vector2(0f, 80f);
        hpRect.sizeDelta = new Vector2(700f, 40f);

        GameObject pausePanel = CreateOverlayPanel(safeRect, "PausePanel", "PAUSED", out TextMeshProUGUI pauseTitle);
        pauseTitle.alignment = TextAlignmentOptions.Center;
        Button resumeButton = CreatePanelButton(pausePanel.transform, "ResumeButton", "Resume", new Vector2(0f, -40f));
        Button pauseMenuButton = CreatePanelButton(pausePanel.transform, "MainMenuButton", "Main Menu", new Vector2(0f, -140f));
        pausePanel.SetActive(false);

        GameObject gachaPanel = CreateGachaPanel(safeRect, out TextMeshProUGUI gachaResult, out TextMeshProUGUI gachaCost, out Button pullButton, out Button closeGachaButton);
        gachaPanel.SetActive(false);

        GameObject winPanel = CreateWinPanel(safeRect, out TextMeshProUGUI winTitle, out TextMeshProUGUI winSubtitle, out RectTransform winRoot, out CanvasGroup winTitleGroup, out Button winContinue, out Button winRestart, out Button winMenu);
        winPanel.SetActive(false);
        SetButtonsInteractable(new[] { winContinue, winRestart, winMenu }, false);

        GameObject losePanel = CreateOverlayPanel(safeRect, "LosePanel", "DEFEATED", out TextMeshProUGUI loseTitle);
        RectTransform loseRoot = losePanel.transform.Find("PanelRoot") as RectTransform;
        CanvasGroup loseTitleGroup = loseTitle.gameObject.AddComponent<CanvasGroup>();
        Button loseRestart = CreatePanelButton(loseRoot, "RestartButton", "Restart", new Vector2(0f, -40f));
        Button loseMenu = CreatePanelButton(loseRoot, "MainMenuButton", "Main Menu", new Vector2(0f, -140f));
        losePanel.SetActive(false);
        SetButtonsInteractable(new[] { loseRestart, loseMenu }, false);

        SerializedObject uiSo = new SerializedObject(ui);
        uiSo.FindProperty("waveText").objectReferenceValue = waveText;
        uiSo.FindProperty("echoHpSlider").objectReferenceValue = hpSlider;
        uiSo.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        uiSo.FindProperty("winPanel").objectReferenceValue = winPanel;
        uiSo.FindProperty("losePanel").objectReferenceValue = losePanel;
        uiSo.FindProperty("winPanelRoot").objectReferenceValue = winRoot;
        uiSo.FindProperty("losePanelRoot").objectReferenceValue = loseRoot;
        uiSo.FindProperty("winTitleText").objectReferenceValue = winTitle;
        uiSo.FindProperty("winSubtitleText").objectReferenceValue = winSubtitle;
        uiSo.FindProperty("winTitleGroup").objectReferenceValue = winTitleGroup;
        uiSo.FindProperty("loseTitleGroup").objectReferenceValue = loseTitleGroup;
        uiSo.FindProperty("winContinueButton").objectReferenceValue = winContinue;
        uiSo.FindProperty("winButtons").arraySize = 2;
        uiSo.FindProperty("winButtons").GetArrayElementAtIndex(0).objectReferenceValue = winRestart;
        uiSo.FindProperty("winButtons").GetArrayElementAtIndex(1).objectReferenceValue = winMenu;
        uiSo.FindProperty("loseButtons").arraySize = 2;
        uiSo.FindProperty("loseButtons").GetArrayElementAtIndex(0).objectReferenceValue = loseRestart;
        uiSo.FindProperty("loseButtons").GetArrayElementAtIndex(1).objectReferenceValue = loseMenu;
        uiSo.FindProperty("currencyText").objectReferenceValue = currencyText;
        uiSo.FindProperty("gachaPanel").objectReferenceValue = gachaPanel;
        uiSo.FindProperty("gachaResultText").objectReferenceValue = gachaResult;
        uiSo.FindProperty("gachaCostText").objectReferenceValue = gachaCost;
        uiSo.FindProperty("pullButton").objectReferenceValue = pullButton;
        uiSo.ApplyModifiedPropertiesWithoutUndo();

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

    private static GameObject CreateUIObject(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static TextMeshProUGUI CreateTMP(string name, RectTransform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = CreateUIObject(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        return tmp;
    }

    private static Button CreateButton(string name, RectTransform parent, string label, Vector2 size)
    {
        GameObject go = CreateUIObject(name, parent);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.25f, 0.35f, 0.95f);
        Button button = go.AddComponent<Button>();

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = CreateTMP("Text", rect, label, 32f, TextAlignmentOptions.Center);
        StretchFull(tmp.rectTransform);

        return button;
    }

    private static Button CreatePanelButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        RectTransform parentRect = parent as RectTransform;
        Button button = CreateButton(name, parentRect, label, new Vector2(420f, 80f));
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        return button;
    }

    private static Slider CreateHPSlider(RectTransform parent)
    {
        GameObject sliderGo = CreateUIObject("EchoHPBar", parent);
        Slider slider = sliderGo.AddComponent<Slider>();
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        RectTransform sliderRect = sliderGo.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(700f, 40f);

        GameObject bg = CreateUIObject("Background", sliderRect);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        StretchFull(bg.GetComponent<RectTransform>());

        GameObject fillArea = CreateUIObject("Fill Area", sliderRect);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        StretchFull(fillAreaRect);
        fillAreaRect.offsetMin = new Vector2(8f, 8f);
        fillAreaRect.offsetMax = new Vector2(-8f, -8f);

        GameObject fill = CreateUIObject("Fill", fillAreaRect);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.7f, 1f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        return slider;
    }

    private static GameObject CreateOverlayPanel(RectTransform parent, string name, string title, out TextMeshProUGUI titleText)
    {
        GameObject panel = CreateUIObject(name, parent);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        StretchFull(panelRect);

        Image dim = panel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject root = CreateUIObject("PanelRoot", panelRect);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(700f, 500f);
        rootRect.localScale = Vector3.zero;

        Image panelBg = root.AddComponent<Image>();
        panelBg.color = new Color(0.15f, 0.2f, 0.3f, 0.98f);

        titleText = CreateTMP("Title", rootRect, title, 64f, TextAlignmentOptions.Center);
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -40f);
        titleRect.sizeDelta = new Vector2(600f, 100f);

        return panel;
    }

    private static void SetButtonsInteractable(Button[] buttons, bool interactable)
    {
        foreach (Button button in buttons)
            button.interactable = interactable;
    }

    private static GameObject CreateWinPanel(RectTransform parent, out TextMeshProUGUI titleText, out TextMeshProUGUI subtitleText, out RectTransform panelRoot, out CanvasGroup titleGroup, out Button continueButton, out Button restartButton, out Button menuButton)
    {
        GameObject panel = CreateUIObject("WinPanel", parent);
        StretchFull(panel.GetComponent<RectTransform>());
        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        GameObject root = CreateUIObject("PanelRoot", panel.GetComponent<RectTransform>());
        panelRoot = root.GetComponent<RectTransform>();
        panelRoot.anchorMin = panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.sizeDelta = new Vector2(750f, 580f);
        panelRoot.localScale = Vector3.zero;
        root.AddComponent<Image>().color = new Color(0.15f, 0.2f, 0.3f, 0.98f);

        titleText = CreateTMP("Title", panelRoot, "VICTORY", 56, TextAlignmentOptions.Center);
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -35f);
        titleRect.sizeDelta = new Vector2(650f, 70f);

        subtitleText = CreateTMP("Subtitle", panelRoot, "", 26, TextAlignmentOptions.Center);
        RectTransform subRect = subtitleText.rectTransform;
        subRect.anchorMin = new Vector2(0.5f, 1f);
        subRect.anchorMax = new Vector2(0.5f, 1f);
        subRect.pivot = new Vector2(0.5f, 1f);
        subRect.anchoredPosition = new Vector2(0f, -120f);
        subRect.sizeDelta = new Vector2(650f, 100f);
        subtitleText.color = new Color(0.75f, 0.85f, 1f);

        titleGroup = titleText.gameObject.AddComponent<CanvasGroup>();

        continueButton = CreatePanelButton(panelRoot, "ContinueLevel2Button", "Level 2", new Vector2(0f, -30f));
        continueButton.GetComponent<Image>().color = new Color(0.25f, 0.55f, 0.35f, 1f);

        restartButton = CreatePanelButton(panelRoot, "RestartButton", "Restart", new Vector2(0f, -120f));
        menuButton = CreatePanelButton(panelRoot, "MainMenuButton", "Main Menu", new Vector2(0f, -210f));

        return panel;
    }

    private static GameObject CreateGachaPanel(RectTransform parent, out TextMeshProUGUI resultText, out TextMeshProUGUI costText, out Button pullButton, out Button closeButton)
    {
        GameObject panel = CreateUIObject("GachaPanel", parent);
        StretchFull(panel.GetComponent<RectTransform>());
        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        GameObject root = CreateUIObject("PanelRoot", panel.GetComponent<RectTransform>());
        RectTransform panelRoot = root.GetComponent<RectTransform>();
        panelRoot.anchorMin = panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.sizeDelta = new Vector2(750f, 620f);
        root.AddComponent<Image>().color = new Color(0.12f, 0.1f, 0.22f, 0.98f);

        TextMeshProUGUI title = CreateTMP("Title", panelRoot, "BANNER", 56, TextAlignmentOptions.Center);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -35f);
        titleRect.sizeDelta = new Vector2(600f, 80f);
        title.color = new Color(1f, 0.85f, 0.4f);

        costText = CreateTMP("CostText", panelRoot, "Single Pull — 100 crystals", 30, TextAlignmentOptions.Center);
        RectTransform costRect = costText.rectTransform;
        costRect.anchorMin = new Vector2(0.5f, 1f);
        costRect.anchorMax = new Vector2(0.5f, 1f);
        costRect.pivot = new Vector2(0.5f, 1f);
        costRect.anchoredPosition = new Vector2(0f, -120f);
        costRect.sizeDelta = new Vector2(600f, 50f);

        resultText = CreateTMP("ResultText", panelRoot, "Pull to recruit allies or upgrade Echo!", 28, TextAlignmentOptions.Center);
        RectTransform resultRect = resultText.rectTransform;
        resultRect.anchorMin = new Vector2(0.5f, 0.5f);
        resultRect.anchorMax = new Vector2(0.5f, 0.5f);
        resultRect.pivot = new Vector2(0.5f, 0.5f);
        resultRect.anchoredPosition = new Vector2(0f, 20f);
        resultRect.sizeDelta = new Vector2(650f, 120f);

        pullButton = CreatePanelButton(panelRoot, "PullButton", "Pull x1", new Vector2(0f, -100f));
        pullButton.GetComponent<Image>().color = new Color(0.55f, 0.3f, 0.85f, 1f);

        closeButton = CreatePanelButton(panelRoot, "CloseButton", "Close", new Vector2(0f, -200f));

        return panel;
    }

    private static void Apply1080x1920(this CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
    }
}
#endif
