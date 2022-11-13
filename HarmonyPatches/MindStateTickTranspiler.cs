using HarmonyLib;
using Mineshaft.JobDrivers;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace Mineshaft.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_MindState), nameof(Pawn_MindState.MindStateTick))]
    internal class MindStateTickTranspiler
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator il)
        {
            var getTerrain = AccessTools.Method(typeof(GridsUtility), nameof(GridsUtility.GetTerrain));
            var isMiner =
                AccessTools.Method(typeof(MindStateTickTranspiler), nameof(MindStateTickTranspiler.IsMiner));

            var list = new List<CodeInstruction>();
            var codes = new List<CodeInstruction>(instructions);

            var patched = false;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(getTerrain))
                {
                    list.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    list.Add(new CodeInstruction(OpCodes.Call, isMiner));
                    list.Add(new CodeInstruction(OpCodes.Brtrue_S, codes[i - 7].operand));
                    codes.InsertRange(i - 6, list);

                    patched = true;
                    break;
                }
            }

            if (!patched)
            {
                Log.Error($"Mineshaft {nameof(MindStateTickTranspiler)} didn't work");
            }

            return codes;
        }

        private static bool IsMiner(Pawn_MindState instance)
        {
            return instance.pawn.jobs.curDriver is JobDriver_WorkInMineshaft;
        }
    }
}