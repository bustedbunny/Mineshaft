using Verse;

namespace Mineshaft
{
    public class CompProperties_MineshaftInfestations : CompProperties
    {
        public float MinRefireDays = 7;
        public CompProperties_MineshaftInfestations()
        {
            compClass = typeof(CompMineshaftInfestations);
        }
    }
}
