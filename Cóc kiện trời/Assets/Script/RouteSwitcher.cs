using UnityEngine;

public class RouteSwitcher : MonoBehaviour
{
    [Header("Cấu hình rẽ")]
    public WaypointCircuit nextCircuit; // Đường tiếp theo sẽ chạy
    public int nextNodeIndex = 0;       // Sẽ chạy đến điểm số mấy của đường đó? (Thường là 0)
    
    [Range(0, 100)]
    public int switchProbability = 50;  // Tỷ lệ rẽ (%) - 50 là 50/50

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem cái gì vừa chạm vào? Có phải xe AI không?
        AICarController aiCar = other.GetComponent<AICarController>();

        if (aiCar != null)
        {
            // Tung xúc xắc (0 đến 100)
            int dice = Random.Range(0, 100);

            // Nếu con số nhỏ hơn tỷ lệ cài đặt -> Thực hiện chuyển đường
            if (dice < switchProbability)
            {
                aiCar.SwitchCircuit(nextCircuit, nextNodeIndex);
                Debug.Log(other.name + " đã đổi hướng!");
            }
        }
    }
}