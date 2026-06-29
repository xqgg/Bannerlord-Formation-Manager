using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using Bannerlord.UIExtenderEx;
using FormationManager.Data;

namespace FormationManager
{
    public class SubModule : MBSubModuleBase
    {
        private static Harmony? _harmony;
        private static UIExtender? _uiExtender;

        private static bool _uiExtenderInitialized;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            _harmony = new Harmony("com.formationmanager");
            _harmony.PatchAll();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            if (!_uiExtenderInitialized)
            {
                _uiExtenderInitialized = true;
                try
                {
                    _uiExtender = UIExtender.Create("FormationManager");
                    _uiExtender.Register(typeof(SubModule).Assembly);
                    _uiExtender.Enable();
                }
                catch (System.Exception ex)
                {
                    try
                    {
                        string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                        string dir = System.IO.Path.Combine(docs, "Mount and Blade II Bannerlord", "Configs");
                        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                        string path = System.IO.Path.Combine(dir, "FormationManager_LoadError.txt");
                        
                        string errorText = ex.ToString();
                        if (ex is System.Reflection.ReflectionTypeLoadException rtle)
                        {
                            errorText += "\nLoader Exceptions:\n";
                            foreach (var le in rtle.LoaderExceptions)
                            {
                                errorText += le.ToString() + "\n";
                            }
                        }
                        System.IO.File.WriteAllText(path, errorText);
                    }
                    catch
                    {
                        // Ignore secondary writing issues
                    }
                }
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            base.OnGameLoaded(game, initializerObject);
            if (game.GameType is Campaign)
            {
                var hero = TaleWorlds.CampaignSystem.Hero.MainHero;
                if (hero != null)
                    FormationAssignmentStore.Load(hero.StringId);
            }
        }

        public override void OnBeforeMissionBehaviorInitialize(Mission mission)
        {
            base.OnBeforeMissionBehaviorInitialize(mission);
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
            FormationAssignmentStore.Save();
        }
    }
}
