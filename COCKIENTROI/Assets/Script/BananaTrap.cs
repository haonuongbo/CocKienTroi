using UnityEngine;
using System.Collections;

/// <summary>
/// Gắn lên prefab Banana_Trap.
/// Khi xe chạm → tìm CarItemManager → gọi ReceiveSlip() → tự hủy.
/// </summary>
public class BananaTrap : MonoBehaviour
{
    [Tooltip("Thời gian xe bị trơn khi dẫm lên vỏ chuối (giây)")]
    public float slipDuration = 1.5f;

    private bool triggered = false; // Tránh trigger 2 lần nếu 2 collider chồng nhau

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        // Tìm CarItemManager — hỗ trợ cả trường hợp collider ở child object
        CarItemManager car = other.GetComponent<CarItemManager>()
                          ?? other.GetComponentInParent<CarItemManager>();

        if (car == null) return; // Không phải xe đua

        triggered = true;
        Debug.Log($"[BananaTrap] '{car.gameObject.name}' dẫm trúng vỏ chuối!");

        // Ẩn ngay để xe khác không dẫm tiếp
        GetComponent<SpriteRenderer>().enabled  = false;
        GetComponent<Collider2D>().enabled       = false;

        // Áp dụng hiệu ứng trượt (không cần VFX prefab)
        car.StartCoroutine(car.ReceiveSlip(null, slipDuration));

        // Hủy object sau khi effect bắt đầu
        Destroy(gameObject, slipDuration + 0.1f);
    }
}
