using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using FormationManager.Data;
using Helpers;

namespace FormationManager.Patches
{
    /// <summary>
    /// Postfix patch on CharacterHelper.SearchForFormationInTroopTree.
    /// 
    /// The OOB deployment screen calls this method to decide which formation cards to activate.
    /// It passes a base troop type and a FormationClass, and expects true if the troop (or any
    /// of its upgrade tree) belongs to that formation.
    /// 
    /// By checking our custom assignment store here, we can tell the OOB screen that a troop
    /// belongs to whatever formation the player has assigned it to — causing that card to
    /// automatically activate without any manual intervention.
    /// </summary>
    [HarmonyPatch(typeof(CharacterHelper), nameof(CharacterHelper.SearchForFormationInTroopTree))]
    internal static class SearchForFormationInTroopTreePatch
    {
        [HarmonyPostfix]
        private static void Postfix(CharacterObject baseTroop, FormationClass formation, ref bool __result)
        {
            // If vanilla already returns true, nothing to do.
            if (__result)
                return;

            var settings = Settings.Instance;
            if (settings != null && !settings.ModEnabled)
                return;

            // Only relevant during campaign (where we have a main party).
            if (Campaign.Current == null)
                return;

            var mainParty = MobileParty.MainParty;
            if (mainParty?.MemberRoster == null)
                return;

            // Check if this troop type is in the player's party
            bool inPlayerParty = false;
            for (int i = 0; i < mainParty.MemberRoster.Count; i++)
            {
                var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                if (element.Character != null && element.Character.StringId == baseTroop.StringId)
                {
                    inPlayerParty = true;
                    break;
                }
            }

            if (!inPlayerParty)
                return;

            // Check if this troop is assigned to the queried formation slot
            int assignedIndex = FormationAssignmentStore.GetAssignment(baseTroop.StringId);
            if (assignedIndex >= 0 && assignedIndex <= 7 && (FormationClass)assignedIndex == formation)
            {
                Logger.Log($"[SearchForFormationInTroopTree] Activating slot {formation} for {baseTroop.StringId}");
                __result = true;
            }
        }
    }
}
