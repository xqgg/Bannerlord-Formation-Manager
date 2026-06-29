using HarmonyLib;
using TaleWorlds.Core;
using FormationManager.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;

namespace FormationManager.Patches
{
    /// <summary>
    /// Prefix patch on BasicCharacterObject.DefaultFormationClass.
    /// Overrides the default formation class of player-side campaign troops.
    /// This integrates directly into both the party screen, OOB deployment, and battle spawning.
    /// </summary>
    [HarmonyPatch(typeof(BasicCharacterObject), "get_DefaultFormationClass")]
    internal static class BasicCharacterObjectDefaultFormationClassPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(BasicCharacterObject __instance, ref FormationClass __result)
        {
            var settings = Settings.Instance;
            if (settings != null && !settings.ModEnabled)
                return true;

            if (__instance == null)
                return true;

            // Only override if we are in campaign mode and the character belongs to the player's mobile party roster
            if (Campaign.Current != null)
            {
                var mainParty = MobileParty.MainParty;
                if (mainParty?.MemberRoster != null)
                {
                    bool inPlayerParty = false;
                    for (int i = 0; i < mainParty.MemberRoster.Count; i++)
                    {
                        var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                        if (element.Character != null && element.Character.StringId == __instance.StringId)
                        {
                            inPlayerParty = true;
                            break;
                        }
                    }

                    if (inPlayerParty)
                    {
                        int assignedIndex = FormationAssignmentStore.GetAssignment(__instance.StringId);
                        if (assignedIndex >= 0 && assignedIndex <= 7)
                        {
                            __result = (FormationClass)assignedIndex;
                            return false; // Skip default getter logic
                        }
                    }
                }
            }

            return true; // Fall back to vanilla logic
        }
    }
}


