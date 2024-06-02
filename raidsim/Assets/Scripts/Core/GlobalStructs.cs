public static class GlobalStructs
{
    [System.Serializable]
    public struct Damage
    {
        public enum DamageApplicationType { normal, percentage, percentageFromMax, set }
        public enum DamageType { none, magical, physical, unique }
        public enum ElementalAspect { none, unaspected, fire, ice, lightning, water, wind, earth, dark, light }
        public enum PhysicalAspect { none, slashing, piercing, blunt }

        public int value;
        public bool negative;
        public DamageType type;
        public ElementalAspect elementalAspect;
        public PhysicalAspect physicalAspect;
        public DamageApplicationType applicationType;

        public Damage(Damage copy)
        {
            value = copy.value;
            type = copy.type;
            negative = copy.negative;
            elementalAspect = copy.elementalAspect;
            physicalAspect = copy.physicalAspect;
            applicationType = copy.applicationType;
        }

        public Damage(Damage copy, int value)
        {
            this.value = value;
            negative = copy.negative;
            type = copy.type;
            elementalAspect = copy.elementalAspect;
            physicalAspect = copy.physicalAspect;
            applicationType = copy.applicationType;
        }

        public Damage(int value, bool negative)
        {
            this.value = value;
            this.negative = negative;
            type = DamageType.magical;
            elementalAspect = ElementalAspect.unaspected;
            physicalAspect = PhysicalAspect.none;
            applicationType = DamageApplicationType.normal;
        }

        public Damage(int value, bool negative, DamageApplicationType applicationType)
        {
            this.value = value;
            this.negative = negative;
            type = DamageType.magical;
            elementalAspect = ElementalAspect.unaspected;
            physicalAspect = PhysicalAspect.none;
            this.applicationType = applicationType;
        }

        public Damage(int value, bool negative, DamageType type, PhysicalAspect physicalAspect)
        {
            this.value = value;
            this.negative = negative;
            this.type = type;
            elementalAspect = ElementalAspect.unaspected;
            this.physicalAspect = physicalAspect;
            applicationType = DamageApplicationType.normal;
        }

        public Damage(int value, bool negative, DamageType type, PhysicalAspect physicalAspect, DamageApplicationType applicationType)
        {
            this.value = value;
            this.negative = negative;
            this.type = type;
            elementalAspect = ElementalAspect.unaspected;
            this.physicalAspect = physicalAspect;
            this.applicationType = applicationType;
        }

        public Damage(int value, bool negative, ElementalAspect elementalAspect)
        {
            this.value = value;
            this.negative = negative;
            type = DamageType.magical;
            this.elementalAspect = elementalAspect;
            physicalAspect = PhysicalAspect.none;
            applicationType = DamageApplicationType.normal;
        }

        public Damage(int value, bool negative, ElementalAspect elementalAspect, DamageApplicationType applicationType)
        {
            this.value = value;
            this.negative = negative;
            type = DamageType.magical;
            this.elementalAspect = elementalAspect;
            physicalAspect = PhysicalAspect.none;
            this.applicationType = applicationType;
        }

        public Damage(int value, bool negative, DamageType type, ElementalAspect elementalAspect, PhysicalAspect physicalAspect)
        {
            this.value = value;
            this.negative = negative;
            this.type = type;
            this.elementalAspect = elementalAspect;
            this.physicalAspect = physicalAspect;
            applicationType = DamageApplicationType.normal;
        }

        public Damage(int value, bool negative, DamageType type, ElementalAspect elementalAspect, PhysicalAspect physicalAspect, DamageApplicationType applicationType)
        {
            this.value = value;
            this.negative = negative;
            this.type = type;
            this.elementalAspect = elementalAspect;
            this.physicalAspect = physicalAspect;
            this.applicationType = applicationType;
        }
    }
}
