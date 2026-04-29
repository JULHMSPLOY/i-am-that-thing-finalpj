using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // เป้าหมายที่กล้องจะตาม
    public Vector3 offset = new Vector3(0f, 15f, -10f); // ระยะห่างของกล้อง
    public float smoothSpeed = 5f; // ความสมูทในการตาม

    void LateUpdate()
    {
        if (target != null)
        {
            // คำนวณตำแหน่งเป้าหมาย + ระยะห่าง
            Vector3 desiredPosition = target.position + offset;

            // ใช้ Lerp เพื่อให้กล้องค่อยๆ เลื่อนตามอย่างนุ่มนวล
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}