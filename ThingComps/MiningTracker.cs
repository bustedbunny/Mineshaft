using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Verse.DamageWorker;

namespace Mineshaft
{
    public class MiningTracker : ThingComp
    {
        private float portionProgress;
        private float portionYieldPct;
        private int lastUsedTick = -99999;
        private const float WorkPerPortionBase = 10000f;
        public float ProgressToNextPortionPercent => portionProgress / 10000f;

        private int lastInjuryTick = 0;
        private static readonly DamageDef[] damageDefs = new DamageDef[] { DamageDefOf.Scratch, DamageDefOf.Cut, DamageDefOf.Blunt, DamageDefOf.Crush };

        private CompProperties_Mineshaft Props => props as CompProperties_Mineshaft;

        private Mineshaft Parent => parent as Mineshaft;

        private float _statValue;

        public override void CompTick()
        {
            bool shouldCheckInjury = Find.TickManager.TicksGame % 2500 == 0;
            foreach (Pawn worker in Parent._workers)
            {
                MiningWorkDone(worker);
            }
            if (shouldCheckInjury && Parent._workers.Any() && Find.TickManager.TicksGame - lastInjuryTick >= 0 && Rand.MTBEventOccurs(Props.mtbDaysForInjury, 60000f, 2500f))
            {
                Pawn worker = Parent._workers.RandomElement();
#if DEBUG
                Log.Message("Mineshaft random ramage event applied to " + worker.NameFullColored);
#endif
                DamageInfo damage = new DamageInfo(damageDefs[Rand.Range(0, damageDefs.Length - 1)], Rand.Range(10, 50), 0, -1, null, null, null, DamageInfo.SourceCategory.Collapse);
                DamageResult dinfo = worker.TakeDamage(damage);
                float totalDamage = dinfo.totalDamageDealt;
                if (worker.Dead || worker.Downed)
                {
                    if (worker.Spawned)
                    {
                        worker.DeSpawn();
                    }
                    Parent.RemoveShafterNow(worker);
                    if (!worker.Destroyed)
                    {
                        worker.Destroy();
                    }
                    if (worker.Corpse != null && !worker.Corpse.Destroyed)
                    {
                        worker.Corpse.Destroy();
                    }
                    Find.LetterStack.ReceiveLetter("Mineshaft_LetterDiedInMineshaftLabel".Translate(worker.LabelShort, worker.Named("PAWN")).CapitalizeFirst(),
                        "Mineshaft_LetterDiedInMineshaft".Translate(worker.LabelShort, GenText.PossessiveCap(worker), worker.Named("PAWN")).CapitalizeFirst(), LetterDefOf.NegativeEvent, worker);
                }
                else
                {
                    Find.LetterStack.ReceiveLetter("Mineshaft_LetterInjuredFromMiningLabel".Translate(worker.LabelShort, worker.Named("PAWN")).CapitalizeFirst(),
                        "Mineshaft_LetterInjuredFromMining".Translate(worker.LabelShort, GenText.ProSubjCap(worker), worker.Named("PAWN")).CapitalizeFirst(), LetterDefOf.NegativeEvent, worker);
                }
                lastInjuryTick = Find.TickManager.TicksGame + Mathf.Min((int)totalDamage * Props.ticksBetweenInjuriesPerDamageDealt, Props.minTicksBetweenInjuries);
            }

        }
        public void MiningWorkDone(Pawn worker)
        {
            _statValue = Props.workingSpeedModifier * worker.GetStatValue(StatDefOf.MiningSpeed);
            portionProgress += _statValue;
            portionYieldPct += _statValue * worker.GetStatValue(StatDefOf.MiningYield) / WorkPerPortionBase;
            lastUsedTick = Find.TickManager.TicksGame;
            if (portionProgress > WorkPerPortionBase)
            {
                TryProducePortion(portionYieldPct, worker);
                portionProgress = 0;
                portionYieldPct = 0;
            }

        }

        private void TryProducePortion(float yieldPct, Pawn worker = null)
        {
            bool nextResource = GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell);
            if (resDef == null)
            {
                return;
            }
            int num = Mathf.Min(countPresent, resDef.deepCountPerPortion);
            if (nextResource)
            {
                parent.Map.deepResourceGrid.SetAt(cell, resDef, countPresent - num);
            }
            int stackCount = Mathf.Max(1, GenMath.RoundRandom((float)num * yieldPct));
            Thing thing = ThingMaker.MakeThing(resDef);
            thing.stackCount = stackCount;
            GenPlace.TryPlaceThing(thing, parent.InteractionCell, parent.Map, ThingPlaceMode.Near, null, (IntVec3 p) => p != parent.Position && p != parent.InteractionCell);
            if (worker != null)
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.Mined, worker.Named(HistoryEventArgsNames.Doer)));
            }
            if (!nextResource || ValuableResourcesPresent())
            {
                return;
            }
            if (DeepDrillUtility.GetBaseResource(parent.Map, parent.Position) == null)
            {
                Messages.Message("Mineshaft_ExhaustedNoFallback".Translate(), parent, MessageTypeDefOf.TaskCompletion);
                return;
            }
            Messages.Message("Mineshaft_Exhausted".Translate(Find.ActiveLanguageWorker.Pluralize(DeepDrillUtility.GetBaseResource(parent.Map, parent.Position).label)), parent, MessageTypeDefOf.TaskCompletion);
            for (int i = 0; i < 21; i++)
            {
                IntVec3 c = cell + GenRadial.RadialPattern[i];
                if (c.InBounds(parent.Map))
                {
                    ThingWithComps firstThingWithComp = c.GetFirstThingWithComp<MiningTracker>(parent.Map);
                    if (firstThingWithComp != null && !firstThingWithComp.GetComp<MiningTracker>().ValuableResourcesPresent())
                    {
                        firstThingWithComp.SetForbidden(value: true);
                    }
                }
            }
        }

        private bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
        {
            return DeepDrillUtility.GetNextResource(parent.Position, parent.Map, out resDef, out countPresent, out cell);
        }

        public bool ValuableResourcesPresent()
        {
            return GetNextResource(out _, out _, out _);
        }

        public bool UsedLastTick()
        {
            return lastUsedTick >= Find.TickManager.TicksGame - 1;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action
                {
                    defaultLabel = "Debug: Produce portion (100% yield)",
                    action = delegate
                    {
                        TryProducePortion(1f);
                    }
                };
                yield return command_Action;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (parent.Spawned)
            {
                GetNextResource(out var resDef, out var _, out var _);
                if (resDef == null)
                {
                    return "DeepDrillNoResources".Translate();
                }
                return "ResourceBelow".Translate() + ": " + resDef.LabelCap + "\n" + "ProgressToNextPortion".Translate() + ": " + ProgressToNextPortionPercent.ToStringPercent("F0");
            }
            return null;
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            //         powerComp = parent.TryGetComp<CompPowerTrader>();

        }
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref portionProgress, "portionProgress", 0f);
            Scribe_Values.Look(ref portionYieldPct, "portionYieldPct", 0f);
            Scribe_Values.Look(ref lastUsedTick, "lastUsedTick", 0);
        }
        public override void PostDeSpawn(Map map)
        {
            portionProgress = 0f;
            portionYieldPct = 0f;
            lastUsedTick = -99999;
        }
    }
}
