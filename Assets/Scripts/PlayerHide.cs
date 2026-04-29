using UnityEngine;

public class PlayerHide : MonoBehaviour
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
            // ตรวจว่าเป็นที่หลบ
            if (hit.collider.CompareTag("Hide"))
            {
                uiText.SetActive(true);

                // หา Outline จาก Object
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

                // ทดสอบว่าหลบได้
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("Player Hidden Successfully");
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