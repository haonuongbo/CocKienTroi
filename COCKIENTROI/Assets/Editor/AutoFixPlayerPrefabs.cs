using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AutoFixPlayerPrefabs
{
    static AutoFixPlayerPrefabs()
    {
        EditorApplication.delayCall += DoFix;
    }

    static void DoFix()
    {
        if (SessionState.GetBool("PlayerPrefabsFixed", false)) return;

        string[] playerPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Player Prefabs" });

        int count = 0;
        foreach (string guid in playerPrefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                CarItemManager manager = prefab.GetComponent<CarItemManager>();
                if (manager != null && !manager.isPlayer)
                {
                    manager.isPlayer = true;
                    EditorUtility.SetDirty(prefab);
                    count++;
                }
            }
        }

        if (count > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log(">>> Đã tự động SỬA LỖI (Bật isPlayer = true) cho " + count + " xe Player!");
        }
        
        SessionState.SetBool("PlayerPrefabsFixed", true);
    }
}
