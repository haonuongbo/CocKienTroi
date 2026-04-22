using UnityEngine;
using UnityEditor;
using System.Linq;

[InitializeOnLoad]
public class AutoAssignRainSprites
{
    static AutoAssignRainSprites()
    {
        EditorApplication.delayCall += DoAssign;
    }

    static void DoAssign()
    {
        if (SessionState.GetBool("RainSpritesAssigned", false)) return;

        string rainPath = "Assets/(6) CÓC KIỆN TRỜI/Ingame/mưa.png";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(rainPath);
        Sprite[] rainSprites = assets.OfType<Sprite>().OrderBy(s => s.name).ToArray();

        if (rainSprites.Length == 0)
        {
            Debug.LogError("Could not find any sprites in " + rainPath);
            return;
        }

        Debug.Log("Found " + rainSprites.Length + " rain sprites. Assigning...");

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Player Prefabs", "Assets/Prefabs/AI Prefabs" });

        int count = 0;
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                CarItemManager manager = prefab.GetComponent<CarItemManager>();
                if (manager != null)
                {
                    manager.rainSprites = rainSprites;
                    EditorUtility.SetDirty(prefab);
                    count++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log(">>> Đã tự động gán hiệu ứng Mưa (Rain Sprites) cho " + count + " xe!");
        
        SessionState.SetBool("RainSpritesAssigned", true);
    }
}
