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
    /// </summary>
    [HarmonyPatch(typeof(OrderOfBattleFormationItemVM), "RefreshFormation", new Type[] { typeof(Formation), typeof(DeploymentFormationClass), typeof(bool) })]
    internal static class RefreshFormationPatch
    {
        [HarmonyPrefix]
        private static void Prefix(OrderOfBattleFormationItemVM __instance, Formation formation, ref DeploymentFormationClass overriddenClass, ref bool mustExist)
        {
            var settings = Settings.Instance;
            if (settings == null || !settings.ModEnabled)
                return;

            if (formation == null)
            {
                Logger.Log("[RefreshFormationPatch] Prefix: formation is null!");
                return;
            }

            int idx = formation.Index;
            Logger.Log($"[RefreshFormationPatch] Prefix called for formation {idx}. Original overriddenClass={overriddenClass}, mustExist={mustExist}");

            bool hasTroops = HasTroopsAssigned(idx);
            Logger.Log($"[RefreshFormationPatch] HasTroopsAssigned({idx}) returned {hasTroops}");

            if (hasTroops)
            {
                DeploymentFormationClass targetClass = GetCustomAssignmentClass(idx);
                Logger.Log($"[RefreshFormationPatch] Target class for formation {idx} is {targetClass}");

                if (targetClass != DeploymentFormationClass.Unset)
                {
                    overriddenClass = targetClass;
                    mustExist = true;
                    Logger.Log($"[RefreshFormationPatch] Overrode formation {idx}: overriddenClass={overriddenClass}, mustExist={mustExist}");
                }
            }
        }

        private static bool HasTroopsAssigned(int formationIndex)
        {
            var mainParty = MobileParty.MainParty;
            if (mainParty == null)
            {
                Logger.Log("[RefreshFormationPatch] MobileParty.MainParty is null!");
                return false;
            }

            if (mainParty.MemberRoster == null)
            {
                Logger.Log("[RefreshFormationPatch] MobileParty.MainParty.MemberRoster is null!");
                return false;
            }

            Logger.Log($"[RefreshFormationPatch] Scanning MemberRoster. Size={mainParty.MemberRoster.Count}");
            for (int i = 0; i < mainParty.MemberRoster.Count; i++)
            {
                var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                if (element.Character == null)
                    continue;

                int assignedIndex = FormationAssignmentStore.GetAssignment(element.Character.StringId);
                if (assignedIndex == formationIndex)
                {
                    Logger.Log($"[RefreshFormationPatch] Found assigned troop: {element.Character.StringId} (Index={assignedIndex}, Count={element.Number}, Wounded={element.WoundedNumber})");
                    if (element.Number > element.WoundedNumber)
                    {
                        return true;
                    }
                }
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
