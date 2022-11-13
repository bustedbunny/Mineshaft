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
