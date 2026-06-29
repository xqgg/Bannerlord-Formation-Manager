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
        private static bool _sandboxPatched;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            // Clear log from previous session so we start fresh
            try
            {
                string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                string logPath = System.IO.Path.Combine(docs, "Mount and Blade II Bannerlord", "Configs", "FormationManager", "log.txt");
                if (System.IO.File.Exists(logPath)) System.IO.File.Delete(logPath);
            }
            catch { }

            try
            {
                _harmony = new Harmony("com.formationmanager");
                _harmony.PatchAll();
                Logger.Log("[SubModule] Harmony.PatchAll() succeeded.");
            }
            catch (System.Exception ex)
            {
                Logger.Log($"[SubModule] Harmony.PatchAll() FAILED: {ex}");
            }
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

            if (!_sandboxPatched)
            {
                _sandboxPatched = true;
                try
                {
                    Type type = AccessTools.TypeByName("Sandbox.SandboxBattleInitializationModel");
                    if (type != null && _harmony != null)
                    {
                        var original = AccessTools.Method(type, "GetAllAvailableTroopTypes");
                        var postfix = AccessTools.Method(typeof(Patches.SandboxBattleInitializationModelPatch), nameof(Patches.SandboxBattleInitializationModelPatch.Postfix));
                        _harmony.Patch(original, postfix: new HarmonyMethod(postfix));
                        Logger.Log("[SubModule] Manually patched SandboxBattleInitializationModel.GetAllAvailableTroopTypes successfully inside OnGameStart.");
                    }
                    else
                    {
                        Logger.Log("[SubModule] SandboxBattleInitializationModel type not found in OnGameStart.");
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Log($"[SubModule] Failed to manually patch SandboxBattleInitializationModel inside OnGameStart: {ex}");
                }
            }
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
            Logger.Log($"[SubModule] OnBeforeMissionBehaviorInitialize fired. Mission={mission?.GetType()?.Name}");
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
            FormationAssignmentStore.Save();
        }
    }
}
