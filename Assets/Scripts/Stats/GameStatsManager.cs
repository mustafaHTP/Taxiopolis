using Framework.Services;
using Game.Managers;
using Game.Stats;
using UnityEngine;

namespace Game.Stats
{
    public class GameStatsManager : MonoBehaviour, IGlobalService
    {
        public GameStats GameStats { get; private set; }

        public void Init()
        {
            // noop
        }

        public void AfterInit()
        {
            // noop
        }

        public void Dispose()
        {
            ClearGameStats();
        }

        public void CreateGameStats()
        {
            ClearGameStats();

            GameStats = new GameStats();
            GameStats.RegisterEvents();
        }

        public void FinalizeStats()
        {
            GameStats.FinalizeStats();
        }

        public void ClearGameStats()
        {
            if (GameStats == null) return;

            GameStats.UnregisterEvents();
            GameStats = null;
        }
    }
}
