using UnityEngine;
using UnityEngine.Events;
using dev.illa4257;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.SaveLoad
{
    public class SavePosition : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        float posX = 0;
        float posY = 0;
        float defaultPosX;
        float defaultPosY;

        public string group = "";
        public string key = "UnnamedPosition";
        public string id = string.Empty;
        [SerializeField] private float randomDelay = 0.5f;

        private string keyX { get { return $"{key}X"; } }
        private string keyY { get { return $"{key}Y"; } }

        public UnityEvent<Vector2> onStart;

        IniStorage ini;

        private void Awake()
        {
            defaultPosX = target.anchoredPosition.x;
            defaultPosY = target.anchoredPosition.y;
            posX = defaultPosX;
            posY = defaultPosY;
            ini = new IniStorage(GlobalVariables.configPath);
        }

        private void Start()
        {
            randomDelay = Random.Range(randomDelay, randomDelay + 0.2f);
            Utilities.FunctionTimer.Create(this, () => OnStart(), Random.Range(1f, 1.25f), $"{group}_{key}{id}_saveposition_onstart_delay", true, false);
        }

        private void OnStart()
        {
            if (ini.Contains(group, $"f{keyX}{id}") && ini.Contains(group, $"f{keyY}{id}"))
            {
                posX = ini.GetFloat(group, $"f{keyX}{id}");
                posY = ini.GetFloat(group, $"f{keyY}{id}");

                target.anchoredPosition = new Vector2(posX, posY);
                onStart.Invoke(target.anchoredPosition);
            }
            else
            {
                target.anchoredPosition = new Vector2(defaultPosX, defaultPosY);
            }
        }

        public void SaveValue(float x, float y)
        {
            SaveValue(new Vector2(x, y));
        }

        public void SaveValue(Vector2 value)
        {
            Utilities.FunctionTimer.Create(this, () => {
                ini.Load(GlobalVariables.configPath);
                posX = value.x;
                posY = value.y;
                ini.Set(group, $"f{keyX}{id}", posX);
                ini.Set(group, $"f{keyY}{id}", posY);
                ini.Save();
            }, randomDelay, $"{group}_{key}{id}_saveposition_savevalue_delay", true, false);
        }

        public void Reload()
        {
            Start();
        }
    }
}