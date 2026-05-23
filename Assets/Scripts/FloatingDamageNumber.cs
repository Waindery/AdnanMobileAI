using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingDamageNumber : MonoBehaviour
{
    private const float Lifetime = 1f;

    private TextMeshPro text;

    public static void Show(Vector3 worldPosition, float damage, Unit attacker)
    {
        if (damage <= 0f)
            return;

        GameObject go = new GameObject("DamageNumber");
        go.transform.position = worldPosition;

        FloatingDamageNumber number = go.AddComponent<FloatingDamageNumber>();
        number.Play(Mathf.RoundToInt(damage), GetColor(attacker));
    }

    private static Color GetColor(Unit attacker)
    {
        if (attacker != null && attacker.Team == UnitTeam.Player)
            return new Color(0.2f, 0.65f, 1f, 1f);

        return new Color(1f, 0.25f, 0.25f, 1f);
    }

    private void Play(int damage, Color color)
    {
        text = gameObject.AddComponent<TextMeshPro>();
        text.text = damage.ToString();
        text.fontSize = 4f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.sortingOrder = 100;

        FaceCamera();

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * 0.8f;

        transform.DOMove(end, Lifetime).SetEase(Ease.OutCubic);
        transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 4, 0.5f);
        DOTween.To(() => text.color.a, SetAlpha, 0f, Lifetime)
            .OnComplete(() => Destroy(gameObject));
    }

    private void LateUpdate()
    {
        FaceCamera();
    }

    private void FaceCamera()
    {
        if (Camera.main == null)
            return;

        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    private void SetAlpha(float alpha)
    {
        if (text == null)
            return;

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}
