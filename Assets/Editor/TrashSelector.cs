using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollectableTrash)), CanEditMultipleObjects]
public class TrashSelector : Editor
{
    public override void OnInspectorGUI()
    {
        CollectableTrash targetScript = (CollectableTrash)target;
        TrashType newType = (TrashType)EditorGUILayout.EnumPopup("Type", targetScript.type);
        string[] nameChoices = new string[targetScript.sprites[(int)newType].sprites.Length];
        for (int i = 0; i < targetScript.sprites[(int)newType].sprites.Length; i++)
        {
            nameChoices[i] = targetScript.sprites[(int)newType].sprites[i].name;
        }
        int trashNameIndex = EditorGUILayout.Popup("Trash Name", Array.IndexOf(nameChoices, targetScript.trashName), nameChoices);
        if (trashNameIndex < 0)
        {
            trashNameIndex = 0;
            UpdateEditor(targetScript, newType, nameChoices, trashNameIndex);
        }
        if (GUI.changed)
        {
            UpdateEditor(targetScript, newType, nameChoices, trashNameIndex);
        }
        DrawDefaultInspector();
    }

    private void UpdateEditor(CollectableTrash targetScript, TrashType newType, string[] nameChoices, int trashNameIndex)
    {
        targetScript.type = newType;
        targetScript.gameObject.GetComponent<SpriteRenderer>().sprite = targetScript.sprites[(int)targetScript.type].sprites[trashNameIndex].sprite;
        targetScript.trashName = nameChoices[trashNameIndex];
        EditorUtility.SetDirty(targetScript);
    }
}