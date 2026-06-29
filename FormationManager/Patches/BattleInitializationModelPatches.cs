using HarmonyLib;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.MountAndBlade;
using FormationManager.Data;

namespace FormationManager.Patches
{
    /// <summary>
    /// Patches the BattleInitializationModels to include player custom-assigned troop classes 
    /// in the list of available troop types.
    /// 
    /// The Order of Battle (OOB) screen calls GetAllAvailableTroopTypes() to determine which 
    /// classes are present in the player's party. By default, campaign mode only detects 
    /// classes based on raw troop XML flags (IsInfantry, IsRanged, IsMounted). By adding our 
    /// custom-assigned classes here, we force the OOB UI to recognize them as available, 
    /// activating the corresponding formation cards.
    /// </summary>
    [HarmonyPatch]
    internal static class SandboxBattleInitializationModelPatch
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            // Dynamically resolve type since Sandbox.dll is a module and not directly referenced.
            Type type = AccessTools.TypeByName("Sandbox.SandboxBattleInitializationModel");
            if (type == null)
            {
                Logger.Log("SandboxBattleInitializationModel type not found. Cannot patch campaign OOB initialization.");
                return null;
            }
            return AccessTools.Method(type, "GetAllAvailableTroopTypes");
        }

        [HarmonyPostfix]
        private static void Postfix(ref List<FormationClass> __result)
        {
            if (__result == null)
                __result = new List<FormationClass>();

            var settings = Settings.Instance;
            if (settings != null && !settings.ModEnabled)
                return;

            if (Campaign.Current == null)
                return;

            var mainParty = MobileParty.MainParty;
            if (mainParty?.MemberRoster == null)
                return;

            for (int i = 0; i < mainParty.MemberRoster.Count; i++)
            {
                var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                if (element.Character == null)
                    continue;

                // Only add if the troop actually exists in the main party roster (non-wounded)
                if (element.Number <= element.WoundedNumber)
                    continue;

                int assignedIndex = FormationAssignmentStore.GetAssignment(element.Character.StringId);
                if (assignedIndex >= 0 && assignedIndex <= 7)
                {
                    FormationClass assignedClass = (FormationClass)assignedIndex;
                    if (!__result.Contains(assignedClass))
                    {
                        Logger.Log($"[SandboxBattleInitializationModelPatch] Adding {assignedClass} to available troop types for {element.Character.StringId}");
                        __result.Add(assignedClass);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(CustomBattleInitializationModel), "GetAllAvailableTroopTypes")]
    internal static class CustomBattleInitializationModelPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref List<FormationClass> __result)
        {
            // For custom battles, we don't have custom assignments saved by player ID, 
            // but we can ensure it doesn't break if result is null.
            if (__result == null)
                __result = new List<FormationClass>();
        }
    }
}
