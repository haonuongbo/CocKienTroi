using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifeTime = 1.5f; // Thời gian tồn tại (giây)

    void Start()
    {
        // Tự động xóa đối tượng này sau khoảng thời gian lifeTime
        Destroy(gameObject, lifeTime);
    }
}