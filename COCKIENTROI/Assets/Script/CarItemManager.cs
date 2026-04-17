using UnityEngine;
using System.Collections;

public class CarItemManager : MonoBehaviour
{
    [Header("Cài đặt Cơ bản")]
    public bool isPlayer = false; 
    public GameObject bananaPrefab; 

    [Header("Cài đặt Hiệu ứng (VFX)")]
    public GameObject lightningVFX; 
    public GameObject hammerVFX;    
    public GameObject rainVFX;      

    // --- Thông tin nội bộ ---
    private int currentItem = 0; // 0: Trống, 1: Tăng tốc, 2: Chuối, 3: Sét, 4: Búa, 5: Lò xo, 6: Mưa
    private Rigidbody2D rb;

    private Controller playerController;
    private PCController pcController;
    private ControlSpeedAnim speedAnimController;
    private ControlSpeedAnimMobile mobileController;
    private AICarController aiController;
    
    // Biến lưu trữ trạng thái gốc để khôi phục
    private float originalMaxSpeed;
    private Vector3 originalScale; // Lưu kích thước gốc của xe

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody2D>();

        playerController = GetComponent<Controller>();
        pcController = GetComponent<PCController>();
        speedAnimController = GetComponent<ControlSpeedAnim>();
        mobileController = GetComponent<ControlSpeedAnimMobile>();
        aiController = GetComponent<AICarController>();

        // Lưu lại tốc độ ban đầu
        if (playerController != null) originalMaxSpeed = playerController.maxSpeed;
        if (pcController != null) originalMaxSpeed = pcController.maxSpeed;
        if (speedAnimController != null) originalMaxSpeed = speedAnimController.maxSpeed;
        if (mobileController != null) originalMaxSpeed = mobileController.maxSpeed;
        if (aiController != null) originalMaxSpeed = aiController.maxSpeed;

        // Lưu lại kích thước ban đầu của xe (để sửa lỗi teo nhỏ/phình to)
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Nếu là Người chơi và đang có vật phẩm -> Nhấn Space để sử dụng
        if (isPlayer && HasItem() && Input.GetKeyDown(KeyCode.Space))
        {
            UseItem();
        }
    }

    public bool HasItem()
    {
        return currentItem != 0;
    }

    // Hàm nhận vật phẩm (Gọi từ ItemBox)
    public void ReceiveItem(int itemId)
    {
        if (HasItem()) return; 

        currentItem = itemId;
        Debug.Log(">>> " + gameObject.name + " vừa nhặt được vật phẩm số: " + itemId);

        if (isPlayer)
        {
            // Tự động sử dụng vật phẩm ngay lập tức khi nhặt được
            UseItem();
        }
        else
        {
            StartCoroutine(AIUseItemRoutine());
        }
    }

    // Bộ não AI tự dùng đồ sau 1 đến 3 giây
    IEnumerator AIUseItemRoutine()
    {
        float randomDelay = Random.Range(1f, 3f);
        yield return new WaitForSeconds(randomDelay);
        
        if (HasItem()) 
        {
            UseItem();
        }
    }

    // Hàm thực thi Vật phẩm
    public void UseItem()
    {
        if (currentItem == 0) return;

        Debug.Log("=== " + gameObject.name + " ĐÃ SỬ DỤNG vật phẩm số: " + currentItem);

        switch (currentItem)
        {
            case 1: StartCoroutine(SpeedBoostEffect()); break; 
            case 2: DropBanana(); break;                       
            case 3: CastLightning(); break;                    
            case 4: StartCoroutine(HammerStrike()); break;     
            case 5: StartCoroutine(SpringJump()); break;       
            case 6: CastRain(); break;                         
        }

        // Dùng xong thì xóa đồ trên tay
        currentItem = 0; 
    }


    // ==========================================
    // CÁC CHIÊU THỨC VÀ HIỆU ỨNG (SKILLS)
    // ==========================================

    // --- 1. TĂNG TỐC ---
    IEnumerator SpeedBoostEffect()
    {
        ChangeMaxSpeed(5f); 
        if (rb != null) rb.AddForce(-transform.up * 15f, ForceMode2D.Impulse); 

        yield return new WaitForSeconds(2f); 

        ChangeMaxSpeed(0f); 
    }

    // --- 2. VỎ CHUỐI ---
    void DropBanana()
    {
        if (bananaPrefab != null)
        {
            Vector3 dropPos = transform.position + (transform.up * 1.5f);
            Instantiate(bananaPrefab, dropPos, transform.rotation);
        }
    }

    // --- 3. TIA SÉT ---
    void CastLightning()
    {
        CarItemManager[] allCars = FindObjectsOfType<CarItemManager>();
        foreach (CarItemManager car in allCars)
        {
            if (car != this)
            {
                car.StartCoroutine(car.ReceiveLightningShock());
            }
        }
    }

    public IEnumerator ReceiveLightningShock()
    {
        if (lightningVFX != null) 
            Instantiate(lightningVFX, transform.position + Vector3.up * 1.5f, Quaternion.identity, transform);

        DisableControls(); 
        
        if (rb != null) 
        {
            rb.linearVelocity = rb.linearVelocity * 0.2f; 
            rb.angularVelocity = 1000f; 
        }

        yield return new WaitForSeconds(1.5f); 

        if (rb != null) 
        {
            rb.angularVelocity = 0f; 
        }
        EnableControls(); 
    }

    // --- 4. BÚA ---
    IEnumerator HammerStrike()
    {
        if (hammerVFX != null) 
            Instantiate(hammerVFX, transform.position, Quaternion.identity, transform);

        // Phóng to dựa trên kích thước gốc
        transform.localScale = originalScale * 1.2f; 

        Collider2D[] hitCars = Physics2D.OverlapCircleAll(transform.position, 4f);
        foreach (Collider2D hit in hitCars)
        {
            if (hit.gameObject != gameObject) 
            {
                Rigidbody2D enemyRb = hit.GetComponent<Rigidbody2D>();
                CarItemManager enemyCar = hit.GetComponent<CarItemManager>();
                
                if (enemyRb != null && enemyCar != null)
                {
                    Vector2 pushDirection = (hit.transform.position - transform.position).normalized;
                    enemyRb.AddForce(pushDirection * 20f, ForceMode2D.Impulse);
                    enemyCar.StartCoroutine(enemyCar.ReceiveLightningShock()); 
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
        
        // Trả về đúng kích thước gốc
        transform.localScale = originalScale; 
    }

    // --- 5. LÒ XO ---
    IEnumerator SpringJump()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) col = GetComponentInParent<Collider2D>();
        
        if (col != null) col.enabled = false; 
        
        // Phóng to dựa trên kích thước gốc
        transform.localScale = originalScale * 1.5f;

        if (rb != null) rb.AddForce(-transform.up * 15f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(1.5f); 

        // Trả về đúng kích thước gốc
        transform.localScale = originalScale;
        
        if (col != null) col.enabled = true; 
    }

    // --- 6. MƯA ---
    void CastRain()
    {
        CarItemManager[] allCars = FindObjectsOfType<CarItemManager>();
        foreach (CarItemManager car in allCars)
        {
            if (car != this)
            {
                car.StartCoroutine(car.ReceiveRainSlippery());
            }
        }
    }

    public IEnumerator ReceiveRainSlippery()
    {
        if (rainVFX != null) 
            Instantiate(rainVFX, transform.position + Vector3.up * 2f, Quaternion.identity, transform);

        float oldSlide = 0.5f;
        if (playerController != null) { oldSlide = playerController.driftSlide; playerController.driftSlide = 0.1f; }
        if (pcController != null) { oldSlide = pcController.driftSlide; pcController.driftSlide = 0.1f; }
        if (speedAnimController != null) { oldSlide = speedAnimController.driftSlide; speedAnimController.driftSlide = 0.1f; }
        if (mobileController != null) { oldSlide = mobileController.driftSlide; mobileController.driftSlide = 0.1f; }
        if (aiController != null) { oldSlide = aiController.driftSlide; aiController.driftSlide = 0.1f; }

        yield return new WaitForSeconds(3f); 

        if (playerController != null) playerController.driftSlide = oldSlide;
        if (pcController != null) pcController.driftSlide = oldSlide;
        if (speedAnimController != null) speedAnimController.driftSlide = oldSlide;
        if (mobileController != null) mobileController.driftSlide = oldSlide;
        if (aiController != null) aiController.driftSlide = oldSlide;
    }


    // ==========================================
    // HÀM HỖ TRỢ CHUNG (TOOLS)
    // ==========================================

    void ChangeMaxSpeed(float amountAdded)
    {
        if (amountAdded == 0) 
        {
            if (playerController != null) playerController.maxSpeed = originalMaxSpeed;
            if (pcController != null) pcController.maxSpeed = originalMaxSpeed;
            if (speedAnimController != null) speedAnimController.maxSpeed = originalMaxSpeed;
            if (mobileController != null) mobileController.maxSpeed = originalMaxSpeed;
            if (aiController != null) aiController.maxSpeed = originalMaxSpeed;
        }
        else 
        {
            if (playerController != null) playerController.maxSpeed = originalMaxSpeed + amountAdded;
            if (pcController != null) pcController.maxSpeed = originalMaxSpeed + amountAdded;
            if (speedAnimController != null) speedAnimController.maxSpeed = originalMaxSpeed + amountAdded;
            if (mobileController != null) mobileController.maxSpeed = originalMaxSpeed + amountAdded;
            if (aiController != null) aiController.maxSpeed = originalMaxSpeed + amountAdded;
        }
    }

    void DisableControls()
    {
        if (playerController != null) playerController.enabled = false;
        if (pcController != null) pcController.enabled = false;
        if (speedAnimController != null) speedAnimController.enabled = false;
        if (mobileController != null) mobileController.enabled = false;
        if (aiController != null) aiController.enabled = false;
    }

    void EnableControls()
    {
        if (playerController != null) playerController.enabled = true;
        if (pcController != null) pcController.enabled = true;
        if (speedAnimController != null) speedAnimController.enabled = true;
        if (mobileController != null) mobileController.enabled = true;
        if (aiController != null) aiController.enabled = true;
    }
}