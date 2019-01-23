# UniWinApi

**UniWinApi** is a Windows API utility with a window controller class for Unity. This allows some window controls that Unity originally does not provide as follows.

* Move the window
* Resize the window
* Maximize or Minimize
* **Transparent (non-rectangular) window** 
* **Accept file dropping**
* **File open dialog (Experimental)**
* Move the mouse pointer
* Send mouse button up/down

[![UniWinApi VRM viewer](http://i.ytimg.com/vi/cq2g-hIGlAs/mqdefault.jpg)](https://youtu.be/cq2g-hIGlAs "UniWinApi VRM viewer v0.4.0 beta")


## Confirmed environment

* Unity 5.6.6f2, Unity 2018.2.20f1
* Windows 10 Pro x64
* GeForce GTX980, GeForce GTX 1070

|Scripting backend|x86_64|x86|
|:----------------|:----:|:-:|
|Mono             |  x   | x |
|IL2CPP           |  x   |   |

## Usage

1. Download .unitypackage file from the [Releases](https://github.com/kirurobo/UniWinApi/releases) page.
2. Impoart the package to your project.
3. Attach "WindowController.cs" to a suitable object
    * Creating an empty object named "WindowController" is recommended.
4. Build the project as PC Standalone application.

## License

[![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png "CC0")](http://creativecommons.org/publicdomain/zero/1.0/deed.ja)

## Contact & Source

* [Twitter: @kirurobo](http://twitter.com/kirurobo)
* [GitHub](http://github.com/kirurobo)


---

# UniWinApi (日本語説明)

**UniWinApi** は Unityでは本来行えない操作を Windows API 経由で行うものです。  
自ウィンドウに対しての処理を行うためのコントローラーが付属します。  
以下のようなことができます。  

* ウィンドウの移動
* ウィンドウサイズ変更
* ウィンドウの最大化、最小化
* **ウィンドウの透過** （枠なしで、四角形でないウィンドウにします） 
* **ファイルのドロップを受け付ける**
* **Windowsのダイアログでファイルを開く（試験実装で単一ファイルのみ）**
* マウスポインタを移動させる
* マウスのボタン操作を送出する

主にデスクトップマスコット的な用途で利用しそうな機能を取り込んでいます。
[![UniWinApi VRM viewer](http://i.ytimg.com/vi/cq2g-hIGlAs/mqdefault.jpg)](https://youtu.be/cq2g-hIGlAs "UniWinApi VRM viewer v0.4.0 beta")


## 確認済み動作環境

* Unity 5.6.6f2, Unity 2018.2.20f1
* Windows 10 Pro x64
* GeForce GTX980, GeForce GTX 1070

|Scripting backend|x86_64|x86|
|:----------------|:----:|:-:|
|Mono             |  ○   | ○ |
|IL2CPP           |  ○   |   |

## 利用方法

1. [Releases](https://github.com/kirurobo/UniWinApi/releases) ページから .unitypackage ファイルをダウンロードします。
2. それをプロジェクトにインポートします。
3. "WindowController.cs" スクリプトを適当なオブジェクトにアタッチしてください。
    * "WindowController" といった名前の空オブジェクト作成をお勧めします。
4. Platform として PC Standalone を選び、ビルドしてください。

UniWinApi が本体ですが、自ウィンドウを操作するものとして WindowController を用意しています。


インスペクタで下記を変更するか、スクリプトから変更するとウィンドウの状態を変えられます。
- IsTransparent … ウィンドウ枠を透明化
- IsTopmost … 常に最前面
- IsMaximized … ウィンドウ最大化
- IsMinimized … ウィンドウ最小化
- EnableFileDrop … ファイルドロップを受け付ける
- EnableDragMove … マウス左ボタンでウィンドウを移動できる

![image](https://user-images.githubusercontent.com/1019117/51594588-1dcb4080-1f38-11e9-9a93-910f59632fc2.png)

ファイルドロップを利用する場合、`OnFilesDropped(string[] files)` というイベントがありますので、そちらに処理を追加してください。
files にはドロップされたファイルのパスが入ります。

ドキュメントは整えられていないため、利用例はサンプルをご覧ください (^-^;


## 注意点
- ウィンドウハンドルを確実に取る方法が分ってないので、別のウィンドウが操作対象になったりするかも知れません。
  - （むしろ別のウィンドウもタイトルやクラス名で指定して操作できます。）
- 閉じるボタンがなくなったり、ウィンドウを見失った場合、タスクマネージャから終了する必要が出るかも知れません。

## FAQ
- エディタ上で透明にすると表示がおかしいのですが。
	- すみません。仕様です。透明化はビルドしたものでご確認ください。
	- Game ウィンドウはどうも背景が塗りつぶされてしまっているようで、透明化されません。
	- ゲームビューが操作対象となりますが、他のビューとドッキングしていた場合はそのウィンドウごと対象となります。


## ライセンス

[![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png "CC0")](http://creativecommons.org/publicdomain/zero/1.0/deed.ja)

UniWinApi は、CC0（パブリックドメイン）としています。  
著作権表示なしで修正、複製、再配布も可能です。好きに使っていただけますが無保証です。

※他の方のブログなど参考にしている部分が大いにありますが、一通り作成した結果、同様の機能を実装した場合は似た内容になるとして、著作権で保護される範囲においては基本的に独自のものとしてCC0にできると判断しています。

## 連絡先・配布元

* [Twitter: @kirurobo](http://twitter.com/kirurobo)
* [GitHub](http://github.com/kirurobo)

## 更新履歴

* 2019/01/23 ファイルオープンダイアログ追加。自ウィンドウ取得方法をアクティブウィンドウ基準に戻した。
* 2018/12/28 namespaceを設定、自ウィンドウ取得をPIDを基準とする方法に修正
* 2018/12/07 UniWinApiのアセット部分を分離
* 2018/09/09 おおよそ想定通りに動作しそうなため、パッケージを公開
* 2018/08/24 UniWinApiとして整理し直した
* 2015/03/03 最小化、最大化時はその直前の状態を保存するようにした
* 2014/04/26 公開用初版