using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Mineshaft.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public static class HarmonyInitialiser
    {
        internal static Harmony harmony;
        internal static HarmonyMethod spawnTranspiler;
        internal static HarmonyMethod spawnBuildingAsPossiblePostfix;
        internal static HarmonyMethod refundPrefix;
        internal static HarmonyMethod spawningWipesPostfix;
        static HarmonyInitialiser()
        {
            harmony = new Harmony("Mineshaft.HarmonyPatch");
            harmony.PatchAll();

            spawnTranspiler = new HarmonyMethod(AccessTools.Method(typeof(SpawnTranspiler), "Transpiler"));
            spawnBuildingAsPossiblePostfix = new HarmonyMethod(AccessTools.Method(typeof(SpawnBuildingAsPossiblePostfix), "Postfix"));
            refundPrefix = new HarmonyMethod(AccessTools.Method(typeof(RefundPrefix), "Prefix"));
            spawningWipesPostfix = new HarmonyMethod(AccessTools.Method(typeof(SpawningWipesPostfix), "Postfix"));


        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    internal static class LoadGamePatches
    {
        internal static void Prefix()
        {
            HarmonyInitialiser.harmony.Patch(SpawnTranspiler.original, transpiler: HarmonyInitialiser.spawnTranspiler);
            HarmonyInitialiser.harmony.Patch(SpawnBuildingAsPossiblePostfix.original, postfix: HarmonyInitialiser.spawnBuildingAsPossiblePostfix);
            HarmonyInitialiser.harmony.Patch(RefundPrefix.original, prefix: HarmonyInitialiser.refundPrefix);
            HarmonyInitialiser.harmony.Patch(SpawningWipesPostfix.original, postfix: HarmonyInitialiser.spawningWipesPostfix);
        }

        internal static void Postfix()
        {
            HarmonyInitialiser.harmony.Unpatch(SpawnTranspiler.original, HarmonyPatchType.Transpiler);
            HarmonyInitialiser.harmony.Unpatch(SpawnBuildingAsPossiblePostfix.original, HarmonyPatchType.Postfix);
            HarmonyInitialiser.harmony.Unpatch(RefundPrefix.original, HarmonyPatchType.Prefix);
            HarmonyInitialiser.harmony.Unpatch(SpawningWipesPostfix.original, HarmonyPatchType.Postfix);

        }

    }
}
