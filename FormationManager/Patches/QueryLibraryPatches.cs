using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace FormationManager.Patches
{
    /// <summary>
    /// Patches TaleWorlds.MountAndBlade.QueryLibrary methods.
    /// Disabled: HarmonyPatch attribute commented out to let agents classify naturally by their true types.
    /// </summary>
    // [HarmonyPatch(typeof(QueryLibrary))]
    public static class QueryLibraryPatches
    {
        private static bool IsModActive()
        {
            return Settings.Instance != null && Settings.Instance.ModEnabled;
        }

        // [HarmonyPatch(nameof(QueryLibrary.IsInfantry))]
        [HarmonyPostfix]
        public static void IsInfantryPostfix(Agent a, ref bool __result)
        {
        }
    }
}
