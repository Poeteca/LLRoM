using RimWorld;
using Verse;

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
            if (isPercent)
            {
                offseter = ((value * 100).ToString() + "% offset");
            }
            else
            {
                if (Stat == StatDefOf.ComfyTemperatureMax || Stat == StatDefOf.ComfyTemperatureMin)
                {
                    if (Prefs.TemperatureMode == TemperatureDisplayMode.Celsius)
                    {
                        offseter = ((value).ToString() + "C offset");
                    }
                    else if (Prefs.TemperatureMode == TemperatureDisplayMode.Fahrenheit)
                    {
                        offseter = ((value * 1.8f).ToString() + "F offset");
                    }
                    else
                    {
                        offseter = ((value).ToString() + "K offset");
                    }
                }
                else
                {
                    offseter = (value.ToString() + " offset");
                }
            }
            return offseter;
        }
    }
}