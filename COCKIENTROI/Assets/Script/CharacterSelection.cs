using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelection : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterData
    {
        public string name;
        public Sprite iconImage;
        public Sprite tabImage;
        public Sprite previewImage;

        // --- MỚI: Thêm biến chứa ảnh bóng cho từng nhân vật ---
        public Sprite shadowImage;
        // -----------------------------------------------------

        [Header("Chỉ số")]
        public Sprite speedStat;
        public Sprite driftStat;
        public Sprite accelStat;

        [TextArea(3, 6)]
        public string description;
    }

    public CharacterData[] characterList;

    [Header("UI Cần Gán")]
    public Image centerPreview; // Ảnh nhân vật

    // --- MỚI: Thêm biến để gán UI Shadow từ Hierarchy ---
    public Image shadowPreview;
    // ----------------------------------------------------

    [Header("Tùy chỉnh vị trí")]
    public float normalPreviewOffsetY = 0;
    public float loweredPreviewOffsetY = -150; // Dịch xuống cho Cọp và Ong
    public int[] lowerCharacterIndices; // Indices của nhân vật cần dịch xuống (ví dụ: Cọp=0, Ong=1)
    
    // --- MỚI: Gán FloatingChar script vào đây ---
    public FloatingChar floatingCharScript;
    // ----------------------------------------

    public Image statSpeed;
    public Image statDrift;
    public Image statAccel;
    public TMP_Text descriptionText;
    public Image[] leftButtons;

    [Header("Top Right UI")]
    public Text moneyText;
    public Text energyText;

    [Header("Audio")]
    public AudioClip selectCharacterSound;
    private AudioSource audioSource;

    private int selectedIndex = 0;

    void Start()
    {
        selectedIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        UpdateUI();
        UpdateCurrencyUI();
    }

    void UpdateCurrencyUI()
    {
        int currentMoney = PlayerPrefs.GetInt("TotalMoney", 1000);
        int currentEnergy = PlayerPrefs.GetInt("TotalEnergy", 1987);

        if (moneyText) moneyText.text = currentMoney.ToString();
        if (energyText) energyText.text = currentEnergy.ToString();
    }

    public void SelectCharacter(int index)
    {
        selectedIndex = index;
        if (audioSource != null && selectCharacterSound != null)
        {
            audioSource.PlayOneShot(selectCharacterSound);
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        CharacterData data = characterList[selectedIndex];

        // Cập nhật ảnh nhân vật
        if (centerPreview)
        {
            centerPreview.sprite = data.previewImage;
            centerPreview.SetNativeSize();

            // --- Dịch chuyển vị trí xuống cho Cọp và Ong dựa trên selectedIndex ---
            RectTransform rectTransform = centerPreview.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 pos = rectTransform.anchoredPosition;
                
                // Kiểm tra xem selectedIndex có nằm trong danh sách nhân vật cần dịch xuống không
                bool isLoweredCharacter = false;
                if (lowerCharacterIndices != null)
                {
                    foreach (int index in lowerCharacterIndices)
                    {
                        if (selectedIndex == index)
                        {
                            isLoweredCharacter = true;
                            break;
                        }
                    }
                }
                
                if (isLoweredCharacter)
                {
                    pos.y = loweredPreviewOffsetY;
                }
                else
                {
                    pos.y = normalPreviewOffsetY;
                }
                
                rectTransform.anchoredPosition = pos;
                Debug.Log($"Character {selectedIndex}: isLowered={isLoweredCharacter}, pos.y={pos.y}");
                
                // --- Enable FloatingChar và reset startPos để nó nhún từ vị trí mới ---
                if (floatingCharScript != null)
                {
                    floatingCharScript.enabled = true;
                    floatingCharScript.ResetStartPosition();
                }
                // ---------------------------------------------------------------
            }
            // --------------------------------------------------
        }

        // --- MỚI: Cập nhật ảnh bóng ---
        if (shadowPreview)
        {
            if (data.shadowImage != null)
            {
                shadowPreview.sprite = data.shadowImage;
                shadowPreview.gameObject.SetActive(true); // Hiện bóng
                shadowPreview.SetNativeSize(); // Co giãn đúng tỷ lệ ảnh gốc
            }
            else
            {
                // Nếu nhân vật này không có file bóng thì ẩn đi để tránh lỗi hiển thị
                shadowPreview.gameObject.SetActive(false);
            }
        }
        // ------------------------------

        if (statSpeed) statSpeed.sprite = data.speedStat;
        if (statDrift) statDrift.sprite = data.driftStat;
        if (statAccel) statAccel.sprite = data.accelStat;

        if (descriptionText) descriptionText.text = data.description;

        // Xử lý logic sắp xếp nút bên trái (như cũ)
        for (int i = 0; i < leftButtons.Length; i++)
        {
            leftButtons[i].transform.SetSiblingIndex(i);
        }

        leftButtons[selectedIndex].transform.SetAsFirstSibling();

        for (int i = 0; i < leftButtons.Length; i++)
        {
            if (i == selectedIndex)
            {
                leftButtons[i].sprite = characterList[i].tabImage;
            }
            else
            {
                leftButtons[i].sprite = characterList[i].iconImage;
            }
            leftButtons[i].SetNativeSize();
        }
    }

    [Header("Cấu hình Scene Tiếp Theo")]
    [Tooltip("Tên Scene Chọn Map (VD: ChonMap)")]
    public string nextSceneName = "ChonMap";

    public void ConfirmAndPlay()
    {
        int costPerGame = 10;
        int currentEnergy = PlayerPrefs.GetInt("TotalEnergy", 1000);

        if (currentEnergy >= costPerGame)
        {
            PlayerPrefs.SetInt("TotalEnergy", currentEnergy - costPerGame);
            
            // ĐÂY LÀ DÒNG LƯU NHÂN VẬT! Phải chạy qua đây máy mới nhớ là Cáo!
            PlayerPrefs.SetInt("SelectedCharacter", selectedIndex);
            PlayerPrefs.Save();
            
            // Chuyển sang màn hình chọn Map thay vì SampleScene
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("Không đủ năng lượng! Cần ít nhất " + costPerGame);
        }
    }
}