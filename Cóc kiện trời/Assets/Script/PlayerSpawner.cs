using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Danh sách nhân vật")]
    public GameObject[] playerPrefabs;

    [Header("Vị trí xuất phát")]
    public Transform spawnPoint;

    [Header("Danh sách Checkpoints trên Map này")]
    [Tooltip("Kéo các cục Checkpoint trong Scene vào đây để truyền cho xe lúc sinh ra")]
    public Transform[] mapCheckpoints;

    [Header("--- CÀI ĐẶT ĐỐI THỦ (AI) ---")]
    [Tooltip("Kéo đủ 5 con AI vào đây theo bảng thứ tự 0-4 (giống hệt Player)")]
    public GameObject[] aiPrefabs;
    
    [Tooltip("Kéo 4 vị trí xuất phát dành cho AI vào đây")]
    public Transform[] aiSpawnPoints;

    private void Awake()
    {
        // Đọc ID nhân vật được lưu từ màn hình Chọn Nhân Vật
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);

        if (playerPrefabs != null && playerPrefabs.Length > 0)
        {
            // Mảng chứa các Tracker để gửi cho Bảng xếp hạng
            RaceProgressTracker[] allTrackers = new RaceProgressTracker[5];

            // Tránh lỗi nếu index hiện tại vượt quá số lượng prefab bạn gắn
            if (selectedIndex < 0 || selectedIndex >= playerPrefabs.Length)
            {
                Debug.LogWarning("Index bị sai (" + selectedIndex + "), tự động chuyển về 0");
                selectedIndex = 0; // Mặc định về con đầu tiên nếu lỗi
            }

            GameObject prefabToSpawn = playerPrefabs[selectedIndex];
            
            Debug.LogError("==== KIỂM TRA SPAWN ====================");
            Debug.LogError("1. Xe bạn đã lưu ở màn hình chọn (Index): " + selectedIndex);
            if (prefabToSpawn != null)
                Debug.LogError("2. Tên con xe MÀ SCRIPT SẼ ĐẺ RA: " + prefabToSpawn.name);
            Debug.LogError("=========================================");
            
            if (prefabToSpawn != null)
            {
                // Sinh ra xe ở vị trí đã chọn
                Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
                Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
                
                GameObject spawnedPlayer = Instantiate(prefabToSpawn, pos, rot);
                
                // Đổi tên cho gọn trong Hierarchy
                spawnedPlayer.name = prefabToSpawn.name; 

                // Tự động gắn Camera bám theo nhân vật mới
                TopDownCameraFollow camFollow = FindFirstObjectByType<TopDownCameraFollow>();
                if (camFollow != null)
                {
                    camFollow.target = spawnedPlayer.transform;
                    camFollow.targetRb = spawnedPlayer.GetComponent<Rigidbody2D>();
                }
                
                // Tự động gán Checkpoints vào RaceProgressTracker của Player mới
                RaceProgressTracker progressTracker = spawnedPlayer.GetComponent<RaceProgressTracker>();
                if (progressTracker == null)
                {
                    progressTracker = spawnedPlayer.AddComponent<RaceProgressTracker>();
                }

                if (progressTracker != null)
                {
                    if (mapCheckpoints != null && mapCheckpoints.Length > 0)
                    {
                        progressTracker.checkpoints = mapCheckpoints;
                    }
                    if (selectedIndex >= 0 && selectedIndex < 5)
                    {
                        allTrackers[selectedIndex] = progressTracker;
                    }
                }
            }

            // SINH RA 4 ĐỐI THỦ AI 
            if (aiPrefabs != null && aiPrefabs.Length > 0 && aiSpawnPoints != null && aiSpawnPoints.Length > 0)
            {
                int spawnIndex = 0;
                for (int i = 0; i < aiPrefabs.Length; i++)
                {
                    // LƯU Ý: Bỏ qua con AI trùng với nhân vật người chơi đã chọn
                    if (i == selectedIndex) continue;

                    // Tránh lỗi nễu chưa kéo đủ SpawnPoint
                    if (spawnIndex >= aiSpawnPoints.Length) break;

                    GameObject aiToSpawn = aiPrefabs[i];
                    if (aiToSpawn != null)
                    {
                        Transform sp = aiSpawnPoints[spawnIndex];
                        GameObject spawnedAI = Instantiate(aiToSpawn, sp.position, sp.rotation);
                        spawnedAI.name = aiToSpawn.name;

                        // Gán Checkpoints cho AI luôn (phục vụ việc xếp hạng nếu có)
                        RaceProgressTracker aiProgress = spawnedAI.GetComponent<RaceProgressTracker>();
                        if (aiProgress == null) 
                        {
                            aiProgress = spawnedAI.AddComponent<RaceProgressTracker>();
                        }

                        if (aiProgress != null)
                        {
                            if (mapCheckpoints != null && mapCheckpoints.Length > 0)
                            {
                                aiProgress.checkpoints = mapCheckpoints;
                            }
                            if (i >= 0 && i < 5)
                            {
                                allTrackers[i] = aiProgress;
                            }
                        }
                        
                        spawnIndex++;
                    }
                }
                // Tự động đẩy dữ liệu sang Bảng Xếp Hạng
                RaceRankManager rankManager = FindFirstObjectByType<RaceRankManager>();
                if (rankManager != null)
                {
                    rankManager.trackers = allTrackers;
                }
            }
        }
    }
}
