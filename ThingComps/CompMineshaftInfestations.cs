using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Mineshaft
{
    public class CompMineshaftInfestations : ThingComp
    {
        private int lastCreatedInfestationTick = -999999;

        private const float PreventInfestationsDist = 10f;

        private CompProperties_MineshaftInfestations Props => props as CompProperties_MineshaftInfestations;
        public bool CanCreateInfestationNow
        {
            get
            {
                MiningTracker comp = parent.GetComp<MiningTracker>();
                if (comp != null && comp.UsedLastTick())
                {
                    return false;
                }
                if (CantFireBecauseCreatedInfestationRecently)
                {
                    return false;
                }
                if (CantFireBecauseSomethingElseCreatedInfestationRecently)
                {
                    return false;
                }
                return true;
            }
        }
        public bool CantFireBecauseCreatedInfestationRecently => Find.TickManager.TicksGame <= lastCreatedInfestationTick + Props.MinRefireDays * 60000;
        public bool CantFireBecauseSomethingElseCreatedInfestationRecently
        {
            get
            {
                if (!parent.Spawned)
                {
                    return false;
                }
                List<Thing> list = parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.CreatesInfestations);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != parent && list[i].Position.InHorDistOf(parent.Position, 10f) && list[i].TryGetComp<CompCreatesInfestations>().CantFireBecauseCreatedInfestationRecently)
                    {
                        return true;
                    }
                }
                if (MineshaftsFiredInfestationRecently)
                {
                    return true;
                }
                return false;
            }
        }
        public void Notify_CreatedInfestation()
        {
            lastCreatedInfestationTick = Find.TickManager.TicksGame;
        }

        public bool MineshaftsFiredInfestationRecently
        {
            get
            {
                foreach (var item in MineshaftStaticCache.allSpawnedMineshafts)
                {
                    if (item != parent && item.Position.InHorDistOf(parent.Position, PreventInfestationsDist))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
