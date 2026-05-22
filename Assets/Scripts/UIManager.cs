using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Slider echoHpSlider;

    [Header("Gacha")]
    [SerializeField] private GameObject gachaPanel;
    [SerializeField] private TextMeshProUGUI gachaResultText;
    [SerializeField] private TextMeshProUGUI gachaCostText;
    [SerializeField] private Button pullButton;

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Panel Animation")]
    [SerializeField] private RectTransform winPanelRoot;
    [SerializeField] private RectTransform losePanelRoot;
    [SerializeField] private TextMeshProUGUI winTitleText;
    [SerializeField] private TextMeshProUGUI winSubtitleText;
    [SerializeField] private CanvasGroup winTitleGroup;
    [SerializeField] private CanvasGroup loseTitleGroup;
    [SerializeField] private Button winContinueButton;
    [SerializeField] private Button[] winButtons;
    [SerializeField] private Button[] loseButtons;

    private Unit echo;
    private Tween hpTween;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        pausePanel?.SetActive(false);
        gachaPanel?.SetActive(false);
        winPanel?.SetActive(false);
        losePanel?.SetActive(false);
        SetPanelButtonsEnabled(winButtons, false);
        SetPanelButtonsEnabled(loseButtons, false);

        if (echoHpSlider != null)
        {
            echoHpSlider.interactable = false;
            echoHpSlider.minValue = 0f;
            echoHpSlider.maxValue = 1f;
            echoHpSlider.value = 1f;
        }

        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnCrystalsChanged += UpdateCurrencyDisplay;
            UpdateCurrencyDisplay(PlayerWallet.Instance.Crystals);
        }

        UpdateGachaCostText();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnCrystalsChanged -= UpdateCurrencyDisplay;
    }

    public void BindEcho(Unit echoUnit)
    {
        if (echo != null)
        {
            echo.OnAttackPerformed -= PlayEchoAttackJuice;
            echo.OnDamaged -= OnEchoDamaged;
        }

        echo = echoUnit;

        if (echo != null)
        {
            echo.OnAttackPerformed += PlayEchoAttackJuice;
            echo.OnDamaged += OnEchoDamaged;
            UpdateHPBar(echo.HPRatio);
        }
    }

    public void UpdateWaveText(int level, int wave, int totalWaves)
    {
        if (waveText != null)
            waveText.text = $"Lv.{level}  Wave {wave}/{totalWaves}";
    }

    public void UpdateLevelDisplay(int level)
    {
        if (currencyText != null && PlayerWallet.Instance != null)
            currencyText.text = $"Lv.{level}  |  Crystals: {PlayerWallet.Instance.Crystals}";
    }

    public void UpdateHPBar(float ratio)
    {
        if (echoHpSlider == null)
            return;

        hpTween?.Kill();
        hpTween = echoHpSlider.DOValue(Mathf.Clamp01(ratio), 0.2f);
    }

    public void OnEchoDamaged()
    {
        if (echo != null)
            UpdateHPBar(echo.HPRatio);
    }

    private void PlayEchoAttackJuice()
    {
        if (echo == null)
            return;

        echo.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 5, 0.5f);
    }

    public void UpdateCurrencyDisplay(int amount)
    {
        int level = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : GameSession.CurrentLevel;
        if (currencyText != null)
            currencyText.text = $"Lv.{level}  |  Crystals: {amount}";
    }

    private void UpdateGachaCostText()
    {
        if (gachaCostText == null || GachaManager.Instance == null)
            return;

        gachaCostText.text = $"Single Pull — {GachaManager.Instance.SinglePullCost} crystals";
    }

    public void OnGachaPressed()
    {
        if (gachaPanel == null)
            return;

        gachaPanel.SetActive(true);
        if (gachaResultText != null)
        {
            gachaResultText.text = "Pull to recruit allies or upgrade Echo!\n(Low chance: you receive nothing.)";
            gachaResultText.color = Color.white;
        }

        Time.timeScale = 0f;
    }

    public void OnCloseGachaPressed()
    {
        if (gachaPanel != null)
            gachaPanel.SetActive(false);

        if (pausePanel == null || !pausePanel.activeSelf)
            Time.timeScale = 1f;
    }

    public void OnPullPressed()
    {
        if (GachaManager.Instance == null)
            return;

        bool success = GachaManager.Instance.TrySinglePull(out string message, out bool gotReward);
        if (gachaResultText != null)
        {
            gachaResultText.text = message;
            gachaResultText.color = gotReward ? Color.white : new Color(1f, 0.45f, 0.45f);
            gachaResultText.transform.DOKill();
            gachaResultText.transform.localScale = Vector3.one;

            if (gotReward)
                gachaResultText.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 4, 0.5f);
            else
                gachaResultText.transform.DOShakePosition(0.35f, new Vector3(12f, 0f, 0f), 12, 90f, false, true);
        }

        if (success && gotReward && pullButton != null)
            pullButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 3, 0.5f);
    }

    public void OnPausePressed()
    {
        if (pausePanel == null)
            return;

        if (gachaPanel != null)
            gachaPanel.SetActive(false);

        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnResumePressed()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gachaPanel == null || !gachaPanel.activeSelf)
            Time.timeScale = 1f;
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void ShowLevelCompletePanel(int completedLevel)
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (winTitleText != null)
            winTitleText.text = $"LEVEL {completedLevel} CLEAR!";

        if (winSubtitleText != null)
        {
            winSubtitleText.gameObject.SetActive(true);
            winSubtitleText.text = completedLevel == 1
                ? $"+{GameSession.Level2CrystalBonus} crystals on Level 2!\nPull allies & upgrades, then continue."
                : string.Empty;
        }

        if (winContinueButton != null)
            winContinueButton.gameObject.SetActive(completedLevel < GameSession.MaxLevel);

        PlayEndPanelAnimation(winPanelRoot, winTitleGroup, GetActiveWinButtons());
    }

    public void ShowFinalVictoryPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (winTitleText != null)
            winTitleText.text = "VICTORY!";

        if (winSubtitleText != null)
        {
            winSubtitleText.gameObject.SetActive(true);
            winSubtitleText.text = "Echo Protocol complete.\nAll levels cleared!";
        }

        if (winContinueButton != null)
            winContinueButton.gameObject.SetActive(false);

        PlayEndPanelAnimation(winPanelRoot, winTitleGroup, GetActiveWinButtons());
    }

    public void OnContinueToLevel2Pressed()
    {
        GameManager.Instance?.ContinueToLevel2();
    }

    private Button[] GetActiveWinButtons()
    {
        if (winButtons == null || winButtons.Length == 0)
            return winButtons;

        var list = new System.Collections.Generic.List<Button>();
        foreach (Button button in winButtons)
        {
            if (button != null && button.gameObject.activeInHierarchy)
                list.Add(button);
        }

        return list.ToArray();
    }

    public void ShowLosePanel()
    {
        if (losePanel != null)
            losePanel.SetActive(true);

        PlayEndPanelAnimation(losePanelRoot, loseTitleGroup, loseButtons);
    }

    private void PlayEndPanelAnimation(RectTransform panelRoot, CanvasGroup titleGroup, Button[] buttons)
    {
        if (panelRoot == null)
            return;

        SetPanelButtonsEnabled(buttons, false);

        if (titleGroup != null)
            titleGroup.alpha = 0f;

        panelRoot.localScale = Vector3.zero;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(panelRoot.DOScale(1.1f, 0.3f).SetEase(Ease.OutBack));
        sequence.Append(panelRoot.DOScale(1f, 0.1f));

        if (titleGroup != null)
            sequence.Append(titleGroup.DOFade(1f, 0.2f));

        sequence.OnComplete(() => SetPanelButtonsEnabled(buttons, true));
    }

    private static void SetPanelButtonsEnabled(Button[] buttons, bool enabled)
    {
        if (buttons == null)
            return;

        foreach (Button button in buttons)
        {
            if (button != null)
                button.interactable = enabled;
        }
    }

    public void OnRestartPressed()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnMainMenuPressed()
    {
        GameManager.Instance?.LoadMainMenu();
    }

    public void Configure(
        TextMeshProUGUI waveLabel,
        Slider hpSlider,
        GameObject pause,
        GameObject win,
        GameObject lose,
        RectTransform winRoot,
        RectTransform loseRoot,
        TextMeshProUGUI winTitle,
        TextMeshProUGUI winSubtitle,
        CanvasGroup winTitleCanvasGroup,
        CanvasGroup loseTitle,
        Button continueLevelButton,
        Button[] winPanelButtons,
        Button[] losePanelButtons)
    {
        waveText = waveLabel;
        echoHpSlider = hpSlider;
        pausePanel = pause;
        winPanel = win;
        losePanel = lose;
        winPanelRoot = winRoot;
        losePanelRoot = loseRoot;
        winTitleText = winTitle;
        winSubtitleText = winSubtitle;
        winTitleGroup = winTitleCanvasGroup;
        loseTitleGroup = loseTitle;
        winContinueButton = continueLevelButton;
        winButtons = winPanelButtons;
        loseButtons = losePanelButtons;
    }

    public void ConfigureGacha(
        TextMeshProUGUI currencyLabel,
        GameObject bannerPanel,
        TextMeshProUGUI resultText,
        TextMeshProUGUI costText,
        Button singlePullButton)
    {
        currencyText = currencyLabel;
        gachaPanel = bannerPanel;
        gachaResultText = resultText;
        gachaCostText = costText;
        pullButton = singlePullButton;
        UpdateGachaCostText();
    }
}
