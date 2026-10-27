using UnityEngine;

namespace Game.Utils
{
    public static class SoundUtils
    {
        public static float ConvertVolumeToDecibels(float volume)
        {
            if (volume <= 0.0001f)
            {
                return -80f; // Minimum decibel level to avoid negative infinity
            }

            return 20f * Mathf.Log10(volume);
        }
    }
}