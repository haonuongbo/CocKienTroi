using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Map Configuration")]
    public int totalMaps = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        InitializeUnlockedMaps();
    }

    /// <summary>
    /// Khởi tạo: Tất cả map đều mở
    /// </summary>
    private void InitializeUnlockedMaps()
    {
        for (int i = 1; i <= totalMaps; i++)
        {
            SetMapUnlocked(i, true);
        }
    }

    /// <summary>
    /// Kiểm tra xem map có được mở hay không
    /// </summary>
    public bool IsMapUnlocked(int mapNumber)
    {
        return PlayerPrefs.GetInt($"MapUnlocked_{mapNumber}", 0) == 1;
    }

    /// <summary>
    /// Mở khóa một map và lưu tiến độ
    /// </summary>
    public void UnlockMap(int mapNumber)
    {
        if (mapNumber > 0 && mapNumber <= totalMaps)
        {
            SetMapUnlocked(mapNumber, true);
            Debug.Log($"Map {mapNumber} has been unlocked!");
        }
    }

    /// <summary>
    /// Xử lý khi thắng một map
    /// </summary>
    public void WinMap(int mapNumber)
    {
        if (mapNumber >= 1 && mapNumber < totalMaps)
        {
            // Mở khóa map tiếp theo
            UnlockMap(mapNumber + 1);
        }
        Debug.Log($"Map {mapNumber} completed! Next map unlocked.");
    }

    /// <summary>
    /// Lưu trạng thái unlock
    /// </summary>
    private void SetMapUnlocked(int mapNumber, bool unlocked)
    {
        PlayerPrefs.SetInt($"MapUnlocked_{mapNumber}", unlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Reset tiến độ (tuỳ chọn, dành cho debug/testing)
    /// </summary>
    public void ResetProgress()
    {
        for (int i = 1; i <= totalMaps; i++)
        {
            PlayerPrefs.DeleteKey($"MapUnlocked_{i}");
        }
        PlayerPrefs.Save();
        InitializeUnlockedMaps();
        Debug.Log("Progress has been reset!");
    }

    /// <summary>
    /// Lấy danh sách các map đã unlock
    /// </summary>
    public int[] GetUnlockedMaps()
    {
        System.Collections.Generic.List<int> unlockedMaps = new System.Collections.Generic.List<int>();
        for (int i = 1; i <= totalMaps; i++)
        {
            if (IsMapUnlocked(i))
            {
                unlockedMaps.Add(i);
            }
        }
        return unlockedMaps.ToArray();
    }
}
