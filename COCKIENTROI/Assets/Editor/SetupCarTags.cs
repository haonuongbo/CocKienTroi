using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor tool: Tự động tạo tag "Car" và gán vào tất cả GameObject
/// có component CarItemManager trong scene hiện tại.
/// Chạy qua menu: Tools → Setup Car Tags
/// </summary>
public class SetupCarTags : EditorWindow
{
    [MenuItem("Tools/Setup Car Tags")]
    public static void Run()
    {
        // ── Bước 1: Đảm bảo tag "Car" tồn tại trong dự án ─────────────
        EnsureTagExists("Car");

        // ── Bước 2: Tìm tất cả CarItemManager trong scene ──────────────
        CarItemManager[] allCars = Object.FindObjectsByType<CarItemManager>(FindObjectsSortMode.None);

        if (allCars.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Setup Car Tags",
                "Không tìm thấy CarItemManager nào trong scene!\nHãy mở đúng scene chứa các xe rồi chạy lại.",
                "OK");
            return;
        }

        // ── Bước 3: Gán tag "Car" cho root GameObject của mỗi xe ────────
        List<string> tagged = new List<string>();
        foreach (CarItemManager car in allCars)
        {
            GameObject root = car.gameObject;

            // Gán tag cho root nếu chưa có
            if (!root.CompareTag("Car"))
            {
                Undo.RecordObject(root, "Set Car Tag");
                root.tag = "Car";
                EditorUtility.SetDirty(root);
                tagged.Add(root.name);
            }

            // Cũng gán cho tất cả collider con trực tiếp (nếu xe dùng child collider)
            foreach (Collider2D col in root.GetComponentsInChildren<Collider2D>())
            {
                if (col == null || col.gameObject == root) continue;
                if (!col.gameObject.CompareTag("Car"))
                {
                    Undo.RecordObject(col.gameObject, "Set Car Tag (child)");
                    col.gameObject.tag = "Car";
                    EditorUtility.SetDirty(col.gameObject);
                    if (!tagged.Contains(col.gameObject.name))
                        tagged.Add(col.gameObject.name + " (collider con của " + root.name + ")");
                }
            }
        }

        // ── Bước 4: Lưu scene và báo kết quả ────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        if (tagged.Count > 0)
        {
            string list = string.Join("\n  • ", tagged);
            EditorUtility.DisplayDialog(
                "Setup Car Tags ✅",
                $"Đã gán tag \"Car\" thành công cho {tagged.Count} object:\n\n  • {list}\n\nNhớ Save scene (Ctrl+S) để lưu lại!",
                "OK");
            Debug.Log($"[SetupCarTags] Đã gán tag \"Car\" cho: {list}");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Setup Car Tags",
                "Tất cả xe đã có tag \"Car\" từ trước rồi, không cần làm gì thêm!",
                "OK");
        }
    }

    // ── Tiện ích: Tạo tag mới nếu chưa có trong TagManager ──────────────
    static void EnsureTagExists(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Kiểm tra tag đã tồn tại chưa
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName)
                return; // Đã có rồi
        }

        // Thêm tag mới
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[SetupCarTags] Đã tạo tag mới: \"{tagName}\"");
    }
}
