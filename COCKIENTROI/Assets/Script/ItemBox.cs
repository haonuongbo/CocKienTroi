using UnityEngine;
using System.Collections;

/// <summary>
/// Gắn lên prefab ItemBox trong scene.
/// Khi xe chạm vào: random item → gọi CarItemManager.GiveItem() → ẩn hộp → respawn sau respawnTime.
/// Mini icon hiển thị đúng item đang chờ và reset mỗi lần respawn.
/// </summary>
public class ItemBox : MonoBehaviour
{
    [Header("Cài Đặt Cơ Bản")]
    public float respawnTime = 3f;

    [Header("Debug / Testing")]
    [Tooltip("Set to 1–6 to always give that item. Set to 0 for normal random behaviour.")]
    [Range(0, 6)]
    public int forceItemId = 0;

    [Header("Mini Icon (icon nhỏ nổi trên hộp)")]
    [Tooltip("Kéo 6 sprite icon vào đây theo thứ tự: 1=Tăng tốc, 2=Chuối, 3=Sét, 4=Búa, 5=Lò xo, 6=Mưa")]
    public Sprite[] itemIcons;          // 6 phần tử, index 0 → item 1
    public Vector3  iconOffset = new Vector3(0f, 1f, 0f);
    public float    iconScale  = 0.5f;

    // --- Nội bộ ---
    private int currentItemId;              // Item đang được lưu trong hộp này (1–6)
    private bool isRespawning = false;      // Đang hồi sinh → bỏ qua trigger

    private Collider2D      myCollider;
    private SpriteRenderer  myRenderer;
    private SpriteRenderer  iconRenderer;   // Renderer của mini icon con

    // ==========================================
    // KHỞI TẠO
    // ==========================================

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        myRenderer = GetComponent<SpriteRenderer>();

        // Tạo GameObject mini icon
        GameObject iconObj = new GameObject("MiniItemIcon");
        iconObj.transform.SetParent(transform);
        iconObj.transform.localPosition = iconOffset;
        iconObj.transform.localScale    = Vector3.one * iconScale;

        iconRenderer = iconObj.AddComponent<SpriteRenderer>();
        if (myRenderer != null)
        {
            iconRenderer.sortingLayerName = myRenderer.sortingLayerName;
            iconRenderer.sortingOrder     = myRenderer.sortingOrder + 1;
        }

        // Random item lần đầu và tắt collider cho đến khi race bắt đầu
        RollNewItem();
        SetColliderEnabled(false);
        StartCoroutine(WaitForRaceStart());
    }

    // Chờ GameManager.IsRacing = true rồi bật collider
    IEnumerator WaitForRaceStart()
    {
        float timeout = Time.realtimeSinceStartup + 60f;
        while (!GameManager.IsRacing && Time.realtimeSinceStartup < timeout)
            yield return new WaitForSecondsRealtime(0.1f);

        // Thêm 0.5s để xe rời vị trí xuất phát trước khi hộp active
        yield return new WaitForSecondsRealtime(0.5f);

        SetColliderEnabled(true);
        Debug.Log($"[ItemBox] '{gameObject.name}' sẵn sàng (item #{currentItemId})");
    }

    // ==========================================
    // KHI XE CHẠM VÀO
    // ==========================================

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isRespawning) return;

        // Tìm CarItemManager trên xe (thử cả GetComponentInParent phòng khi collider nằm ở child)
        CarItemManager car = other.GetComponent<CarItemManager>()
                          ?? other.GetComponentInParent<CarItemManager>();

        if (car == null) return;    // Không phải xe đua — bỏ qua

        Debug.Log($"[ItemBox] '{car.gameObject.name}' nhặt item #{currentItemId}!");

        // Kích hoạt item NGAY LẬP TỨC
        car.GiveItem(currentItemId);

        // Bắt đầu quá trình respawn
        isRespawning = true;
        StartCoroutine(RespawnRoutine());
    }

    // ==========================================
    // RESPAWN
    // ==========================================

    IEnumerator RespawnRoutine()
    {
        // Ẩn hộp ngay
        SetVisible(false);
        SetColliderEnabled(false);

        yield return new WaitForSeconds(respawnTime);

        // Random item MỚI cho lần nhặt tiếp theo
        RollNewItem();

        // Hiện lại hộp
        SetVisible(true);
        SetColliderEnabled(true);
        isRespawning = false;

        Debug.Log($"[ItemBox] '{gameObject.name}' hồi sinh với item #{currentItemId}");
    }

    // ==========================================
    // TIỆN ÍCH
    // ==========================================

    /// <summary>Random item 1–6 và cập nhật icon ngay lập tức.</summary>
    void RollNewItem()
    {
        // If a forced item is set (1–6), always use it — useful for testing.
        if (forceItemId >= 1 && forceItemId <= 6)
        {
            currentItemId = forceItemId;
        }
        else
        {
            // Giảm tỉ lệ xuất hiện của Sét (item 3) xuống còn khoảng 1/10 (10%)
            int roll = Random.Range(1, 101); // 1 đến 100
            if (roll <= 10)
            {
                currentItemId = 3; // Sét (10%)
            }
            else
            {
                // 5 item còn lại chia đều 90% còn lại
                int[] otherItems = { 1, 2, 4, 5, 6 };
                currentItemId = otherItems[Random.Range(0, otherItems.Length)];
            }
        }

        // Cập nhật mini icon — index 0 = item 1, nên trừ 1
        if (iconRenderer != null && itemIcons != null && itemIcons.Length >= 6)
            iconRenderer.sprite = itemIcons[currentItemId - 1];
    }

    void SetColliderEnabled(bool value)
    {
        if (myCollider != null) myCollider.enabled = value;
    }

    void SetVisible(bool value)
    {
        if (myRenderer  != null) myRenderer.enabled  = value;
        if (iconRenderer != null) iconRenderer.enabled = value;
    }
}
