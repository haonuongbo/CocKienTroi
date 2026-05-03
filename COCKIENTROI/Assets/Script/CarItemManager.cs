using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gắn lên từng xe đua (Player + AI).
/// Nhận item từ ItemBox và kích hoạt hiệu ứng ngay lập tức.
/// Sử dụng ICarController để tương thích với mọi loại script điều khiển.
/// </summary>
public class CarItemManager : MonoBehaviour
{
    [Header("VFX Prefabs")]
    public GameObject bananaPrefab;
    public GameObject lightningVFX;
    public GameObject hammerVFX;
    public GameObject rainVFX;

    // --- Dữ liệu nội bộ ---
    private Rigidbody2D rb;
    private List<ICarController> controllers = new List<ICarController>();

    // Giá trị gốc — cache lại một lần trong Start()
    private float originalMaxSpeed;
    private float originalDriftSlide;
    private Vector3 originalScale;

    // Bộ đếm để hỗ trợ nhiều effect xếp chồng (stacking)
    private int stunCount = 0;
    private int slipCount = 0;

    // Lưu controller đang active trước khi bị stun
    private List<ICarController> activeControllersBeforeStun = new List<ICarController>();

    // ==========================================
    // KHỞI TẠO
    // ==========================================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody2D>();

        // Thu thập ICarController — dùng HashSet để tránh duplicate
        // (GetComponentsInChildren bao gồm cả root nên sẽ trùng với GetComponents)
        var seen = new HashSet<ICarController>();
        foreach (var c in GetComponents<ICarController>())          if (seen.Add(c)) controllers.Add(c);
        foreach (var c in GetComponentsInChildren<ICarController>()) if (seen.Add(c)) controllers.Add(c);

        if (controllers.Count == 0)
            Debug.LogWarning($"[CarItemManager] '{gameObject.name}' không tìm thấy ICarController nào!");

        // Cache giá trị gốc từ controller đầu tiên tìm được
        if (controllers.Count > 0)
        {
            originalMaxSpeed   = controllers[0].MaxSpeed;
            originalDriftSlide = controllers[0].DriftSlide;
        }

        originalScale = transform.localScale;
    }

    // ==========================================
    // ĐIỂM VÀO DUY NHẤT — GọI TỪ ItemBox
    // ==========================================

    /// <summary>Kích hoạt item ngay lập tức. itemId: 1-6.</summary>
    public void GiveItem(int itemId)
    {
        Debug.Log($"[CarItemManager] '{gameObject.name}' nhận item #{itemId}");
        switch (itemId)
        {
            case 1: StartCoroutine(Effect_SpeedBoost());   break;
            case 2: Effect_DropBanana();                   break;
            case 3: Effect_Lightning();                    break;
            case 4: StartCoroutine(Effect_Hammer());       break;
            case 5: StartCoroutine(Effect_SpringJump());   break;
            case 6: Effect_Rain();                         break;
            default: Debug.LogWarning($"[CarItemManager] Item #{itemId} không hợp lệ!"); break;
        }
    }

    // ==========================================
    // ITEM 1 — TĂNG TỐC (2 giây)
    // ==========================================

    IEnumerator Effect_SpeedBoost()
    {
        SetAllMaxSpeed(originalMaxSpeed * 2f);
        yield return new WaitForSeconds(2f);
        SetAllMaxSpeed(originalMaxSpeed);
    }

    // ==========================================
    // ITEM 2 — VỎ CHUỐI
    // ==========================================

    void Effect_DropBanana()
    {
        if (bananaPrefab == null) { Debug.LogWarning("[CarItemManager] Chưa gán bananaPrefab!"); return; }

        // Thả ra phía sau xe (transform.up là hướng tiến, -up là phía sau)
        Vector3 dropPos = transform.position + (transform.up * 1.5f);
        GameObject banana = Instantiate(bananaPrefab, dropPos, transform.rotation);
        IgnoreCollisionWith(banana);
    }

    // ==========================================
    // ITEM 3 — TIA SÉT (làm tê liệt tất cả xe khác 1.5s)
    // ==========================================

    void Effect_Lightning()
    {
        CarItemManager[] all = FindObjectsOfType<CarItemManager>();
        foreach (var car in all)
        {
            if (car != this)
                car.StartCoroutine(car.ReceiveStun(lightningVFX, 1.5f, 0.3f, 200f));
        }
    }

    // ==========================================
    // ITEM 4 — BÚA XOAY (5 giây, bán kính 8f)
    // ==========================================

    IEnumerator Effect_Hammer()
    {
        float duration    = 5f;
        float rotSpeed    = 360f;
        float radius      = 8f;
        float hitCooldown = 1.5f;

        // Tạo trục xoay
        GameObject pivot = new GameObject("HammerPivot");
        pivot.transform.SetParent(transform);
        pivot.transform.localPosition = Vector3.zero;

        if (hammerVFX != null)
        {
            GameObject h = Instantiate(hammerVFX, pivot.transform);
            h.transform.localPosition = new Vector3(2f, 0f, 0f);
            IgnoreCollisionWith(h);
        }

        Dictionary<CarItemManager, float> lastHit = new Dictionary<CarItemManager, float>();
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (pivot != null) pivot.transform.Rotate(0f, 0f, rotSpeed * Time.deltaTime);

            // Kiểm tra xe lân cận
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                CarItemManager enemy = hit.GetComponent<CarItemManager>()
                                    ?? hit.GetComponentInParent<CarItemManager>();

                // Bỏ qua nếu là chính xe mình (kể cả child collider)
                if (enemy == null || enemy == this) continue;

                // Cooldown mỗi xe
                if (lastHit.TryGetValue(enemy, out float lastTime) && Time.time - lastTime < hitCooldown)
                    continue;

                lastHit[enemy] = Time.time;

                // Chỉ đẩy xe địch ra xa — không stun, không hiệu ứng sét
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>()
                                   ?? enemy.GetComponentInParent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 push = (enemy.transform.position - transform.position).normalized;
                    enemyRb.AddForce(push * 15f, ForceMode2D.Impulse); // Lực vừa đủ văng xa
                }
            }

            yield return null;
        }

        if (pivot != null) Destroy(pivot);
    }

    // ==========================================
    // ITEM 5 — LÒ XO / PHÓNG TO (1.5 giây)
    // ==========================================

    IEnumerator Effect_SpringJump()
    {
        // Tắt collider để tránh kẹt tường khi phóng to
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach (var c in cols) if (c != null) c.enabled = false;

        transform.localScale = originalScale * 1.5f;

        yield return new WaitForSeconds(1.5f);

        transform.localScale = originalScale;
        foreach (var c in cols) if (c != null) c.enabled = true;
    }

    // ==========================================
    // ITEM 6 — MƯA (làm trơn tất cả xe khác 3s)
    // ==========================================

    void Effect_Rain()
    {
        CarItemManager[] all = FindObjectsOfType<CarItemManager>();
        foreach (var car in all)
        {
            if (car != this)
                car.StartCoroutine(car.ReceiveSlip(rainVFX, 3f));
        }
    }

    // ==========================================
    // HIỆU ỨNG NHẬN (PUBLIC — xe khác gọi vào)
    // ==========================================

    /// <summary>Tê liệt xe: giảm tốc + spin + tắt điều khiển trong <duration> giây.</summary>
    public IEnumerator ReceiveStun(GameObject vfxPrefab, float duration, float speedMult, float spin)
    {
        stunCount++;

        // Spawn VFX
        GameObject vfxObj = null;
        if (vfxPrefab != null)
        {
            vfxObj = Instantiate(vfxPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity, transform);
            IgnoreCollisionWith(vfxObj);
        }

        // Tắt điều khiển và áp lực vật lý
        SetAllControlEnabled(false);
        if (rb != null)
        {
            rb.linearVelocity *= speedMult;
            rb.angularVelocity = spin;
        }

        yield return new WaitForSeconds(duration);

        if (rb != null) rb.angularVelocity = 0f;

        stunCount--;
        if (stunCount <= 0)
        {
            stunCount = 0;
            SetAllControlEnabled(true);
        }

        if (vfxObj != null) Destroy(vfxObj);
    }

    /// <summary>Làm xe trơn (giảm driftSlide) trong <duration> giây.</summary>
    public IEnumerator ReceiveSlip(GameObject vfxPrefab, float duration)
    {
        slipCount++;

        // Spawn VFX
        GameObject vfxObj = null;
        if (vfxPrefab != null)
        {
            vfxObj = Instantiate(vfxPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
            IgnoreCollisionWith(vfxObj);
        }

        // Áp dụng slip
        SetAllDriftSlide(0.05f);

        yield return new WaitForSeconds(duration);

        slipCount--;
        if (slipCount <= 0)
        {
            slipCount = 0;
            SetAllDriftSlide(originalDriftSlide);
        }

        if (vfxObj != null) Destroy(vfxObj);
    }

    // ==========================================
    // TIỆN ÍCH NỘI BỘ
    // ==========================================

    void SetAllMaxSpeed(float speed)
    {
        foreach (var c in controllers) c.MaxSpeed = speed;
    }

    void SetAllDriftSlide(float slide)
    {
        foreach (var c in controllers) c.DriftSlide = slide;
    }

    void SetAllControlEnabled(bool enabled)
    {
        if (!enabled)
        {
            if (stunCount == 1) // Lần đầu tiên bị stun
            {
                activeControllersBeforeStun.Clear();
                foreach (var c in controllers)
                {
                    if (c is MonoBehaviour mb && mb.enabled)
                    {
                        activeControllersBeforeStun.Add(c);
                        c.SetControlEnabled(false);
                    }
                }
            }
        }
        else
        {
            if (stunCount == 0) // Hết stun
            {
                foreach (var c in activeControllersBeforeStun)
                {
                    c.SetControlEnabled(true);
                }
                activeControllersBeforeStun.Clear();
            }
        }
    }

    /// <summary>Tắt va chạm giữa VFX vừa tạo và xe này — tránh xe bị văng.</summary>
    void IgnoreCollisionWith(GameObject vfx)
    {
        if (vfx == null) return;
        Collider2D[] carCols = GetComponentsInChildren<Collider2D>();
        Collider2D[] vfxCols = vfx.GetComponentsInChildren<Collider2D>();
        foreach (var cc in carCols)
            foreach (var vc in vfxCols)
                if (cc != null && vc != null) Physics2D.IgnoreCollision(cc, vc);
    }
}
