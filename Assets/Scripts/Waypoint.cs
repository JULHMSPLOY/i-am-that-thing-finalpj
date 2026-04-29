using UnityEngine;

public class Waypoint : MonoBehaviour 
{
    // รายการจุดที่จุดนี้สามารถเดินไปถึงได้โดยตรง
    public Waypoint[] edges; 

    // ช่วยให้มองเห็นเส้นเชื่อมต่อในหน้า Scene View
    void OnDrawGizmos() 
    {
        if (edges == null) return;
        
        Gizmos.color = Color.green; // สีของเส้นเชื่อม
        foreach (Waypoint neighbor in edges) 
        {
            if (neighbor != null) 
            {
                // วาดเส้นจากจุดนี้ไปยังจุดเพื่อนบ้าน
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}