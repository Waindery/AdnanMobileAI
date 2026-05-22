using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private GameObject tapIcon;

    void Start()
    {
        Time.timeScale = 1f;

        if (tapIcon != null)
        {
            Sequence s = DOTween.Sequence();
            s.Append(tapIcon.transform.DOLocalMove(new Vector3(80f, -120f, 0f), 2f));
        }
    }

    public void LoginButton()
    {
        Time.timeScale = 1f;
        SceneLoader.Load("GameScene");
    }

    public void LatestNewsButton()
    {
        Debug.Log("LatestNewsButton Tapped.");

        SceneLoader.Load("LatestSceneScene");
    }
}
