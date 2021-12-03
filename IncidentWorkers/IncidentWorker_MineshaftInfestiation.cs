using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Mineshaft
{
    public class IncidentWorker_MineshaftInfestation : IncidentWorker
    {
        private static readonly List<Mineshaft> tmpShafts = new List<Mineshaft>();
        private const float MinPointsFactor = 0.3f;
        private const float MaxPointsFactor = 0.6f;
        private const float MinPoints = 200f;
        private const float MaxPoints = 1000f;
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (Faction.OfInsects == null)
            {
                return false;
            }
            Map map = (Map)parms.target;
            tmpShafts.Clear();
            GetUsableMineshafts(map, tmpShafts);
            return tmpShafts.Any();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            tmpShafts.Clear();
            GetUsableMineshafts(map, tmpShafts);
            if (!tmpShafts.TryRandomElement(out var mineshaft))
            {
                return false;
            }
            IntVec3 cell = mineshaft.InteractionCell;
            if (!cell.Walkable(map))
            {
                return false;
            }
            TunnelHiveSpawner tunnelHiveSpawner = (TunnelHiveSpawner)ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner);
            tunnelHiveSpawner.spawnHive = false;
            tunnelHiveSpawner.insectsPoints = Mathf.Clamp(parms.points * Rand.Range(MinPointsFactor, MaxPointsFactor), MinPoints, MaxPoints);
            tunnelHiveSpawner.spawnedByInfestationThingComp = true;
            GenSpawn.Spawn(tunnelHiveSpawner, cell, map, WipeMode.FullRefund);
            mineshaft.TryGetComp<CompMineshaftInfestations>().Notify_CreatedInfestation();
            SendStandardLetter(parms, new TargetInfo(tunnelHiveSpawner.Position, map));
            return true;
        }

        private static void GetUsableMineshafts(Map map, List<Mineshaft> outShafts)
        {
            outShafts.Clear();
            foreach (var item in MineshaftStaticCache.allSpawnedMineshafts.Where(x => x.Map == map))
            {
                if (item.Faction == Faction.OfPlayer && item.TryGetComp<CompMineshaftInfestations>().CanCreateInfestationNow)
                {
                    outShafts.Add(item);
                }
            }
        }
    }
}
