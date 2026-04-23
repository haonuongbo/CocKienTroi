#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FixItemBoxCollider : EditorWindow
{
    [MenuItem("Tools/Fix ItemBox Collider")]
    public static void Run()
    {
        int count = 0;
        
        // Cập nhật Prefab
        string prefabPath = "Assets/Prefabs/Items Prefabs/ItemBox.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            BoxCollider2D bc = prefab.GetComponent<BoxCollider2D>();
            if (bc != null)
            {
                bc.size = new Vector2(5f, 5f);
                EditorUtility.SetDirty(prefab);
                count++;
            }
        }

        // Cập nhật các ItemBox trong Scene hiện tại
        ItemBox[] sceneBoxes = FindObjectsOfType<ItemBox>(true);
        foreach (var box in sceneBoxes)
        {
            BoxCollider2D bc = box.GetComponent<BoxCollider2D>();
            if (bc != null)
            {
                // Nếu collider size bị lố > 100, reset về 5
                if (bc.size.x > 100f || bc.size.y > 100f)
                {
                    Undo.RecordObject(bc, "Fix ItemBox Collider Size");
                    bc.size = new Vector2(5f, 5f);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(bc);
                    count++;
                }
            }
        }

        if (count > 0)
        {
            AssetDatabase.SaveAssets();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[FixItemBoxCollider] Đã sửa lỗi Collider khổng lồ cho {count} ItemBox (Prefab + Scene)!");
            EditorUtility.DisplayDialog("Thành công", $"Đã sửa lỗi {count} BoxCollider2D bị khổng lồ (50000x50000) thành 5x5.", "OK");
        }
        else
        {
            Debug.Log("[FixItemBoxCollider] Không tìm thấy collider nào cần sửa.");
        }
    }
}
#endif
