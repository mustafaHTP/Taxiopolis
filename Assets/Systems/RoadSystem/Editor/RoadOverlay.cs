using RoadSystem.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Road Edit Tools")]
[Icon(RoadSystemDefaults.RoadOverlayIconPath)]
public class RoadOverlay : Overlay
{
    private VisualElement _container;

    public override VisualElement CreatePanelContent()
    {
        _container = new VisualElement();

        UpdateContent();

        return _container;
    }

    public override void OnWillBeDestroyed()
    {
        base.OnWillBeDestroyed();

        RoadEditManager.OnModeChanged -= RoadEditMode_OnModeChanged;
        Selection.selectionChanged -= Selection_SelectionChanged;
    }

    public override void OnCreated()
    {
        base.OnCreated();
        RoadEditManager.OnModeChanged += RoadEditMode_OnModeChanged;
        Selection.selectionChanged += Selection_SelectionChanged;
    }

    private void CheckVisibility()
    {
        displayed =
            (RoadEditManager.RoadEditMode != RoadEditMode.None);

        if (displayed)
        {
            UpdateContent();
        }
    }

    private void Selection_SelectionChanged()
    {
        CheckVisibility();
    }

    private void RoadEditMode_OnModeChanged()
    {
        CheckVisibility();
    }

    private void UpdateContent()
    {
        if (_container == null) return;

        _container.Clear();

        var visualElements =
            GetRoadEditModeContent(RoadEditManager.RoadEditMode);

        foreach (var visualElement in visualElements)
        {
            _container.Add(visualElement);
        }
    }

    private IEnumerable<VisualElement> GetRoadEditModeContent(RoadEditMode roadEditMode)
    {
        IEnumerable<VisualElement> visualElements = Enumerable.Empty<VisualElement>();

        switch (roadEditMode)
        {
            case RoadEditMode.RoadEdit:
                visualElements = GetRoadEditModeEditContent();
                break;
            case RoadEditMode.RoadSpawnPoint:
                visualElements = GetRoadSpawnPointModeContent();
                break;
            case RoadEditMode.RoadLink:
                visualElements = GetRoadLinkModeContent();
                break;
            case RoadEditMode.TrafficLightLink:
                visualElements = GetTrafficLightLinkModeContent();
                break;
            case RoadEditMode.SpawnRoad:
                visualElements = GetSpawnRoadModeContent();
                break;
        }

        return visualElements;
    }

    private IEnumerable<VisualElement> GetSpawnRoadModeContent()
    {
        List<VisualElement> visualElements = new();

        var label =
            new Label("Spawn: Ctrl (HOLD) + Click");

        label.style.paddingTop = 4;
        label.style.paddingBottom = 4;
        label.style.paddingLeft = 8;
        label.style.paddingRight = 8;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;

        visualElements.Add(label);

        return visualElements;
    }

    private IEnumerable<VisualElement> GetTrafficLightLinkModeContent()
    {
        List<VisualElement> visualElements = new();

        var label =
            new Label("To link traffic lights click buttons." +
            "\nTo remove connection press X button.");

        label.style.paddingTop = 4;
        label.style.paddingBottom = 4;
        label.style.paddingLeft = 8;
        label.style.paddingRight = 8;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;

        visualElements.Add(label);

        return visualElements;
    }

    private IEnumerable<VisualElement> GetRoadSpawnPointModeContent()
    {
        List<VisualElement> visualElements = new();

        var label =
            new Label("Determine spawn points by clicking buttons");

        label.style.paddingTop = 4;
        label.style.paddingBottom = 4;
        label.style.paddingLeft = 8;
        label.style.paddingRight = 8;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;

        visualElements.Add(label);

        return visualElements;
    }

    private IEnumerable<VisualElement> GetRoadEditModeEditContent()
    {
        List<VisualElement> visualElements = new();

        var label =
           new Label("Add last: Ctrl (HOLD) + Click" +
           "\nInsert: Ctrl (HOLD) + Shift (HOLD) + Click" +
           "\nRemove last: Backspace" +
           "\nRemove: Shift (HOLD) + Click");

        label.style.paddingTop = 4;
        label.style.paddingBottom = 4;
        label.style.paddingLeft = 8;
        label.style.paddingRight = 8;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;

        visualElements.Add(label);

        return visualElements;
    }

    private IEnumerable<VisualElement> GetRoadLinkModeContent()
    {
        List<VisualElement> visualElements = new();

        var label =
            new Label("To link road points click buttons." +
            "\nTo remove connection press X button.");

        label.style.paddingTop = 4;
        label.style.paddingBottom = 4;
        label.style.paddingLeft = 8;
        label.style.paddingRight = 8;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;

        visualElements.Add(label);

        return visualElements;
    }
}