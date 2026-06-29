using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using FormationManager.Data;

namespace FormationManager.Patches
{
    /// <summary>
    /// Postfix patch on Mission.SpawnTroop.
    /// After an agent is spawned, if the player has assigned their troop type to a specific
    /// formation, we immediately move the agent into that formation.
    /// This fires for both initial spawns and reinforcement waves.
    /// </summary>
    [HarmonyPatch(typeof(Mission))]
    [HarmonyPatch(nameof(Mission.SpawnTroop))]
    internal static class MissionAgentSpawnPatch
    {
        [HarmonyPostfix]
        private static void Postfix(
            Mission __instance,
            IAgentOriginBase troopOrigin,
            bool isPlayerSide,
            bool hasFormation,
            Agent? __result)
        {
            if (__result == null)
                return;

            var settings = Settings.Instance;
            if (settings != null && !settings.ModEnabled)
                return;

            if (!isPlayerSide)
                return;

            if (!hasFormation)
                return;

            var character = troopOrigin.Troop;
            if (character == null)
                return;

            int assignedIndex = FormationAssignmentStore.GetAssignment(character.StringId);
            if (assignedIndex < 0)
                return;

            // Validate the formation index is in range for this mission
            var team = __instance.PlayerTeam;
            if (team == null)
                return;

            var formationClass = (FormationClass)assignedIndex;
            var formation = team.GetFormation(formationClass);
            if (formation == null)
                return;

            // Move the agent to the target formation
            __result.Formation = formation;
        }
    }
}
