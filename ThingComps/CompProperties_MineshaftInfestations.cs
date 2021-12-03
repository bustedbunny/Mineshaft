using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
