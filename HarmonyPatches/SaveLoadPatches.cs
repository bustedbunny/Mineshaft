using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Mineshaft.HarmonyPatches
{
    internal class SpawningWipesPostfix
    {
        internal static readonly MethodInfo original = AccessTools.Method(typeof(GenSpawn), "SpawningWipes");
        internal static void Postfix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
        {
            if (__result)
            {
                ThingDef def = oldEntDef as ThingDef;
                if (def.thingClass == typeof(Mineshaft))
                {
                    __result = false;
                    return;
                }
                def = newEntDef as ThingDef;
                if (def.thingClass == typeof(Mineshaft))
                {
                    __result = false;
                    return;
                }
            }
        }
    }
    internal class RefundPrefix
    {
        internal static readonly MethodInfo original = AccessTools.Method(typeof(GenSpawn), "Refund");
        internal static bool Prefix(Thing thing)
        {
            if (thing is Mineshaft)
            {
                return false;
            }
            return true;
        }
    }
    internal class SpawnBuildingAsPossiblePostfix
    {
        internal static readonly MethodInfo original = AccessTools.Method(typeof(GenSpawn), "SpawnBuildingAsPossible");
        internal static void Postfix(Building building, Map map, bool respawningAfterLoad)
        {
            if (building is Mineshaft && !building.Spawned)
            {
                GenSpawn.Spawn(building, building.Position, map, building.Rotation, WipeMode.FullRefund, respawningAfterLoad);
            }
        }
    }
    internal class SpawnTranspiler
    {
        internal static readonly MethodInfo original = AccessTools.Method(typeof(GenSpawn), "Spawn", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) });
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            FieldInfo passability = AccessTools.Field(typeof(BuildableDef), nameof(BuildableDef.passability));
            MethodInfo isMineshaft = AccessTools.Method(typeof(SpawnTranspiler), nameof(SpawnTranspiler.IsMineshaft));

            Label label = il.DefineLabel();
            List<CodeInstruction> list = new List<CodeInstruction>();
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_0 &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 2].opcode == OpCodes.Ldfld &&
                    codes[i + 2].operand == passability &&
                    codes[i + 3].opcode == OpCodes.Ldc_I4_2)
                {
                    list.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    list.Add(new CodeInstruction(OpCodes.Call, isMineshaft));
                    list.Add(new CodeInstruction(OpCodes.Brfalse_S, label));
                    list.Add(new CodeInstruction(OpCodes.Ret));
                    list.Add(new CodeInstruction(OpCodes.Nop).WithLabels(label));
                    codes.InsertRange(i + 5, list);
                }
            }
            return codes;
        }

        private static bool IsMineshaft(Thing thing)
        {
            return thing is Mineshaft;
        }
    }
}
