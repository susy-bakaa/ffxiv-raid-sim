using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalStructs
{
    [System.Serializable]
    public struct Damage
    {
        public enum DamageType { none, magic, physical, dark }
        public enum ElementalAspect { none, unaspected, fire, wind, ice, water, lightning, earth }

        public int value;
        public DamageType type;
        public ElementalAspect aspect;
    }
}
