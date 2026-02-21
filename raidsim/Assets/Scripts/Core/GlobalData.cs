// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Characters;

namespace dev.susybaka.raidsim.Core
{
    public static class GlobalData
    {
        public enum Role { meleeDps, magicalRangedDps, physicalRangedDps, tank, healer, unassigned }
        public enum Sector { N, E, S, W }
        public enum SubSector { NE, SE, SW, NW }
        public enum RngMode { PureRandom, NoRepeatConsecutive, ShuffleBag }
        public enum SlotKind { Empty, Action, Macro }
        public enum RecastType { standard, longGcd, stackedOgcd }

        public struct ActionInfo
        {
            public CharacterAction action;
            public CharacterState source;
            public CharacterState target;
            public bool sourceIsPlayer;
            public bool targetIsPlayer;

            public ActionInfo(CharacterAction action, CharacterState source, CharacterState target)
            {
                this.action = action;
                this.source = source;
                this.target = target;
                sourceIsPlayer = false;
                targetIsPlayer = false;

                if (source != null && source == FightTimeline.Instance.player)
                {
                    sourceIsPlayer = true;
                }
                if (target != null && target == FightTimeline.Instance.player)
                {
                    targetIsPlayer = true;
                }
            }
        }

        [System.Serializable]
        public struct Damage
        {
            public enum DamageApplicationType { normal, percentage, percentageFromMax, set }
            public enum DamageType { none, magical, physical, unique }
            public enum ElementalAspect { none, unaspected, fire, ice, lightning, water, wind, earth, dark, light }
            public enum PhysicalAspect { none, slashing, piercing, blunt }

            public string name;
            public long value;
            public bool negative;
            public bool ignoreDamageReductions;
            public DamageType type;
            public ElementalAspect elementalAspect;
            public PhysicalAspect physicalAspect;
            public DamageApplicationType applicationType;
            public CharacterState source;

            public Damage(Damage copy, string name = "")
            {
                if (string.IsNullOrEmpty(name))
                    this.name = copy.name;
                else
                    this.name = name;
                value = copy.value;
                type = copy.type;
                negative = copy.negative;
                ignoreDamageReductions = copy.ignoreDamageReductions;
                elementalAspect = copy.elementalAspect;
                physicalAspect = copy.physicalAspect;
                applicationType = copy.applicationType;
                source = copy.source;
            }

            public Damage(Damage copy, long value, string name = "")
            {
                if (string.IsNullOrEmpty(name))
                    this.name = copy.name;
                else
                    this.name = name;
                this.value = value;
                negative = copy.negative;
                ignoreDamageReductions = copy.ignoreDamageReductions;
                type = copy.type;
                elementalAspect = copy.elementalAspect;
                physicalAspect = copy.physicalAspect;
                applicationType = copy.applicationType;
                source = copy.source;
            }

            public Damage(long value, bool negative, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                ignoreDamageReductions = false;
                type = DamageType.magical;
                elementalAspect = ElementalAspect.unaspected;
                physicalAspect = PhysicalAspect.none;
                applicationType = DamageApplicationType.normal;
                source = null;
            }

            public Damage(long value, bool negative, CharacterState source, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                ignoreDamageReductions = false;
                type = DamageType.magical;
                elementalAspect = ElementalAspect.unaspected;
                physicalAspect = PhysicalAspect.none;
                applicationType = DamageApplicationType.normal;
                this.source = source;
            }

            public Damage(Damage copy, bool negative, string name = "")
            {
                if (string.IsNullOrEmpty(name))
                    this.name = copy.name;
                else
                    this.name = name;
                value = copy.value;
                this.negative = negative;
                ignoreDamageReductions = copy.ignoreDamageReductions;
                type = copy.type;
                elementalAspect = copy.elementalAspect;
                physicalAspect = copy.physicalAspect;
                applicationType = copy.applicationType;
                source = copy.source;
            }

            public Damage(Damage copy, CharacterState source, string name = "")
            {
                if (string.IsNullOrEmpty(name))
                    this.name = copy.name;
                else
                    this.name = name;
                value = copy.value;
                negative = copy.negative;
                ignoreDamageReductions = copy.ignoreDamageReductions;
                type = copy.type;
                elementalAspect = copy.elementalAspect;
                physicalAspect = copy.physicalAspect;
                applicationType = copy.applicationType;
                this.source = source;
            }

            public Damage(Damage copy, long value, bool negative, string name = "")
            {
                if (string.IsNullOrEmpty(name))
                    this.name = copy.name;
                else
                    this.name = name;
                this.value = value;
                this.negative = negative;
                ignoreDamageReductions = copy.ignoreDamageReductions;
                type = copy.type;
                elementalAspect = copy.elementalAspect;
                physicalAspect = copy.physicalAspect;
                applicationType = copy.applicationType;
                source = copy.source;
            }

            public Damage(long value, bool negative, bool ignoreDamageReductions, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                this.ignoreDamageReductions = ignoreDamageReductions;
                type = DamageType.magical;
                elementalAspect = ElementalAspect.unaspected;
                physicalAspect = PhysicalAspect.none;
                applicationType = DamageApplicationType.normal;
                source = null;
            }

            public Damage(long value, bool negative, bool ignoreDamageReductions, DamageApplicationType applicationType, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                this.ignoreDamageReductions = ignoreDamageReductions;
                type = DamageType.magical;
                elementalAspect = ElementalAspect.unaspected;
                physicalAspect = PhysicalAspect.none;
                this.applicationType = applicationType;
                source = null;
            }

            public Damage(long value, bool negative, bool ignoreDamageReductions, DamageType type, PhysicalAspect physicalAspect, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                this.ignoreDamageReductions = ignoreDamageReductions;
                this.type = type;
                elementalAspect = ElementalAspect.unaspected;
                this.physicalAspect = physicalAspect;
                applicationType = DamageApplicationType.normal;
                source = null;
            }

            public Damage(long value, bool negative, bool ignoreDamageReductions, DamageType type, PhysicalAspect physicalAspect, DamageApplicationType applicationType, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                this.ignoreDamageReductions = ignoreDamageReductions;
                this.type = type;
                elementalAspect = ElementalAspect.unaspected;
                this.physicalAspect = physicalAspect;
                this.applicationType = applicationType;
                source = null;
            }

            public Damage(long value, bool negative, bool ignoreDamageReductions, ElementalAspect elementalAspect, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                this.ignoreDamageReductions = ignoreDamageReductions;
                type = DamageType.magical;
                this.elementalAspect = elementalAspect;
                physicalAspect = PhysicalAspect.none;
                applicationType = DamageApplicationType.normal;
                source = null;
            }

            public Damage(long value, bool negative, bool ignoreDamageReductions, ElementalAspect elementalAspect, DamageApplicationType applicationType, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                this.ignoreDamageReductions = ignoreDamageReductions;
                type = DamageType.magical;
                this.elementalAspect = elementalAspect;
                physicalAspect = PhysicalAspect.none;
                this.applicationType = applicationType;
                source = null;
            }

            public Damage(long value, bool negative, bool ignoreDamageReductions, DamageType type, ElementalAspect elementalAspect, PhysicalAspect physicalAspect, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                this.ignoreDamageReductions = ignoreDamageReductions;
                this.type = type;
                this.elementalAspect = elementalAspect;
                this.physicalAspect = physicalAspect;
                applicationType = DamageApplicationType.normal;
                source = null;
            }

            public Damage(long value, bool negative, bool ignoreDamageReductions, DamageType type, ElementalAspect elementalAspect, PhysicalAspect physicalAspect, DamageApplicationType applicationType, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                this.ignoreDamageReductions = ignoreDamageReductions;
                this.type = type;
                this.elementalAspect = elementalAspect;
                this.physicalAspect = physicalAspect;
                this.applicationType = applicationType;
                source = null;
            }

            public Damage(long value, bool negative, bool ignoreDamageReductions, DamageType type, ElementalAspect elementalAspect, PhysicalAspect physicalAspect, DamageApplicationType applicationType, CharacterState source, string name = "")
            {
                this.name = name;
                this.value = value;
                this.negative = negative;
                this.ignoreDamageReductions = ignoreDamageReductions;
                this.type = type;
                this.elementalAspect = elementalAspect;
                this.physicalAspect = physicalAspect;
                this.applicationType = applicationType;
                this.source = source;
            }
        }

        public struct CharacterCollection
        {
            public List<CharacterState> values;

            public CharacterCollection(List<CharacterState> values)
            {
                this.values = values;
            }

            public CharacterCollection(CharacterState[] values)
            {
                this.values = new List<CharacterState>();
                this.values.AddRange(values);
            }
        }

        [System.Serializable]
        public class Flag
        {
            public enum AggregateLogic
            {
                AllTrue,   // AND: All flags must be true
                AnyTrue,   // OR: At least one flag must be true
                Threshold  // At least a certain percentage of flags must be true
            }

            public string name;
            public List<FlagValue> values = new List<FlagValue>(); // Unity-friendly serialization
            private Dictionary<string, bool> runtimeDictionary; // Backing dictionary for runtime lookups
            private bool isDirty = true; // Tracks if the dictionary needs updating

            public AggregateLogic logic = AggregateLogic.AllTrue;
            [Min(0f)] public float thresholdPercentage = 0.0f;

            public bool value => Evaluate();

            public Flag(Flag copyFrom)
            {
                name = copyFrom.name;
                logic = copyFrom.logic;
                thresholdPercentage = copyFrom.thresholdPercentage;
                values = new List<FlagValue>(copyFrom.values);
            }

            public Flag(string name, AggregateLogic logic = AggregateLogic.AllTrue, float thresholdPercentage = 0.0f)
            {
                this.name = name;
                this.logic = logic;
                this.thresholdPercentage = thresholdPercentage;
                this.values = new List<FlagValue>();
            }

            public Flag(string name, List<FlagValue> values = null, AggregateLogic logic = AggregateLogic.AllTrue, float thresholdPercentage = 0.0f)
            {
                this.name = name;
                this.logic = logic;
                this.thresholdPercentage = thresholdPercentage;

                if (values != null)
                {
                    this.values = values;
                    isDirty = true; // Mark dictionary for update
                }
                else
                {
                    this.values = new List<FlagValue>();
                }
            }

            public void SetFlag(string name, bool value)
            {
                FlagValue existing = values.Find(v => v.name == name);
                if (existing != null)
                {
                    existing.value = value;
                }
                else
                {
                    values.Add(new FlagValue(name, value));
                }

                isDirty = true; // Mark dictionary for update
            }

            public void RemoveFlag(string name)
            {
                if (values.RemoveAll(v => v.name == name) > 0)
                {
                    isDirty = true; // Mark dictionary for update
                }
            }

            public void ResetFlag()
            {
                values.RemoveAll(v => v.name != "base" && v.name != "toggleTargetable");
                isDirty = true; // Mark dictionary for update
            }

            public void ForceUpdate()
            {
                isDirty = true; // Mark dictionary for update
                UpdateRuntimeDictionary();
            }

            private void UpdateRuntimeDictionary()
            {
                if (isDirty)
                {
                    runtimeDictionary = new Dictionary<string, bool>();
                    foreach (var flagValue in values)
                    {
                        if (!runtimeDictionary.ContainsKey(flagValue.name))
                        {
                            runtimeDictionary.Add(flagValue.name, flagValue.value);
                        }
                        else
                        {
                            Debug.LogWarning($"Duplicate key '{flagValue.name}' found in Flag '{name}'. Skipping this entry.");
                        }
                    }
                    isDirty = false;
                }
            }

            public bool GetFlagValue(string key)
            {
                UpdateRuntimeDictionary();
                return runtimeDictionary.TryGetValue(key, out var value) ? value : false;
            }

            private bool Evaluate()
            {
                UpdateRuntimeDictionary();

                switch (logic)
                {
                    case AggregateLogic.AllTrue:
                        if (runtimeDictionary != null && runtimeDictionary.Count > 0)
                            return !runtimeDictionary.ContainsValue(false);
                        else
                            return false;

                    case AggregateLogic.AnyTrue:
                        if (runtimeDictionary != null && runtimeDictionary.Count > 0)
                            return runtimeDictionary.ContainsValue(true);
                        else
                            return false;

                    case AggregateLogic.Threshold:
                        if (runtimeDictionary == null || runtimeDictionary.Count == 0)
                            return false;
                        int trueCount = runtimeDictionary.Values.Count(v => v);
                        float percentage = (float)trueCount / runtimeDictionary.Count * 100;
                        return percentage >= thresholdPercentage;

                    default:
                        throw new InvalidOperationException("Unsupported logic type.");
                }
            }

            [System.Serializable]
            public class FlagValue
            {
                public string name;
                public bool value;

                public FlagValue(string name, bool value)
                {
                    this.name = name;
                    this.value = value;
                }
            }
        }

        [System.Serializable]
        public struct Axis
        {
            public bool x;
            public bool y;
            public bool z;

            public Axis(bool x, bool y, bool z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public bool All()
            {
                return x && y && z;
            }

            public bool Any()
            {
                return x || y || z;
            }

            public bool None()
            {
                return !x && !y && !z;
            }
        }

        [System.Serializable]
        public struct IndexMapping
        {
            public string name;
            public int previousIndex;
            public int nextIndex;
        }

        [System.Serializable]
        public struct RoleSelection
        {
            public string name;
            public List<Role> roles;

            public RoleSelection(string name, List<Role> roles)
            {
                this.name = name;
                this.roles = roles;
            }

            public RoleSelection(string name, Role role)
            {
                this.name = name;
                this.roles = new List<Role> { role };
            }

            public static bool operator ==(RoleSelection obj1, RoleSelection obj2)
            {
                return obj1.Equals(obj2);
            }

            public static bool operator !=(RoleSelection obj1, RoleSelection obj2)
            {
                return !obj1.Equals(obj2);
            }

            public override bool Equals(object obj)
            {
                if (obj is RoleSelection)
                {
                    RoleSelection other = (RoleSelection)obj;
                    return this.name == other.name && this.roles.SequenceEqual(other.roles);
                }
                return false;
            }

            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 23 + (name != null ? name.GetHashCode() : 0);
                hash = hash * 23 + (roles != null ? roles.GetHashCode() : 0);
                return hash;
            }
        }

        [System.Serializable]
        public struct Transformation
        {
            public string name;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;
            public bool relative;

            public Transformation(string name, Vector3 position, Vector3 rotation, Vector3 scale, bool relative)
            {
                this.name = name;
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
                this.relative = relative;
            }
        }

        [System.Serializable]
        public struct SlotBinding
        {
            public SlotKind kind;
            public string id; // ActionId or MacroId (or empty)
        }
    }
}