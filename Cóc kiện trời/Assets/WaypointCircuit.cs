using UnityEngine;
using System.Collections.Generic;

public class WaypointCircuit : MonoBehaviour
{
    [Header("Cấu hình")]
    public Color lineColor = Color.yellow; // Màu vàng cho hợp với đường đất
    public List<Transform> waypoints = new List<Transform>();

    // Vẽ đường nối các điểm trong Scene để dễ nhìn
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = lineColor;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Transform current = waypoints[i];
            // Nối điểm hiện tại với điểm tiếp theo (hoặc vòng về điểm đầu nếu là điểm cuối)
            Transform next = waypoints[(i + 1) % waypoints.Count];

            if (current != null && next != null)
            {
                Gizmos.DrawLine(current.position, next.position);
                Gizmos.DrawSphere(current.position, 0.5f); // Vẽ điểm tròn tại vị trí mốc
            }
        }
    }
    
    // Hàm tiện ích để lấy điểm tiếp theo
    public Transform GetNextWaypoint(int currentIndex)
    {
        if (waypoints.Count == 0) return null;
        return waypoints[(currentIndex + 1) % waypoints.Count];
    }
}