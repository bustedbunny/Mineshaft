using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mineshaft.HarmonyPatches
{
    [HarmonyPatch(typeof(CompCreatesInfestations), "CanCreateInfestationNow", MethodType.Getter)]
    internal class CanCreateInfestationNowPostfix
    {
        internal static void Postfix(CompCreatesInfestations __instance, ref bool __result)
        {
            if (__result)
            {
                foreach (var item in MineshaftStaticCache.allSpawnedMineshafts)
                {
                    if (item != __instance.parent && item.Position.InHorDistOf(__instance.parent.Position, 10f))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
    }
}
