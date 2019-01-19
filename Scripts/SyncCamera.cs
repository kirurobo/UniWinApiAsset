using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

[InitializeOnLoad]
public class SyncCamera
{

    static int selected = 0;
    static Rect windowRect = new Rect(10, 20, 100, 24);

    static SyncCamera()
    {
        SceneView.onSceneGUIDelegate += (sceneView) => {

            if (SceneView.focusedWindow != sceneView)
                return;

            var cameras = Object.FindObjectsOfType<Camera>();

            Handles.BeginGUI();

            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

            int windowID = EditorGUIUtility.GetControlID(FocusType.Passive, windowRect);

            windowRect = GUILayout.Window(windowID, windowRect, (id) => {

                string[] displayNames = new string[] { "None", "" };
                ArrayUtility.AddRange(ref displayNames, cameras.Select<Camera, string>(c => c.name).ToArray());
                selected = EditorGUILayout.Popup(selected, displayNames);

                GUI.DragWindow();

            }, "Sync Camera");

            Handles.EndGUI();

            int index = selected - 2;


            if (index >= 0)
            {
                var camera = cameras[index];
                camera.transform.position = sceneView.camera.transform.position;
                camera.transform.rotation = sceneView.camera.transform.rotation;
                //sceneView.camera.transform.position = camera.transform.position;
                //sceneView.camera.transform.rotation = camera.transform.rotation;
            }

        };
    }
}