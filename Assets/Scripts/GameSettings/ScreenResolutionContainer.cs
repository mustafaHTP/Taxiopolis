using UnityEngine;

namespace Game.Settings
{
    [CreateAssetMenu(fileName = "New ScreenResolution Container", menuName = "Game/GameSettings/Screen Resolution Container")]
    public class ScreenResolutionContainer : ScriptableObject
    {
        [SerializeField] private Vector2Int[] _screenResolutions;

        public Vector2Int[] ScreenResolutions => _screenResolutions;
    }
}