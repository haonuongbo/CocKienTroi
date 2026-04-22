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
        if (isRespawning) return; 

        // Ngăn vũ khí/hiệu ứng vô tình ăn mất hộp
        if (other.gameObject.name.Contains("Hammer") || other.gameObject.name.Contains("VFX")) return;

        CarItemManager carItem = other.GetComponentInParent<CarItemManager>();
        
        if (carItem != null)
        {
            if (carItem.CanPickUpItem())
            {
                isRespawning = true; 
                carItem.ReceiveItem(currentItem); 
                StartCoroutine(RespawnRoutine());
            }
            else
            {
                Debug.Log($"[ItemBox] Xe {carItem.gameObject.name} chạm vào hộp nhưng không thể nhặt (Chưa hết 1.5s đầu game HOẶC xe đang có sẵn đồ trên tay chưa xài!)");
            }
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