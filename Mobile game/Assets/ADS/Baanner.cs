using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAd : MonoBehaviour
{
    [SerializeField] private BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    [SerializeField] private string _androidAdUnitId = "Banner_Android";
    [SerializeField] private string _iOSAdUnitId = "Banner_iOS";

    private string _adUnitId;
    private bool _isLoaded;

    void Awake()
    {
        _adUnitId = GetAdUnitIdForCurrentPlatform();

        if (string.IsNullOrEmpty(_adUnitId))
        {
            Debug.LogWarning("BannerAd: No Ad Unit Id for this platform.");
            return;
        }

        Advertisement.Banner.SetPosition(_bannerPosition);
    }

    private string GetAdUnitIdForCurrentPlatform()
    {
        // These references happen at runtime, so both serialized fields are "used"
        // and CS0414 warnings go away.
        return Application.platform switch
        {
            RuntimePlatform.IPhonePlayer => _iOSAdUnitId,
            RuntimePlatform.Android => _androidAdUnitId,
            RuntimePlatform.OSXEditor => _androidAdUnitId,
            RuntimePlatform.WindowsEditor => _androidAdUnitId,
            _ => null
        };
    }

    public void LoadBanner()
    {
        if (string.IsNullOrEmpty(_adUnitId)) return;
        if (_isLoaded) return;

        Advertisement.Banner.Load(_adUnitId, new BannerLoadOptions
        {
            loadCallback = () =>
            {
                _isLoaded = true;
                Debug.Log("BannerAd: Banner loaded");
                ShowBanner();
            },
            errorCallback = (msg) =>
            {
                _isLoaded = false;
                Debug.LogWarning("BannerAd: Banner load error: " + msg);
            }
        });
    }

    public void ShowBanner()
    {
        if (string.IsNullOrEmpty(_adUnitId)) return;

        if (!_isLoaded)
        {
            LoadBanner(); // will auto-show after loaded
            return;
        }

        Advertisement.Banner.Show(_adUnitId);
    }

    public void HideBanner()
    {
        Advertisement.Banner.Hide();
    }
}
