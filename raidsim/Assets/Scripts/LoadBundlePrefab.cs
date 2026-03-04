// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.raidsim.Visuals;

namespace dev.susybaka.raidsim
{
    public class LoadBundlePrefab : MonoBehaviour
    {
        private AssetHandler handler;

        [SerializeField, AssetBundleName] private string bundleName;
        [SerializeField] private string assetName;
        [SerializeField] private Transform _parent;
        [SerializeField] private GameObject placeHolder;

        private SimpleShaderFade fade;

        void OnEnable()
        {
            if (fade == null)
                fade = GetComponent<SimpleShaderFade>();

            if (AssetHandler.Instance.HasBundleLoaded(bundleName))
            {
                GameObject prefab = AssetHandler.Instance.GetAsset(bundleName, assetName);

                if (prefab != null)
                {
                    Destroy(placeHolder);
                    GameObject spawned = Instantiate(prefab, _parent);
                    spawned.transform.localPosition = prefab.transform.localPosition;
                    spawned.transform.localRotation = prefab.transform.localRotation;
                    spawned.transform.localScale = prefab.transform.localScale;
                    if (fade != null)
                    {
                        fade.ReInitialize();
                    }
                }
            }
        }
    }
}