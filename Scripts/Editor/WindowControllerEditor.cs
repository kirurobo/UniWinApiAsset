using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Kirurobo
{
    /// <summary>
    /// WindowControllerのためのUnityエディタカスタマイズ
    /// </summary>
    [CustomEditor(typeof(WindowController))]
    public class WindowControllerEditor : Editor
    {
        private EditorWindow gameViewWindow;

        private bool isWarningDismissed = false;

        void OnEnable()
        {
            LoadSettings();
        }

        void OnDisable()
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            isWarningDismissed = EditorUserSettings.GetConfigValue("WindowController_IS_WARNING DISMISSED") == "1";
        }

        private void SaveSettings()
        {
            EditorUserSettings.SetConfigValue("WindowController_IS_WARNING DISMISSED", isWarningDismissed ? "1" : "0");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // 推奨設定のチェック
            if (!isWarningDismissed)
            {
                // 自動調整ボタンを表示させるならtrueとなる
                bool showButton = false;

                if (!PlayerSettings.runInBackground)
                {
                    EditorGUILayout.HelpBox("'Run in background' is recommended.", MessageType.Warning);
                    showButton = true;
                }

                if (!PlayerSettings.resizableWindow)
                {
                    EditorGUILayout.HelpBox("'Resizable window' is recommended.", MessageType.Warning);
                    showButton = true;
                }

#if UNITY_2018_1_OR_NEWER
        if (PlayerSettings.fullScreenMode != FullScreenMode.Windowed)
        {
            EditorGUILayout.HelpBox("It is recommmended to select 'Windowed' in fullscreen mode.", MessageType.Warning);
            showButton = true;
        }

        if (showButton)
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Apply all recommended settings"))
            {
                PlayerSettings.runInBackground = true;
                PlayerSettings.resizableWindow = true;
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            }
        }
#else
                if (PlayerSettings.defaultIsFullScreen)
                {
                    EditorGUILayout.HelpBox("'Default is full screen' is not recommended.", MessageType.Warning);
                    showButton = true;
                }

                //  チェックに引っかかればボタンを表示
                if (showButton)
                {
                    GUI.backgroundColor = Color.green;
                    if (
                        GUILayout.Button(
                            "✔ Apply all recommended settings",
                            GUILayout.MinHeight(30f)
                        ))
                    {
                        PlayerSettings.runInBackground = true;
                        PlayerSettings.resizableWindow = true;
                        PlayerSettings.defaultIsFullScreen = false;
                    }

                    GUI.backgroundColor = Color.red;
                    if (
                        GUILayout.Button(
                            "✘ Dismiss this validation",
                            GUILayout.MinHeight(30f)
                        ))
                    {
                        isWarningDismissed = true;
                        //SaveSettings();
                    }

                    EditorGUILayout.Space();
                }
#endif
            }
        }

        // 参考 http://baba-s.hatenablog.com/entry/2017/09/17/135018
        /// <summary>
        /// ゲームビューのEditorWindowを取得
        /// </summary>
        /// <returns></returns>
        public static EditorWindow GetGameView()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            var gameView = EditorWindow.GetWindow(type);
            return gameView;
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class WindowControllerReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

    /// <summary>
    /// Set to readonly during playing
    /// Reference: http://ponkotsu-hiyorin.hateblo.jp/entry/2015/10/20/003042
    /// Reference: https://forum.unity.com/threads/c-class-property-with-reflection-in-propertydrawer-not-saving-to-prefab.473942/
    /// </summary>
    [CustomPropertyDrawer(typeof(BoolPropertyAttribute))]
    public class WindowControllerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if ((property.type == "bool") && (property.name[0] == '_'))
                {
                    Object obj = property.serializedObject.targetObject;
                    string propertyName = property.name.Substring(1);
                    PropertyInfo info = obj.GetType().GetProperty(propertyName);
                    MethodInfo getMethod = default(MethodInfo);
                    MethodInfo setMethod = default(MethodInfo);
                    if (info.CanRead) { getMethod = info.GetGetMethod(); }
                    if (info.CanWrite) { setMethod = info.GetSetMethod(); }

                    bool oldValue = property.boolValue;
                    if (getMethod != null)
                    {
                        oldValue = (bool)getMethod.Invoke(obj, null);
                    }
                    GUI.enabled = (setMethod != null);
                    EditorGUI.PropertyField(position, property, label, true);
                    GUI.enabled = true;
                    bool newValue = property.boolValue;
                    if ((setMethod != null) && (oldValue != newValue))
                    {
                        setMethod.Invoke(obj, new[] { (object)newValue });
                    }
                }
                else
                {
                    // Readonly
                    GUI.enabled = false;
                    EditorGUI.PropertyField(position, property, label, true);
                    GUI.enabled = true;
                }
            }
            else
            {
                // Default value
                EditorGUI.PropertyField(position, property, label, true);
            }

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}