using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Mineshaft
{
    public class Command_EjectPawn : Command
    {
        public HashSet<Pawn> pawns;
        public Mineshaft mineshaft;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if (pawns == null)
            {
                Log.Error("Pawns list is not assigned for Command_EjectPawn.");
                return;
            }
            foreach (Pawn pawn in pawns)
            {
                void action()
                {
                    pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                }
                list.Add(new FloatMenuOption(pawn.NameFullColored, action));
            }
            if (list.Any())
            {
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }
    }
}
