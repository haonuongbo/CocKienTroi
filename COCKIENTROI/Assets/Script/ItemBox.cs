using UnityEngine;
using System.Collections;

public class ItemBox : MonoBehaviour
{
    public float respawnTime = 3f; // Thời gian hộp xuất hiện lại sau khi bị ăn

    [Header("Mini Icon Settings")]
    public Sprite[] itemIcons; // Kéo thả 6 icon vào đây: 0=Tăng tốc, 1=Chuối, 2=Sét, 3=Búa, 4=Lò xo, 5=Mưa
    public Vector3 iconOffset = new Vector3(0, 1f, 0); // Vị trí icon lơ lửng trên hộp
    public float iconScale = 0.5f; // Kích thước của mini icon
    
    private int currentItem;
    private SpriteRenderer iconSpriteRenderer;
    private bool isRespawning = false;

    void Start()
    {
        // Tạo một GameObject con để hiển thị mini icon
        GameObject iconObj = new GameObject("MiniItemIcon");
        iconObj.transform.SetParent(this.transform);
        iconObj.transform.localPosition = iconOffset;
        iconObj.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
        
        iconSpriteRenderer = iconObj.AddComponent<SpriteRenderer>();
        SpriteRenderer mainSr = GetComponent<SpriteRenderer>();
        if (mainSr != null)
        {
            iconSpriteRenderer.sortingLayerName = mainSr.sortingLayerName;
            iconSpriteRenderer.sortingOrder = mainSr.sortingLayerName == "Default" ? mainSr.sortingOrder + 1 : 5;
        }

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
        if (isRespawning) return; // Đã bị ăn rồi thì bỏ qua

        // CHỈ cho phép va chạm với thân xe thật (Solid Collider), bỏ qua các vùng quét (Radar) hoặc vũ khí (Trigger)
        if (other.isTrigger) return;

        // Chỉ lấy CarItemManager ở đúng object đang va chạm (tránh việc vũ khí con cái va chạm lại tính cho xe mẹ)
        CarItemManager carItem = other.GetComponent<CarItemManager>();
        
        if (carItem != null && !carItem.HasItem())
        {
            isRespawning = true; // KHÓA NGAY LẬP TỨC TRONG CÙNG FRAME để tránh lỗi
            carItem.ReceiveItem(currentItem); 
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        // Tắt toàn bộ MeshRenderer hoặc SpriteRenderer
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) if (r != null) r.enabled = false;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders) if (c != null) c.enabled = false;

        yield return new WaitForSeconds(respawnTime); // Đợi 3s

        GenerateRandomItem();

        // Bật lại
        foreach (var r in renderers) if (r != null) r.enabled = true;
        foreach (var c in colliders) if (c != null) c.enabled = true;
        
        isRespawning = false; 
    }
}