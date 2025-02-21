using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class AssetHandler : MonoBehaviour
{
    public static AssetHandler Instance;

    public bool log = true;

    private AssetBundle currentBundle;
    private string currentBundleName;
    private Coroutine ieLoadAssetBundle;

    // Cache for loaded assets (avoids reloading assets from disk)
    private Dictionary<string, Object> assetCache = new Dictionary<string, Object>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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

        string bundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
        if (!File.Exists(bundlePath))
        {
            Debug.LogError($"AssetBundle not found at: '{bundlePath}'!");
            ieLoadAssetBundle = null;
            yield break;
        }

        if (log)
            Debug.Log($"Loading AssetBundle: '{bundleName}'");
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return bundleRequest;

        currentBundle = bundleRequest.assetBundle;
        currentBundleName = bundleName;

        if (currentBundle == null)
        {
            Debug.LogError($"Failed to load AssetBundle: '{bundleName}'!");
            ieLoadAssetBundle = null;
            yield break;
        }
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
