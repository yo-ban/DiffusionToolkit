# Diffusion Toolkit

Diffusion Toolkit は、AI生成画像向けのメタデータインデクサーおよびビューアーです。増え続けるコレクションの整理、検索、並べ替えを支援することを目的としています。

# 使い方

使い方は比較的シンプルですが、知っておくと便利なヒントやコツ、ショートカットが多数あります。詳細は[Getting Started](https://github.com/RupertAvery/DiffusionToolkit/tree/master/Diffusion.Toolkit/Tips.md)のドキュメントを参照してください。

Bill Meeks氏によるデモンストレーション動画をご用意しました（旧バージョンのものです）。

[![Organize your AI Images](https://img.youtube.com/vi/r7J3n1LjojE/hqdefault.jpg)](https://www.youtube.com/watch?v=r7J3n1LjojE&ab_channel=BillMeeks)

# インストール

* 現在 Windows のみ対応
* 最新リリースを[ダウンロード](https://github.com/RupertAvery/DiffusionToolkit/releases/latest
)
    * 最新リリースの **> Assets** を探して展開し、zipファイル **Diffusion.Toolkit.v1.x.zip** を取得してください。
* すべてのファイルをフォルダーに解凍してください
* 必要に応じて、[.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)をインストールしてください（未インストールの場合）

# ソースからビルド

## 前提条件

* Visual Studio 2026 が必要
* [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)（デスクトップランタイムを含む）

## ビルド手順

* このリポジトリをクローン
* `publish.cmd` を実行

`build` という名前のフォルダーが作成され、必要なファイルがすべて格納されます。

# 機能

* 画像および動画をスキャンし、プロンプトやその他のメタデータ（PNGInfo）を保存・インデックス化
* 画像とメタデータを簡単に表示
* メタデータ経由で画像や動画を検索
* 画像にタグ付け
    * お気に入り
    * 評価（1〜10）
    * NSFW
* 画像の並べ替え
    * 作成日順
    * 美的スコア（Aesthetic Score）順
    * 評価順
* キーワードによるNSFWの自動タグ付け
* NSFWタグ付き画像をぼかし表示
    * NSFW
* アルバム
    * 画像を選択し、右クリック > アルバムに追加
    * アルバムへ画像をドラッグ＆ドロップ
* カスタムタグ
* フォルダービュー
* プロンプトの表示と検索
    * プロンプトの一覧と使用状況
    * ネガティブプロンプトの一覧と使用状況
    * プロンプトに関連する画像の一覧
* ドラッグ＆ドロップ
    * 別フォルダーへドラッグ＆ドロップで移動（CTRLを押しながらドラッグでコピー）

# 対応フォーマット

* JPG/JPEG + EXIF
* PNG
* WebP
* .TXT メタデータ
* MP4

# 対応メタデータフォーマット

* AUTOMATIC1111 および A1111 互換メタデータ
  * Tensor.Art
  * SDNext
* InvokeAI（Dream/sd-metadata/invokeai_metadata）
* NovelAI
* Stable Diffusion
* EasyDiffusion
* RuinedFooocus
* Fooocus
* FooocusMRE
* Stable Swarm

メタデータを持たない画像でも、評価やアルバムなど他の機能は使用可能です！

# 寄付

<a href="https://www.buymeacoffee.com/rupertavery" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-green.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>

または

<a href="https://www.paypal.me/rupertavery" target="_blank"><img src="https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif" alt="Donate"></a>

# スクリーンショット

![Screenshot 2024-02-09 183808](https://github.com/RupertAvery/DiffusionToolkit/assets/1910659/437781da-e905-412a-bbe6-e179f51ac020)

![Screenshot 2024-02-09 183625](https://github.com/RupertAvery/DiffusionToolkit/assets/1910659/20e57f5a-be4e-468f-9bfb-fe309ecfe5f1)


# FAQ

## 画像のメタデータ（PNGInfo）を表示するには？

プレビューペインを表示した状態で、サムネイルビューまたはプレビューペインにフォーカスを合わせて `I` キーを押すと、メタデータの表示/非表示を切り替えられます。プレビューペイン右下の目アイコンをクリックすることも可能です。

## 「メタデータを再構築（Rebuild Metadata）」とは？いつ使うべき？

「メタデータの再構築」は、すべての画像を再スキャンし、新規または更新されたメタデータをデータベースに反映します。カスタムタグ（評価、お気に入り、NSFW）には影響しません。

Diffusion Toolkit の新バージョンがリリースされ、既存画像に含まれるメタデータに新たに対応した場合にのみ再構築が必要です。

## 画像を別のフォルダーに移動できますか？

Diffusion フォルダー内で別フォルダーへ移動する場合は、**右クリック > 移動（Move）** コマンドを使用してください。Diffusion Toolkit が移動を処理し、移動後もすべてのメタデータ（お気に入り、評価、NSFW）を保持します。

エクスプローラーや他のアプリケーションで移動した場合でも、Diffusion フォルダー配下であれば、フォルダーの再スキャンや画像の再構築時に削除を検出したのち新規ファイルとして再検出されます。ただし、お気に入りや評価など Toolkit 固有の情報は失われます。
