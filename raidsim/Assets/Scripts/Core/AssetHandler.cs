using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif

public class AssetHandler : MonoBehaviour
{
    public static AssetHandler Instance;

    public string[] sharedBundles = new string[] { "common" };
    public bool useExternalBundles = true;
    public bool disable = false;
    public bool log = true;

    private AssetBundle currentBundle;
    private string currentBundleName;
    private Coroutine ieLoadAssetBundle;
    private Coroutine ieLoadSharedAssetBundle;

    // Cache for loaded assets (avoids reloading assets from disk)
    private Dictionary<string, Object> assetCache = new Dictionary<string, Object>();

    private Dictionary<string, AssetBundle> currentSharedBundles = new Dictionary<string, AssetBundle>();

#if ENABLE_EXTERNAL_BUNDLES
    private const string externalBundlesUrl = "https://assets.susybaka.dev/raidsim/bundles/{0}?v={1}";
#else
    private const string externalBundlesUrl = "";
#endif
    private int gameVersion = -1;

#if UNITY_EDITOR
    private string m_bundleName = "common";
    [Button("Print Bundle URL")]
    public void PrintBundleUrl()
    {
        Debug.Log(string.Format(externalBundlesUrl, m_bundleName, gameVersion));
    }
#endif

    private void Awake()
    {
#if ENABLE_EXTERNAL_BUNDLES && !UNITY_EDITOR
        useExternalBundles = true;
#elif !ENABLE_EXTERNAL_BUNDLES && !UNITY_EDITOR
        useExternalBundles = false;
#endif
        gameVersion = GlobalVariables.versionNumber;

        if (disable)
        {
            Destroy(gameObject);
            return;
        }
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (currentSharedBundles == null || currentSharedBundles.Keys.Count <= 0)
        {
            currentSharedBundles = new Dictionary<string, AssetBundle>();

            LoadCommonAssetBundle();
        }
    }
    
    public void LoadCommonAssetBundle()
    {
        if (log)
            Debug.Log("Loading default common AssetBundles.");

        LoadCommonAssetBundleInternal(sharedBundles);
    }

    public void LoadCommonAssetBundle(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            Debug.LogError("Requested AssetBundle name is an empty string!");
            return;
        }

        LoadCommonAssetBundleInternal(new string[] { bundleName });
    }

    public void LoadCommonAssetBundle(string[] bundleNames)
    {
        LoadCommonAssetBundleInternal(bundleNames);
    }

    private void LoadCommonAssetBundleInternal(string[] names)
    {
        if (names != null && names.Length > 0)
        {
            foreach (string bundleName in names)
            {
                if (!string.IsNullOrEmpty(bundleName))
                {
                    if (string.IsNullOrEmpty(bundleName))
                    {
                        if (log)
                            Debug.Log($"Requested common AssetBundle '{bundleName}' is an empty string!");
                        return;
                    }

                    if (currentSharedBundles.ContainsKey(bundleName))
                    {
                        if (log)
                            Debug.Log($"Requested common AssetBundle '{bundleName}' is already loaded!");
                        return;
                    }

                    if (ieLoadSharedAssetBundle == null)
                        ieLoadSharedAssetBundle = StartCoroutine(IE_LoadCommonAssetBundle(bundleName));
                }
            }
        }
        else
        {
            if (log)
                Debug.Log("Requested common AssetBundle names are null or empty! No bundles to load.");
            return;
        }
    }

    private IEnumerator IE_LoadCommonAssetBundle(string bundleName)
    {
        if (currentSharedBundles.ContainsKey(bundleName))
        {
            if (log)
                Debug.Log($"Requested common AssetBundle '{bundleName}' is already loaded!");
            ieLoadSharedAssetBundle = null;
            yield break;
        }

        string bundlePath = string.Empty;

        if (useExternalBundles)
        {
            bundlePath = string.Format(externalBundlesUrl, bundleName, gameVersion);
        }
        else
        {
            bundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
        }
#if (UNITY_WEBGL || ENABLE_EXTERNAL_BUNDLES) && !UNITY_EDITOR
        if (log)
            Debug.Log($"Loading common AssetBundle: '{bundleName}'");

        // For WebGL, use UnityWebRequest to load the asset bundle
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bundlePath);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load common AssetBundle: " + request.error);
            ieLoadSharedAssetBundle = null;
            yield break;
        }

        AssetBundle loadedBundle = DownloadHandlerAssetBundle.GetContent(request);
#else
        if (!File.Exists(bundlePath))
        {
            Debug.LogError($"Common AssetBundle not found at: '{bundlePath}'!");
            ieLoadSharedAssetBundle = null;
            yield break;
        }

        if (log)
            Debug.Log($"Loading common AssetBundle: '{bundleName}'");

        // For other platforms, use AssetBundle.LoadFromFileAsync
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return bundleRequest;

        AssetBundle loadedBundle = bundleRequest.assetBundle;
#endif

        if (loadedBundle == null)
        {
            Debug.LogError($"Failed to load common AssetBundle: '{bundleName}'");
            ieLoadSharedAssetBundle = null;
            yield break;
        }

        currentSharedBundles.Add(bundleName, loadedBundle);

        if (log)
            Debug.Log($"Common AssetBundle loaded: '{bundleName}'!");
        ieLoadSharedAssetBundle = null;
    }

    public void LoadSceneAssetBundle(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            if (log)
                Debug.Log($"Requested AssetBundle '{bundleName}' is an empty string!");

            if (log && currentBundle != null)
                Debug.Log($"Unloading previous AssetBundle: '{currentBundleName}'");
            
            UnloadBundle();
            return;
        }

        if (currentBundle != null && bundleName == currentBundleName)
        {
            if (log)
                Debug.Log($"Requested AssetBundle '{bundleName}' is already loaded!");
            return;
        }

        if (ieLoadAssetBundle == null)
            ieLoadAssetBundle = StartCoroutine(IE_LoadAssetBundle(bundleName));
    }

    private IEnumerator IE_LoadAssetBundle(string bundleName)
    {
        if (currentBundle != null && bundleName == currentBundleName)
        {
            if (log)
                Debug.Log($"Requested AssetBundle '{bundleName}' is already loaded!");
            ieLoadAssetBundle = null;
            yield break;
        }

        // Unload the previous bundle if switching to a new one
        if (currentBundle != null)
        {
            if (log)
                Debug.Log($"Unloading previous AssetBundle: '{currentBundleName}'");
            currentBundle.Unload(true);
            currentBundle = null;
            ClearCache(); // Clear cached assets since the bundle changed
        }

        string bundlePath = string.Empty;
        
        if (useExternalBundles)
        {
            bundlePath = string.Format(externalBundlesUrl, bundleName, gameVersion);
        }
        else
        {
            bundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
        }
#if (UNITY_WEBGL || ENABLE_EXTERNAL_BUNDLES) && !UNITY_EDITOR
        if (log)
            Debug.Log($"Loading AssetBundle: '{bundleName}'");

        // For WebGL, use UnityWebRequest to load the asset bundle
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(bundlePath);
        yield return request.SendWebRequest();

        if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load AssetBundle: " + request.error);
            ieLoadAssetBundle = null;
            yield break;
        }

        AssetBundle loadedBundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
#else
        if (!File.Exists(bundlePath))
        {
            Debug.LogError($"AssetBundle not found at: '{bundlePath}'!");
            ieLoadAssetBundle = null;
            yield break;
        }

        if (log)
            Debug.Log($"Loading AssetBundle: '{bundleName}'");

        // For other platforms, use AssetBundle.LoadFromFileAsync
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return bundleRequest;

        AssetBundle loadedBundle = bundleRequest.assetBundle;
#endif

        if (loadedBundle == null)
        {
            Debug.LogError($"Failed to load AssetBundle: '{bundleName}'");
            ieLoadAssetBundle = null;
            yield break;
        }

        currentBundle = loadedBundle;
        currentBundleName = bundleName;
        
        if (log)
            Debug.Log($"AssetBundle loaded: '{bundleName}'!");
        ieLoadAssetBundle = null;
    }

    public GameObject GetAsset(string assetName)
    {
        if (log)
            Debug.Log($"Requested asset: '{assetName}'");

        if (currentBundle == null)
        {
            Debug.LogError("No AssetBundle is currently loaded.");
            return null;
        }

        // Check if the asset is already loaded (cached)
        if (assetCache.TryGetValue(assetName, out Object cachedAsset))
        {
            if (log)
                Debug.Log($"Using cached asset: '{assetName}'");
            return Instantiate(cachedAsset as GameObject);
        }

        // Load the asset from the AssetBundle if not cached
        GameObject asset = currentBundle.LoadAsset<GameObject>(assetName);
        if (asset != null)
        {
            if (log)
                Debug.Log($"Loading and caching a fresh copy of asset: '{assetName}'");
            assetCache[assetName] = asset; // Cache the asset for future use
            return Instantiate(asset);
        }
        else
        {
            Debug.LogError($"Asset '{assetName}' not found in AssetBundle.");
            return null;
        }
    }

    public void UnloadBundle()
    {
        if (currentBundle != null)
        {
            if (log)
                Debug.Log($"Unloading AssetBundle: '{currentBundleName}'");
            currentBundle.Unload(true);
            currentBundle = null;
            ClearCache();
        }
    }

    public void ClearCache()
    {
        if (log)
            Debug.Log("Clearing cached assets.");
        assetCache.Clear();
    }

    public bool HasBundleLoaded(string bundleName)
    {
        return currentBundle != null && currentBundleName == bundleName;
    }

    public bool HasBundleLoaded()
    {
        return currentBundle != null;
    }
}
