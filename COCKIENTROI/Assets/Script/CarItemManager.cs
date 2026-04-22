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

    private int slipperyCount = 0;
    private float origPlayerSlide;
    private float origPcSlide;
    private float origSpeedAnimSlide;
    private float origMobileSlide;
    private float origAiSlide;

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

        // Lưu lại kích thước ban đầu của xe
        originalScale = transform.localScale;

        // Lưu giá trị driftSlide gốc
        if (playerController != null) origPlayerSlide = playerController.driftSlide;
        if (pcController != null) origPcSlide = pcController.driftSlide;
        if (speedAnimController != null) origSpeedAnimSlide = speedAnimController.driftSlide;
        if (mobileController != null) origMobileSlide = mobileController.driftSlide;
        if (aiController != null) origAiSlide = aiController.driftSlide;
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

    // --- 4. BÚA (Búa xoay xung quanh xe) ---
    IEnumerator HammerStrike()
    {
        float hammerDuration = 5f; // Thời gian búa xoay
        float rotationSpeed = 360f; // Tốc độ xoay
        
        // Tạo trục xoay
        GameObject hammerPivot = new GameObject("HammerPivot");
        hammerPivot.transform.SetParent(this.transform);
        hammerPivot.transform.localPosition = Vector3.zero;

        // Nếu có prefab hammerVFX thì tạo ra, đặt cách xe một khoảng
        if (hammerVFX != null) 
        {
            GameObject activeHammer = Instantiate(hammerVFX, hammerPivot.transform);
            activeHammer.transform.localPosition = new Vector3(2f, 0, 0); // Khoảng cách búa xoay quanh xe
        }

        float timer = 0f;
        while (timer < hammerDuration)
        {
            timer += Time.deltaTime;
            
            // Xoay búa
            if (hammerPivot != null)
            {
                hammerPivot.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            }

            // Kiểm tra va chạm liên tục trong lúc xoay
            Collider2D[] hitCars = Physics2D.OverlapCircleAll(transform.position, 3f);
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
                        
                        // Cho kẻ địch dính sát thương
                        enemyCar.StartCoroutine(enemyCar.ReceiveLightningShock()); 
                    }
                }
            }
            yield return null;
        }

        // Xóa búa xoay
        if (hammerPivot != null)
        {
            Destroy(hammerPivot);
        }
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

        slipperyCount++;
        
        if (playerController != null) playerController.driftSlide = 0.1f;
        if (pcController != null) pcController.driftSlide = 0.1f;
        if (speedAnimController != null) speedAnimController.driftSlide = 0.1f;
        if (mobileController != null) mobileController.driftSlide = 0.1f;
        if (aiController != null) aiController.driftSlide = 0.1f;

        yield return new WaitForSeconds(3f); 

        slipperyCount--;
        if (slipperyCount <= 0)
        {
            slipperyCount = 0;
            if (playerController != null) playerController.driftSlide = origPlayerSlide;
            if (pcController != null) pcController.driftSlide = origPcSlide;
            if (speedAnimController != null) speedAnimController.driftSlide = origSpeedAnimSlide;
            if (mobileController != null) mobileController.driftSlide = origMobileSlide;
            if (aiController != null) aiController.driftSlide = origAiSlide;
        }
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

    private int disableCount = 0;
    private bool wasPlayerControllerEnabled;
    private bool wasPcControllerEnabled;
    private bool wasSpeedAnimControllerEnabled;
    private bool wasMobileControllerEnabled;
    private bool wasAiControllerEnabled;

    void DisableControls()
    {
        if (disableCount == 0)
        {
            // Lưu trạng thái trước khi tắt
            if (playerController != null) wasPlayerControllerEnabled = playerController.enabled;
            if (pcController != null) wasPcControllerEnabled = pcController.enabled;
            if (speedAnimController != null) wasSpeedAnimControllerEnabled = speedAnimController.enabled;
            if (mobileController != null) wasMobileControllerEnabled = mobileController.enabled;
            if (aiController != null) wasAiControllerEnabled = aiController.enabled;

            // Tắt script
            if (playerController != null) playerController.enabled = false;
            if (pcController != null) pcController.enabled = false;
            if (speedAnimController != null) speedAnimController.enabled = false;
            if (mobileController != null) mobileController.enabled = false;
            if (aiController != null) aiController.enabled = false;
        }
        disableCount++;
    }

    void EnableControls()
    {
        disableCount--;
        if (disableCount <= 0)
        {
            disableCount = 0;
            // Phục hồi trạng thái cũ thay vì bật mù quáng
            if (playerController != null) playerController.enabled = wasPlayerControllerEnabled;
            if (pcController != null) pcController.enabled = wasPcControllerEnabled;
            if (speedAnimController != null) speedAnimController.enabled = wasSpeedAnimControllerEnabled;
            if (mobileController != null) mobileController.enabled = wasMobileControllerEnabled;
            if (aiController != null) aiController.enabled = wasAiControllerEnabled;
        }
    }
}