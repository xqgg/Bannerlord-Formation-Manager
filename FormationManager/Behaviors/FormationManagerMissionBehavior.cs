using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using FormationManager.Data;

namespace FormationManager.Behaviors
{
    /// <summary>
    /// Mission behavior that registers our formation override with the game's built-in
    /// GetAgentTroopClass_Override event. This event is the single source of truth
    /// that the OOB deployment screen, spawning system, and reinforcement system all
    /// call to determine which formation class a troop belongs to.
    /// </summary>
    internal class FormationManagerMissionBehavior : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            Mission.GetAgentTroopClass_Override += OnGetAgentTroopClass;
        }

        public override void OnRemoveBehavior()
        {
            Mission.GetAgentTroopClass_Override -= OnGetAgentTroopClass;
            base.OnRemoveBehavior();
        }

        private FormationClass OnGetAgentTroopClass(BattleSideEnum side, BasicCharacterObject character)
        {
            // Vanilla fallback
            FormationClass vanillaClass = character?.DefaultFormationClass ?? FormationClass.Infantry;

            var settings = Settings.Instance;
            if (settings != null && !settings.ModEnabled)
                return vanillaClass;

            if (character == null)
                return vanillaClass;

            // Only override in campaign mode
            if (Campaign.Current == null)
                return vanillaClass;

            var mainParty = MobileParty.MainParty;
            if (mainParty?.MemberRoster == null)
                return vanillaClass;

            // Only apply override if this character type is actually in the player's party roster
            bool inPlayerParty = false;
            for (int i = 0; i < mainParty.MemberRoster.Count; i++)
            {
                var element = mainParty.MemberRoster.GetElementCopyAtIndex(i);
                if (element.Character != null && element.Character.StringId == character.StringId)
                {
                    inPlayerParty = true;
                    break;
                }
            }

            if (!inPlayerParty)
                return vanillaClass;

            int assignedIndex = FormationAssignmentStore.GetAssignment(character.StringId);
            if (assignedIndex >= 0 && assignedIndex <= 7)
                return (FormationClass)assignedIndex;

            return vanillaClass;
        }
    }
}
