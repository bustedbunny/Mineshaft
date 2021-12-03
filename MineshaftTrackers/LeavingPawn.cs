using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Mineshaft.MineshaftTrackers
{
    public class LeavingPawn : Shafter
    {
        public LeavingPawn() { }
        public LeavingPawn(Pawn pawn, int exitTick)
        {
            this.pawn = pawn;
            this.endTick = exitTick;
        }
        public override void Expired()
        {
            mineshaft.ShafterLeft(pawn);
        }
    }
}
