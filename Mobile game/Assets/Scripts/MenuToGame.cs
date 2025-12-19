using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class MenuToGame : MonoBehaviour
{
    [Header("Cinemachine")]
    public CinemachineVirtualCameraBase menuCam;
    public CinemachineVirtualCameraBase gameplayCam;
    public CinemachineBrain brain;

    [Header("UI")]
    public GameObject mainMenuUI;
    public GameObject playerUI;

    [Header("Hard stop")]
    public GameObject[] disableDuringMenu;

    [Header("Timing")]
    public float fallbackBlendWait = 1.2f;

    bool _starting;

    void Awake()
    {
        if (!brain) brain = Camera.main.GetComponent<CinemachineBrain>();

        if (mainMenuUI) mainMenuUI.SetActive(true);
        if (playerUI) playerUI.SetActive(false);

        SetObjectsActive(false);

        if (menuCam) menuCam.Priority = 20;
        if (gameplayCam) gameplayCam.Priority = 10;
    }

    public void OnPlayPressed()
    {
        if (_starting) return;
        _starting = true;
        StartCoroutine(BlendThenStart());
    }

    IEnumerator BlendThenStart()
    {
        if (menuCam) menuCam.Priority = 0;
        if (gameplayCam) gameplayCam.Priority = 30;

        yield return null;

        if (brain != null)
        {
            float t = 0f;
            while (brain.IsBlending && t < fallbackBlendWait)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fallbackBlendWait);
        }

        if (mainMenuUI) mainMenuUI.SetActive(false);
        if (playerUI) playerUI.SetActive(true);

        SetObjectsActive(true);
    }

    void SetObjectsActive(bool active)
    {
        if (disableDuringMenu == null) return;
        foreach (var go in disableDuringMenu)
            if (go) go.SetActive(active);
    }
}
