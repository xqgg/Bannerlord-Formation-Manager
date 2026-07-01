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
            return _parent?.GetAllAvailableTroopTypes() ?? new List<FormationClass>();
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
