using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NewWindow : EditorWindow {

    static NewWindow myWindow;

    static readonly string colorPrefName = "Playmode tint";
    static string originalPlaymodeTint;

    [MenuItem("Tools/Preview")]
    static void Preview()
    {
        //var gameView = GetGameView();

        if (!myWindow)
        {
            originalPlaymodeTint = EditorPrefs.GetString(colorPrefName);
            Debug.Log(originalPlaymodeTint);
            EditorPrefs.SetString(colorPrefName, "Background;0;0;0;0");

            myWindow = CreateInstance<NewWindow>();
        }

        myWindow.ShowUtility();
        //myWindow.Show();
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
    private GUIStyle style;

    private Kirurobo.WindowController controller;

    private void OnGUI()
    {
        //if (texture == null || texture.width != Screen.width || texture.height != Screen.height)
        //{
        //    CreateTexture();
        //}

        if (texture == null)
        {
            texture = Texture2D.blackTexture;
        }
        GUI.skin.window.margin = new RectOffset(50, 50, 50, 100);
        GUI.skin.window.normal.textColor = Color.red;

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

                //GUIStyle gstyle = new GUIStyle();
                //gstyle.normal.background = tex;
                //GUI.skin.window.normal.background = tex;

                if (position.width != tex.width || position.height != tex.height)
                {
                    position = new Rect(position.x, position.y, tex.width, tex.height);
                }

                //tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                //tex.Apply();

                //EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), tex, controller.material);
                GUI.DrawTexture(new Rect(0, 0, position.width, position.height), tex, ScaleMode.ScaleToFit, true);
            }
            //Object.Destroy(tex);
        }

        GUI.Label(new Rect(0, 0, 100, 40), "Preview");
    }

    private void Update()
    {
        this.Repaint();
    }

    private void OnDestroy()
    {
        EditorPrefs.SetString(colorPrefName, originalPlaymodeTint);
    }

    private void CreateTexture()
    {
        texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
    }
}
