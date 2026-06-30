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
    /// We keep this class but disable the subscription to GetAgentTroopClass_Override
    /// because we are no longer lying about agent classes.
    /// </summary>
    [HarmonyPatch(typeof(Mission))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new System.Type[] { typeof(MissionInitializerRecord), typeof(MissionState), typeof(bool) })]
    internal static class MissionConstructorPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Mission __instance)
        {
            // Disabled the override subscription to respect native troop types.
            // Logger.Log("MissionConstructorPatch.Postfix fired. Subscribing to GetAgentTroopClass_Override.");
            // __instance.GetAgentTroopClass_Override += OnGetAgentTroopClass;
        }

        private static FormationClass OnGetAgentTroopClass(BattleSideEnum side, BasicCharacterObject character)
        {
            return character?.DefaultFormationClass ?? FormationClass.Infantry;
        }
    }
}
