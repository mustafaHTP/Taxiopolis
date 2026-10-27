using UnityEngine;

namespace Game.Systems.ComboSystem
{
    [CreateAssetMenu(fileName = "New Combo Data", menuName = "Game/Combo System/Combo Data")]
    public class ComboData : ScriptableObject
    {
        [SerializeField] private ComboType _type;
        [SerializeField]
        [Tooltip("Give cool name to combo instead of drift, crazy drift !")]
        private string _coolName;
        [SerializeField] private int _reward;

        public ComboType Type => _type;
        public string CoolName => _coolName;
        public int Reward => _reward;
    }
}
