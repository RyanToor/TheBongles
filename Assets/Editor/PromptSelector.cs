using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InputManager.AbilityPrompt))]
public class PromptSelector : PropertyDrawer
{
    float height = 0;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        height = base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
        if (property.FindPropertyRelative("promptType").intValue == 0)
        {
            return height * 4;
        }
        else
        {
            return height * 3;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Vector2 location = new(position.x, position.y);
        Vector2 dimensions = new(position.width, position.height);
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.LabelField(new Rect(location.x, location.y, dimensions.x, EditorGUIUtility.singleLineHeight), label);
        SerializedProperty promptType = property.FindPropertyRelative("promptType");
        promptType.intValue = EditorGUI.Popup(new Rect(location.x, position.y + height, dimensions.x, dimensions.y), "Prompt Type", promptType.intValue, promptType.enumNames);
        if (promptType.intValue == 0)
        {
            SerializedProperty text = property.FindPropertyRelative("text");
            text.stringValue = EditorGUI.TextField(new Rect(location.x, position.y + height * 2, dimensions.x, EditorGUIUtility.singleLineHeight), text.stringValue);
            SerializedProperty colour = property.FindPropertyRelative("colour");
            colour.colorValue = EditorGUI.ColorField(new Rect(location.x, position.y + height * 3, dimensions.x, EditorGUIUtility.singleLineHeight), colour.colorValue);
        }
        else
        {
            SerializedProperty sprite = property.FindPropertyRelative("sprite");
            sprite.objectReferenceValue = EditorGUI.ObjectField(new Rect(location.x, position.y + height * 2, dimensions.x, EditorGUIUtility.singleLineHeight), sprite.objectReferenceValue, typeof(Sprite), false);
        }
        /*InputManager targetScript = (InputManager)target;
        DrawDefaultInspector();
        for (int i = 0; i < targetScript.abilityPrompts.Length; i++)
        {
            for (int j = 0; j < targetScript.abilityPrompts[i].abilityPrompts.Length; j++)
            {
                InputManager.AbilityPrompt.PromptType type = (InputManager.AbilityPrompt.PromptType)EditorGUILayout.EnumPopup("Prompt Type", targetScript.abilityPrompts[i].abilityPrompts[j].promptType);
                if (type == InputManager.AbilityPrompt.PromptType.text)
                {
                    string text = EditorGUILayout.TextField("Text", targetScript.abilityPrompts[i].abilityPrompts[j].text);
                    Color colour = EditorGUILayout.ColorField("Colour", targetScript.abilityPrompts[i].abilityPrompts[j].colour);
                    if (GUI.changed)
                    {
                        UpdateEditor(targetScript, i, j, type, text, colour);
                    }
                }
                else
                {
                    Sprite sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", targetScript.abilityPrompts[i].abilityPrompts[j].sprite, typeof(Sprite), false);
                    if (GUI.changed)
                    {
                        UpdateEditor(targetScript, i, j, type, sprite);
                    }
                }
            }
        }*/
    }

    private void UpdateEditor(InputManager targetScript, int i, int j, InputManager.AbilityPrompt.PromptType newType, string text = null, Color colour = new())
    {
        UpdateEditor(targetScript, i, j, newType, text, colour, null);
    }

    private void UpdateEditor(InputManager targetScript, int i, int j, InputManager.AbilityPrompt.PromptType newType, Sprite sprite)
    {
        UpdateEditor(targetScript, i, j, newType, null, new(), sprite);
    }

    private void UpdateEditor(InputManager targetScript, int i, int j, InputManager.AbilityPrompt.PromptType newType, string text, Color colour, Sprite sprite)
    {
        InputManager.AbilityPrompt prompt = targetScript.abilityPrompts[i].abilityPrompts[j];
        prompt.promptType = newType;
        if (newType == InputManager.AbilityPrompt.PromptType.text)
        {
            prompt.text = text;
            prompt.colour = colour;
        }
        else
        {
            prompt.sprite = sprite;
        }
        EditorUtility.SetDirty(targetScript);
    }
}