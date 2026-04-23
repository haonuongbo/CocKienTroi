using UnityEngine;
using TMPro;

public class SpeedometerNeedleUI : MonoBehaviour
{
    private TMP_Text speedTextComponent;
    [Header("UI")]
    [SerializeField] private RectTransform needle;
    [SerializeField] private GameObject speedNumberObject;
    [SerializeField] private GameObject speedUnitObject;

    [Header("Needle Rotation")]
    [SerializeField] private float minNeedleAngle = -79f; // slow tilt (limited)
    [SerializeField] private float maxNeedleAngle = 70f;  // fast tilt (limited)
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Speed Source")]
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private float maxSpeedForNeedle = 10f;

    private float baseNeedleAngle;
    private float currentAngle;

    private void Awake()
    {
        AutoBindReferences();
        // HideSpeedText();
    }

    private void Start()
    {
        TryFindPlayerRigidbody();
        if (needle != null)
        {
            baseNeedleAngle = NormalizeSignedAngle(needle.localEulerAngles.z);
            currentAngle = baseNeedleAngle;
        }

        if (speedNumberObject != null)
        {
            speedTextComponent = speedNumberObject.GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        if (needle == null)
        {
            return;
        }

        if (playerRigidbody == null)
        {
            TryFindPlayerRigidbody();
            if (playerRigidbody == null) return;
        }

        float speed = playerRigidbody.linearVelocity.magnitude;
        float normalized = Mathf.Clamp01(speed / Mathf.Max(0.01f, maxSpeedForNeedle));
        // Fixed mapping (inverted to match flipped gauge): slow -> red side, fast -> green side.
        float targetTilt = Mathf.Lerp(maxNeedleAngle, minNeedleAngle, normalized);
        float targetAngle = baseNeedleAngle + targetTilt;
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);
        needle.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

        // --- Cập nhật con số Tốc độ hiển thị ---
        if (speedTextComponent != null)
        {
            // Tốc độ ảo (tối đa hiển thị là 120km/h) dựa trên tỉ lệ chuẩn normalized
            // Xe đứng yên = 0.0 , Xe max speed = 120.0
            float displayedSpeed = normalized * 120f;
            speedTextComponent.text = displayedSpeed.ToString("F1");
        }
    }

    private void AutoBindReferences()
    {
        if (needle == null)
        {
            Transform needleTf = transform.Find("Needle");
            if (needleTf != null) needle = needleTf as RectTransform;
        }

        if (speedNumberObject == null)
        {
            Transform t = transform.Find("Txt_Speed");
            if (t != null) speedNumberObject = t.gameObject;
        }

        if (speedUnitObject == null)
        {
            Transform t = transform.Find("Txt_SpeedUnit");
            if (t != null) speedUnitObject = t.gameObject;
        }
    }

    private void HideSpeedText()
    {
        if (speedNumberObject != null) speedNumberObject.SetActive(false);
        if (speedUnitObject != null) speedUnitObject.SetActive(false);
    }

    private void TryFindPlayerRigidbody()
    {
        if (playerRigidbody != null) return;

        ControlSpeedAnim playerControl = FindFirstObjectByType<ControlSpeedAnim>();
        if (playerControl != null)
        {
            playerRigidbody = playerControl.GetComponent<Rigidbody2D>();
            if (maxSpeedForNeedle <= 0.01f) maxSpeedForNeedle = playerControl.maxSpeed;
            return;
        }

        PCController pcController = FindFirstObjectByType<PCController>();
        if (pcController != null)
        {
            playerRigidbody = pcController.GetComponent<Rigidbody2D>();
        }
    }

    private float NormalizeSignedAngle(float angle)
    {
        return Mathf.DeltaAngle(0f, angle);
    }
}
