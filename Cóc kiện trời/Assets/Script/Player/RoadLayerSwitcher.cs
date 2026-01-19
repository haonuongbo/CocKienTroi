using UnityEngine;

public class RoadLayerSwitcher : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LowerPathDisabler"))
        {
            gameObject.layer = LayerMask.NameToLayer("UpperRoad");
            sr.sortingOrder = 3;

        }
        else if (other.CompareTag("UpperPathDisabler"))
        {
            gameObject.layer = LayerMask.NameToLayer("LowerRoad");
            sr.sortingOrder = 1;
        }
        else if (other.CompareTag("RefreshDisabler"))
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
            sr.sortingOrder = 3;
        }
    }
}
