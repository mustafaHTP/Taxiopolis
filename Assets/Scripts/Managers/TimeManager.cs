using Framework.Services;
using UnityEngine;

namespace Game.Managers
{
    /// <summary>
    /// Manages time-related functionalities within the game.
    /// </summary>
    public class TimeManager : MonoBehaviour, IGlobalService
    {
        private const float DefaultFixedDeltaTime = 0.02f;
        private const float DefaultGameTimeScale = 1f;
        private const float DefaultPauseTimeScale = 0f;

        private float _previousTimeScale = DefaultGameTimeScale;
        private bool _isPaused = false;

        public void Init()
        {
            ResetTimeScale();
        }

        public void AfterInit()
        {
            // noop
        }

        public void Dispose()
        {
            // noop
        }

        public void SetPause(bool isPaused)
        {
            _isPaused = isPaused;

            float targetTimeScale;
            if (_isPaused)
            {
                targetTimeScale = DefaultPauseTimeScale;
                _previousTimeScale = Time.timeScale;
            }
            else
            {
                targetTimeScale = _previousTimeScale;
            }

            SetTimeScale(targetTimeScale);
        }

        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = DefaultFixedDeltaTime * Time.timeScale;
        }

        public void ResetTimeScale()
        {
            Time.timeScale = DefaultGameTimeScale;
            Time.fixedDeltaTime = DefaultFixedDeltaTime * Time.timeScale;
        }
    }
}
