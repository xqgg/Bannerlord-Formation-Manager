using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace FormationManager.Patches
{
    /// <summary>
    /// Patches TaleWorlds.MountAndBlade.QueryLibrary methods.
    /// 
    /// By default, Bannerlord's QueryLibrary classifies agents (e.g. IsCavalry, IsInfantry)
    /// strictly by their physical characteristics (like having a mount or a ranged weapon).
    /// Since the user assigns foot troops to cavalry/horse-archer slots, QueryLibrary would
    /// classify them as Infantry/Ranged, causing OOB card class counts to show 0.
    /// 
    /// By overriding these queries to check the agent's actual assigned Formation Index (0-7),
    /// we ensure that foot troops assigned to cavalry/horse-archer cards are counted correctly 
    /// in the OOB UI class counts.
    /// </summary>
    [HarmonyPatch(typeof(QueryLibrary))]
    public static class QueryLibraryPatches
    {
        private static bool IsModActive()
        {
            return Settings.Instance != null && Settings.Instance.ModEnabled;
        }

        [HarmonyPatch(nameof(QueryLibrary.IsInfantry))]
        [HarmonyPostfix]
        public static void IsInfantryPostfix(Agent agent, ref bool __result)
        {
            if (!IsModActive()) return;
            var formation = agent?.Formation;
            if (formation != null)
            {
                int idx = formation.Index;
                __result = (idx == 0 || idx == 4 || idx == 5);
            }
        }

        [HarmonyPatch(nameof(QueryLibrary.IsInfantryWithoutBanner))]
        [HarmonyPostfix]
        public static void IsInfantryWithoutBannerPostfix(Agent agent, ref bool __result)
        {
            if (!IsModActive()) return;
            var formation = agent?.Formation;
            if (formation != null)
            {
                int idx = formation.Index;
                __result = (idx == 0 || idx == 4 || idx == 5) && agent?.Banner == null;
            }
        }

        [HarmonyPatch(nameof(QueryLibrary.IsRanged))]
        [HarmonyPostfix]
        public static void IsRangedPostfix(Agent agent, ref bool __result)
        {
            if (!IsModActive()) return;
            var formation = agent?.Formation;
            if (formation != null)
            {
                __result = (formation.Index == 1);
            }
        }

        [HarmonyPatch(nameof(QueryLibrary.IsRangedWithoutBanner))]
        [HarmonyPostfix]
        public static void IsRangedWithoutBannerPostfix(Agent agent, ref bool __result)
        {
            if (!IsModActive()) return;
            var formation = agent?.Formation;
            if (formation != null)
            {
                __result = (formation.Index == 1) && agent?.Banner == null;
            }
        }

        [HarmonyPatch(nameof(QueryLibrary.IsCavalry))]
        [HarmonyPostfix]
        public static void IsCavalryPostfix(Agent agent, ref bool __result)
        {
            if (!IsModActive()) return;
            var formation = agent?.Formation;
            if (formation != null)
            {
                int idx = formation.Index;
                __result = (idx == 2 || idx == 6 || idx == 7);
            }
        }

        [HarmonyPatch(nameof(QueryLibrary.IsCavalryWithoutBanner))]
        [HarmonyPostfix]
        public static void IsCavalryWithoutBannerPostfix(Agent agent, ref bool __result)
        {
            if (!IsModActive()) return;
            var formation = agent?.Formation;
            if (formation != null)
            {
                int idx = formation.Index;
                __result = (idx == 2 || idx == 6 || idx == 7) && agent?.Banner == null;
            }
        }

        [HarmonyPatch(nameof(QueryLibrary.IsRangedCavalry))]
        [HarmonyPostfix]
        public static void IsRangedCavalryPostfix(Agent agent, ref bool __result)
        {
            if (!IsModActive()) return;
            var formation = agent?.Formation;
            if (formation != null)
            {
                __result = (formation.Index == 3);
            }
        }

        [HarmonyPatch(nameof(QueryLibrary.IsRangedCavalryWithoutBanner))]
        [HarmonyPostfix]
        public static void IsRangedCavalryWithoutBannerPostfix(Agent agent, ref bool __result)
        {
            if (!IsModActive()) return;
            var formation = agent?.Formation;
            if (formation != null)
            {
                __result = (formation.Index == 3) && agent?.Banner == null;
            }
        }
    }
}
