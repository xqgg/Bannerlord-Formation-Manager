using System;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Library;
using FormationManager.Data;

namespace FormationManager.UI
{
    /// <summary>
    /// Extends each troop row in the party screen with a formation assignment badge.
    /// Click cycles through formations 1-8 then back to "none".
    /// Right-click clears the assignment.
    /// </summary>
    [ViewModelMixin(nameof(PartyCharacterVM.RefreshValues))]
    internal sealed class PartyCharacterVMMixin : BaseViewModelMixin<PartyCharacterVM>
    {
        // Formation labels: index 0 = "—" (none), 1-8 = formation numbers
        private static readonly string[] Labels = { "—", "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };

        private string _formationLabel = "—";
        private bool _isModEnabled;

        public PartyCharacterVMMixin(PartyCharacterVM vm) : base(vm)
        {
            RefreshFormationLabel();
        }

        private void RefreshFormationLabel()
        {
            var settings = Settings.Instance;
            _isModEnabled = settings?.ModEnabled ?? false;

            if (!_isModEnabled || ViewModel == null)
            {
                FormationLabel = "—";
                return;
            }

            var character = ViewModel.Character;
            if (character == null)
            {
                FormationLabel = "—";
                return;
            }

            int idx = FormationAssignmentStore.GetAssignment(character.StringId);
            FormationLabel = idx >= 0 && idx <= 7 ? Labels[idx + 1] : "—";
        }

        /// <summary>The formation badge label bound to the XML widget.</summary>
        [DataSourceProperty]
        public string FormationLabel
        {
            get => _formationLabel;
            set
            {
                if (_formationLabel == value) return;
                _formationLabel = value;
                OnPropertyChanged(nameof(FormationLabel));
            }
        }

        /// <summary>Whether the formation badge should be visible.</summary>
        [DataSourceProperty]
        public bool IsFormationBadgeVisible
        {
            get => _isModEnabled && ViewModel?.IsMainHero == false;
        }

        /// <summary>Cycles the formation assignment forward: none → F1 → F2 → ... → F8 → none.</summary>
        [DataSourceProperty]
        public Action ExecuteCycleFormation => () =>
        {
            var character = ViewModel?.Character;
            if (character == null) return;

            int current = FormationAssignmentStore.GetAssignment(character.StringId);
            int next = current + 1;
            if (next > 7) next = -1; // wrap back to none

            if (next < 0)
                FormationAssignmentStore.ClearAssignment(character.StringId);
            else
                FormationAssignmentStore.SetAssignment(character.StringId, next);

            FormationAssignmentStore.Save();
            RefreshFormationLabel();
        };

        /// <summary>Clears the formation assignment (bound to right-click in XML).</summary>
        [DataSourceProperty]
        public Action ExecuteClearFormation => () =>
        {
            var character = ViewModel?.Character;
            if (character == null) return;
            FormationAssignmentStore.ClearAssignment(character.StringId);
            FormationAssignmentStore.Save();
            RefreshFormationLabel();
        };

        public override void OnRefresh()
        {
            RefreshFormationLabel();
        }
    }
}
