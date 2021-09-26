/**
 * UniWinApi sample
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: CC0 https://creativecommons.org/publicdomain/zero/1.0/
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace Kirurobo
{

    /// <summary>
    /// Set editable the bool property
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class BoolPropertyAttribute : PropertyAttribute { }

    /// <summary>
    /// Set the attribute as readonly
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute { }


    /// <summary>
    /// ウィンドウ操作をとりまとめるクラス
    /// </summary>
    public class WindowController : MonoBehaviour
    {
        /// <summary>
        /// Hit testing methods
        /// </summary>
        public enum HitTestType
        {
            /// <summary>
            /// No hit testing.
            /// </summary>
            None = 0,
            
            /// <summary>
            /// See a pixel under the cursor. Comfortable but slow.
            /// </summary>
            Opacity = 1,
            
            /// <summary>
            /// Only objects that target the raycast will be hit
            /// </summary>
            Raycast = 2,
        }
    
        /// <summary>
        /// Window controller
        /// </summary>
        public UniWinApi uniWin;

        /// <summary>
        /// 操作を透過する状態か
        /// </summary>
        public bool isClickThrough
        {
            get { return _isClickThrough; }
        }
        private bool _isClickThrough = true;

        /// <summary>
        /// Is this window transparent
        /// </summary>
        public bool isTransparent
        {
            get { return _isTransparent; }
            set { SetTransparent(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set transparent on startup")]
        private bool _isTransparent = false;

        /// <summary>
        /// Is this window minimized
        /// </summary>
        public bool isTopmost
        {
            get { return ((uniWin == null || !uniWin.IsActive) ? _isTopmost : _isTopmost = uniWin.IsTopmost); }
            set { SetTopmost(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set topmost on startup")]
        private bool _isTopmost = false;

        /// <summary>
        /// Is this window maximized
        /// </summary>
        public bool isMaximized
        {
            get { return ((uniWin == null || !uniWin.IsActive) ? _isMaximized : _isMaximized = uniWin.IsMaximized); }
            set { SetMaximized(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set maximized on startup")]
        private bool _isMaximized = false;

        /// <summary>
        /// Is this window minimized
        /// </summary>
        public bool isMinimized
        {
            get { return ((uniWin == null || !uniWin.IsActive) ? _isMinimized : _isMinimized = uniWin.IsMinimized); }
            set { SetMinimized(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set minimized on startup")]
        private bool _isMinimized = false;

        private bool isInitiallyTopmost;
        private bool isInitiallyMaximized;
        private bool isInitiallyMinimized;
        
        /// <summary>
        /// Enable file drop
        /// </summary>
        public bool enableFileDrop
        {
            get { return _enableFileDrop; }
            set
            {
                if (value) { BeginFileDrop(); }
                else { EndFileDrop(); }
            }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set enable file-drop on startup")]
        private bool _enableFileDrop = false;

        /// <summary>
        /// The window will move by mouse dragging, if true.
        /// </summary>
        [Tooltip("Make the window draggable while a left mouse button is pressed")]
        public bool enableDragMove = true;

        /// <summary>
        /// Method of transparency, alpha or color-key
        /// </summary>
        [FormerlySerializedAs("transparentMethod")] public UniWinApi.TransparentTypes transparentType = UniWinApi.TransparentTypes.Alpha;

        /// <summary>
        /// Hit testing method
        /// </summary>
        public HitTestType hitTestType = HitTestType.Opacity;
        
        // カメラの背景をアルファゼロの黒に置き換えるため、本来の背景を保存しておく変数
        private CameraClearFlags originalCameraClearFlags;
        private Color originalCameraBackground;

        [Header("Advanced setting")]

        /// <summary>
        /// Modify the User Interface Privilege Isolation (UIPI) message filter when starting to enable file drop.
        /// This option allows file dragging from lower privilege windows if your app runs with administrator privilege.
        /// </summary>
        [Tooltip("This option must be set before enable file drop")]
        public bool allowDropFromLowerPrivilege = false;

        [Header("Status")]

        /// <summary>
        /// Is the mouse pointer on an opaque pixel
        /// </summary>
        [SerializeField, ReadOnly, Tooltip("Is the mouse pointer on an opaque pixel or an object? (Read only)")]
        private bool onObject = true;

        /// <summary>
        /// The cut off threshold of alpha value.
        /// </summary>
        private float opaqueThreshold = 0.1f;

        /// <summary>
        /// The key color when the transparent type is color-key
        /// </summary>
        public Color32 keyColor
        {
            get { return (uniWin != null) ? uniWin.ChromakeyColor : new Color32(1, 0, 1, 0); }
            set { if (uniWin != null) { uniWin.ChromakeyColor = value; } }
        }

        /// <summary>
        /// Pixel color under the mouse pointer. (Read only)
        /// </summary>
        [ReadOnly, Tooltip("Pixel color under the mouse pointer. (Read only)")]
        public Color pickedColor;

        /// <summary>
        /// Is the window is dragged?
        /// </summary>
        private bool isDragging = false;

        /// <summary>
        /// ドラッグ開始時のウィンドウ内座標[px]
        /// </summary>
        private Vector2 dragStartedPosition;

        /// <summary>
        /// 描画の上にタッチがあればそのfingerIdが入る
        /// </summary>
        //[SerializeField]
        private int activeFingerId = -1;

        /// <summary>
        /// 最後のドラッグはマウスによるものか、タッチによるものか
        /// </summary>
        //[SerializeField]
        private bool wasUsingMouse;

        /// <summary>
        /// 現在対象としているウィンドウが自分自身らしいと確認できたらtrueとする
        /// </summary>
        private bool isWindowChecked = false;

        /// <summary>
        /// カメラのインスタンス
        /// </summary>
        private Camera currentCamera;

        /// <summary>
        /// タッチがBeganとなったものを受け渡すためのリスト
        /// PickColorCoroutine()実行のタイミングではどうもtouch.phaseがうまくとれないようなのでこれで渡してみる
        /// </summary>
        private Touch? firstTouch = null;

        /// <summary>
        /// ファイルドロップ時のイベントハンドラー。 UniWinApiの OnFilesDropped にそのまま渡す。
        /// </summary>
        public event UniWinApi.FilesDropped OnFilesDropped
        {
            add { if (uniWin != null) { uniWin.OnFilesDropped += value; } }
            remove { if (uniWin != null) { uniWin.OnFilesDropped -= value; } }
        }

        /// <summary>
        /// ウィンドウ状態が変化したときに発生するイベント
        /// </summary>
        public event OnStateChangedDelegate OnStateChanged;
        public delegate void OnStateChangedDelegate();

        /// <summary>
        /// 表示されたテクスチャ
        /// </summary>
        private Texture2D colorPickerTexture = null;


        // Use this for initialization
        void Awake()
        {
            // ウィンドウが準備できたタイミングで初期値を設定できるよう保存しておく
            isInitiallyTopmost = _isTopmost;
            isInitiallyMaximized = _isMaximized;
            isInitiallyMinimized = _isMinimized;
            
            Input.simulateMouseWithTouches = false;

            if (!currentCamera)
            {
                // メインカメラを探す
                currentCamera = Camera.main;

                // もしメインカメラが見つからなければ、Findで探す
                if (!currentCamera)
                {
                    currentCamera = FindObjectOfType<Camera>();
                }
            }

            // カメラの元の背景を記憶
            if (currentCamera)
            {
                originalCameraClearFlags = currentCamera.clearFlags;
                originalCameraBackground = currentCamera.backgroundColor;

            }

            // マウス下描画色抽出用テクスチャを準備
            colorPickerTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

#if (UNITY_WIN || UNITY_STANDALONE_WIN)
            // ウィンドウ制御用のインスタンス作成
            uniWin = new UniWinApi();

            // 透過方式の指定
            uniWin.TransparentType = transparentType;
            
            // 自分のウィンドウを取得
            FindMyWindow();
#endif
#if UNITY_EDITOR
            // エディタのウィンドウ配置が変化した際の呼び出し
            EditorApplicationUtility.windowsReordered += () => {
                this.isWindowChecked = false;   // ウィンドウが不確かであるとする
                //Debug.Log("Editor windows reordered");
            };
#endif
        }

        void Start()
        {
            // マウスカーソル直下の色を取得するコルーチンを開始
            StartCoroutine(PickColorCoroutine());
            
            // エディタの場合、ウィンドウの準備ができるまで時間がかかるようなのでコルーチンで最大化などの初期状態を設定してみる
            StartCoroutine((ApplyInitialStyle()));
        }

        void OnDestroy()
        {
            if (uniWin != null)
            {
                uniWin.Dispose();
            }
        }

        // Update is called once per frame
        void Update()
        {
            // 自ウィンドウ取得状態が不確かなら探しなおす
            //  マウス押下が取れるのはすなわちフォーカスがあるとき
            if (Input.anyKey)
            {
                UpdateWindow();
            }

            // マウスドラッグでウィンドウ移動
            DragMove();

            // キー、マウス操作の下ウィンドウへの透過状態を更新
            UpdateClickThrough();


            // ウィンドウ枠が復活している場合があるので監視するため、呼ぶ
            if (uniWin != null)
            {
                uniWin.Update();

                // 最小化は外部要因で解除されがちなのでチェック
                bool stateChanged = false;
                if (_isMinimized != uniWin.IsMinimized)
                {
                    _isMinimized = uniWin.IsMinimized;
                    stateChanged = true;
                }

                // 最大化もチェック
                if (_isMaximized != uniWin.IsMaximized)
                {
                    _isMaximized = uniWin.IsMaximized;
                    stateChanged = true;
                }

                if (stateChanged)
                {
                    StateChangedEvent();
                }
            }
        }

        /// <summary>
        /// ウィンドウ状態が変わったときに呼ぶイベントを処理
        /// </summary>
        private void StateChangedEvent()
        {
            if (OnStateChanged != null)
            {
                OnStateChanged();
            }
        }

        /// <summary>
        /// 最大化時以外なら、マウスドラッグによってウィンドウを移動
        /// </summary>
        void DragMove()
        {
            if (uniWin == null) return;

            // ドラッグでの移動が無効化されていた場合
            if (!enableDragMove)
            {
                isDragging = false;
                return;
            }

            //Debug.Log(Input.touchCount);

            // 最大化状態ならウィンドウ移動は行わないようにする
            bool isFullScreen = uniWin.IsMaximized;

            // フルスクリーンならウィンドウ移動は行わない
#if !UNITY_EDITOR
            //  エディタだと true になってしまうようなので、エディタ以外でのみ確認
            if (Screen.fullScreen) isFullScreen = true;
#endif
            if (isFullScreen)
            {
                isDragging = false;
                return;
            }

            // マウスによるドラッグ開始の判定
            if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
            {
                // EventSystemで反応した場合、UI上としてドラッグ対象外
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    dragStartedPosition = Input.mousePosition;
                    isDragging = true;
                    wasUsingMouse = true;
                    //Debug.Log("Start mouse dragging");
                }
            }

            bool touching = (activeFingerId >= 0);

            int targetTouchIndex = -1;
            if (activeFingerId < 0)
            {
                // まだ追跡中の指が無かった場合、Beganとなるタッチがあればそれを追跡候補に挙げる
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        //Debug.Log("Touch began");
                        //targetTouchIndex = i;
                        firstTouch = Input.GetTouch(i);     // まだドラッグ開始とはせず、透過画素判定に回す。
                        break;
                    }
                }
            }
            else
            {
                // 追跡中の指がある場合
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (activeFingerId == Input.GetTouch(i).fingerId)
                    {
                        targetTouchIndex = i;
                        break;
                    }
                }
            }

            // タッチによるドラッグ開始の判定
            if (targetTouchIndex >= 0 && !isDragging)
            {
                dragStartedPosition = Input.GetTouch(targetTouchIndex).position;
                //activeFingerId = Input.GetTouch(targetTouchIndex).fingerId;
                isDragging = true;
                wasUsingMouse = false;
                //Debug.Log("Start touch dragging");
            }

            // ドラッグ終了の判定
            if (wasUsingMouse && !Input.GetMouseButton(0))
            {
                //Debug.Log("End mouse dragging");
                activeFingerId = -1;
                isDragging = false;
            }
            else if (!wasUsingMouse && targetTouchIndex < 0)
            {
                //if (touching) Debug.Log("End touch dragging");
                activeFingerId = -1;
                isDragging = false;
            }

            // ドラッグ中ならば、ウィンドウ位置を更新
            if (isDragging)
            {
                Vector2 mousePos;
                if (wasUsingMouse)
                {
                    mousePos = Input.mousePosition;
                    Vector2 delta = mousePos - dragStartedPosition;
                    delta.y = -delta.y;     // Y座標は反転

                    Vector2 windowPosition = uniWin.GetPosition();  // 現在のウィンドウ位置を取得
                    windowPosition += delta; // ウィンドウ位置に上下左右移動分を加える
                    uniWin.SetPosition(windowPosition);   // ウィンドウ位置を設定
                }
                else
                {
                    Touch touch = Input.GetTouch(targetTouchIndex);
                    Vector2 delta = touch.position - dragStartedPosition;
                    delta.y = -delta.y;     // Y座標は反転

                    Vector2 windowPosition = uniWin.GetPosition();  // 現在のウィンドウ位置を取得
                    windowPosition += delta; // ウィンドウ位置に上下左右移動分を加える
                    uniWin.SetPosition(windowPosition);   // ウィンドウ位置を設定
                }
            }
        }

        /// <summary>
        /// 画素の色を基に操作受付を切り替える
        /// </summary>
        void UpdateClickThrough()
        {
            // マウスカーソル非表示状態ならば透明画素上と同扱い
            bool opaque = (onObject && !UniWinApi.GetCursorVisible());

            if (_isClickThrough)
            {
                if (opaque)
                {
                    if (uniWin != null) uniWin.EnableClickThrough(false);
                    _isClickThrough = false;
                }
            }
            else
            {
                if (isTransparent && !opaque && !isDragging)
                {
                    if (uniWin != null) uniWin.EnableClickThrough(true);
                    _isClickThrough = true;
                }
            }
        }

        /// <summary>
        /// OnPostRenderではGUI描画前になってしまうため、コルーチンを用意
        /// </summary>
        /// <returns></returns>
        private IEnumerator PickColorCoroutine()
        {
            while (Application.isPlaying)
            {
                yield return new WaitForEndOfFrame();
                
                if (hitTestType == HitTestType.Opacity)
                {
                    HitTestByOpaquePixel();
                }
                else if (hitTestType == HitTestType.Raycast)
                {
                    HitTestByRaycast();
                }
                else
                {
                    // ヒットテスト無しの場合は常にtrue
                    onObject = true;
                }
            }
            yield return null;
        }

        /// <summary>
        /// マウス下の画素があるかどうかを確認
        /// </summary>
        /// <param name="cam"></param>
        private void HitTestByOpaquePixel()
        {
            Vector2 mousePos;
　          mousePos = Input.mousePosition;

            //// コルーチン & WaitForEndOfFrame ではなく、OnPostRenderで呼ぶならば、MSAAによって上下反転しないといけない？
            //if (QualitySettings.antiAliasing > 1) mousePos.y = camRect.height - mousePos.y;

            // タッチ開始点が指定されれば、それを調べる
            if (firstTouch != null)
            {
                Touch touch = (Touch)firstTouch;
                Vector2 pos = touch.position;

                firstTouch = null;

                if (GetOnOpaquePixel(pos))
                {
                    onObject = true;
                    activeFingerId = touch.fingerId;
                    return;
                }
            }

            // マウス座標を調べる
            if (GetOnOpaquePixel(mousePos))
            {
                //Debug.Log("Mouse " + mousePos);
                onObject = true;
                //activeFingerId = -1;    // タッチ追跡は解除
                return;
            }
            else
            {
                onObject = false;
            }
        }

        /// <summary>
        /// 指定座標の画素が透明か否かを返す
        /// </summary>
        /// <param name="mousePos">座標[px]。必ず描画範囲内であること。</param>
        /// <returns></returns>
        private bool GetOnOpaquePixel(Vector2 mousePos)
        {
            float w = Screen.width;
            float h = Screen.height;
            //Debug.Log(w + ", " + h);

            // 画面外であれば透明と同様
            if (
                mousePos.x < 0 || mousePos.x >= w
                || mousePos.y < 0 || mousePos.y >= h
                )
            {
                return false;
            }

            // 透過状態でなければ、範囲内なら不透過扱いとする
            if (!_isTransparent) return true;

            // LayeredWindowならばクリックスルーはOSに任せるため、ウィンドウ内ならtrueを返しておく
            if (transparentType == UniWinApi.TransparentTypes.ColorKey) return true;

            // 指定座標の描画結果を見て判断
            try   // WaitForEndOfFrame のタイミングで実行すればtryは無くても大丈夫？
            {
                // Reference http://tsubakit1.hateblo.jp/entry/20131203/1386000440
                colorPickerTexture.ReadPixels(new Rect(mousePos, Vector2.one), 0, 0);
                //Color color = colorPickerTexture.GetPixel(0, 0);
                Color color = colorPickerTexture.GetPixels32()[0];  // こちらの方が僅かに速い？
                pickedColor = color;

                return (color.a >= opaqueThreshold);  // αがしきい値以上ならば不透過とする
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// マウス下にオブジェクトがあるかどうかを確認
        /// </summary>
        private void HitTestByRaycast()
        {
            // uGUIの上と判定されれば、終了
            if (EventSystem.current.IsPointerOverGameObject())
            {
                onObject = true;
                return;
            }

            // Raycastで判定
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                onObject = true;
            }
            else
            {
                onObject = false;
            }
        }

        /// <summary>
        /// 自分のウィンドウハンドルを見つける
        /// </summary>
        private void FindMyWindow()
        {
            // ウィンドウが確かではないとしておく
            isWindowChecked = false;

            // 現在このウィンドウがアクティブでなければ、取得はやめておく
            if (!Application.isFocused) return;

            // 今アクティブなウィンドウを取得
            var window = UniWinApi.FindWindow();
            if (window == null) return;

            // 見つかったウィンドウを利用開始
            uniWin.SetWindow(window);

            // // 初期設定を反映
            // SetTopmost(_isTopmost);
            // SetMaximized(_isMaximized);
            // SetMinimized(_isMinimized);
            SetTopmost(isInitiallyTopmost);
            SetMaximized(isInitiallyMaximized);
            SetMinimized(isInitiallyMinimized);
            
            SetTransparent(_isTransparent);
            if (_enableFileDrop) BeginFileDrop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator ApplyInitialStyle()
        {
            // ウインドウの準備ができるまで待つ
            while (uniWin == null)
            {
                yield return null;
            }

            // さらに1フレーム待つ
            yield return null;

            // 初期値を設定
            SetTopmost(isInitiallyTopmost);
            SetMaximized(isInitiallyMaximized);
            SetMinimized(isInitiallyMinimized);
        }

        /// <summary>
        /// 自分のウィンドウハンドルが不確かならば探しなおす
        /// </summary>
        private void UpdateWindow()
        {
            if (uniWin == null) return;

            // もしウィンドウハンドル取得に失敗していたら再取得
            if (!uniWin.IsActive)
            {
                //Debug.Log("Window is not active");
                FindMyWindow();
            }
            else if (!isWindowChecked)
            {
                // 自分自身のウィンドウか未確認の場合

                // 今アクティブなウィンドウが自分自身かをチェック
                if (uniWin.CheckActiveWindow())
                {
                    isWindowChecked = true; // どうやら正しくウィンドウをつかめているよう
                }
                else
                {
                    // ウィンドウが違っているようなので、もう一度アクティブウィンドウを取得
                    uniWin.Reset();
                    uniWin.Dispose();
                    uniWin = new UniWinApi();
                    FindMyWindow();
                }
            }
        }

        /// <summary>
        /// ウィンドウへのフォーカスが変化したときに呼ばれる
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
            //Debug.Log("Focus:" + focus);

            if (focus)
            {
                UpdateWindow();
            }

        }

        /// <summary>
        /// ウィンドウ透過状態になった際、自動的に背景を透明単色に変更する
        /// </summary>
        /// <param name="isTransparent"></param>
        void SetCameraBackground(bool isTransparent)
        {
            // カメラが特定できていなければ何もしない
            if (!currentCamera) return;

            if (isTransparent)
            {
                currentCamera.clearFlags = CameraClearFlags.SolidColor;
                if (uniWin.TransparentType == UniWinApi.TransparentTypes.ColorKey)
                {
                    currentCamera.backgroundColor = uniWin.ChromakeyColor;
                }
                else
                {
                    currentCamera.backgroundColor = Color.clear;
                }
            }
            else
            {
                currentCamera.clearFlags = originalCameraClearFlags;
                currentCamera.backgroundColor = originalCameraBackground;
            }
        }

        /// <summary>
        /// 透明化状態を切替
        /// </summary>
        /// <param name="enabled"></param>
        public void SetTransparent(bool enabled)
        {
            //if (_isTransparent == transparent) return;

            _isTransparent = enabled;
            SetCameraBackground(enabled);

            if (uniWin != null)
            {
                uniWin.EnableTransparent(enabled);
            }
            UpdateClickThrough();
            StateChangedEvent();
        }

        /// <summary>
        /// 透明化方式を設定
        /// </summary>
        /// <param name="type"></param>
        public void SetTransparentType(UniWinApi.TransparentTypes type)
        {
            //Debug.Log(Screen.width + " : " + Screen.height);
            //uniWin.SetPosition(Vector2.zero);

            // 透過モード変更
            uniWin.TransparentType = type;
            transparentType = uniWin.TransparentType;
            if (isTransparent)
            {
                // 透明化状態だったならば再度透明化を設定し直す
                SetTransparent(false);
                SetTransparent(true);
            }
        }

        /// <summary>
        /// 最大化を切替
        /// </summary>
        public void SetMaximized(bool maximized)
        {
            //if (_isMaximized == maximized) return;
            if (uniWin == null)
            {
                _isMaximized = maximized;
            }
            else
            {

                if (maximized)
                {
                    uniWin.Maximize();
                }
                else if (uniWin.IsMaximized)
                {
                    uniWin.Restore();
                }
                _isMaximized = uniWin.IsMaximized;
            }
            StateChangedEvent();
        }

        /// <summary>
        /// 最小化を切替
        /// </summary>
        public void SetMinimized(bool minimized)
        {
            //if (_isMinimized == minimized) return;
            if (uniWin == null)
            {
                _isMinimized = minimized;
            }
            else
            {
                if (minimized)
                {
                    uniWin.Minimize();
                }
                else if (uniWin.IsMinimized)
                {
                    uniWin.Restore();
                }
                _isMinimized = uniWin.IsMinimized;
            }
            StateChangedEvent();
        }

        /// <summary>
        /// 最前面を切替
        /// </summary>
        /// <param name="topmost"></param>
        public void SetTopmost(bool topmost)
        {
            //if (_isTopmost == topmost) return;
            if (uniWin == null) return;

            uniWin.EnableTopmost(topmost);
            _isTopmost = uniWin.IsTopmost;
            StateChangedEvent();
        }

        /// <summary>
        /// Begin to accept file drop.
        /// </summary>
        public void BeginFileDrop()
        {
            if (uniWin != null)
            {
                uniWin.BeginFileDrop();

                if (allowDropFromLowerPrivilege)
                {
                    AllowFileDropFromLowerPrivilege();
                }
            }
            _enableFileDrop = true;
        }

        /// <summary>
        /// End to accept file drop.
        /// </summary>
        public void EndFileDrop()
        {
            if (uniWin != null)
            {
                uniWin.EndFileDrop();
            }
            _enableFileDrop = false;
        }

        /// <summary>
        /// Modifies the privilege filter to allow file drop from lower privilege window
        /// </summary>
        public void AllowFileDropFromLowerPrivilege()
        {
            if (uniWin != null)
            {
                uniWin.AllowFileDraggingFromProgramsWithLowerPrivileges();
            }
        }

        /// <summary>
        /// Disallow the privilege filter
        /// </summary>
        public void DisallowFileDropFromLowerPrivilege()
        {
            if (uniWin != null)
            {
                uniWin.DisallowFileDraggingFromProgramsWithLowerPrivileges();
            }
        }

        /// <summary>
        /// Show open file dialog.
        /// </summary>
        /// <returns></returns>
        public string ShowOpenFileDialog(string filter = "All files|*.*")
        {
            if (uniWin != null)
            {
                return uniWin.ShowOpenFileDialog(filter);
            }
            return null;
        }

        /// <summary>
        /// 終了時にはウィンドウプロシージャを戻す処理が必要
        /// </summary>
        void OnApplicationQuit()
        {
            if (Application.isPlaying)
            {
                if (uniWin != null)
                {
                    uniWin.Dispose();
                }
            }
        }

        /// <summary>
        /// 自分のウィンドウにフォーカスを与える
        /// </summary>
        public void Focus()
        {
            if (uniWin != null)
            {
                uniWin.SetFocus();
            }
        }
    }


#if UNITY_EDITOR
    // エディタ実行時専用
    // ゲームビューが閉じたり開いたりされたときの対応

    // 参考 http://baba-s.hatenablog.com/entry/2017/12/04/090000#Unity-%E3%82%A8%E3%83%87%E3%82%A3%E3%82%BF%E3%81%AE%E5%90%84%E3%82%A6%E3%82%A3%E3%83%B3%E3%83%89%E3%82%A6%E3%81%AE%E4%BD%8D%E7%BD%AE%E3%81%8C%E5%A4%89%E6%9B%B4%E3%81%95%E3%82%8C%E3%81%9F%E6%99%82
    [InitializeOnLoad]
    public static class EditorApplicationUtility
    {
        private const BindingFlags BINDING_ATTR = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly FieldInfo m_info = typeof(EditorApplication).GetField("windowsReordered", BINDING_ATTR);

        /// <summary>
        /// エディタウィンドウ配置変化時に呼ばれる
        /// </summary>
        public static EditorApplication.CallbackFunction windowsReordered
        {
            get
            {
                return m_info.GetValue(null) as EditorApplication.CallbackFunction;
            }
            set
            {
                var functions = m_info.GetValue(null) as EditorApplication.CallbackFunction;
                functions += value;
                m_info.SetValue(null, functions);
            }
        }
    }
#endif

}
