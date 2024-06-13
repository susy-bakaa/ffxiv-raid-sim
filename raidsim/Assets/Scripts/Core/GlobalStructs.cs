public static class GlobalStructs
{
    [System.Serializable]
    public struct Damage
    {
        public enum DamageApplicationType { normal, percentage, percentageFromMax, set }
        public enum DamageType { none, magical, physical, unique }
        public enum ElementalAspect { none, unaspected, fire, ice, lightning, water, wind, earth, dark, light }
        public enum PhysicalAspect { none, slashing, piercing, blunt }

        public string name;
        public int value;
        public bool negative;
        public bool ignoreDamageReductions;
        public DamageType type;
        public ElementalAspect elementalAspect;
        public PhysicalAspect physicalAspect;
        public DamageApplicationType applicationType;

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
        }

        public Damage(Damage copy, int value, string name = "")
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
        }

        public Damage(int value, bool negative, string name = "")
        {
            this.name = name;
            this.value = value;
            this.negative = negative;
            ignoreDamageReductions = false;
            type = DamageType.magical;
            elementalAspect = ElementalAspect.unaspected;
            physicalAspect = PhysicalAspect.none;
            applicationType = DamageApplicationType.normal;
        }

        public Damage(int value, bool negative, bool ignoreDamageReductions, string name = "")
        {
            this.name = name;
            this.value = value;
            this.negative = negative;
            this.ignoreDamageReductions = ignoreDamageReductions;
            type = DamageType.magical;
            elementalAspect = ElementalAspect.unaspected;
            physicalAspect = PhysicalAspect.none;
            applicationType = DamageApplicationType.normal;
        }

        public Damage(int value, bool negative, bool ignoreDamageReductions, DamageApplicationType applicationType, string name = "")
        {
            this.name = name;
            this.value = value;
            this.negative = negative;
            this.ignoreDamageReductions = ignoreDamageReductions;
            type = DamageType.magical;
            elementalAspect = ElementalAspect.unaspected;
            physicalAspect = PhysicalAspect.none;
            this.applicationType = applicationType;
        }

        public Damage(int value, bool negative, bool ignoreDamageReductions, DamageType type, PhysicalAspect physicalAspect, string name = "")
        {
            this.name = name;
            this.value = value;
            this.negative = negative;
            this.ignoreDamageReductions = ignoreDamageReductions;
            this.type = type;
            elementalAspect = ElementalAspect.unaspected;
            this.physicalAspect = physicalAspect;
            applicationType = DamageApplicationType.normal;
        }

        public Damage(int value, bool negative, bool ignoreDamageReductions, DamageType type, PhysicalAspect physicalAspect, DamageApplicationType applicationType, string name = "")
        {
            this.name = name;
            this.value = value;
            this.negative = negative;
            this.ignoreDamageReductions = ignoreDamageReductions;
            this.type = type;
            elementalAspect = ElementalAspect.unaspected;
            this.physicalAspect = physicalAspect;
            this.applicationType = applicationType;
        }

        public Damage(int value, bool negative, bool ignoreDamageReductions, ElementalAspect elementalAspect, string name = "")
        {
            this.name = name;
            this.value = value;
            this.negative = negative;
            this.ignoreDamageReductions = ignoreDamageReductions;
            type = DamageType.magical;
            this.elementalAspect = elementalAspect;
            physicalAspect = PhysicalAspect.none;
            applicationType = DamageApplicationType.normal;
        }

        public Damage(int value, bool negative, bool ignoreDamageReductions, ElementalAspect elementalAspect, DamageApplicationType applicationType, string name = "")
        {
            this.name = name;
            this.value = value;
            this.negative = negative;
            this.ignoreDamageReductions = ignoreDamageReductions;
            type = DamageType.magical;
            this.elementalAspect = elementalAspect;
            physicalAspect = PhysicalAspect.none;
            this.applicationType = applicationType;
        }

        public Damage(int value, bool negative, bool ignoreDamageReductions, DamageType type, ElementalAspect elementalAspect, PhysicalAspect physicalAspect, string name = "")
        {
            this.name = name;
            this.value = value;
            this.negative = negative;
            this.ignoreDamageReductions = ignoreDamageReductions;
            this.type = type;
            this.elementalAspect = elementalAspect;
            this.physicalAspect = physicalAspect;
            applicationType = DamageApplicationType.normal;
        }

        public Damage(int value, bool negative, bool ignoreDamageReductions, DamageType type, ElementalAspect elementalAspect, PhysicalAspect physicalAspect, DamageApplicationType applicationType, string name = "")
        {
            this.name = name;
            this.value = value;
            this.negative = negative;
            this.ignoreDamageReductions = ignoreDamageReductions;
            this.type = type;
            this.elementalAspect = elementalAspect;
            this.physicalAspect = physicalAspect;
            this.applicationType = applicationType;
        }
    }
}
