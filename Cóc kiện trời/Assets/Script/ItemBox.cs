using UnityEngine;
using System.Collections;

public class ItemBox : MonoBehaviour
{
    public float respawnTime = 3f; // Thời gian hộp xuất hiện lại sau khi bị ăn
    private SpriteRenderer spriteRenderer;
    private Collider2D boxCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đối tượng chạm vào có script CarItemManager không
        CarItemManager carItem = other.GetComponent<CarItemManager>();
        
        if (carItem != null && !carItem.HasItem())
        {
            // Random từ 1 đến 2 (Tạm thời làm 2 món cơ bản: 1=Tăng tốc, 2=Chuối)
            // Sau này bạn có thể tăng số lượng lên để thêm Tia sét, Búa...
            int randomItem = Random.Range(1, 7); 
            
            carItem.ReceiveItem(randomItem); // Tặng vật phẩm cho xe

            // Ẩn hộp đi và bắt đầu đếm ngược để hiện lại
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        // Tắt hình ảnh và va chạm
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        // Bật lại
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
    }
}