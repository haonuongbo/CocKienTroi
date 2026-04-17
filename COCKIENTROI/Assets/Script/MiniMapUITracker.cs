using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapUITracker : MonoBehaviour
{
    [Header("Mini map UI")]
    [SerializeField] private RectTransform miniMapRect;
    [SerializeField] private RectTransform playerDotTemplate;
    [SerializeField] private RectTransform aiDotTemplate;
    [SerializeField] private RectTransform itemDotTemplate;

    [Header("World bounds mapped to mini map")]
    [SerializeField] private Vector2 worldMin = new Vector2(-1200f, -1200f);
    [SerializeField] private Vector2 worldMax = new Vector2(1200f, 1200f);
    [SerializeField] private bool autoCalculateWorldBounds = true;
    [SerializeField] private LayerMask mapColliderLayers = ~0;
    [SerializeField] private bool ignoreTriggerColliders = true;

    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string aiTag = "AI";
    [SerializeField] private string itemTag = "ItemBox";
    [SerializeField] private bool useTagDetection = true;

    private Transform playerTarget;
    private RectTransform playerDot;

    private readonly List<Transform> aiTargets = new List<Transform>();
    private readonly List<RectTransform> aiDots = new List<RectTransform>();

    private readonly List<ItemBox> itemTargets = new List<ItemBox>();
    private readonly List<RectTransform> itemDots = new List<RectTransform>();

    private float nextRefreshTime;
    private const float RefreshInterval = 0.75f;
    private bool worldBoundsAutoCalculated;

    private void Awake()
    {
        AutoBindUIReferences();
    }

    private void Start()
    {
        SetupTemplates();
        TryAutoCalculateWorldBounds();
        RefreshTargetsAndDots();
    }

    private void Update()
    {
        if (Time.unscaledTime >= nextRefreshTime)
        {
            nextRefreshTime = Time.unscaledTime + RefreshInterval;
            TryAutoCalculateWorldBounds();
            RefreshTargetsAndDots();
        }

        UpdateDotPosition(playerTarget, playerDot);

        for (int i = 0; i < aiTargets.Count; i++)
        {
            UpdateDotPosition(aiTargets[i], aiDots[i]);
        }

        for (int i = 0; i < itemTargets.Count; i++)
        {
            if (itemDots[i] == null) continue;
            
            bool isActive = itemTargets[i] != null && 
                            itemTargets[i].gameObject.activeInHierarchy && 
                            itemTargets[i].GetComponent<SpriteRenderer>().enabled;

            if (isActive)
            {
                if (!itemDots[i].gameObject.activeSelf) itemDots[i].gameObject.SetActive(true);
                UpdateDotPosition(itemTargets[i].transform, itemDots[i]);
                itemDots[i].localScale = Vector3.one * 0.75f; // Thu nhỏ icon ItemBox một chút
            }
            else
            {
                if (itemDots[i].gameObject.activeSelf) itemDots[i].gameObject.SetActive(false);
            }
        }
    }

    private void AutoBindUIReferences()
    {
        if (miniMapRect == null)
        {
            miniMapRect = transform as RectTransform;
        }

        if (miniMapRect == null)
        {
            return;
        }

        if (playerDotTemplate == null)
        {
            Transform t = miniMapRect.Find("Mini_Xe (1)");
            if (t != null) playerDotTemplate = t as RectTransform;
        }

        if (aiDotTemplate == null)
        {
            Transform t = miniMapRect.Find("Mini_Xe");
            if (t != null) aiDotTemplate = t as RectTransform;
        }
    }

    private void SetupTemplates()
    {
        if (playerDotTemplate == null || aiDotTemplate == null)
        {
            return;
        }

        // Ensure player is green and AI is red.
        SetImageColor(playerDotTemplate, Color.green);
        SetImageColor(aiDotTemplate, Color.red);

        // Setup ItemBox template
        if (itemDotTemplate == null)
        {
            itemDotTemplate = Instantiate(aiDotTemplate, miniMapRect);
            itemDotTemplate.name = "Mini_Item_Template";
            itemDotTemplate.gameObject.SetActive(false);
        }
        SetImageColor(itemDotTemplate, Color.yellow); // Hiển thị BoxItem với màu vàng

        playerDot = playerDotTemplate;
    }

    private void RefreshTargetsAndDots()
    {
        playerTarget = FindPlayerTarget();
        FindAiTargets(aiTargets);
        FindItemTargets(itemTargets);
        SyncAiDotsWithTargets();
        SyncItemDotsWithTargets();
    }

    private Transform FindPlayerTarget()
    {
        // Prefer component-based detection so wrong tags do not break mapping.
        ControlSpeedAnim player = FindFirstObjectByType<ControlSpeedAnim>();
        if (player != null) return player.transform;

        PCController playerCar = FindFirstObjectByType<PCController>();
        if (playerCar != null) return playerCar.transform;

        if (useTagDetection)
        {
            GameObject taggedPlayer = GameObject.FindWithTag(playerTag);
            if (taggedPlayer != null) return taggedPlayer.transform;
        }

        return null;
    }

    private void FindAiTargets(List<Transform> output)
    {
        output.Clear();

        AICarController[] aiCars = FindObjectsByType<AICarController>(FindObjectsSortMode.None);
        for (int i = 0; i < aiCars.Length; i++)
        {
            if (aiCars[i] != null) output.Add(aiCars[i].transform);
        }

        if (output.Count == 0 && useTagDetection)
        {
            GameObject[] taggedAis = GameObject.FindGameObjectsWithTag(aiTag);
            for (int i = 0; i < taggedAis.Length; i++)
            {
                if (taggedAis[i] != null) output.Add(taggedAis[i].transform);
            }
        }

        // Safety: never duplicate player into AI list.
        for (int i = output.Count - 1; i >= 0; i--)
        {
            if (output[i] == null || output[i] == playerTarget)
            {
                output.RemoveAt(i);
            }
        }
    }

    private void SyncAiDotsWithTargets()
    {
        if (aiDotTemplate == null) return;

        // Reuse existing Mini_Xe as the first AI dot to avoid one extra static red marker.
        if (aiDots.Count == 0)
        {
            aiDots.Add(aiDotTemplate);
        }

        while (aiDots.Count < aiTargets.Count)
        {
            RectTransform newDot = Instantiate(aiDotTemplate, miniMapRect);
            newDot.name = "Mini_AI_" + aiDots.Count;
            SetImageColor(newDot, Color.red);
            aiDots.Add(newDot);
        }

        for (int i = 0; i < aiDots.Count; i++)
        {
            aiDots[i].gameObject.SetActive(i < aiTargets.Count);
        }
    }

    private void FindItemTargets(List<ItemBox> output)
    {
        output.Clear();
        ItemBox[] items = FindObjectsByType<ItemBox>(FindObjectsSortMode.None);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null) output.Add(items[i]);
        }
    }

    private void SyncItemDotsWithTargets()
    {
        if (itemDotTemplate == null) return;

        while (itemDots.Count < itemTargets.Count)
        {
            RectTransform newDot = Instantiate(itemDotTemplate, miniMapRect);
            newDot.name = "Mini_Item_" + itemDots.Count;
            SetImageColor(newDot, Color.yellow);
            itemDots.Add(newDot);
        }

        for (int i = 0; i < itemDots.Count; i++)
        {
            itemDots[i].gameObject.SetActive(i < itemTargets.Count);
        }
    }

    private void UpdateDotPosition(Transform target, RectTransform dot)
    {
        if (target == null || dot == null || miniMapRect == null)
        {
            return;
        }

        Vector2 worldPos = new Vector2(target.position.x, target.position.y);
        float nx = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
        float ny = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.y);

        float localX = Mathf.Lerp(-miniMapRect.rect.width * 0.5f, miniMapRect.rect.width * 0.5f, nx);
        float localY = Mathf.Lerp(-miniMapRect.rect.height * 0.5f, miniMapRect.rect.height * 0.5f, ny);

        dot.anchoredPosition = new Vector2(localX, localY);
    }

    private void TryAutoCalculateWorldBounds()
    {
        if (!autoCalculateWorldBounds || worldBoundsAutoCalculated)
        {
            return;
        }

        Collider2D[] colliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        bool hasBounds = false;
        Bounds mapBounds = new Bounds(Vector3.zero, Vector3.zero);

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D col = colliders[i];
            if (col == null) continue;
            if (!col.gameObject.activeInHierarchy) continue;
            if (ignoreTriggerColliders && col.isTrigger) continue;
            if (((1 << col.gameObject.layer) & mapColliderLayers.value) == 0) continue;
            if (IsDynamicRacerCollider(col.transform)) continue;

            if (!hasBounds)
            {
                mapBounds = col.bounds;
                hasBounds = true;
            }
            else
            {
                mapBounds.Encapsulate(col.bounds);
            }
        }

        if (!hasBounds)
        {
            return;
        }

        worldMin = new Vector2(mapBounds.min.x, mapBounds.min.y);
        worldMax = new Vector2(mapBounds.max.x, mapBounds.max.y);
        worldBoundsAutoCalculated = true;
    }

    private bool IsDynamicRacerCollider(Transform t)
    {
        if (t == null) return false;

        if (t.GetComponentInParent<ControlSpeedAnim>() != null) return true;
        if (t.GetComponentInParent<PCController>() != null) return true;
        if (t.GetComponentInParent<AICarController>() != null) return true;

        return false;
    }

    private void SetImageColor(RectTransform t, Color color)
    {
        Image image = t != null ? t.GetComponent<Image>() : null;
        if (image != null)
        {
            image.color = color;
        }
    }
}
