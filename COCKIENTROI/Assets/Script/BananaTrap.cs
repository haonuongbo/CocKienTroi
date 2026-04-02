using UnityEngine;
using System.Collections;

public class BananaTrap : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Controller player = other.GetComponent<Controller>();
        AICarController ai = other.GetComponent<AICarController>();
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        // Chỉ kích hoạt nếu thứ đạp trúng là xe đua
        if (player != null || ai != null)
        {
            StartCoroutine(ApplySlipEffect(player, ai, rb));
        }
    }

    IEnumerator ApplySlipEffect(Controller player, AICarController ai, Rigidbody2D rb)
    {
        // 1. Ẩn vỏ chuối ngay lập tức để xe khác không dẫm đè lên
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // 2. Tắt script điều khiển (Làm xe mất lái)
        if (player != null) player.enabled = false;
        if (ai != null) ai.enabled = false;

        // 3. Hiệu ứng trượt: Giảm mạnh vận tốc và ép xe xoay mòng mòng
        if (rb != null)
        {
            rb.linearVelocity = rb.linearVelocity * 0.4f; // Trôi chậm lại
            rb.angularVelocity = 720f; // Xoay 2 vòng mỗi giây (720 độ)
        }

        // 4. Chờ 1.5 giây
        yield return new WaitForSeconds(1.5f);

        // 5. Hết thời gian trượt: Khôi phục lại trạng thái
        if (rb != null) rb.angularVelocity = 0f; // Dừng xoay
        
        // Bật lại điều khiển
        if (player != null) player.enabled = true;
        if (ai != null) ai.enabled = true;

        // Xóa vỏ chuối
        Destroy(gameObject);
    }
}