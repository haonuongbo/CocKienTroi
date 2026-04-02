using UnityEngine;
using System.Collections.Generic;

public class WaypointCircuit : MonoBehaviour
{
    [Header("Cấu hình")]
    public Color lineColor = Color.yellow; // Màu vàng để vẽ đường nối các điểm
    public List<Transform> waypoints = new List<Transform>(); // Danh sách chứa các điểm mốc

    // Hàm này giúp vẽ đường nối các điểm trong màn hình Scene để bạn dễ nhìn
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
                Gizmos.DrawLine(current.position, next.position); // Vẽ đường thẳng
                Gizmos.DrawSphere(current.position, 0.5f); // Vẽ cục tròn tại vị trí mốc
            }
        }
    }
}