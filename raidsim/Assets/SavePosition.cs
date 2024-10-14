using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SavePosition : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    float posX = 0;
    float posY = 0;

    public string group = "";
    public string key = "UnnamedPosition";
    [SerializeField] private float randomDelay = 0.5f;

    private string keyX { get { return $"{key}X"; } }
    private string keyY { get { return $"{key}Y"; } }

    public UnityEvent<Vector2> onStart;

    IniStorage ini;

    void Awake()
    {
        posX = target.anchoredPosition.x;
        posY = target.anchoredPosition.y;
        ini = new IniStorage(GlobalVariables.configPath);
    }

    void Start()
    {
        randomDelay = Random.Range(randomDelay, randomDelay + 0.2f);
        Utilities.FunctionTimer.Create(this, () => OnStart(), Random.Range(1f, 1.25f), $"{group}_{key}_saveposition_onstart_delay", true, false);
    }

    private void OnStart()
    {
        if (ini.Contains(group, $"f{keyX}") && ini.Contains(group, $"f{keyY}"))
        {
            posX = ini.GetFloat(group, $"f{keyX}");
            posY = ini.GetFloat(group, $"f{keyY}");

            target.anchoredPosition = new Vector2(posX, posY);
            onStart.Invoke(target.anchoredPosition);
        }
        //else
        //{
        //    SaveValue(posX, posY);
        //}
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
            ini.Set(group, $"f{keyX}", posX);
            ini.Set(group, $"f{keyY}", posY);
            ini.Save();
        }, randomDelay, $"{group}_{key}_saveposition_savevalue_delay", true, false);
        //Debug.Log($"SaveValue {value} | X {posX} Y {posY} | keyX f{keyX} keyY f{keyY} | {gameObject.name}");
    }
}
