using RimWorld;
using static UnityEngine.Random;
using Verse;

namespace LLRoM
{
    public class StatModifier
    {
        public StatDef Stat;
        public float value = 1;
        public string GetString()
        {
            return ("x" + (value * 100).ToString() + "%");
        }
    }
}