using System;
using UnityEngine;

namespace RoadSystem.Options
{
    [CreateAssetMenu(fileName = "RoadSystemOptions", menuName = "Tools/Road System/Options")]
    public class RoadSystemOptionsSO : ScriptableObject
    {
        public CommonSettings Common = new();
        public NoneModeSettings NoneMode = new();
        public RoadEditModeSettings RoadEditMode = new();
        public RoadLinkModeSettings RoadLinkMode = new();
        public RoadSpawnPointModeSettings RoadSpawnPointMode = new();
        public TrafficLightLinkModeSettings TrafficLightLinkMode = new();

        public void Reset()
        {
            Common = new();
            NoneMode = new();
            RoadEditMode = new();
            RoadLinkMode = new();
            RoadSpawnPointMode = new();
        }

        [Serializable]
        public class CommonSettings
        {
            public float RoadPointRadius = 0.8f;
            public Color RoadPointColor = new(0.0588f, 1f, 0.0196f, 1f);
            public float RoadLineThickness = 2f;
            public float RoadArrowThickness = 2f;
            public float TrafficLightRadius = 0.8f;
            public Color TrafficLightSelectedColor = new(1f, 0f, 0f, 1f);
            public Color TrafficLightNonSelectedColor = new(1f, 0f, 0f, 0.4f);
            public float TrafficLightControllerRadius = 0.8f;
            public Color TrafficLightControllerColor = Color.cyan;
            public Vector3 LabelOffset = new(3f, 0.5f, 2f);
            public Vector2 LabelSize = Vector2.one;
            public int LabelFontSize = 16;
            public Color LabelTextColor = Color.green;
            public Vector2Int LabelPaddingHorizontal = new(4, 4);
            public Vector2Int LabelPaddingVertical = new(2, 2);
            public Color LabelBackgroundColor = new(0f, 0f, 0f, 0.6f);
        }

        [Serializable]
        public class NoneModeSettings
        {
            public Color SelectedColor = new(0f, 1f, 0f, 1f);
            public Color NonSelectedColor = new(0f, 1f, 0f, 0.4f);
        }

        [Serializable]
        public class RoadEditModeSettings
        {
            public Color SelectedLineColor = new(1f, 1f, 0f, 1f);
            public Color NonSelectedLineColor = new(1f, 1f, 0f, 0.4f);
            public float RemoveRoadPointButtonSize = 0.15f;
            public Color RemoveRoadPointButtonColor = Color.red;
            public float PreviewRoadPointSize = 0.8f;
            public Color PreviewAddRoadPointColor = Color.red;
            public float PreviewAddRoadPointNormalSize = 2f;
            public Color PreviewAddRoadPointNormalColor = Color.yellow;
            public float PreviewInsertRoadPointSize = 1.5f;
            public Color PreviewInsertLineColor = Color.cyan;
            public Color PreviewInsertRoadPointColor = Color.cyan;
        }

        [Serializable]
        public class RoadLinkModeSettings
        {
            public Color SelectedLineColor = new(1f, 0.533f, 0f, 1f);
            public Color NonSelectedLineColor = new(1f, 0.533f, 0f, 0.4f);
            public Color RoadPointSelectedButtonColor = Color.magenta;
            public Color RoadPointNonSelectedButtonColor = Color.white;
            public float RoadPointButtonSize = 0.15f;
            public Color SelectedRoadPointConnectionColor = new(0f, 1f, 0.137f, 1f);
            public Color NonSelectedRoadPointConnectionColor = new(0f, 1f, 0.137f, 0.4f);
            public Color PreviewLinkLineColor = Color.green;
            public Vector2 RemoveConnectionButtonOffset = new(-10f, -10f);
            public Vector2 RemoveConnectionButtonSize = new(20f, 20f);
        }

        [Serializable]
        public class RoadSpawnPointModeSettings
        {
            public Color SelectedLineColor = new(0f, 1f, 0.965f, 1f);
            public Color NonSelectedLineColor = new(0f, 1f, 0.965f, 0.4f);
            public Color SelectedRoadPointButtonColor = new(0f, 1f, 0f, 1f);
            public Color NonSelectedRoadPointButtonColor = new(1f, 0f, 0f, 1f);
            public float ButtonSize = 0.15f;
        }

        [Serializable]
        public class TrafficLightLinkModeSettings
        {
            public Color LineColor = new(1f, 0f, 0f, 1f);
            public Color RoadPointSelectedButtonColor = Color.green;
            public Color RoadPointNonSelectedButtonColor = Color.red;
            public float RoadPointButtonSize = 0.35f;
            public Color ConnectionColor = Color.green;
            public Vector2 RemoveConnectionButtonOffset = new(-10f, -10f);
            public Vector2 RemoveConnectionButtonSize = new(20f, 20f);
        }
    }
}
