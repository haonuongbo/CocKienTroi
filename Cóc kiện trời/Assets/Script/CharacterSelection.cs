using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterData
    {
        public string name;
        public Sprite iconImage;
        public Sprite tabImage;
        public Sprite previewImage;

        [Header("Chỉ số")]
        public Sprite speedStat;
        public Sprite driftStat;
        public Sprite accelStat;

        [TextArea(3, 6)]
        public string description;
    }

    public CharacterData[] characterList;

    [Header("UI Cần Gán")]
    public Image centerPreview;
    public Image statSpeed;
    public Image statDrift;
    public Image statAccel;
    public Text descriptionText;
    public Image[] leftButtons;

    [Header("Top Right UI")]
    public Text moneyText;
    public Text energyText;

    private int selectedIndex = 0;

    void Start()
    {
        selectedIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
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
        UpdateUI();
    }

    void UpdateUI()
    {
        CharacterData data = characterList[selectedIndex];

        centerPreview.sprite = data.previewImage;
        centerPreview.SetNativeSize();

        if (statSpeed) statSpeed.sprite = data.speedStat;
        if (statDrift) statDrift.sprite = data.driftStat;
        if (statAccel) statAccel.sprite = data.accelStat;

        if (descriptionText) descriptionText.text = data.description;

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

    public void ConfirmAndPlay()
    {
        int costPerGame = 10;
        int currentEnergy = PlayerPrefs.GetInt("TotalEnergy", 1000);

        if (currentEnergy >= costPerGame)
        {
            // Trừ năng lượng
            PlayerPrefs.SetInt("TotalEnergy", currentEnergy - costPerGame);

            // Lưu nhân vật và vào game
            PlayerPrefs.SetInt("SelectedCharacter", selectedIndex);
            PlayerPrefs.Save();

            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.Log("Không đủ năng lượng! Cần ít nhất " + costPerGame);
            // Có thể hiện Popup "Nạp thêm năng lượng" ở đây
        }
    }
}