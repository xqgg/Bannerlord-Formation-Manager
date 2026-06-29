using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using Bannerlord.UIExtenderEx;
using FormationManager.Data;
using FormationManager.Patches;

namespace FormationManager
{
    public class SubModule : MBSubModuleBase
    {
        private static Harmony? _harmony;
        private static UIExtender? _uiExtender;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            _harmony = new Harmony("com.formationmanager");
            _harmony.PatchAll();

            _uiExtender = UIExtender.Create("FormationManager");
            _uiExtender.Register(typeof(SubModule).Assembly);
            _uiExtender.Enable();
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
            // Patch is already applied globally; no per-mission registration needed.
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
            FormationAssignmentStore.Save();
        }
    }
}
