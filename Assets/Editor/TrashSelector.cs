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
        if (GUI.changed)
        {
            targetScript.type = newType;
            targetScript.gameObject.GetComponent<SpriteRenderer>().sprite = targetScript.sprites[(int)targetScript.type].sprites[trashNameIndex].sprite;
            targetScript.trashName = nameChoices[trashNameIndex];
            EditorUtility.SetDirty(targetScript);
        }
        DrawDefaultInspector();
    }
}