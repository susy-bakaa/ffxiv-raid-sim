using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static CharacterState;
using static PartyList;
using static StatusEffectData;

public static class Utilities
{
    private static float lastTimeBasedRateLimitAttempt = 0f;

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

    public static bool TimedRateLimiter(float interval)
    {
        if (Time.time - lastTimeBasedRateLimitAttempt >= interval)
        {
            lastTimeBasedRateLimitAttempt = Time.time;
            return true;
        }

        return false;
    }
    /* 
    // Usage:
    float cycleInterval = 1f;

    void Update()
    {
        if (Utilities.RateLimiter(cycleInterval))
        {
            //this will happen once per second
        }
    }
    */

    private class MonoBehaviourHook : MonoBehaviour
    {
        public Action onUpdate;
        public string label;
        public GameObject trackedObject;
        public bool usesUnscaledTime;
        public bool onlyAllowOneInstance;
        public float time;

        void Update()
        {
            if (trackedObject == null)
            {
                // If the tracked object is null, stop the timer and prevent further updates
                FunctionTimer.StopTimer(label);
            }
            else if (onUpdate != null)
            {
                onUpdate();
            }
            else
            {
                FunctionTimer.StopTimer(label);
            }
        }

        void OnDestroy()
        {
            FunctionTimer.StopTimer(label);
        }
    }

    public class FunctionTimer
    {
        private static List<FunctionTimer> activeTimerList;
        private static GameObject initGameObject;
        private static bool eventSubbed = false;
        private static int currentId;

        private static void InitIfNeeded()
        {
            if (!eventSubbed)
            {
                eventSubbed = true;
                SceneManager.activeSceneChanged += CleanUp;
            }

            if (initGameObject == null)
            {
                int newId = UnityEngine.Random.Range(0, 10000);
                do
                {
                    newId = UnityEngine.Random.Range(0, 10000);
                }
                while (currentId == newId);
                currentId = newId;

                initGameObject = new GameObject($"FunctionTimer_Initializer_For_Scene_{SceneManager.GetActiveScene().name.Replace(" ", "_")}_{currentId}");
                activeTimerList = new List<FunctionTimer>();
            }
        }
        public static void CleanUp(Scene scene1, Scene scene2)
        {
            if (string.IsNullOrEmpty(scene1.name))
                return;

            Debug.Log($"FunctionTimer CleanUp called! {scene1.name} {scene2.name}");
            if (activeTimerList != null && activeTimerList.Count > 0)
                activeTimerList.Clear();
            activeTimerList = new List<FunctionTimer>();
            UnityEngine.Object.Destroy(initGameObject);
        }

        public static FunctionTimer Create(MonoBehaviour source, Action action, float timer, string name = null, bool useUnscaledTime = false, bool onlyAllowOneInstance = false)
        {
            InitIfNeeded();

            if (name != null)
                name = name.Replace(" ", "_").Replace("(", "").Replace(")", "");

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

            //Debug.Log($"Utilities: FunctionTimer {name} was created with timer of {timer}. Unscaled {useUnscaledTime} OnlyOne {onlyAllowOneInstance}");

            GameObject gameObject = new GameObject($"FunctionTimer{(name != null ? $"_{name}" : "")}", typeof(MonoBehaviourHook));

            gameObject.transform.SetParent(initGameObject.transform);

            FunctionTimer functionTimer = new FunctionTimer(action, timer, gameObject, name, useUnscaledTime);

            MonoBehaviourHook monoBehaviourHook = gameObject.GetComponent<MonoBehaviourHook>();

            monoBehaviourHook.trackedObject = source.gameObject;
            monoBehaviourHook.onUpdate = functionTimer.Update;
            monoBehaviourHook.label = name != null ? name : "null";
            monoBehaviourHook.usesUnscaledTime = useUnscaledTime;
            monoBehaviourHook.onlyAllowOneInstance = onlyAllowOneInstance;
            monoBehaviourHook.time = timer;

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

    public static bool ContainsCharacterState(this IEnumerable<PartyMember> members, CharacterState state)
    {
        foreach (var member in members)
        {
            if (member.characterState == state)
            {
                return true;
            }
        }
        return false;
    }

    public static bool ContainsInfoPair(this IEnumerable<StatusEffectInfoArray> effectPairs, StatusEffectInfoArray effectPair)
    {
        foreach (var ep in effectPairs)
        {
            if (ep.name == effectPair.name && ep.effectInfos.Length == effectPair.effectInfos.Length)
            {
                for (int i = 0; i < ep.effectInfos.Length; i++)
                {
                    if (ep.effectInfos[i].data == effectPair.effectInfos[i].data && ep.effectInfos[i].name == effectPair.effectInfos[i].name && ep.effectInfos[i].tag == effectPair.effectInfos[i].tag && ep.effectInfos[i].stacks == effectPair.effectInfos[i].stacks)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static void RemoveInfoPair(this List<StatusEffectInfoArray> effectPairs, StatusEffectInfoArray effectPair)
    {
        foreach (var ep in effectPairs)
        {
            if (ep.name == effectPair.name && ep.effectInfos.Length == effectPair.effectInfos.Length)
            {
                for (int i = 0; i < ep.effectInfos.Length; i++)
                {
                    if (ep.effectInfos[i].data == effectPair.effectInfos[i].data && ep.effectInfos[i].name == effectPair.effectInfos[i].name && ep.effectInfos[i].tag == effectPair.effectInfos[i].tag && ep.effectInfos[i].stacks == effectPair.effectInfos[i].stacks)
                    {
                        effectPairs.Remove(ep);
                        return;
                    }
                }
            }
        }
    }

    public static GameObject FindInactiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Inserts automatically spaces before all capital letters in a string. Using '#' you can exlude parts of the string if you do not want capitalization for the whole string.
    /// </summary>
    /// <param name="input">The string being processed.</param>
    /// <returns></returns>
    public static string InsertSpaceBeforeCapitals(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Split the string by '#' and process each segment
        string[] segments = input.Split('#');
        StringBuilder result = new StringBuilder();

        for (int i = 0; i < segments.Length; i++)
        {
            if (i % 2 == 0)
            {
                // Process segments outside of '#' (even indices)
                result.Append(ProcessSegment(segments[i]));
            }
            else
            {
                // Append segments inside '#' without processing (odd indices)
                result.Append(segments[i]);
            }

            // Reassemble with '#' where necessary
            if (i < segments.Length - 1)
            {
                result.Append('#');
            }
        }

        return result.ToString();
    }

    private static string ProcessSegment(string segment)
    {
        // Regular expression pattern to match capital letters not at the start of the string
        string pattern = "(?<!^)([A-Z])";

        // Insert a space before each match
        return Regex.Replace(segment, pattern, " $1");
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