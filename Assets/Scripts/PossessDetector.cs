using UnityEngine;

public class PossessDetector : MonoBehaviour
{
    private Outline currentOutline;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Outline outline = other.GetComponentInParent<Outline>();

            if (outline != null)
            {
                outline.enabled = true;
                currentOutline = outline;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (currentOutline != null)
            {
                currentOutline.enabled = false;
                currentOutline = null;
            }
        }
    }
}