using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using FormationManager.Data;

namespace FormationManager.Models
{
    /// <summary>
    /// Native campaign model that decorators the default BattleInitializationModel.
    /// Intercepts available troop types natively to register custom classes 
    /// without relying on Harmony in-memory patching for this component.
    /// </summary>
    public class FormationManagerBattleInitializationModel : BattleInitializationModel
    {
        private readonly BattleInitializationModel _parent;

        public FormationManagerBattleInitializationModel(BattleInitializationModel parent)
        {
            _parent = parent;
        }

        public override List<FormationClass> GetAllAvailableTroopTypes()
        {
            // Call parent model first to get vanilla classes
            List<FormationClass> result = _parent?.GetAllAvailableTroopTypes() ?? new List<FormationClass>();

            var settings = Settings.Instance;
            if (settings != null && !settings.ModEnabled)
                return result;

            if (Campaign.Current == null)
                return result;

            var mainParty = MobileParty.MainParty;
            if (mainParty?.MemberRoster == null)
                return result;

            for (int i = 0; i < mainParty.MemberRoster.Count; i++)
            {
                var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                if (element.Character == null)
                    continue;

                // Only consider non-wounded, active troops
                if (element.Number <= element.WoundedNumber)
                    continue;

                int assignedIndex = FormationAssignmentStore.GetAssignment(element.Character.StringId);
                if (assignedIndex >= 0 && assignedIndex <= 7)
                {
                    FormationClass assignedClass = (FormationClass)assignedIndex;
                    if (!result.Contains(assignedClass))
                    {
                        Logger.Log($"[FormationManagerBattleInitializationModel] Natively registering {assignedClass} for {element.Character.StringId}");
                        result.Add(assignedClass);
                    }
                }
            }

            return result;
        }

        protected override bool CanPlayerSideDeployWithOrderOfBattleAux()
        {
            if (_parent == null)
                return false;

            var method = typeof(BattleInitializationModel).GetMethod("CanPlayerSideDeployWithOrderOfBattleAux", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method == null)
                return false;

            return (bool)method.Invoke(_parent, null);
        }
    }
}
