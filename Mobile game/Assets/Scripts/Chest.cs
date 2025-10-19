using UnityEngine;
using CandyCoded.HapticFeedback;

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

        HapticFeedback.LightFeedback();

        if (deactivate) gameObject.SetActive(false);
        else Destroy(gameObject);
    }
}
