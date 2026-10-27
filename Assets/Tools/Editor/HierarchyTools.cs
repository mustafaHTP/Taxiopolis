using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A collection of editor utilities for managing GameObjects in the hierarchy.
/// Add more hierarchy-related tools here in the future (rename, organize, etc.).
/// </summary>
public static class HierarchyTools
{
    // --- SORT CHILDREN ALPHABETICALLY ---

    [MenuItem("GameObject/Hierarchy Tools/Sort Children Alphabetically", false, 0)]
    [MenuItem("CONTEXT/Transform/Sort Children Alphabetically")]
    private static void SortChildrenAlphabetically(MenuCommand command)
    {
        Transform target = command.context as Transform ?? Selection.activeTransform;

        if (target == null)
        {
            Debug.LogWarning("No GameObject selected to sort.");
            return;
        }

        SortChildren(target);
    }

    /// <summary>
    /// Sorts all direct children of the given parent Transform alphabetically.
    /// </summary>
    private static void SortChildren(Transform parent)
    {
        if (parent.childCount <= 1)
        {
            Debug.Log($"'{parent.name}' has less than 2 children — no sorting needed.");
            return;
        }

        var children = parent.Cast<Transform>()
                             .OrderBy(t => t.name, System.StringComparer.OrdinalIgnoreCase)
                             .ToList();

        Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "Sort Children Alphabetically");

        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }

        Debug.Log($"Sorted {children.Count} children of '{parent.name}' alphabetically.");
    }

    // --- FUTURE EXPANSION EXAMPLES ---
    // public static void RenameChildrenSequentially(Transform parent, string baseName = "Child") { ... }
    // public static void RemoveMissingScripts(Transform parent) { ... }
    // public static void GroupSelectedObjects(string groupName = "New Group") { ... }
}

