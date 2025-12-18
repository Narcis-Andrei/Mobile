using UnityEngine;

public class NearestChestPointer : MonoBehaviour
{
    public Transform player;
    public Camera cam;

    [Header("UI")]
    public RectTransform arrowUI;
    public RectTransform centerUI;
    public float radius = 140f;
    public string chestTag = "Chest";

    [Header("Behavior")]
    public bool rotateArrowToPoint = true;
    public bool orbitClockwise = true;

    [Header("No chest behavior")]
    public bool keepLastDirectionWhenNoChest = true;

    Vector2 lastUiDir = Vector2.up;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (!arrowUI) arrowUI = transform as RectTransform;
        if (!centerUI && arrowUI) centerUI = arrowUI.parent as RectTransform;

    }

    void Update()
    {
        if (!player || !cam || !arrowUI || !centerUI) return;

        var chests = GameObject.FindGameObjectsWithTag(chestTag);

        Transform nearest = null;
        float bestSqr = float.PositiveInfinity;

        Vector3 playerPos = player.position;

        if (chests != null)
        {
            for (int i = 0; i < chests.Length; i++)
            {
                if (!chests[i]) continue;
                Vector3 d = chests[i].transform.position - playerPos;
                d.y = 0f;
                float sqr = d.sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = chests[i].transform;
                }
            }
        }

        Vector2 uiDir = lastUiDir;

        if (nearest)
        {
            Vector3 dir = nearest.position - playerPos;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
            {
                dir.Normalize();

                Vector3 localDir = cam.transform.InverseTransformDirection(dir);
                localDir.y = 0f;

                if (localDir.sqrMagnitude > 0.0001f)
                {
                    localDir.Normalize();
                    uiDir = new Vector2(localDir.x, localDir.z);

                    if (uiDir.sqrMagnitude > 0.0001f)
                        uiDir.Normalize();
                }
            }

            if (!orbitClockwise) uiDir.x = -uiDir.x;

            lastUiDir = uiDir;
        }
        else
        {
            if (!keepLastDirectionWhenNoChest)
                uiDir = Vector2.up;
        }

        Vector2 centerPos;
        if (arrowUI.parent == centerUI.parent)
            centerPos = centerUI.anchoredPosition;
        else if (arrowUI.parent == centerUI)
            centerPos = Vector2.zero;
        else
            centerPos = arrowUI.anchoredPosition;

        arrowUI.anchoredPosition = centerPos + (uiDir * radius);

        if (rotateArrowToPoint)
        {
            float ang = Mathf.Atan2(uiDir.y, uiDir.x) * Mathf.Rad2Deg;
            arrowUI.localRotation = Quaternion.Euler(0f, 0f, ang - 90f);
        }
    }
}
