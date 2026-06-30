using System;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using FormationManager.Data;

namespace FormationManager.Patches
{
    /// <summary>
    /// Patches the LogicalClass and PhysicalClass properties of Formations.
    /// 
    /// The Order of Battle (OOB) screen only supports the 4 basic classes: 
    /// Infantry (0), Ranged (1), Cavalry (2), and HorseArcher (3). If a formation 
    /// has a subclass like HeavyCavalry (7) or HeavyInfantry (5), the OOB UI 
    /// defaults it to "unset" and hides the card.
    /// 
    /// By mapping subclasses to their corresponding basic classes, we trick the 
    /// OOB screen into recognizing formations 4-7 as valid basic formations, 
    /// allowing cards 5-8 to activate natively when populated with troops.
    /// </summary>
    [HarmonyPatch(typeof(Formation), "get_LogicalClass")]
    internal static class FormationLogicalClassPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref FormationClass __result)
        {
            var settings = Settings.Instance;
            if (settings == null || !settings.ModEnabled)
                return;

            __result = MapToBasicClass(__result);
        }

        public static FormationClass MapToBasicClass(FormationClass fc)
        {
            switch (fc)
            {
                case FormationClass.Skirmisher:      // 4
                case FormationClass.HeavyInfantry:   // 5
                    return FormationClass.Infantry;  // 0
                case FormationClass.LightCavalry:    // 6
                case FormationClass.HeavyCavalry:    // 7
                    return FormationClass.Cavalry;   // 2
                default:
                    return fc;
            }
        }
    }

    [HarmonyPatch(typeof(Formation), "get_PhysicalClass")]
    internal static class FormationPhysicalClassPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref FormationClass __result)
        {
            var settings = Settings.Instance;
            if (settings == null || !settings.ModEnabled)
                return;

            __result = FormationLogicalClassPatch.MapToBasicClass(__result);
        }
    }

    /// <summary>
    /// Patches the RefreshFormation method on the OOB formation item VM.
    /// 
    /// When the OOB screen initializes, it checks whether a card index contains units.
    /// Since the OOB distribution logic hasn't run yet, all cards (except those containing
    /// the main hero or basic vanilla classes) are seen as empty and remain inactive (unset).
    /// 
    /// By prefixing this call and forcing cards with player custom assignments to activate
    /// with their target basic classes (Infantry/Cavalry/etc.) and setting isNew = true,
    /// we force the OOB deployment screen to show these slots as active cards from the start.
    /// </summary>
    [HarmonyPatch(typeof(OrderOfBattleFormationItemVM), "RefreshFormation", new Type[] { typeof(Formation), typeof(DeploymentFormationClass), typeof(bool) })]
    internal static class RefreshFormationPatch
    {
        [HarmonyPrefix]
        private static void Prefix(OrderOfBattleFormationItemVM __instance, Formation formation, ref DeploymentFormationClass formationClass, ref bool isNew)
        {
            var settings = Settings.Instance;
            if (settings == null || !settings.ModEnabled)
                return;

            if (formation == null)
                return;

            int idx = formation.Index;
            if (HasTroopsAssigned(idx))
            {
                DeploymentFormationClass targetClass = GetCustomAssignmentClass(idx);
                if (targetClass != DeploymentFormationClass.Unset)
                {
                    formationClass = targetClass;
                    isNew = true; // Force card activation
                }
            }
        }

        private static bool HasTroopsAssigned(int formationIndex)
        {
            var mainParty = MobileParty.MainParty;
            if (mainParty?.MemberRoster == null)
                return false;

            for (int i = 0; i < mainParty.MemberRoster.Count; i++)
            {
                var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                if (element.Character == null)
                    continue;

                if (element.Number <= element.WoundedNumber)
                    continue;

                int assignedIndex = FormationAssignmentStore.GetAssignment(element.Character.StringId);
                if (assignedIndex == formationIndex)
                    return true;
            }

            return false;
        }

        private static DeploymentFormationClass GetCustomAssignmentClass(int formationIndex)
        {
            if (formationIndex == 0 || formationIndex == 4 || formationIndex == 5)
                return DeploymentFormationClass.Infantry;
            if (formationIndex == 1)
                return DeploymentFormationClass.Ranged;
            if (formationIndex == 2 || formationIndex == 6 || formationIndex == 7)
                return DeploymentFormationClass.Cavalry;
            if (formationIndex == 3)
                return DeploymentFormationClass.HorseArcher;

            return DeploymentFormationClass.Unset;
        }
    }
}
