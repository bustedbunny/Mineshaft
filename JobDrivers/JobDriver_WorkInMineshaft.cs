using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Mineshaft.JobDrivers
{
    public class JobDriver_WorkInMineshaft : JobDriver
    {

        private bool reloadingAfterSave = true;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toil doWork = new Toil
            {
                socialMode = RandomSocialMode.Off,
                defaultCompleteMode = ToilCompleteMode.Never,
                activeSkill = () => SkillDefOf.Mining,
            };
            doWork.initAction = delegate
            {
                Pawn actor = doWork.actor;
                Mineshaft mineshaft = TargetThingA as Mineshaft;
                mineshaft.RegisterWorker(actor);
                actor.Map.dynamicDrawManager.DeRegisterDrawable(actor);


            };
            doWork.tickAction = delegate
            {

                Pawn actor = doWork.actor;
                if (reloadingAfterSave)
                {
                    actor.Map.dynamicDrawManager.DeRegisterDrawable(actor);
                    reloadingAfterSave = false;
                }
                if (actor.IsHashIntervalTick(2000))
                {
                    ThinkResult thinkResult = actor.thinker.MainThinkNodeRoot.TryIssueJobPackage(actor, default);
                    if (thinkResult.IsValid && thinkResult.Job.def != DefOfClass.Mineshaft_EnterMineshaft)
                    {
                        #if DEBUG
                        Log.Message("Interrupted job: " + thinkResult.Job.def.defName);
                        #endif
                        
                        actor.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                        return;
                    }
                }
                actor.skills.Learn(SkillDefOf.Mining, 0.07f);
            };

            AddFinishAction(delegate
            {
#if DEBUG
                Log.Message("Trying to leave mineshaft " + Find.TickManager.TicksGame);
#endif
                Pawn actor = doWork.actor;
                Mineshaft shaft = (Mineshaft)TargetThingA;
                actor.Map.dynamicDrawManager.RegisterDrawable(actor);
                shaft.DeregisterWorker(actor);
                actor.Position = shaft.InteractionCell;
                shaft._shaftersToAdd.Add((actor, ShafterTransferMode.Leaving));
            });
            yield return doWork;

        }
    }
}
