using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Mineshaft.MineshaftTrackers
{
    public abstract class Shafter : IExposable
    {
        public Pawn pawn;
        public Mineshaft mineshaft;
        public int endTick;

        public void Tick()
        {
            if (Find.TickManager.TicksGame >= endTick)
            {
                Expired();
            }
        }
        public abstract void Expired();

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref mineshaft, "mineshaft");
            Scribe_Values.Look(ref endTick, "exitTick");
        }
    }


}
