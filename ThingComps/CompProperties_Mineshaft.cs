using Verse;

namespace Mineshaft
{
    public class CompProperties_Mineshaft : CompProperties
    {
        public float workingSpeedModifier = 1f;
        public int ticksBetweenInjuriesPerDamageDealt = 750;
        public int minTicksBetweenInjuries = 10000;
        public float mtbDaysForInjury = 0.5f;
        public CompProperties_Mineshaft()
        {
            compClass = typeof(MiningTracker);
        }
    }
}
