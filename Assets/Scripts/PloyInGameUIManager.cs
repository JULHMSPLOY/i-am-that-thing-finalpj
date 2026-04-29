using UnityEngine;
using UnityEngine.SceneManagement;

public class PloyInGameUIManager : MonoBehaviour
{
    //public void GoHome()
    //{
    //    SceneManager.LoadScene("Scene/MainMenu");
    //}

    public void GoToMainMenu()
    {
        // โหลดหน้าเมนูหลัก (Index 0) ผ่านระบบ Transition
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

}