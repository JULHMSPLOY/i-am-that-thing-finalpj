using UnityEngine;

public class HideDetector : MonoBehaviour
{
    private Outline currentOutline;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hide"))
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
        if (other.CompareTag("Hide"))
        {
            if (currentOutline != null)
            {
                currentOutline.enabled = false;
                currentOutline = null;
            }
        }
    }
}