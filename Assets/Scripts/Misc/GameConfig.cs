using UnityEngine;

namespace Game.Misc
{
    [CreateAssetMenu(fileName = "New Game Config", menuName = "Game/Config/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("How much time player starts with at the start")]
        private float _startTimeInSeconds;
        [SerializeField]
        [Tooltip("It indicates that player should hurry if the game time under that hurry up time")]
        private float _hurryUpTime;
        [SerializeField]
        [Tooltip("should end game on game time end")]
        private bool _endGameOnGameTimeEnd;
        [SerializeField] private float _gameOverSequenceDuration;

        public float StartTimeInSeconds => _startTimeInSeconds;
        public bool EndGameOnGameTimeEnd => _endGameOnGameTimeEnd;
        public float GameOverSequenceDuration => _gameOverSequenceDuration;
        public float HurryUpTime => _hurryUpTime;
    }
}
