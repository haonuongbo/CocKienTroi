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
    public Transform selectedContainer; // Gán object "Selected" từ Hierarchy

    private Transform listContainer; // Parent gốc của danh sách icon

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
        if (leftButtons.Length > 0)
            listContainer = leftButtons[0].transform.parent;
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

    void SnapToContainer(Image button, Transform targetParent)
    {
        if (button == null || targetParent == null)
        {
            return;
        }

        button.transform.SetParent(targetParent, false);

        RectTransform buttonRect = button.rectTransform;
        RectTransform parentRect = targetParent as RectTransform;
        if (buttonRect != null && parentRect != null)
        {
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.localPosition = Vector3.zero;
        }
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

        // Đưa tất cả icon về lại listContainer với đúng thứ tự
        if (listContainer != null)
        {
            for (int i = 0; i < leftButtons.Length; i++)
            {
                leftButtons[i].transform.SetParent(listContainer, false);
                leftButtons[i].transform.SetSiblingIndex(i);
                leftButtons[i].sprite = characterList[i].iconImage;
                leftButtons[i].transform.localScale = Vector3.one;
                leftButtons[i].SetNativeSize();
            }
        }

        // Di chuyển icon nhân vật được chọn sang container "Selected"
        if (selectedContainer != null)
        {
            SnapToContainer(leftButtons[selectedIndex], selectedContainer);
            leftButtons[selectedIndex].transform.localScale = Vector3.one;
            leftButtons[selectedIndex].sprite = characterList[selectedIndex].tabImage;
            leftButtons[selectedIndex].SetNativeSize();
        }
    }

    [Header("Cấu hình Scene Tiếp Theo")]
    [Tooltip("Tên Scene Chọn Map (VD: ChonMap)")]
    public string nextSceneName = "MAP 1 RUNG";

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
            
            // Chuyển sang màn hình chọn Map qua Loading scene
            PlayerPrefs.SetString("NextSceneToLoad", nextSceneName);
            SceneManager.LoadScene("Loading");
        }
        else
        {
            Debug.Log("Không đủ năng lượng! Cần ít nhất " + costPerGame);
        }
    }
}