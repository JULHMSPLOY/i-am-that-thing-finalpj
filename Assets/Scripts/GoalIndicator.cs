using UnityEngine;

public class GoalIndicator : MonoBehaviour
{
    public Transform goal;
    public Camera playerCamera;

    void Update()
    {
        Vector3 screenPos = playerCamera.WorldToScreenPoint(goal.position);

        if (screenPos.z > 0)
        {
            transform.position = screenPos;
        }
    }
}
