using UnityEngine;
using System.Collections;

public class ItemBox : MonoBehaviour
{
    public float respawnTime = 3f; // Thời gian hộp xuất hiện lại sau khi bị ăn
    private SpriteRenderer spriteRenderer;
    private Collider2D boxCollider;

    [Header("Mini Icon Settings")]
    public Sprite[] itemIcons; // Kéo thả 6 icon vào đây: 0=Tăng tốc, 1=Chuối, 2=Sét, 3=Búa, 4=Lò xo, 5=Mưa
    public Vector3 iconOffset = new Vector3(0, 1f, 0); // Vị trí icon lơ lửng trên hộp
    public float iconScale = 0.5f; // Kích thước của mini icon
    
    private int currentItem;
    private SpriteRenderer iconSpriteRenderer;
    private bool isRespawning = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();

        // Tạo một GameObject con để hiển thị mini icon
        GameObject iconObj = new GameObject("MiniItemIcon");
        iconObj.transform.SetParent(this.transform);
        iconObj.transform.localPosition = iconOffset;
        iconObj.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
        
        iconSpriteRenderer = iconObj.AddComponent<SpriteRenderer>();
        iconSpriteRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        iconSpriteRenderer.sortingOrder = spriteRenderer.sortingLayerName == "Default" ? spriteRenderer.sortingOrder + 1 : 5;

        // Random item đầu tiên khi game bắt đầu
        GenerateRandomItem();
    }

    void GenerateRandomItem()
    {
        // 1=Tăng tốc, 2=Chuối, 3=Sét, 4=Búa, 5=Lò xo, 6=Mưa
        currentItem = Random.Range(1, 7); 
        
        // Hiển thị icon tương ứng nếu mảng itemIcons đã được gán (mảng tính từ 0)
        if (itemIcons != null && itemIcons.Length >= 6)
        {
            iconSpriteRenderer.sprite = itemIcons[currentItem - 1];
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isRespawning) return; // Nếu hộp đang ẩn thì không cho ăn nữa để tránh lỗi nhiều xe ăn cùng lúc

        // Kiểm tra xem đối tượng chạm vào có script CarItemManager không
        CarItemManager carItem = other.GetComponentInParent<CarItemManager>();
        
        if (carItem != null && !carItem.HasItem())
        {
            carItem.ReceiveItem(currentItem); // Tặng vật phẩm cho xe

            // Ẩn hộp đi và bắt đầu đếm ngược để hiện lại
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true; // Đánh dấu là đang ẩn

        // Tắt hình ảnh và va chạm
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;
        if (iconSpriteRenderer != null) iconSpriteRenderer.enabled = false;

        yield return new WaitForSeconds(respawnTime); // Đợi 3s

        // Tạo item mới ngẫu nhiên
        GenerateRandomItem();

        // Bật lại
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
        if (iconSpriteRenderer != null) iconSpriteRenderer.enabled = true;
        
        isRespawning = false; // Đánh dấu là đã hiện lại
    }
}