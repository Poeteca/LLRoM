using RimWorld;

namespace LLRoM
{
    public class StatModifier
    {
        public StatDef Stat;
        public float value = 1;
        public string GetString()
        {
            return ((value * 100).ToString() + "%");
        }
    }
}