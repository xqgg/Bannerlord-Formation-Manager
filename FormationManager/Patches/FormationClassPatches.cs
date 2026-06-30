using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;

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
}
