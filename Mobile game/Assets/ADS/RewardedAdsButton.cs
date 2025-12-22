using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class RewardedAdsButton : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] Button _showAdButton;
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";

    // Reference to the chest menu so we can trigger a reroll after a completed rewarded ad:
    [SerializeField] ChestRewardMenu chestMenu;

    string _adUnitId = null; // This will remain null for unsupported platforms

    void Awake()
    {
        // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#elif UNITY_EDITOR
        // Optional: use Android id while testing in the Editor.
        _adUnitId = _androidAdUnitId;
#endif

        // Disable the button until the ad is ready to show:
        if (_showAdButton != null)
            _showAdButton.interactable = false;
    }

    // Call this public method when you want to get an ad ready to show.
    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        if (string.IsNullOrEmpty(_adUnitId))
        {
            Debug.LogWarning("RewardedAdsButton: No Ad Unit Id for this platform.");
            return;
        }

        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    // If the ad successfully loads, add a listener to the button and enable it:
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);

        if (adUnitId.Equals(_adUnitId))
        {
            // Configure the button to call the ShowAd() method when clicked:
            if (_showAdButton != null)
            {
                _showAdButton.onClick.RemoveListener(ShowAd); // avoid duplicates if LoadAd() is called again
                _showAdButton.onClick.AddListener(ShowAd);

                // Enable the button for users to click:
                _showAdButton.interactable = true;
            }
        }
    }

    // Implement a method to execute when the user clicks the button:
    public void ShowAd()
    {
        if (string.IsNullOrEmpty(_adUnitId))
        {
            Debug.LogWarning("RewardedAdsButton: Cannot show, Ad Unit Id is null/empty.");
            return;
        }

        // Disable the button:
        if (_showAdButton != null)
            _showAdButton.interactable = false;

        // Then show the ad:
        Advertisement.Show(_adUnitId, this);
    }

    // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            // Grant a reward:
            chestMenu?.Reroll();
        }

        // Optional: load the next rewarded ad right away so the button can re-enable next time
        // (it still won't reroll again because GameManager blocks rerolls with _rerollUsed).
        LoadAd();
    }

    // Implement Load and Show Listener error callbacks:
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");

        // If showing fails, re-load so the button can be used again later.
        LoadAd();

        // Use the error details to determine whether to try to load another ad.
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    void OnDestroy()
    {
        // Clean up the button listeners:
        if (_showAdButton != null)
            _showAdButton.onClick.RemoveAllListeners();
    }
}