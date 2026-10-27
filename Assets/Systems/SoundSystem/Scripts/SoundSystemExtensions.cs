using UnityEngine;

namespace Game.Systems.SoundSystem
{
    public static class SoundSystemExtensions
    {
        public static SoundData GetRandomSound(this SoundData[] soundData)
        {
            if (soundData == null || soundData.Length == 0)
                return null;

            int randomIndex = Random.Range(0, soundData.Length);

            return soundData[randomIndex];
        }
    }
}
