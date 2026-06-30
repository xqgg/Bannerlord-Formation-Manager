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
    // We disable the LogicalClass and PhysicalClass patches to avoid the "class lie".
    // Agents will be evaluated by their true native classes.
    
    /*
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
                case FormationClass.Skirmisher:
                case FormationClass.HeavyInfantry:
                    return FormationClass.Infantry;
                case FormationClass.LightCavalry:
                case FormationClass.HeavyCavalry:
                    return FormationClass.Cavalry;
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
    */

    /// <summary>
    /// Patches the RefreshFormation method on the OOB formation item VM.
    /// Dynamically activates custom cards using the native classes of the assigned troops.
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

        public static DeploymentFormationClass GetCustomAssignmentClass(int formationIndex)
        {
            var mainParty = MobileParty.MainParty;
            if (mainParty?.MemberRoster != null)
            {
                for (int i = 0; i < mainParty.MemberRoster.Count; i++)
                {
                    var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                    if (element.Character == null) continue;
                    if (element.Number <= element.WoundedNumber) continue;

                    int assignedIndex = FormationAssignmentStore.GetAssignment(element.Character.StringId);
                    if (assignedIndex == formationIndex)
                    {
                        var nativeClass = element.Character.DefaultFormationClass;
                        return MapToDeploymentClass(nativeClass);
                    }
                }
            }

            var mainHero = Hero.MainHero;
            if (mainHero != null)
            {
                int assignedIndex = FormationAssignmentStore.GetAssignment(mainHero.CharacterObject.StringId);
                if (assignedIndex == formationIndex)
                {
                    return MapToDeploymentClass(mainHero.CharacterObject.DefaultFormationClass);
                }
            }

            // Fallback to default OOB slot classes if no custom assignment
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

        public static DeploymentFormationClass MapToDeploymentClass(FormationClass fc)
        {
            switch (fc)
            {
                case FormationClass.Infantry:
                case FormationClass.HeavyInfantry:
                case FormationClass.Skirmisher:
                    return DeploymentFormationClass.Infantry;
                case FormationClass.Ranged:
                    return DeploymentFormationClass.Ranged;
                case FormationClass.Cavalry:
                case FormationClass.LightCavalry:
                case FormationClass.HeavyCavalry:
                    return DeploymentFormationClass.Cavalry;
                case FormationClass.HorseArcher:
                    return DeploymentFormationClass.HorseArcher;
                default:
                    return DeploymentFormationClass.Unset;
            }
        }
    }

    /// <summary>
    /// Patches the SetInitialHeroFormations method on the OOB VM.
    /// Programmatically distributes card weights based on dynamic troop native classes.
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

            DeploymentFormationClass[] cardClasses = new DeploymentFormationClass[8];
            for (int i = 0; i < 8; i++)
            {
                cardClasses[i] = RefreshFormationPatch.GetCustomAssignmentClass(i);
            }

            int[] classCounts = new int[8];

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
                        classCounts[assignedIndex] += count;
                    }
                }
            }

            var mainHero = Hero.MainHero;
            if (mainHero != null)
            {
                int assignedIndex = FormationAssignmentStore.GetAssignment(mainHero.CharacterObject.StringId);
                if (assignedIndex >= 0 && assignedIndex <= 7)
                {
                    classCounts[assignedIndex] += 1;
                }
                else
                {
                    classCounts[2] += 1; // Default main hero to Cavalry slot
                }
            }

            int[] totalByClass = new int[7]; // DeploymentFormationClass has values 0 to 6
            for (int i = 0; i < 8; i++)
            {
                int classVal = (int)cardClasses[i];
                if (classVal >= 0 && classVal < 7)
                {
                    totalByClass[classVal] += classCounts[i];
                }
            }

            var formationsList = __instance.FormationsFirstHalf.Concat(__instance.FormationsSecondHalf).ToList();

            foreach (var item in formationsList)
            {
                if (item.Formation == null) continue;
                int idx = item.Formation.Index;
                var cardClass = cardClasses[idx];

                foreach (var classVM in item.Classes)
                {
                    if (classVM.IsUnset) continue;

                    if (RefreshFormationPatch.MapToDeploymentClass(classVM.Class) == cardClass)
                    {
                        int total = totalByClass[(int)cardClass];
                        int targetWeight = 0;
                        if (total > 0)
                        {
                            targetWeight = (int)Math.Round((double)classCounts[idx] / total * 100);
                        }
                        classVM.Weight = targetWeight;
                        Logger.Log($"[SetInitialHeroFormationsPatch] Set formation {idx} class {classVM.Class} weight to {targetWeight}%");
                    }
                }
            }
        }
    }
}
