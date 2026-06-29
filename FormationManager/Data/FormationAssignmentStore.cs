using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TaleWorlds.Library;

namespace FormationManager.Data
{
    /// <summary>
    /// Persists troop-type-to-formation assignments as an external JSON file per campaign hero.
    /// Key   = CharacterObject.StringId
    /// Value = formation index 0-7 (-1 = no override)
    /// </summary>
    public static class FormationAssignmentStore
    {
        private static string? _currentHeroId;
        private static Dictionary<string, int> _assignments = new();
        private static bool _isDirty;

        private static string GetConfigDir()
        {
            // Documents\Mount and Blade II Bannerlord\Configs\FormationManager\
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(docs, "Mount and Blade II Bannerlord", "Configs", "FormationManager");
        }

        private static string GetFilePath(string heroId)
            => Path.Combine(GetConfigDir(), $"{heroId}.json");

        public static void Load(string heroId)
        {
            _currentHeroId = heroId;
            _assignments = new Dictionary<string, int>();
            _isDirty = false;

            string path = GetFilePath(heroId);
            if (!File.Exists(path))
                return;

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<StorageModel>(json);
                if (data?.Assignments != null)
                    _assignments = data.Assignments;
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[FormationManager] Failed to load assignment data: {ex.Message}",
                    new Color(0.9f, 0.3f, 0.3f)));
            }
        }

        public static void Save()
        {
            if (_currentHeroId == null || !_isDirty)
                return;

            try
            {
                string dir = GetConfigDir();
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var model = new StorageModel { Assignments = _assignments };
                string json = JsonConvert.SerializeObject(model, Formatting.Indented);
                File.WriteAllText(GetFilePath(_currentHeroId), json);
                _isDirty = false;
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[FormationManager] Failed to save assignment data: {ex.Message}",
                    new Color(0.9f, 0.3f, 0.3f)));
            }
        }

        /// <summary>Returns the assigned formation index (0-7), or -1 if none is set.</summary>
        public static int GetAssignment(string troopId)
            => _assignments.TryGetValue(troopId, out int idx) ? idx : -1;

        public static void SetAssignment(string troopId, int formationIndex)
        {
            if (formationIndex < 0)
            {
                ClearAssignment(troopId);
                return;
            }
            _assignments[troopId] = formationIndex;
            _isDirty = true;
        }

        public static void ClearAssignment(string troopId)
        {
            if (_assignments.Remove(troopId))
                _isDirty = true;
        }

        public static bool HasAnyAssignments => _assignments.Count > 0;

        private class StorageModel
        {
            public Dictionary<string, int> Assignments { get; set; } = new();
        }
    }
}
