using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private GameObject tapIcon;

    void Start()
    {
        //menuCanvasGroup.DOFade(0f, 1f);

        //tapIcon.transform.DOLocalMove(new Vector3(0f, 50f, 0f), 2f);

        Sequence s = DOTween.Sequence();

        s.Append(tapIcon.transform.DOLocalMove(new Vector3(80f, -120f, 0f), 2f));

    }

    public void LoginButton()
    {
        Debug.Log("LoginButton Tapped.");

        SceneManager.LoadScene("GameScene");
    }

    public void LatestNewsButton()
    {
        Debug.Log("LatestNewsButton Tapped.");

        SceneManager.LoadScene("LatestNewsScene");
    }
}
