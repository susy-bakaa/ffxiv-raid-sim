using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static CharacterState;

public static class Utilities
{
    public static bool RateLimiter(int frequency)
    {
        if (Time.frameCount % frequency == 0)
        {
            return true;
        }

        else
        {
            return false;
        }
    }
    /* 
    // Usage:
    int cycleFrequency = 30;

    void Update()
    {
        if (Utilities.RateLimiter(cycleFrequency))
        {
            //this will happen once per 30 frames
        }
    }
    */

    private class MonoBehaviourHook : MonoBehaviour
    {
        public Action onUpdate;
        void Update()
        {
            if (onUpdate != null)
                onUpdate();
        }
    }

    public class FunctionTimer
    {
        private static List<FunctionTimer> activeTimerList;
        private static GameObject initGameObject;
        private static void InitIfNeeded()
        {
            if (initGameObject == null)
            {
                initGameObject = new GameObject("FunctionTimer_Initializer");
                activeTimerList = new List<FunctionTimer>();
            }
        }

        public static FunctionTimer Create(Action action, float timer, string name = null, bool useUnscaledTime = false, bool onlyAllowOneInstance = false)
        {
            InitIfNeeded();

            if (onlyAllowOneInstance)
            {
                foreach (var fTimer in activeTimerList)
                {
                    if (fTimer.name == name)
                    {
                        return null;
                    }
                }
            }

            GameObject gameObject = new GameObject("FunctionTimer", typeof(MonoBehaviourHook));

            FunctionTimer functionTimer = new FunctionTimer(action, timer, gameObject, name, useUnscaledTime);

            gameObject.GetComponent<MonoBehaviourHook>().onUpdate = functionTimer.Update;

            activeTimerList.Add(functionTimer);
            return functionTimer;
        }

        private static void RemoveTimer(FunctionTimer functionTimer)
        {
            InitIfNeeded();
            activeTimerList.Remove(functionTimer);
        }

        public static void StopTimer(string name)
        {
            if (activeTimerList != null && activeTimerList.Count > 0)
            {
                for (int i = 0; i < activeTimerList.Count; i++)
                {
                    if (activeTimerList[i].name == name)
                    {
                        activeTimerList[i].DestroySelf();
                        i--;
                    }
                }
            }
        }

        private Action action;
        private float timer;
        private GameObject gameObject;
        private string name;
        private bool isDestroyed;
        private bool useUnscaledTime;

        private FunctionTimer(Action action, float timer, GameObject gameObject, string name, bool useUnscaledTime)
        {
            this.action = action;
            this.timer = timer;
            this.gameObject = gameObject;
            this.name = name;
            if (timer <= 0)
            {
                DestroySelf(true);
            }
            else
                isDestroyed = false;
            this.useUnscaledTime = useUnscaledTime;
        }

        public void Update()
        {
            if (!isDestroyed)
            {
                if (useUnscaledTime)
                    timer -= Time.unscaledDeltaTime;
                else
                    timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    action();
                    DestroySelf();
                }
            }
        }

        private void DestroySelf(bool skipList = false)
        {
            isDestroyed = true;
            UnityEngine.Object.Destroy(gameObject);
            if (!skipList)
                RemoveTimer(this);
        }

        private static void DummyAction()
        {
#if UNITY_EDITOR
            Debug.Log("Dummy action performed.");
#endif
        }
    }

    public static GameObject GetChildWithName<T>(this T obj, string name) where T : Component
    {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);

        if (childTrans != null)
        {
            return childTrans.gameObject;
        }

        return null;
    }

    public static T GetRandomItem<T>(this IList<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        for (var i = list.Count - 1; i > 1; i--)
        {
            var j = UnityEngine.Random.Range(0, i + 1);
            var value = list[j];
            list[j] = list[i];
            list[i] = value;
        }
    }

    public static bool TryAdd<T>(this List<T> list, T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool TryRemove<T>(this List<T> list, T item)
    {
        if (list.Contains(item))
        {
            list.Remove(item);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static float Map(this float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }

    public static int ToInt(this bool from)
    {
        if (from == true)
            return 1;
        else
            return 0;
    }

    public static bool ToBool(this int from)
    {
        if (from > 0)
            return true;
        else
            return false;
    }

    public static bool ToBool(this float from)
    {
        if (from > 0f)
            return true;
        else
            return false;
    }

    public static void Populate<T>(this T[] arr, T value)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
    }

    public static bool ContainsKey(this List<Shield> list, string key)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].key == key)
                return true;
        }
        return false;
    }

    public static void RemoveKey(this List<Shield> list, string key)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].key == key)
                list.RemoveAt(i);
        }
    }

    public static string InsertSpaceBeforeCapitals(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Regular expression pattern to match capital letters not at the start of the string
        string pattern = "(?<!^)([A-Z])";

        // Insert a space before each match
        string result = Regex.Replace(input, pattern, " $1");

        return result;
    }

    public static bool IsReversePitch(this AudioSource source)
    {
        return source.pitch < 0f;
    }

    public static float GetClipRemainingTime(this AudioSource source)
    {
        // Calculate the remainingTime of the given AudioSource,
        // if we keep playing with the same pitch.
        float remainingTime = (source.clip.length - source.time) / source.pitch;
        return source.IsReversePitch() ?
            (source.clip.length + remainingTime) :
            remainingTime;
    }

    public static Vector3 Mul(this Vector3 lv3, Vector3 rv3)
    {
        return Multiply(lv3, rv3);
    }
    public static Vector3 Mul(this Vector3 lv3, Vector2 rv2)
    {
        return Multiply(lv3, new Vector3(rv2.x, rv2.y, 1));
    }
    private static Vector3 Multiply(Vector3 lv3, Vector3 rv3)
    {
        return new Vector3(lv3.x * rv3.x, lv3.y * rv3.y, lv3.z * rv3.z);
    }

    public static Vector3 Sum(this Vector3 lv3, Vector3 rv3)
    {
        return new Vector3(lv3.x + rv3.x, lv3.y + rv3.y, lv3.z + rv3.z);
    }

    public static Vector3 Dif(this Vector3 lv3, Vector3 rv3)
    {
        return new Vector3(lv3.x - rv3.x, lv3.y - rv3.y, lv3.z - rv3.z);
    }

    public static Vector3 Div(this Vector3 lv3, Vector3 rv3)
    {
        return Division(lv3, rv3);
    }
    public static Vector3 Div(this Vector3 lv3, Vector2 rv2)
    {
        return Division(lv3, new Vector3(rv2.x, rv2.y, 1));
    }
    private static Vector3 Division(this Vector3 lv3, Vector3 rv3)
    {
        return new Vector3(lv3.x / rv3.x, lv3.y / rv3.y, lv3.z / rv3.z);
    }

    public static Vector2 Mul(this Vector2 lv2, Vector2 rv2)
    {
        return Multiply(lv2, rv2);
    }
    public static Vector2 Mul(this Vector2 lv2, Vector3 rv3)
    {
        return Multiply(lv2, new Vector2(rv3.x, rv3.y));
    }
    private static Vector2 Multiply(this Vector2 lv2, Vector2 rv2)
    {
        return new Vector2(lv2.x * rv2.x, lv2.y * rv2.y);
    }

    public static Vector2 Sum(this Vector2 lv2, Vector3 rv2)
    {
        return new Vector2(lv2.x + rv2.x, lv2.y + rv2.y);
    }

    public static Vector2 Dif(this Vector2 lv2, Vector2 rv2)
    {
        return new Vector2(lv2.x - rv2.x, lv2.y - rv2.y);
    }

    public static Vector2 Div(this Vector2 lv2, Vector2 rv2)
    {
        return Division(lv2, rv2);
    }
    public static Vector2 Div(this Vector2 lv2, Vector3 rv3)
    {
        return Division(lv2, new Vector2(rv3.x, rv3.y));
    }
    private static Vector2 Division(this Vector2 lv2, Vector2 rv2)
    {
        return new Vector2(lv2.x / rv2.x, lv2.y / rv2.y);
    }
}

public class GameObjectSorter : IComparer
{
    // Calls CaseInsensitiveComparer.Compare on the gameObject name string.
    int IComparer.Compare(System.Object x, System.Object y)
    {
        return ((new CaseInsensitiveComparer()).Compare(((GameObject)x).name, ((GameObject)y).name));
    }
}

public static class AnimatorExtensions
{
    public static void SwitchAnimatorController(this Animator animator, RuntimeAnimatorController animatorController)
    {
        // save states
        List<AnimParam> parms = SaveAnimationStates(animator);
        float[] layerWeights = SaveLayerWeights(animator);
        AnimatorStateInfo[] animatorStateInfo = SaveLayerStateInfo(animator);

        // swap
        animator.runtimeAnimatorController = animatorController;

        // restore states
        RerstoreAnimationState(parms, animator);
        RestoreLayerWeights(animator, layerWeights);
        RestaureLayerStateInfor(animator, animatorStateInfo);
    }

    private static AnimatorStateInfo[] SaveLayerStateInfo(Animator animator)
    {
        int animatorLayerCount = animator.layerCount;
        AnimatorStateInfo[] animatorStateInfo = new AnimatorStateInfo[animatorLayerCount];
        for (int i = 0; i < animatorLayerCount; i++)
        {
            animatorStateInfo[i] = animator.GetCurrentAnimatorStateInfo(i);
        }
        return animatorStateInfo;
    }

    private static void RestaureLayerStateInfor(Animator animator, AnimatorStateInfo[] animatorStateInfo)
    {
        for (int i = 0; i < animator.layerCount; i++)
        {
            animator.Play(animatorStateInfo[i].shortNameHash, i, animatorStateInfo[i].normalizedTime);
        }
    }

    private static float[] SaveLayerWeights(Animator animator)
    {
        int animatorLayerCount = animator.layerCount;
        float[] layerWeights = new float[animatorLayerCount];
        for (int i = 0; i < animatorLayerCount; i++)
        {
            layerWeights[i] = animator.GetLayerWeight(i);
        }
        return layerWeights;
    }

    private static void RestoreLayerWeights(Animator animator, float[] layerWeights)
    {
        for (int i = 0; i < layerWeights.Length; i++)
        {
            animator.SetLayerWeight(i, layerWeights[i]);
        }
    }

    private class AnimParam
    {
        public AnimatorControllerParameterType type;
        public string paramName;
        object data;

        public AnimParam(Animator animator, string paramName, AnimatorControllerParameterType type)
        {
            this.type = type;
            this.paramName = paramName;
            switch (type)
            {
                case AnimatorControllerParameterType.Int:
                    this.data = (int)animator.GetInteger(paramName);
                    break;
                case AnimatorControllerParameterType.Float:
                    this.data = (float)animator.GetFloat(paramName);
                    break;
                case AnimatorControllerParameterType.Bool:
                    this.data = (bool)animator.GetBool(paramName);
                    break;
            }
        }

        public object getData()
        {
            return data;
        }
    }

    static List<AnimParam> SaveAnimationStates(Animator animator)
    {
        List<AnimParam> parms = new List<AnimParam>();
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            AnimParam ap = new AnimParam(animator, p.name, p.type);
            parms.Add(ap);
        }
        return parms;
    }

    static void RerstoreAnimationState(List<AnimParam> parms, Animator animator)
    {
        foreach (AnimParam p in parms)
        {
            switch (p.type)
            {
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(p.paramName, (int)p.getData());
                    break;
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(p.paramName, (float)p.getData());
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(p.paramName, (bool)p.getData());
                    break;
            }
        }
    }
}

/// <summary>
/// Vector3 extension methods and extra functions.
/// </summary>
public struct Vector3e
{
    /// <summary>
    /// Default Vector3 value. This value is represented as (-1, -1, -1).
    /// </summary>
    public static Vector3 Default()
    {
        return new Vector3(-1, -1, -1);
    }
    /// <summary>
    /// Multiply two Vector3 values together. Returns the result.
    /// </summary>
    public static Vector3 Multiply(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
    /// <summary>
    /// Subtract two Vector3 values together. Returns the result.
    /// </summary>
    public static Vector3 Subtract(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }
    /// <summary>
    /// Sum two Vector3 values together. Returns the result.
    /// </summary>
    public static Vector3 Sum(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }
    /// <summary>
    /// Divide two Vector3 values together. Returns the result.
    /// </summary>
    public static Vector3 Divide(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
    }
}
/// <summary>
/// Vector2 extension methods and extra functions.
/// </summary>
public struct Vector2e
{
    /// <summary>
    /// Default Vector2 value. This value is represented as (-1, -1).
    /// </summary>
    public static Vector2 Default()
    {
        return new Vector2(-1, -1);
    }
    /// <summary>
    /// Check if Vector2 v1 is bigger (true) or smaller (false) than Vector2 v2.
    /// </summary>
    public static bool IsBigger(Vector3 v1, Vector3 v2)
    {
        if (v1.x > v2.x && v1.y > v2.y)
            return true;
        else
            return false;
    }
    /// <summary>
    /// Check if Vector2 v1 is smaller (true) or bigger (false) than Vector2 v2.
    /// </summary>
    public static bool IsSmaller(Vector3 v1, Vector3 v2)
    {
        if (v1.x < v2.x && v1.y < v2.y)
            return true;
        else
            return false;
    }
    /// <summary>
    /// Check if Vector2 v1 is equal or bigger (true) or not equal and smaller (false) than Vector2 v2.
    /// </summary>
    public static bool IsBiggerOrEqual(Vector3 v1, Vector3 v2)
    {
        if (v1.x >= v2.x && v1.y >= v2.y)
            return true;
        else
            return false;
    }
    /// <summary>
    /// Check if Vector2 v1 is equal or smaller (true) or not equal and bigger (false) than Vector2 v2.
    /// </summary>
    public static bool IsSmallerOrEqual(Vector3 v1, Vector3 v2)
    {
        if (v1.x <= v2.x && v1.y <= v2.y)
            return true;
        else
            return false;
    }
}