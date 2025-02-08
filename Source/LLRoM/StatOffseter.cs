using RimWorld;
using Verse;
using static UnityEngine.Random;

namespace LLRoM
{
    public class StatOffseter
    {
        public StatDef Stat;
        public float value = 0;
        public bool isPercent = false;
        public string GetString()
        {
            string offseter;
            string signed = "+";
            if (value < 0)
            {
                signed = "";
            }
            if (isPercent)
            {
                offseter = (signed + (value * 100).ToString() + "%");
            }
            else
            {
                if (Stat == StatDefOf.ComfyTemperatureMax || Stat == StatDefOf.ComfyTemperatureMin)
                {
                    if (Prefs.TemperatureMode == TemperatureDisplayMode.Celsius)
                    {
                        offseter = (signed +(value).ToString() + "C");
                    }
                    else if (Prefs.TemperatureMode == TemperatureDisplayMode.Fahrenheit)
                    {
                        offseter = (signed + (value * 1.8f).ToString() + "F");
                    }
                    else
                    {
                        offseter = (signed + (value).ToString() + "K");
                    }
                }
                else
                {
                    offseter = (signed + value.ToString());
                }
            }
            return offseter;
        }
    }
}