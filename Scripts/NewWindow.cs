using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NewWindow : EditorWindow {

    static NewWindow myWindow;

    static string originalBackground;

    [MenuItem("Tools/Preview")]
    static void Preview()
    {
        //var gameView = GetGameView();

        if (!myWindow)
        {
            originalBackground = EditorPrefs.GetString("Background");
            Debug.Log(originalBackground);
            EditorPrefs.SetString("Background", "Background;0;0;0;0");

            myWindow = CreateInstance<NewWindow>();
        }
        myWindow.ShowUtility();
        //myWindow.Show();
 
        //SceneView.onSceneGUIDelegate += (sceneView) =>
        //{
        //    var cameras = Object.FindObjectOfType<Camera>();
        //    Handles.BeginGUI();
        //    GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        //};
    }

    // 参考 http://baba-s.hatenablog.com/entry/2017/09/17/135018
    public static EditorWindow GetGameView()
    {
        var assembly = typeof(EditorWindow).Assembly;
        var type = assembly.GetType("UnityEditor.GameView");
        var gameView = EditorWindow.GetWindow(type);
        return gameView;
    }

    private Texture2D texture;


    private Kirurobo.WindowController controller;

    private void OnGUI()
    {
        //if (texture == null || texture.width != Screen.width || texture.height != Screen.height)
        //{
        //    CreateTexture();
        //}

        if (Application.isPlaying)
        {
            if (controller == null)
            {
                controller = FindObjectOfType<Kirurobo.WindowController>();
            }
            //var tex = ScreenCapture.CaptureScreenshotAsTexture();
            var tex = controller.screenTexture;
            if (tex != null)
            {
                if (position.width != tex.width || position.height != tex.height)
                {
                    position = new Rect(position.x, position.y, tex.width, tex.height);
                }

                //tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                //tex.Apply();

                //EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), tex);
                //GUI.DrawTexture(new Rect(0, 0, position.width, position.height), tex, ScaleMode.StretchToFill, true);
                EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), tex, controller.material);
                //EditorGUI.DrawPreviewTexture(new Rect(10, 10, position.width / 2, position.height / 2), tex);
            }
            //Object.Destroy(tex);
        }
    }

    private void Update()
    {
        this.Repaint();
    }

    private void OnDestroy()
    {
        EditorPrefs.SetString("Background", originalBackground);
    }

    private void CreateTexture()
    {
        texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
    }
}
