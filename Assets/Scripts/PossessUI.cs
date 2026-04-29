using UnityEngine;

public class PossessUI : MonoBehaviour
{
    public GameObject uiText;
    public Camera playerCamera;
    public float interactDistance = 3f;

    Outline currentOutline;

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                uiText.SetActive(true);

                // หา Outline จาก Parent
                Outline outline = hit.collider.GetComponentInParent<Outline>();

                if (outline != null)
                {
                    if (currentOutline != outline)
                    {
                        ClearOutline();
                        outline.enabled = true;
                        currentOutline = outline;
                    }
                }

                return;
            }
        }

        uiText.SetActive(false);
        ClearOutline();
    }

    void ClearOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
    }
}