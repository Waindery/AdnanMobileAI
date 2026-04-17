using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
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
