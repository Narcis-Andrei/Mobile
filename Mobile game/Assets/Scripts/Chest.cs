using UnityEngine;
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
using CandyCoded.HapticFeedback;
#endif

public class Chest : MonoBehaviour
{
    [Min(0.1f)] public float activationRadius = 1.3f;
    public bool deactivate = false;

    private void Reset()
    {
        var col =GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = activationRadius;
    }

    public void OnValidate()
    {
        var col =GetComponent<SphereCollider>();
        if (col) col.radius = activationRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance) GameManager.Instance.ShowCollectablesMenu();

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        HapticFeedback.LightFeedback();
#endif

        if (deactivate) gameObject.SetActive(false);
        else Destroy(gameObject);
    }
}
