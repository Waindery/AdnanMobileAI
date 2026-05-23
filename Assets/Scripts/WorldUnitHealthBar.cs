using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldUnitHealthBar : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.28f, 0f);

    private Unit unit;
    private Slider slider;
    private TextMeshProUGUI hpText;
    private RectTransform root;
    private Renderer unitRenderer;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        unitRenderer = GetComponentInChildren<Renderer>();
        BuildHealthBar();
    }

    private void OnEnable()
    {
        if (unit != null)
        {
            unit.OnDamaged += UpdateHealthBar;
            unit.OnDeath += HandleDeath;
        }

        UpdateHealthBar();
    }

    private void OnDisable()
    {
        if (unit != null)
        {
            unit.OnDamaged -= UpdateHealthBar;
            unit.OnDeath -= HandleDeath;
        }
    }

    private void LateUpdate()
    {
        if (root == null)
            return;

        root.position = GetBarPosition();

        if (Camera.main != null)
            root.rotation = Quaternion.LookRotation(root.position - Camera.main.transform.position);
    }

    private Vector3 GetBarPosition()
    {
        if (unitRenderer != null)
            return unitRenderer.bounds.center + Vector3.up * (unitRenderer.bounds.extents.y + offset.y);

        return transform.position + Vector3.up * 1.25f;
    }

    private void BuildHealthBar()
    {
        GameObject canvasGo = new GameObject("WorldHealthBar");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50;

        root = canvasGo.GetComponent<RectTransform>();
        root.sizeDelta = new Vector2(110f, 30f);
        root.localScale = Vector3.one * 0.008f;

        GameObject bgGo = CreateRect("Background", root);
        Image bg = bgGo.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
        Stretch(bgGo.GetComponent<RectTransform>());

        GameObject sliderGo = CreateRect("Slider", root);
        slider = sliderGo.AddComponent<Slider>();
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        Stretch(sliderGo.GetComponent<RectTransform>());

        GameObject fillArea = CreateRect("Fill Area", sliderGo.GetComponent<RectTransform>());
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        Stretch(fillAreaRect);
        fillAreaRect.offsetMin = new Vector2(3f, 3f);
        fillAreaRect.offsetMax = new Vector2(-3f, -3f);

        GameObject fillGo = CreateRect("Fill", fillAreaRect);
        Image fill = fillGo.AddComponent<Image>();
        fill.color = new Color(1f, 0.15f, 0.15f, 1f);
        RectTransform fillRect = fillGo.GetComponent<RectTransform>();
        Stretch(fillRect);

        slider.fillRect = fillRect;
        slider.targetGraphic = fill;

        GameObject textGo = CreateRect("HPText", root);
        hpText = textGo.AddComponent<TextMeshProUGUI>();
        hpText.fontSize = 14f;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = Color.white;
        hpText.raycastTarget = false;
        Stretch(textGo.GetComponent<RectTransform>());
    }

    private GameObject CreateRect(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.GetComponent<RectTransform>().SetParent(parent, false);
        return go;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void UpdateHealthBar()
    {
        if (unit == null || slider == null || hpText == null)
            return;

        slider.value = unit.HPRatio;
        hpText.text = $"{Mathf.CeilToInt(unit.CurrentHP)} / {Mathf.CeilToInt(unit.MaxHP)}";
    }

    private void HandleDeath(Unit deadUnit)
    {
        if (root != null)
            root.gameObject.SetActive(false);
    }
}
