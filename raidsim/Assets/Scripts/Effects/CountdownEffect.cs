using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    public class CountdownEffect : MonoBehaviour
    {
        public Transform target;
        public int userShaderFadeMaterial = -1;
        public Texture[] textures;

        private SimpleShaderFade shaderFade;
        private bool visible = false;
        private Material mat;
        private int textureHash = Shader.PropertyToID("_Main");

        private void Start()
        {
            shaderFade = GetComponent<SimpleShaderFade>();
            mat = target.GetComponent<Renderer>().material;
            if (textures != null && textures.Length > 0)
            {
                mat.SetTexture(textureHash, textures[textures.Length - 1]);
            }
            if (shaderFade != null)
            {
                shaderFade.FadeOut(0f);
                visible = false;
            }
            else
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        public void SetTexture(float time)
        {
            if (textures != null && textures.Length > 0)
            {
                int index = Mathf.RoundToInt(time - 1f);
                //Debug.Log($"index {index} time {time}");
                SetTexture(index);
            }
        }

        public void SetTexture(int index)
        {
            if (textures != null && textures.Length > 0 && index < textures.Length && index > -1)
            {
                mat.SetTexture(textureHash, textures[index]);
                //Debug.Log($"SetTexture {index}");
                if (!visible)
                {
                    if (shaderFade != null)
                    {
                        shaderFade.FadeIn(0.25f);
                    }
                    else
                    {
                        foreach (Transform child in transform)
                        {
                            child.gameObject.SetActive(true);
                        }
                    }
                    visible = true;
                }
            }
            else if (textures != null && (index < 0 || index >= textures.Length))
            {
                if (visible)
                {
                    if (shaderFade != null)
                    {
                        shaderFade.FadeOut(0.25f);
                    }
                    else
                    {
                        foreach (Transform child in transform)
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                    visible = false;
                }
            }
            else
            {
                if (visible)
                {
                    if (shaderFade != null)
                    {
                        shaderFade.FadeOut(0.25f);
                    }
                    else
                    {
                        foreach (Transform child in transform)
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                    visible = false;
                }
            }
        }
    }
}