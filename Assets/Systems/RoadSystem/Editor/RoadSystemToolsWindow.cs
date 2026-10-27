using Game.Utils;
using RoadSystem;
using RoadSystem.TrafficLightControl;
using RoadSystem.Utils;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class RoadSystemToolsWindow : EditorWindow
{
    private Vector3? _previewRoadSpawnPosition = null;
    private Vector3? _previewRoadSpawnNormal = null;
    private Vector3? _previewTrafficLightControllerSpawnPosition = null;
    private Vector3? _previewTrafficLightControllerSpawnNormal = null;
    private readonly Dictionary<string, VisualElement> _visualElements = new();

    [MenuItem("Tools/Road System/Tools Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<RoadSystemToolsWindow>("Road System Tools");
    }

    public void CreateGUI()
    {
        var headerLabel = new Label("Road System Tools");
        headerLabel.style.marginBottom = 10;
        headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        headerLabel.style.fontSize = 20;
        headerLabel.style.alignSelf = Align.Center;

        var modeLabel = new Label($"Mode: {RoadEditManager.RoadEditMode}");
        modeLabel.style.marginBottom = 5;
        modeLabel.style.unityFontStyleAndWeight = FontStyle.BoldAndItalic;
        modeLabel.style.fontSize = 14;
        modeLabel.style.alignSelf = Align.FlexStart;

        var cancelModeButton = new Button(() => RoadEditManager.ChangeMode(RoadEditMode.None))
        {
            text = "Cancel Mode (Esc)",
        };
        cancelModeButton.style.backgroundColor = new Color(0.749f, 0.012f, 0.071f);
        cancelModeButton.style.alignSelf = Align.FlexEnd;
        cancelModeButton.RegisterCallback<MouseEnterEvent>(evt =>
        {
            cancelModeButton.style.backgroundColor = new Color(0.9f, 0.1f, 0.1f);
        });
        cancelModeButton.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            cancelModeButton.style.backgroundColor = new Color(0.749f, 0.012f, 0.071f);
        });

        var scrollView = new ScrollView(ScrollViewMode.Vertical);

        var showRoadsToggle = new Toggle("Show Roads")
        {
            value = RoadEditManager.IsRoadGizmoVisible
        };
        var showTrafficLightsToggle = new Toggle("Show Traffic Lights")
        {
            value = RoadEditManager.IsTrafficLightGizmoVisible
        };

        showRoadsToggle.RegisterValueChangedCallback(evt =>
        {
            RoadEditManager.IsRoadGizmoVisible = evt.newValue;
            SceneView.RepaintAll();
        });

        showTrafficLightsToggle.RegisterValueChangedCallback(evt =>
        {
            RoadEditManager.IsTrafficLightGizmoVisible = evt.newValue;
            SceneView.RepaintAll();
        });

        // Road Tools
        Foldout roadfoldout = new()
        {
            text = "Road Tools"
        };

        // Traffic Light Tools
        Foldout trafficLightfoldout = new()
        {
            text = "Traffic Light Tools"
        };

        // Common Tools
        Foldout commonfoldout = new()
        {
            text = "Common Tools"
        };

        // Horizontal container
        var buttonRow = new VisualElement();
        buttonRow.style.flexDirection = FlexDirection.Row;
        buttonRow.style.flexWrap = Wrap.Wrap;
        buttonRow.style.justifyContent = Justify.SpaceBetween;
        buttonRow.style.alignItems = Align.Center;
        buttonRow.style.marginTop = 10;
        buttonRow.style.marginBottom = 10;

        // Horizontal container
        var buttonRow2 = new VisualElement();
        buttonRow2.style.flexDirection = FlexDirection.Row;
        buttonRow2.style.flexWrap = Wrap.Wrap;
        buttonRow2.style.justifyContent = Justify.SpaceBetween;
        buttonRow2.style.alignItems = Align.Center;
        buttonRow2.style.marginTop = 10;

        // Horizontal container
        var buttonRow3 = new VisualElement();
        buttonRow3.style.flexDirection = FlexDirection.Column;
        buttonRow3.style.flexWrap = Wrap.Wrap;
        buttonRow3.style.justifyContent = Justify.Center;
        buttonRow3.style.alignItems = Align.Stretch;
        buttonRow3.style.marginTop = 10;

        // Button textures
        var roadEditTexture =
            EditorGUIUtility.Load("Assets/Systems/RoadSystem/Icons/edit-text.png") as Texture2D;
        var roadLinkTexture =
            EditorGUIUtility.Load("Assets/Systems/RoadSystem/Icons/hyperlink.png") as Texture2D;
        var roadSpawnPointTexture =
            EditorGUIUtility.Load("Assets/Systems/RoadSystem/Icons/round.png") as Texture2D;
        var roadSpawnTexture =
            EditorGUIUtility.Load("Assets/Systems/RoadSystem/Icons/build-road.png") as Texture2D;
        var trafficLightLinkTexture =
            EditorGUIUtility.Load("Assets/Systems/RoadSystem/Icons/traffic-light.png") as Texture2D;
        var refreshRoadPointsTexture =
            EditorGUIUtility.Load("Assets/Systems/RoadSystem/Icons/recycle.png") as Texture2D;
        var trafficLightControllerSpawnTexture =
            EditorGUIUtility.Load("Assets/Systems/RoadSystem/Icons/add-traffic-light-controller.png") as Texture2D;
        var toggleAllSpawnPointsTexture =
            EditorGUIUtility.Load("Assets/Systems/RoadSystem/Icons/toggle-spawn-points.png") as Texture2D;
        var clearSpawnPointsTexture =
            EditorGUIUtility.Load("Assets/Systems/RoadSystem/Icons/clear-spawn-points.png") as Texture2D;

        // Create buttons
        var roadEditButton = CreateButton(
            roadEditTexture,
            "Road Edit Mode",
            () =>
            {
                if (!(Selection.activeObject != null && Selection.activeObject.GetComponent<Road>()))
                {
                    LogX.LogWarning("Please select a road to enter edit mode.");

                    return;
                }

                if (Selection.count > 1)
                {
                    LogX.LogWarning("Please select only one road to enter edit mode.");

                    return;
                }

                if (RoadEditManager.RoadEditMode != RoadEditMode.RoadEdit)
                {
                    RoadEditManager.ChangeMode(RoadEditMode.RoadEdit);
                }
                else
                {
                    RoadEditManager.ChangeMode(RoadEditMode.None);
                }
            });
        var roadLinkButton = CreateButton(
            roadLinkTexture,
            "Road Link Mode",
            () =>
            {
                if (!(Selection.activeObject != null && Selection.activeObject.GetComponent<Road>()))
                {
                    LogX.LogWarning("Please select a road to enter link mode.");
                    return;
                }

                if (Selection.count > 1)
                {
                    LogX.LogWarning("Please select only one road to enter edit mode.");

                    return;
                }

                if (RoadEditManager.RoadEditMode != RoadEditMode.RoadLink)
                {
                    RoadEditManager.ChangeMode(RoadEditMode.RoadLink);
                }
                else
                {
                    RoadEditManager.ChangeMode(RoadEditMode.None);
                }

                Debug.Log("Link clicked");
            });
        var roadSpawnPointButton = CreateButton(
            roadSpawnPointTexture,
            "Road Spawn Point Mode",
            () =>
            {
                if (!(Selection.activeObject != null && Selection.activeObject.GetComponent<Road>()))
                {
                    LogX.LogWarning("Please select a road to enter spawn point mode.");
                    return;
                }

                if (Selection.count > 1)
                {
                    LogX.LogWarning("Please select only one road to enter edit mode.");

                    return;
                }

                if (RoadEditManager.RoadEditMode != RoadEditMode.RoadSpawnPoint)
                {
                    RoadEditManager.ChangeMode(RoadEditMode.RoadSpawnPoint);
                }
                else
                {
                    RoadEditManager.ChangeMode(RoadEditMode.None);
                }
            });

        var spawnRoadButton = CreateButton(
            roadSpawnTexture,
            "Road Spawn Mode",
            () =>
            {
                if (RoadEditManager.RoadEditMode != RoadEditMode.SpawnRoad)
                {
                    RoadEditManager.ChangeMode(RoadEditMode.SpawnRoad);
                }
                else
                {
                    RoadEditManager.ChangeMode(RoadEditMode.None);
                }
            });

        var trafficLightLinkButton = CreateButton(
            trafficLightLinkTexture,
            "Connect traffic lights and road point",
            () =>
            {
                if (!(Selection.activeObject != null && Selection.activeObject.GetComponent<TrafficLight>()))
                {
                    LogX.LogWarning("Please select a traffic light to enter traffic light link mode.");
                    return;
                }

                if (Selection.count > 1)
                {
                    LogX.LogWarning("Please select only one traffic light to enter traffic light link mode.");

                    return;
                }

                if (RoadEditManager.RoadEditMode != RoadEditMode.TrafficLightLink)
                {
                    RoadEditManager.ChangeMode(RoadEditMode.TrafficLightLink);
                }
                else
                {
                    RoadEditManager.ChangeMode(RoadEditMode.None);
                }
            });

        var spawnTrafficLightControllerButton = CreateButton(
            trafficLightControllerSpawnTexture,
            "Spawn Traffic Light Controller",
            () =>
            {
                if (RoadEditManager.RoadEditMode != RoadEditMode.SpawnTrafficLightController)
                {
                    RoadEditManager.ChangeMode(RoadEditMode.SpawnTrafficLightController);
                }
                else
                {
                    RoadEditManager.ChangeMode(RoadEditMode.None);
                }
            });

        var refreshRoadPointsButton = CreateButton(
            refreshRoadPointsTexture,
            "Regenerate roadpoints",
            () =>
            {
                RefreshRoadPoints();
            });

        var clearSpawnPointsButton = CreateButton(
            clearSpawnPointsTexture,
            "Clear Spawn Points",
            () =>
            {
                var roads = FindObjectsByType<RoadSpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (roads.Length == 0)
                {
                    LogX.LogWarning("No roads found in the scene. Please add a road to clear spawn points.");
                    return;
                }
                for (int i = 0; i < roads.Length; i++)
                {
                    var roadSpawnPoint = roads[i];
                    DestroyImmediate(roadSpawnPoint);

                }
                LogX.LogDebug("Cleared all spawn points from all roads in the scene.");
            });

        var toggleAllSpawnPointsButton = CreateButton(
            toggleAllSpawnPointsTexture,
            "Toggle All Spawn Points",
            () =>
            {
                var roadPoints = FindObjectsByType<RoadPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (roadPoints.Length == 0)
                {
                    LogX.LogWarning("No roads found in the scene. Please add a road to toggle spawn points.");
                    return;
                }
                for (int i = 0; i < roadPoints.Length; i++)
                {
                    if (!roadPoints[i].TryGetComponent(out RoadSpawnPoint roadSpawnPoint))
                    {
                        roadPoints[i].AddComponent<RoadSpawnPoint>();
                    }
                }
                LogX.LogDebug("Toggled spawn points on all road points in the scene.");
            });

        // Construct UI
        var root = rootVisualElement;
        root.Add(headerLabel);
        root.Add(modeLabel);
        root.Add(cancelModeButton);
        root.Add(scrollView);
        scrollView.Add(roadfoldout);
        scrollView.Add(trafficLightfoldout);
        scrollView.Add(commonfoldout);
        roadfoldout.Add(buttonRow);
        trafficLightfoldout.Add(buttonRow2);
        commonfoldout.Add(buttonRow3);

        // Add buttons
        buttonRow.Add(roadEditButton);
        buttonRow.Add(roadLinkButton);
        buttonRow.Add(roadSpawnPointButton);
        buttonRow.Add(spawnRoadButton);

        buttonRow2.Add(trafficLightLinkButton);
        buttonRow2.Add(spawnTrafficLightControllerButton);

        buttonRow3.Add(refreshRoadPointsButton);
        buttonRow3.Add(toggleAllSpawnPointsButton);
        buttonRow3.Add(clearSpawnPointsButton);
        buttonRow3.Add(showRoadsToggle);
        buttonRow3.Add(showTrafficLightsToggle);

        _visualElements.Clear();
        _visualElements.Add("modeLabel", modeLabel);
        _visualElements.Add("roadEditButton", roadEditButton);
        _visualElements.Add("roadLinkButton", roadLinkButton);
        _visualElements.Add("roadSpawnPointButton", roadSpawnPointButton);
        _visualElements.Add("spawnRoadButton", spawnRoadButton);
        _visualElements.Add("trafficLightLinkButton", trafficLightLinkButton);
        _visualElements.Add("spawnTrafficLightControllerButton", spawnTrafficLightControllerButton);

        RegisterEventsForVisualElements();
    }

    private Button CreateButton(Texture2D icon, string tooltip, Action onClick, Func<bool> isEnabled = null)
    {
        var btn = new Button(onClick);
        btn.style.height = 50;
        btn.style.flexGrow = 1;
        btn.style.borderTopColor = Color.green;
        btn.tooltip = tooltip;
        btn.style.flexDirection = FlexDirection.Row;
        btn.style.justifyContent = Justify.Center;
        btn.style.alignItems = Align.Center;
        btn.SetEnabled(isEnabled?.Invoke() ?? true);

        if (icon != null)
        {
            var img = new Image { image = icon };
            img.style.width = 32;
            img.style.height = 32;

            btn.Add(img);
        }

        return btn;
    }

    private void RegisterEventsForVisualElements()
    {
        RoadEditManager.OnModeChanged += RoadEditManager_OnModeChanged;
        SceneView.duringSceneGui += SceneView_duringSceneGui;
    }

    private void SceneView_duringSceneGui(SceneView obj)
    {
        if (Event.current.type == EventType.KeyDown
            && Event.current.keyCode == KeyCode.Escape)
        {
            RoadEditManager.ChangeMode(RoadEditMode.None);

            Event.current.Use();
        }
    }

    private void UnregisterEventsForVisualElements()
    {
        RoadEditManager.OnModeChanged -= RoadEditManager_OnModeChanged;
        SceneView.duringSceneGui -= SceneView_duringSceneGui;
    }

    private void RoadEditManager_OnModeChanged()
    {
        // Update mode label for user feedback
        if (_visualElements.TryGetValue("modeLabel", out var modeLabel))
        {
            (modeLabel as Label).text = $"Mode: {RoadEditManager.RoadEditMode}";
        }

        // Change style of buttons based on mode
        // It is for user feedback it is not a requirement
        // Buttons that are tied to road edit modes, get highlighted when active
        RemoveActiveButtonStyle("roadEditButton");
        RemoveActiveButtonStyle("roadLinkButton");
        RemoveActiveButtonStyle("roadSpawnPointButton");
        RemoveActiveButtonStyle("spawnRoadButton");
        RemoveActiveButtonStyle("trafficLightLinkButton");
        RemoveActiveButtonStyle("spawnTrafficLightControllerButton");
        switch (RoadEditManager.RoadEditMode)
        {
            case RoadEditMode.None:
                // Nothing to do
                break;
            case RoadEditMode.RoadEdit:
                ApplyActiveButtonStyle("roadEditButton");
                break;
            case RoadEditMode.RoadLink:
                ApplyActiveButtonStyle("roadLinkButton");
                break;
            case RoadEditMode.RoadSpawnPoint:
                ApplyActiveButtonStyle("roadSpawnPointButton");
                break;
            case RoadEditMode.SpawnRoad:
                ApplyActiveButtonStyle("spawnRoadButton");
                break;
            case RoadEditMode.TrafficLightLink:
                ApplyActiveButtonStyle("trafficLightLinkButton");
                break;
            case RoadEditMode.SpawnTrafficLightController:
                ApplyActiveButtonStyle("spawnTrafficLightControllerButton");
                break;
        }

        ResetFields();
    }

    private void ApplyActiveButtonStyle(string visualElementKey)
    {
        if (_visualElements.TryGetValue(visualElementKey, out var button))
        {
            button.style.backgroundColor = new Color(0.031f, 0.78f, 0);
        }
    }

    private void RemoveActiveButtonStyle(string visualElementKey)
    {
        if (_visualElements.TryGetValue(visualElementKey, out var button))
        {
            button.style.backgroundColor = StyleKeyword.Null;
        }
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        UnregisterEventsForVisualElements();

        RoadEditManager.ChangeMode(RoadEditMode.None);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        switch (RoadEditManager.RoadEditMode)
        {
            case RoadEditMode.SpawnRoad:
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                HandleSpawnRoad(e);
                break;
            case RoadEditMode.SpawnTrafficLightController:
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                HandleSpawnTrafficLightController(e);
                break;
        }
    }

    private void RefreshRoadPoints()
    {
        var roads = FindObjectsByType<Road>(
            FindObjectsInactive.Exclude,
             FindObjectsSortMode.None);
        if (roads.Length == 0)
        {
            LogX.LogWarning("No roads found in the scene. Please add a road to refresh road points.");

            return;
        }

        for (int i = 0; i < roads.Length; i++)
        {
            var currentRoad = roads[i];

            RoadSystemUtils.RemoveNonExistingRoadPoints(currentRoad);
            RoadSystemUtils.RemoveNonExistingConnections(currentRoad);
        }
    }

    private void HandleSpawnTrafficLightController(Event e)
    {
        // OP: Spawn traffic light controller
        if (e.control && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TrafficLightController trafficLightControllerPrefab =
                    RoadSystemFactory.LoadPrefab<TrafficLightController>(
                        RoadSystemDefaults.TrafficLightControllerPrefabPath);
                if (trafficLightControllerPrefab == null)
                {
                    LogX.LogError("Failed to load Traffic Light Controller prefab.");

                    return;
                }

                var newTrafficLightController =
                    Instantiate(
                        trafficLightControllerPrefab,
                        hit.point,
                        Quaternion.LookRotation(hit.normal));

                newTrafficLightController.name = "Traffic Light Controller";

                Undo.RegisterCreatedObjectUndo(
                    newTrafficLightController,
                    "Spawn Traffic Light Controller");

                Selection.activeGameObject = newTrafficLightController.gameObject;

                LogX.LogDebug("Traffic Light Controller spawned at position: " + hit.point);

                SceneView.RepaintAll();
            }

            RoadSystemUtils.UseEventIfNeeded(e);
            SceneView.RepaintAll();
        }

        // Preview
        if (e.control && e.type == EventType.MouseMove)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _previewTrafficLightControllerSpawnPosition = hit.point;
                _previewTrafficLightControllerSpawnNormal = hit.normal;
            }
            else
            {
                _previewTrafficLightControllerSpawnPosition = null;
                _previewTrafficLightControllerSpawnNormal = null;
            }

            RoadSystemUtils.UseEventIfNeeded(e);
            SceneView.RepaintAll();
        }


        if (!e.control || (e.type == EventType.MouseUp && e.button == 0))
        {
            _previewTrafficLightControllerSpawnPosition = null;
            _previewTrafficLightControllerSpawnNormal = null;
        }

        if (_previewTrafficLightControllerSpawnPosition.HasValue)
        {
            Handles.color = Color.cyan;
            Handles.SphereHandleCap(
                0,
                _previewTrafficLightControllerSpawnPosition.Value,
                Quaternion.identity,
                1f,
                EventType.Repaint);
            Handles.ArrowHandleCap(
                0,
                _previewTrafficLightControllerSpawnPosition.Value,
                Quaternion.LookRotation(_previewTrafficLightControllerSpawnNormal.Value),
                4f,
                EventType.Repaint);
        }
    }

    private void HandleSpawnRoad(Event e)
    {
        // OP: Spawn road
        if (e.control && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Road roadPrefab = RoadSystemFactory.LoadPrefab<Road>(
                    RoadSystemDefaults.RoadPrefabPath);
                if (roadPrefab == null)
                {
                    LogX.LogError("Failed to load Road prefab.");

                    return;
                }

                var newRoad =
                    Instantiate(
                        roadPrefab,
                        hit.point,
                        Quaternion.LookRotation(hit.normal));

                newRoad.name = "Road";

                Undo.RegisterCreatedObjectUndo(newRoad, "Spawn Road");

                Selection.activeGameObject = newRoad.gameObject;

                LogX.LogDebug("Road spawned at position: " + hit.point);

                RoadSystemUtils.UseEventIfNeeded(e);
                SceneView.RepaintAll();
            }

            RoadSystemUtils.UseEventIfNeeded(e);
            SceneView.RepaintAll();
        }

        // Preview
        if (e.control && e.type == EventType.MouseMove)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _previewRoadSpawnPosition = hit.point;
                _previewRoadSpawnNormal = hit.normal;
            }
            else
            {
                _previewRoadSpawnPosition = null;
                _previewRoadSpawnNormal = null;
            }

            RoadSystemUtils.UseEventIfNeeded(e);
            SceneView.RepaintAll();
        }

        if (!e.control || (e.type == EventType.MouseUp && e.button == 0))
        {
            _previewRoadSpawnPosition = null;
            _previewRoadSpawnNormal = null;
        }

        if (_previewRoadSpawnPosition.HasValue)
        {
            Handles.color = Color.green;
            Handles.SphereHandleCap(
                0,
                _previewRoadSpawnPosition.Value,
                Quaternion.identity,
                1f,
                EventType.Repaint);
            Handles.ArrowHandleCap(
                0,
                _previewRoadSpawnPosition.Value,
                Quaternion.LookRotation(_previewRoadSpawnNormal.Value),
                4f,
                EventType.Repaint);
        }
    }

    private void ResetFields()
    {
        _previewRoadSpawnPosition = null;
        _previewRoadSpawnNormal = null;
        _previewTrafficLightControllerSpawnPosition = null;
        _previewTrafficLightControllerSpawnNormal = null;
    }
}
