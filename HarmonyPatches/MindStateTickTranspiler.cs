using HarmonyLib;
using Mineshaft.JobDrivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Mineshaft.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_MindState), nameof(Pawn_MindState.MindStateTick))]
    internal class MindStateTickTranspiler
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            MethodInfo getTerrain = AccessTools.Method(typeof(GridsUtility), nameof(GridsUtility.GetTerrain));
            MethodInfo isMiner = AccessTools.Method(typeof(MindStateTickTranspiler), nameof(MindStateTickTranspiler.IsMiner));

            List<CodeInstruction> list = new List<CodeInstruction>();
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call &&
                    codes[i].operand == getTerrain)
                {
                    list.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    list.Add(new CodeInstruction(OpCodes.Call, isMiner));
                    list.Add(new CodeInstruction(OpCodes.Brtrue_S, codes[i - 7].operand));
                    codes.InsertRange(i - 6, list);
                    break;
                }
            }
            return codes;
        }

        private static bool IsMiner(Pawn_MindState instance)
        {
            return instance.pawn.jobs.curDriver is JobDriver_WorkInMineshaft;
        }
    }
}
