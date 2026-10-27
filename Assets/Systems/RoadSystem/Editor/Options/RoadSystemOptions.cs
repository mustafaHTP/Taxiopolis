using Game.Utils;
using RoadSystem.Utils;

namespace RoadSystem.Options
{
    public static class RoadSystemOptions
    {
        private static RoadSystemOptionsSO _optionsCached;

        public static RoadSystemOptionsSO Options
        {
            get
            {
                if (_optionsCached != null)
                {
                    return _optionsCached;
                }

                var options = RoadSystemFactory.LoadPrefab<RoadSystemOptionsSO>(
                    RoadSystemDefaults.RoadSystemOptionsPath);
                if (options == null)
                {
                    LogX.LogError($"RoadSystemOptionsSO not found. Please create it under {RoadSystemDefaults.RoadSystemOptionsPath}");

                    return null;
                }

                _optionsCached = options;

                return _optionsCached;
            }
        }
    }
}

