using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class FadeInOutTest : MonoBehaviour
{
    [SerializeField] private CanvasGroup menuCanvasGroup;

    void Start()
    {
        menuCanvasGroup.DOFade(0f, 1f);
    }

}
