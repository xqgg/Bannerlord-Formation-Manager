using System;
using System.Linq;
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
                return;

            int idx = formation.Index;
            bool hasTroops = HasTroopsAssigned(idx);

            if (hasTroops)
            {
                DeploymentFormationClass targetClass = GetCustomAssignmentClass(idx);
                if (targetClass != DeploymentFormationClass.Unset)
                {
                    overriddenClass = targetClass;
                    mustExist = true; // Force card activation
                }
            }
        }

        [HarmonyPostfix]
        private static void Postfix(OrderOfBattleFormationItemVM __instance, Formation formation)
        {
            var settings = Settings.Instance;
            if (settings == null || !settings.ModEnabled)
                return;

            if (formation == null)
                return;

            int idx = formation.Index;
            var selector = __instance.FormationClassSelector;
            if (selector == null)
                return;

            var selectedItem = selector.SelectedItem;
            var selectedClass = selectedItem != null ? selectedItem.FormationClass.ToString() : "null";
            Logger.Log($"[RefreshFormationPatch] Postfix for formation {idx}: SelectedIndex={selector.SelectedIndex}, SelectedItem.FormationClass={selectedClass}");
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

                int assignedIndex = FormationAssignmentStore.GetAssignment(element.Character.StringId);
                if (assignedIndex == formationIndex)
                {
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

    /// <summary>
    /// Patches the SetInitialHeroFormations method on the OOB VM.
    /// 
    /// By default, newly activated cards will have 0% weight, so 100% of the class's troops
    /// will default to the first active card of that class.
    /// 
    /// By intercepting after hero formations are configured, we calculate the exact ratio
    /// of troops assigned to each card based on the player's party roster configuration,
    /// and set the corresponding class VM weights programmatically. This ensures the OOB
    /// engine distributes the correct number of troops to each card.
    /// </summary>
    [HarmonyPatch(typeof(OrderOfBattleVM), "SetInitialHeroFormations")]
    internal static class SetInitialHeroFormationsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(OrderOfBattleVM __instance)
        {
            var settings = Settings.Instance;
            if (settings == null || !settings.ModEnabled)
                return;

            Logger.Log("[SetInitialHeroFormationsPatch] Postfix: Distributing card weights based on custom assignments...");

            int[] infantryCounts = new int[8];
            int[] rangedCounts = new int[8];
            int[] cavalryCounts = new int[8];
            int[] horseArcherCounts = new int[8];

            var mainParty = MobileParty.MainParty;
            if (mainParty?.MemberRoster != null)
            {
                for (int i = 0; i < mainParty.MemberRoster.Count; i++)
                {
                    var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                    if (element.Character == null) continue;
                    if (element.Number <= element.WoundedNumber) continue;

                    int assignedIndex = FormationAssignmentStore.GetAssignment(element.Character.StringId);
                    if (assignedIndex >= 0 && assignedIndex <= 7)
                    {
                        int count = element.Number - element.WoundedNumber;
                        if (assignedIndex == 0 || assignedIndex == 4 || assignedIndex == 5)
                            infantryCounts[assignedIndex] += count;
                        else if (assignedIndex == 1)
                            rangedCounts[assignedIndex] += count;
                        else if (assignedIndex == 2 || assignedIndex == 6 || assignedIndex == 7)
                            cavalryCounts[assignedIndex] += count;
                        else if (assignedIndex == 3)
                            horseArcherCounts[assignedIndex] += count;
                    }
                }
            }

            var mainHero = Hero.MainHero;
            if (mainHero != null)
            {
                int assignedIndex = FormationAssignmentStore.GetAssignment(mainHero.CharacterObject.StringId);
                if (assignedIndex >= 0 && assignedIndex <= 7)
                {
                    if (assignedIndex == 0 || assignedIndex == 4 || assignedIndex == 5)
                        infantryCounts[assignedIndex] += 1;
                    else if (assignedIndex == 1)
                        rangedCounts[assignedIndex] += 1;
                    else if (assignedIndex == 2 || assignedIndex == 6 || assignedIndex == 7)
                        cavalryCounts[assignedIndex] += 1;
                    else if (assignedIndex == 3)
                        horseArcherCounts[assignedIndex] += 1;
                }
                else
                {
                    cavalryCounts[2] += 1; // Default main hero to Cavalry slot
                }
            }

            int totalInfantry = infantryCounts.Sum();
            int totalRanged = rangedCounts.Sum();
            int totalCavalry = cavalryCounts.Sum();
            int totalHorseArcher = horseArcherCounts.Sum();

            Logger.Log($"[SetInitialHeroFormationsPatch] Calculated totals: Infantry={totalInfantry}, Ranged={totalRanged}, Cavalry={totalCavalry}, HorseArcher={totalHorseArcher}");

            var formationsList = __instance.FormationsFirstHalf.Concat(__instance.FormationsSecondHalf).ToList();

            foreach (var item in formationsList)
            {
                if (item.Formation == null) continue;
                int idx = item.Formation.Index;

                foreach (var classVM in item.Classes)
                {
                    if (classVM.IsUnset) continue;

                    int targetWeight = 0;
                    if (classVM.Class == FormationClass.Infantry)
                    {
                        if (totalInfantry > 0)
                            targetWeight = (int)Math.Round((double)infantryCounts[idx] / totalInfantry * 100);
                    }
                    else if (classVM.Class == FormationClass.Ranged)
                    {
                        if (totalRanged > 0)
                            targetWeight = (int)Math.Round((double)rangedCounts[idx] / totalRanged * 100);
                    }
                    else if (classVM.Class == FormationClass.Cavalry)
                    {
                        if (totalCavalry > 0)
                            targetWeight = (int)Math.Round((double)cavalryCounts[idx] / totalCavalry * 100);
                    }
                    else if (classVM.Class == FormationClass.HorseArcher)
                    {
                        if (totalHorseArcher > 0)
                            targetWeight = (int)Math.Round((double)horseArcherCounts[idx] / totalHorseArcher * 100);
                    }

                    classVM.Weight = targetWeight;
                    Logger.Log($"[SetInitialHeroFormationsPatch] Set formation {idx} class {classVM.Class} weight to {targetWeight}%");
                }
            }
        }
    }
}
