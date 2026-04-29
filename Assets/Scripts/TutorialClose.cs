using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialClose : MonoBehaviour
{
    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}