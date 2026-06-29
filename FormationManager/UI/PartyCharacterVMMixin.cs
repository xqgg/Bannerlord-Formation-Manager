using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Library;
using FormationManager.Data;

namespace FormationManager.UI
{
    /// <summary>
    /// Extends each troop row in the party screen with a formation assignment badge.
    /// Properties here bind directly into the injected widget from PartyTroopTupleFormationBadgePatch.
    /// - Click  : cycles none → F1 → F2 → … → F8 → none
    /// - Right-click: clears assignment immediately
    /// </summary>
    [ViewModelMixin(nameof(PartyCharacterVM.RefreshValues))]
    internal sealed class PartyCharacterVMMixin : BaseViewModelMixin<PartyCharacterVM>
    {
        private static readonly string[] Labels = { "\u2014", "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };

        private string _formationLabel = "\u2014";
        private bool _isFormationBadgeVisible;

        private static int _instantiationCount = 0;

        public PartyCharacterVMMixin(PartyCharacterVM vm) : base(vm)
        {
            _instantiationCount++;
            try
            {
                string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                string path = System.IO.Path.Combine(docs, "Mount and Blade II Bannerlord", "Configs", "FormationManager_VMDebug.txt");
                System.IO.File.WriteAllText(path, $"Mixin instantiated. Count: {_instantiationCount}. Last Troop: {vm?.Character?.Name?.ToString() ?? "null"}");
            }
            catch {}
            Refresh();
        }

        // ── Bindable properties ────────────────────────────────────────────────

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

        [DataSourceProperty]
        public bool IsFormationBadgeVisible
        {
            get => _isFormationBadgeVisible;
            set
            {
                if (_isFormationBadgeVisible == value) return;
                _isFormationBadgeVisible = value;
                OnPropertyChanged(nameof(IsFormationBadgeVisible));
            }
        }

        // ── Commands (void methods — bound via Command.Click in the widget XML) ──

        [DataSourceProperty]
        public void ExecuteCycleFormation()
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
            Refresh();
        }

        [DataSourceProperty]
        public void ExecuteClearFormation()
        {
            var character = ViewModel?.Character;
            if (character == null) return;
            FormationAssignmentStore.ClearAssignment(character.StringId);
            FormationAssignmentStore.Save();
            Refresh();
        }

        // ── Internal refresh ──────────────────────────────────────────────────

        private void Refresh()
        {
            var settings = Settings.Instance;
            bool modEnabled = settings?.ModEnabled ?? true;

            // Badge is only shown for regular (non-hero) player-side troops when mod is on
            IsFormationBadgeVisible = modEnabled && ViewModel != null && !ViewModel.IsMainHero && !ViewModel.IsPrisoner;

            if (ViewModel?.Character == null || !modEnabled)
            {
                FormationLabel = "\u2014";
                return;
            }

            int idx = FormationAssignmentStore.GetAssignment(ViewModel.Character.StringId);
            FormationLabel = (idx >= 0 && idx <= 7) ? Labels[idx + 1] : "\u2014";
        }

        public override void OnRefresh() => Refresh();
    }
}
