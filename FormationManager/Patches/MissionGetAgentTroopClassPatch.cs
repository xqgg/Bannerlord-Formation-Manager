using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using FormationManager.Data;

namespace FormationManager.Patches
{
    /// <summary>
    /// Prefix patch on Mission.GetAgentTroopClass.
    /// Overrides the formation class of player-side troops based on the player's custom assignments.
    /// This seamlessly integrates with the Order of Battle (OOB) deployment layout and actual battlefield spawning.
    /// </summary>
    [HarmonyPatch(typeof(Mission))]
    [HarmonyPatch(nameof(Mission.GetAgentTroopClass))]
    internal static class MissionGetAgentTroopClassPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(
            BattleSideEnum battleSide,
            BasicCharacterObject agentCharacter,
            ref FormationClass __result)
        {
            // Only override player-side troops
            if (Mission.Current?.PlayerTeam != null && battleSide != Mission.Current.PlayerTeam.Side)
                return true;

            var settings = Settings.Instance;
            if (settings != null && !settings.ModEnabled)
                return true;

            if (agentCharacter == null)
                return true;

            int assignedIndex = FormationAssignmentStore.GetAssignment(agentCharacter.StringId);
            if (assignedIndex >= 0 && assignedIndex <= 7)
            {
                __result = (FormationClass)assignedIndex;
                return false; // Skip vanilla logic
            }

            return true; // Fall back to vanilla logic
        }
    }
}

