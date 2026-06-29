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
            __instance.GetAgentTroopClass_Override += OnGetAgentTroopClass;
        }

        private static FormationClass OnGetAgentTroopClass(BattleSideEnum side, BasicCharacterObject character)
        {
            FormationClass vanillaClass = character?.DefaultFormationClass ?? FormationClass.Infantry;

            var settings = Settings.Instance;
            if (settings != null && !settings.ModEnabled)
                return vanillaClass;

            if (character == null || Campaign.Current == null)
                return vanillaClass;

            // Ensure we only override the player's side to avoid breaking enemy deployment/AI.
            if (Mission.Current != null && Mission.Current.PlayerTeam != null)
            {
                if (side != Mission.Current.PlayerTeam.Side)
                {
                    return vanillaClass;
                }
            }

            var mainParty = MobileParty.MainParty;
            if (mainParty?.MemberRoster == null)
                return vanillaClass;

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
                return vanillaClass;

            int assignedIndex = FormationAssignmentStore.GetAssignment(character.StringId);
            if (assignedIndex >= 0 && assignedIndex <= 7)
                return (FormationClass)assignedIndex;

            return vanillaClass;
        }
    }
}
