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
