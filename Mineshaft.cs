using HotSwap;
using Mineshaft.MineshaftTrackers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mineshaft
{
#if DEBUG
    [HotSwappable]
#endif
    public class Mineshaft : Building, IThingHolder
    {

        public Mineshaft()
        {
            _shafters = new HashSet<Shafter>();
            innerContainer = new ThingOwner<Pawn>();
            _workers = new HashSet<Pawn>();
        }

        private ThingOwner<Pawn> innerContainer;
        internal HashSet<Shafter> _shafters;
        internal HashSet<Pawn> _workers;

        private readonly HashSet<Shafter> _shaftersToRemove = new HashSet<Shafter>();
        public readonly HashSet<(Pawn, ShafterTransferMode)> _shaftersToAdd = new HashSet<(Pawn, ShafterTransferMode)>();


        private MiningTracker _miningTracker;
        private void ResetWorkersPositions()
        {
            int ind = 0;
            IntVec3 pos = DrawPos.ToIntVec3();
            int zOffset = 0;
            foreach (Pawn worker in _workers)
            {
                int sign = ind % 2 == 0 ? 1 : -1;
                worker.Position = new IntVec3(pos.x + (ind / 2 * sign), 0, pos.z + zOffset);
                ind++;
                if (ind > 3)
                {
                    ind = 0;
                    zOffset++;
                }
            }
        }
        public void RegisterWorker(Pawn pawn)
        {
            if (_workers.Add(pawn))
            {
                ResetWorkersPositions();
            }
        }
        public void DeregisterWorker(Pawn pawn)
        {
            if (_workers.Remove(pawn))
            {
                ResetWorkersPositions();
            }
        }
        public void AddQueuedShafters()
        {
            if (_shaftersToAdd.Any())
            {
                foreach (var shafter in _shaftersToAdd)
                {
                    TransferPawnIntoContainer(shafter.Item1, shafter.Item2);
                }
                _shaftersToAdd.Clear();
            }
        }
        public void RemoveQueuedShafters()
        {
            if (_shaftersToRemove.Any())
            {
                foreach (var shafter in _shaftersToRemove)
                {
                    _shafters.Remove(shafter);
                }
                _shaftersToRemove.Clear();
            }
        }
        private void TickShafters()
        {
            foreach (var shafter in _shafters)
            {
                if (!shafter.pawn.Destroyed)
                {
                    shafter.Tick();
                }
                else
                {
                    _shaftersToRemove.Add(shafter);
                }
            }
        }
        public override void Tick()
        {
            TickShafters();
            base.Tick();
            AddQueuedShafters();
            RemoveQueuedShafters();
        }
        private static int pathLength = 1500;
        private static int maxPathLength = 3000;
        public void TransferPawnIntoContainer(Pawn pawn, ShafterTransferMode mode)
        {
            Shafter shafter = (Shafter)Activator.CreateInstance((mode == ShafterTransferMode.Entering) ? typeof(EnteringPawn) : typeof(LeavingPawn));
            shafter.pawn = pawn;
            shafter.mineshaft = this;
            shafter.endTick = Find.TickManager.TicksGame + Mathf.Min((int)(pathLength / pawn.GetStatValue(StatDefOf.MoveSpeed)), maxPathLength);
            _shafters.Add(shafter);
            if (pawn.Spawned)
            {
                pawn.DeSpawn();
            }
            if (pawn.holdingOwner != null)
            {
                pawn.holdingOwner.TryTransferToContainer(pawn, innerContainer);
            }
            else
            {
                innerContainer.TryAdd(pawn);
            }
        }
        public void ShafterEntered(Pawn pawn)
        {
            if (!innerContainer.TryDrop(pawn, DrawPos.ToIntVec3(), Map, ThingPlaceMode.Near, out Pawn lastResultingThing))
            {
                Log.Error($"Couldn't excecute ShafterEntered {pawn.NameFullColored} {pawn.ThingID}");
                _shaftersToRemove.Add(_shafters.First(x => x.pawn == pawn));
                return;
            }
            _shaftersToRemove.Add(_shafters.First(x => x.pawn == pawn));
            Job job = JobMaker.MakeJob(DefOfClass.Mineshaft_WorkInMineshaft, this);
            pawn.jobs.StartJob(job);
        }
        public void ShafterLeft(Pawn pawn)
        {
            if (!innerContainer.TryDrop(pawn, InteractionCell, Map, ThingPlaceMode.Near, out Pawn lastResultingThing))
            {
                Log.Error($"Couldn't excecute ShafterLeft {pawn.NameFullColored} {pawn.ThingID}");
                _shaftersToRemove.Add(_shafters.First(x => x.pawn == pawn));
                return;
            }
            _shaftersToRemove.Add(_shafters.First(x => x.pawn == pawn));
            Job job = JobMaker.MakeJob(JobDefOf.Wait);
            job.expiryInterval = 250;
            pawn.jobs.StartJob(job);
#if DEBUG
            Log.Message("New job is: " + pawn.jobs?.curJob.def.defName);
#endif
        }
        public void RemoveShafterNow(Pawn pawn)
        {
            _shafters.RemoveWhere(x => x.pawn == pawn);
            _shaftersToAdd.RemoveWhere(x => x.Item1 == pawn);
            _shaftersToRemove.RemoveWhere(x => x.pawn == pawn);
            if (innerContainer.Contains(pawn))
            {
                innerContainer.TryDrop(pawn, InteractionCell, Map, ThingPlaceMode.Near, out Pawn lastResultingThing);
            }
            DeregisterWorker(pawn);
        }
        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public string GetContainerString()
        {
            if (!innerContainer.Any)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("Mineshaft_PawnsInside".Translate());
            foreach (Shafter shafter in _shafters)
            {
                sb.AppendLine();
                sb.Append(
                    "Mineshaft_PawnInside".Translate(shafter.pawn.NameFullColored,
                    (shafter is LeavingPawn) ? "Mineshaft_Leaving".TranslateSimple() : "Mineshaft_Entering".TranslateSimple(),
                    (shafter.endTick - Find.TickManager.TicksGame).ToStringSecondsFromTicks())
                    );
            }
            return sb.ToString();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            _miningTracker = GetComp<MiningTracker>();
            if (_miningTracker == null)
            {
                Log.Error("MiningTracker comp couldn't initialise. Mineshaft will not work");
            }
            MineshaftStaticCache.allSpawnedMineshafts.Add(this);
            ResetWorkersPositions();


            if (def.HasModExtension<MineshaftDefModExtension>())
            {
                MineshaftDefModExtension extension = def.GetModExtension<MineshaftDefModExtension>();
                pathLength = (extension.pathLength > -1) ? extension.pathLength : pathLength;
                maxPathLength = (extension.maxPathLength > -1) ? extension.maxPathLength : maxPathLength;
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach (IntVec3 cell in this.OccupiedRect())
            {
                Map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
                RoofCollapserImmediate.DropRoofInCells(cell, Map);
                Map.roofGrid.SetRoof(cell, null);
            }
            base.DeSpawn(mode);
            MineshaftStaticCache.allSpawnedMineshafts.Remove(this);
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public bool CanEnter(Pawn pawn = null)
        {
            if (isForbidden)
            {
                return false;
            }
            if (_workers.Count + _shafters.Count  > 7 && !_workers.Contains(pawn))
            {
                return false;
            }
            if (pawn != null)
            {
                return !pawn.skills?.GetSkill(SkillDefOf.Mining).TotallyDisabled ?? false;
            }
            return true;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }
            if (CanEnter(selPawn) && selPawn.CurJob.targetA.Thing != this)
            {
                void action()
                {
                    Job job = JobMaker.MakeJob(DefOfClass.Mineshaft_EnterMineshaft, this);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.MiscWork);
                }
                yield return new FloatMenuOption("Mineshaft_FloatMenuOptionLabel".Translate(), action);
            }
        }
        private List<Pawn> _tmpSelPawns;
        public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetMultiSelectFloatMenuOptions(selPawns))
            {
                yield return floatMenuOption;
            }
            if (selPawns.Any(x => CanEnter(x) && x.CurJob.targetA.Thing != this))
            {
                _tmpSelPawns = new List<Pawn>(selPawns.Where(x => CanEnter(x) && x.CurJob.targetA.Thing != this));
                void action()
                {
                    foreach (Pawn selPawn in _tmpSelPawns)
                    {
                        if (CanEnter(selPawn) && selPawn.CurJob.targetA.Thing != this)
                        {
                            Job job = JobMaker.MakeJob(DefOfClass.Mineshaft_EnterMineshaft, this);
                            if (!selPawn.jobs.TryTakeOrderedJob(job, JobTag.MiscWork))
                            {
                                Log.Error("Couldn't take ordered job from mineshaft float menu option.");
                            }
                            continue;
                        }
                    }
                };
                yield return new FloatMenuOption("Mineshaft_FloatMenuOptionLabel".Translate(), action);
            }
        }

        private bool isForbidden = false;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Faction != Faction.OfPlayer)
            {
                yield break;
            }

            yield return ForbiddenGizmo();
            if (_workers.Any())
            {
                yield return EjectPawnGizmo();
            }
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
        }
        private Command_EjectPawn EjectPawnGizmo()
        {
            Command_EjectPawn gizmo = new Command_EjectPawn
            {
                defaultLabel = "Mineshaft_EjectPawnGizmoLabel".Translate(),
                icon = MineshaftTextures.Force_Exit,
                pawns = _workers
            };
            return gizmo;

        }
        private Command_Toggle ForbiddenGizmo()
        {
            Command_Toggle command_Toggle = new Command_Toggle
            {
                hotKey = KeyBindingDefOf.Command_ItemForbid,
                icon = TexCommand.ForbidOff,
                isActive = () => !isForbidden,
                defaultLabel = "CommandAllow".TranslateWithBackup("DesignatorUnforbid"),
                activateIfAmbiguous = false
            };
            if (isForbidden)
            {
                command_Toggle.defaultDesc = "CommandForbiddenDesc".TranslateWithBackup("DesignatorUnforbidDesc");
            }
            else
            {
                command_Toggle.defaultDesc = "CommandNotForbiddenDesc".TranslateWithBackup("DesignatorForbidDesc");
            }
            command_Toggle.toggleAction = delegate
            {
                isForbidden = !isForbidden;
                if (isForbidden)
                {
                    List<Pawn> list = new List<Pawn>(_workers);
                    foreach (Pawn worker in list)
                    {
                        worker.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                    List<Shafter> list2 = new List<Shafter>(_shafters);
                    foreach (Shafter shafter in list2)
                    {
                        if (shafter is EnteringPawn enterer)
                        {
                            _shafters.Remove(shafter);
                            LeavingPawn leaver = new LeavingPawn(enterer.pawn, 1500 - enterer.endTick)
                            {
                                mineshaft = this
                            };
                            _shafters.Add(leaver);
                        }
                    }
                }
            };
            return command_Toggle;
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();

            return text + GetContainerString();
        }
        private ThingOwner<Pawn> _scribeList;
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref _scribeList, "scribeList", this);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Collections.Look(ref _shafters, "shafters", LookMode.Deep);
            Scribe_Collections.Look(ref _workers, "workers", LookMode.Reference);
            Scribe_Values.Look(ref isForbidden, "isForbidden");
        }

        private Rect _rect;
        private Vector2 _vector;
        private Vector2 _vector2;
        private readonly static Vector3 offset = new Vector3(0.3f, 0, 0.3f);
        private readonly static Vector3 offset2 = new Vector3(0.2f, 0, 0.2f);
        public override void DrawGUIOverlay()
        {
            foreach (Pawn worker in _workers)
            {
                _vector = (worker.DrawPos - offset).MapToUIPosition();
                _vector2 = (worker.DrawPos + offset2).MapToUIPosition();
                _rect.x = _vector.x;
                _rect.y = _vector2.y;
                _rect.width = _vector2.x - _vector.x;
                _rect.height = _vector.y - _vector2.y;
                Graphics.DrawTexture(_rect, MineshaftTextures.MinerIcon);
            }
        }
    }
}
