using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Mineshaft.MineshaftTrackers
{
    public class EnteringPawn : Shafter
    {
        public EnteringPawn() { }
        public EnteringPawn(Pawn pawn, int enterTick)
        {
            this.pawn = pawn;
            this.endTick = enterTick;
        }
        public override void Expired()
        {
            mineshaft.ShafterEntered(pawn);
        }
    }
}
