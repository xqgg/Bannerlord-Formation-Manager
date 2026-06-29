using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using FormationManager.Data;

namespace FormationManager.Patches
{
    /// <summary>
    /// Postfix patch on the Mission constructor.
    /// Hooks GetAgentTroopClass_Override early so the deployment screen correctly reads 
    /// player custom formations during its initial class scan.
    /// </summary>
    [HarmonyPatch(typeof(Mission))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new System.Type[] { typeof(MissionInitializerRecord), typeof(MissionState), typeof(bool) })]
    internal static class MissionConstructorPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Mission __instance)
        {
            Logger.Log("MissionConstructorPatch.Postfix fired. Subscribing to GetAgentTroopClass_Override.");
            __instance.GetAgentTroopClass_Override += OnGetAgentTroopClass;
        }

        private static FormationClass OnGetAgentTroopClass(BattleSideEnum side, BasicCharacterObject character)
        {
            FormationClass vanillaClass = character?.DefaultFormationClass ?? FormationClass.Infantry;

            var settings = Settings.Instance;
            if (settings != null && !settings.ModEnabled)
                return vanillaClass;

            if (character == null || Campaign.Current == null)
            {
                Logger.Log($"OnGetAgentTroopClass: early exit. character={character?.StringId ?? "null"}, campaign={Campaign.Current != null}");
                return vanillaClass;
            }

            // Log the state of Mission.Current and PlayerTeam
            string playerTeamSide = "null";
            if (Mission.Current != null && Mission.Current.PlayerTeam != null)
                playerTeamSide = Mission.Current.PlayerTeam.Side.ToString();

            // Side check
            if (Mission.Current != null && Mission.Current.PlayerTeam != null)
            {
                if (side != Mission.Current.PlayerTeam.Side)
                {
                    Logger.Log($"OnGetAgentTroopClass: side rejected. troop={character.StringId}, troopSide={side}, playerSide={playerTeamSide}");
                    return vanillaClass;
                }
            }
            else
            {
                // PlayerTeam is null - this is the early scan phase before teams are created.
                // We allow the lookup to proceed without a side filter here.
                Logger.Log($"OnGetAgentTroopClass: PlayerTeam is null (early scan). troop={character.StringId}, side={side}. Proceeding without side filter.");
            }

            var mainParty = MobileParty.MainParty;
            if (mainParty?.MemberRoster == null)
            {
                Logger.Log($"OnGetAgentTroopClass: MainParty or Roster is null. troop={character.StringId}");
                return vanillaClass;
            }

            // Ensure the character exists in the player's party roster
            bool inPlayerParty = false;
            for (int i = 0; i < mainParty.MemberRoster.Count; i++)
            {
                var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                if (element.Character != null && element.Character.StringId == character.StringId)
                {
                    inPlayerParty = true;
                    break;
                }
            }

            if (!inPlayerParty)
            {
                Logger.Log($"OnGetAgentTroopClass: {character.StringId} not in player party. Returning vanilla {vanillaClass}.");
                return vanillaClass;
            }

            int assignedIndex = FormationAssignmentStore.GetAssignment(character.StringId);
            if (assignedIndex >= 0 && assignedIndex <= 7)
            {
                Logger.Log($"OnGetAgentTroopClass: OVERRIDE {character.StringId} -> {(FormationClass)assignedIndex} (was {vanillaClass})");
                return (FormationClass)assignedIndex;
            }

            Logger.Log($"OnGetAgentTroopClass: no assignment for {character.StringId}. Returning vanilla {vanillaClass}.");
            return vanillaClass;
        }
    }
}
