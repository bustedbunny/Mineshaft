using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Mineshaft.JobDrivers
{
    public class JobDriver_EnterMineshaft : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_General.Wait(1);
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Mineshaft shaft = (Mineshaft)TargetThingA;
                if (shaft.CanEnter())
                {
#if DEBUG
                    Log.Message("Trying to enter " + Find.TickManager.TicksGame);
#endif
                    shaft.TransferPawnIntoContainer(toil.actor, ShafterTransferMode.Entering);
                }
            };
            yield return toil;
        }
    }
}

