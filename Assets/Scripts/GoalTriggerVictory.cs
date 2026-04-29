using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalTriggerVictory : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene("Scene/Victory");
            }
            else
            {
                SceneManager.LoadScene("Scene/Victory");
            }
            //SceneManager.LoadScene("Scene/Victory");
        }
        
    }
}