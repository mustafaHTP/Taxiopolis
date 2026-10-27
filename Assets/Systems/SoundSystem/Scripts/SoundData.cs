using UnityEngine;
using UnityEngine.Audio;

namespace Game.Systems.SoundSystem
{
    [CreateAssetMenu(fileName = "New Sound Data", menuName = "Game/Sound System/Sound Data")]
    public class SoundData : ScriptableObject
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField]
        [Range(0f, 1f)]
        private float _volume = 1f;
        [SerializeField] private bool _isLoop;
        [SerializeField] private AudioMixerGroup _audioMixerGroup;

        public AudioClip Clip => _clip;
        public float Volume => _volume;
        public AudioMixerGroup AudioMixerGroup => _audioMixerGroup;
        public bool IsLoop => _isLoop;
    }
}
