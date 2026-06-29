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
            var settings = Settings.Instance;
            bool isModEnabled = settings?.ModEnabled ?? true;

            // Only proceed if mod is enabled and character is valid
            if (!isModEnabled || agentCharacter == null)
                return true;

            int assignedIndex = FormationAssignmentStore.GetAssignment(agentCharacter.StringId);
            
            // Log the query to help debug what OOB is seeing
            try
            {
                string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                string path = System.IO.Path.Combine(docs, "Mount and Blade II Bannerlord", "Configs", "FormationManager_OOBDebug.txt");
                
                string playerTeamSideStr = Mission.Current?.PlayerTeam != null ? Mission.Current.PlayerTeam.Side.ToString() : "null";
                string mapEventSideStr = TaleWorlds.CampaignSystem.MapEvents.MapEvent.PlayerMapEvent != null 
                    ? TaleWorlds.CampaignSystem.MapEvents.MapEvent.PlayerMapEvent.PlayerSide.ToString() : "null";

                System.IO.File.AppendAllText(path, 
                    $"GetAgentTroopClass: Character={agentCharacter.Name}, QuerySide={battleSide}, " +
                    $"PlayerTeamSide={playerTeamSideStr}, MapEventSide={mapEventSideStr}, " +
                    $"AssignedIdx={assignedIndex}\n");
            }
            catch {}

            // Determine if the side matches the player side
            bool isPlayerSide = false;
            if (Mission.Current?.PlayerTeam != null)
            {
                isPlayerSide = (battleSide == Mission.Current.PlayerTeam.Side);
            }
            else if (TaleWorlds.CampaignSystem.MapEvents.MapEvent.PlayerMapEvent != null)
            {
                isPlayerSide = (battleSide == TaleWorlds.CampaignSystem.MapEvents.MapEvent.PlayerMapEvent.PlayerSide);
            }
            else
            {
                // Fallback for custom battle / early load where player is Attacker by default
                isPlayerSide = (battleSide == BattleSideEnum.Attacker);
            }

            if (!isPlayerSide)
                return true;

            if (assignedIndex >= 0 && assignedIndex <= 7)
            {
                __result = (FormationClass)assignedIndex;
                return false; // Skip vanilla logic
            }

            return true; // Fall back to vanilla logic
        }
    }
}

