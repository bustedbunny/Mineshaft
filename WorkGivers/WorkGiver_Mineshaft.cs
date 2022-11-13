using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Mineshaft
{
    public class WorkGiver_Mineshaft : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return MineshaftStaticCache.allSpawnedMineshafts.Where(x => x.Map == pawn.Map);
        }
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !MineshaftStaticCache.allSpawnedMineshafts.Any();
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            Mineshaft shaft = t as Mineshaft;
            return shaft.CanEnter(pawn);
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(DefOfClass.Mineshaft_EnterMineshaft, t);
        }
    }
}
