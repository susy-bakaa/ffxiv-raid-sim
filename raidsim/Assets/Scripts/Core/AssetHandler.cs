using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

#if UNITY_EDITOR
using NaughtyAttributes;
#endif

namespace dev.susybaka.raidsim.Core
{
    public class AssetHandler : MonoBehaviour
    {
        public static AssetHandler Instance;

        public string[] sharedBundles = new string[] { "common" };
        public bool useExternalBundles = true;
        public bool disable = false;
        public bool log = true;

        private Dictionary<string, AssetBundle> currentBundles = new Dictionary<string, AssetBundle>();
        //private List<string> currentBundleNames = new List<string>();
        private Coroutine ieLoadAssetBundle;
        private Coroutine ieLoadSharedAssetBundle;

        // Cache for loaded assets (avoids reloading assets from disk)
        private Dictionary<string, Object> assetCache = new Dictionary<string, Object>();
        private Dictionary<string, AssetBundle> currentSharedBundles = new Dictionary<string, AssetBundle>();

        private string bundleExtension = string.Empty;
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
            bundleExtension = GlobalVariables.assetBundleExtension;

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
            if (currentBundles == null || currentBundles.Keys.Count <= 0)
            {
                currentBundles = new Dictionary<string, AssetBundle>();
            }
        }

        #region Common AssetBundles
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
                foreach (string name in names)
                {
                    string bundleName = name;

                    if (!bundleName.EndsWith(bundleExtension))
                        bundleName += bundleExtension;

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
        #endregion

        public void LoadSceneAssetBundle(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.Log("Requested AssetBundle name is an empty string!");
                return;
            }

            LoadSceneAssetBundle(new string[] { bundleName });
        }

        public void LoadSceneAssetBundle(string[] bundleNames)
        {
            // Check if scene has no bundles and unload existing ones if that's the case
            if (bundleNames == null || bundleNames.Length < 1)
            {
                if (log)
                    Debug.Log($"Requested {bundleNames.Length} scene AssetBundles are all empty strings and all bundles will be skipped.");

                if (log && currentBundles != null && currentBundles.Count > 0)
                    Debug.Log($"Unloading {currentBundles.Count} previous scene AssetBundles.");

                UnloadBundles(currentBundles.Keys.ToArray());
                return;
            }

            // Add the extension to all of the bundle names if not already present
            for (int i = 0; i < bundleNames.Length; i++)
            {
                if (!bundleNames[i].EndsWith(bundleExtension))
                    bundleNames[i] += bundleExtension;
            }

            if (currentBundles != null && currentBundles.Count > 0)
            {
                if (log)
                    Debug.Log($"Trying to unload all {currentBundles.Count} unused previous scene AssetBundles.");

                List<string> currentBundleNames = currentBundles.Keys.ToList();
                List<string> previousBundles = currentBundles.Keys.ToList();

                for (int i = 0; i < currentBundleNames.Count; i++)
                {
                    if (previousBundles.Contains(currentBundleNames[i]))
                    {
                        if (log)
                            Debug.Log($"Requested AssetBundle '{currentBundleNames[i]}' of index {i} is already loaded and will not be unloaded.");

                        previousBundles.Remove(currentBundleNames[i]);
                        currentBundleNames.Remove(currentBundleNames[i]);
                        i--;
                    }
                }

                if (log)
                    Debug.Log($"Unloading all {previousBundles.Count} unused previous scene AssetBundles.");

                if (previousBundles.Count <= 0)
                {
                    if (log)
                        Debug.Log("No unused previous AssetBundles to unload.");
                }
                else
                {
                    UnloadBundles(previousBundles.ToArray());
                }
            }

            if (ieLoadAssetBundle == null)
                ieLoadAssetBundle = StartCoroutine(IE_LoadAssetBundle(bundleNames));
        }

        private IEnumerator IE_LoadAssetBundle(string[] bundleNames)
        {
            string bundleName = string.Empty;
            List<string> alreadyLoaded = new List<string>();

            for (int i = 0; i < bundleNames.Length; i++)
            {
                bundleName = bundleNames[i];

                if (currentBundles != null && currentBundles.Count > 0 && currentBundles.ContainsKey(bundleName))
                {
                    if (log)
                        Debug.Log($"Requested AssetBundle '{bundleName}' of index {i} is already loaded!");
                    alreadyLoaded.Add(bundleName);
                    continue;
                }
                else if (!alreadyLoaded.Contains(bundleName))
                {
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
                        Debug.Log($"Loading AssetBundle: '{bundleName}' of index {i}");

                    // For WebGL, use UnityWebRequest to load the asset bundle
                    UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(bundlePath);
                    yield return request.SendWebRequest();

                    if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Failed to load AssetBundle: " + request.error);
                        continue;
                    }

                    AssetBundle loadedBundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
#else
                    if (!File.Exists(bundlePath))
                    {
                        Debug.LogError($"AssetBundle not found at: '{bundlePath}'!");
                        continue;
                    }

                    if (log)
                        Debug.Log($"Loading AssetBundle: '{bundleName}' of index {i}");

                    // For other platforms, use AssetBundle.LoadFromFileAsync
                    AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
                    yield return bundleRequest;

                    AssetBundle loadedBundle = bundleRequest.assetBundle;
#endif

                    if (loadedBundle == null)
                    {
                        Debug.LogError($"Failed to load AssetBundle: '{bundleName}' of index {i}");
                        continue;
                    }

                    currentBundles[bundleName] = loadedBundle;

                    if (log)
                        Debug.Log($"AssetBundle loaded: '{bundleName}' of index {i}!");
                }
            }
            yield return null;
            ieLoadAssetBundle = null;
        }

        public GameObject GetAsset(string bundle, string assetName)
        {
            if (!bundle.EndsWith(bundleExtension))
                bundle += bundleExtension;

            if (log)
                Debug.Log($"Requested asset: '{assetName}'");

            if (currentBundles == null || currentBundles.Count < 1)
            {
                Debug.LogError("No AssetBundles are currently loaded.");
                return null;
            }

            if (string.IsNullOrEmpty(assetName) || string.IsNullOrEmpty(bundle))
            {
                Debug.LogError("Requested Asset or Bundle name is an empty string!");
                return null;
            }

            if (!currentBundles.ContainsKey(bundle))
            {
                Debug.LogError($"AssetBundle '{bundle}' is not currently loaded or is missing.");
                return null;
            }

            // Check if the asset is already loaded (cached)
            if (assetCache.TryGetValue(assetName, out Object cachedAsset))
            {
                if (log)
                    Debug.Log($"Using cached asset: '{assetName}' from bundle '{bundle}'");
                return Instantiate(cachedAsset as GameObject);
            }

            // Load the asset from the AssetBundle if not cached
            GameObject asset = currentBundles[bundle].LoadAsset<GameObject>(assetName);
            if (asset != null)
            {
                if (log)
                    Debug.Log($"Loading and caching a fresh copy of asset: '{assetName}' from bundle '{bundle}'");
                assetCache[assetName] = asset; // Cache the asset for future use
                return Instantiate(asset);
            }
            else
            {
                Debug.LogError($"Asset not found: '{assetName}' from bundle '{bundle}'");
                return null;
            }
        }

        public void UnloadBundles(string[] bundleNames)
        {
            if (bundleNames == null || bundleNames.Length < 1)
            {
                Debug.LogWarning("No AssetBundles to unload. The provided bundle names array is either null or empty.");
                return;
            }

            if (log)
                Debug.Log($"Unloading multiple AssetBundles: {bundleNames.Length}");

            if (currentBundles != null && currentBundles.Count > 0)
            {
                for (int i = 0; i < bundleNames.Length; i++)
                {
                    if (log)
                        Debug.Log($"Unloading AssetBundle: '{bundleNames[i]}' of index {i} out of {bundleNames.Length - 1}");

                    if (currentBundles.ContainsKey(bundleNames[i]))
                    {
                        UnloadBundle(bundleNames[i]);
                    }
                }
            }
        }

        public void UnloadBundle(string bundleName)
        {
            if (currentBundles != null && currentBundles.Count > 0 && currentBundles.ContainsKey(bundleName))
            {
                currentBundles[bundleName].Unload(true);
                currentBundles[bundleName] = null;
                currentBundles.Remove(bundleName);
                ClearCache(); // Clear cached assets since the bundles changed
            }
        }

        // TODO: Update to only clear the cache for the specific bundle that was unloaded instead of all of the cached assets.
        public void ClearCache()
        {
            if (log)
                Debug.Log("Clearing cached assets.");

            assetCache.Clear();
        }

        public bool HasBundleLoaded(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
                return false;

            if (bundleName == "<None>")
                return false;

            if (!bundleName.EndsWith(bundleExtension))
                bundleName += bundleExtension;

            return currentBundles != null && currentBundles.Count > 0 && currentBundles.ContainsKey(bundleName);
        }

        public bool HasAnyBundleLoaded()
        {
            return currentBundles != null && currentBundles.Count > 0;
        }

        private bool BundleNameIsEmptyOrNull(string name)
        {
            if (string.IsNullOrEmpty(name))
                return true;
            if (name == bundleExtension)
                return true;

            return false;
        }
    }
}