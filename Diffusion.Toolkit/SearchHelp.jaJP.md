# 検索ヘルプ

クエリまたはフィルタを使って画像を検索できます。

[クエリ](#クエリ)では検索語を入力し、素早く検索することを想定しています。プロンプト以外の項目でも検索できます。詳しくは[パラメータ検索](#パラメータ検索)を参照してください。

[フィルタ](#フィルタ)では各パラメータを個別に分離し、ComfyUI プロパティの検索をより細かく制御できます。クエリ構文を覚えたくない場合（ただし構文はとてもシンプルです）、あるいはインターフェースベースで検索したい場合に向いています。

いずれの方法でも、**検索 > クエリ／フィルタを保存**オプションでクエリを保存できます。

* [クエリ](#クエリ)
    * [クエリ構文](#クエリ構文)
    * [シンプルなプロンプト検索](#シンプルなプロンプト検索)
    * [パラメータ検索](#パラメータ検索)
    * [対応パラメータ](#対応パラメータ)
        * [ネガティブプロンプト](#ネガティブプロンプト)
        * [ステップ数](#ステップ数)
        * [サンプラー](#サンプラー)
        * [Classifier-Free Guidance Scale (CFG/Scale)](#classifier-free-guidance-scale-cfgscale)
        * [シード](#シード)
        * [サイズ](#サイズ)
        * [モデルハッシュ](#モデルハッシュ)
        * [モデル名](#モデル名)
        * [美しさスコア](#美しさスコア)
        * [Hyper Networks](#hyper-networks)
        * [お気に入り](#お気に入り)
        * [評価](#評価)
        * [NSFW](#nsfw)
        * [メタデータなし](#メタデータなし)
        * [削除対象](#削除対象)
        * [作成日](#作成日)
        * [パス](#パス)
        * [フォルダ](#フォルダ)
    * [検索に関する注意](#検索に関する注意)
    * [複数値での検索](#複数値での検索)
    * [ワークフロープロパティと生メタデータの検索](#ワークフロープロパティと生メタデータの検索)
* [フィルタ](#フィルタ)
    * [メタデータタブ](#メタデータタブ)
    * [ワークフロータブ](#ワークフロータブ)


# クエリ構文

サムネイル領域の上にあるテキストボックスが**クエリ**入力欄です。ここに探したい内容を入力します。**クエリ構文**を使うことで、プロンプト以外の項目でも検索できます。例えばファイルの[パス](#パス)や、[作成日](#作成日)の範囲でも検索可能です。複数の条件を組み合わせて、より絞り込んだ検索もできます。

クエリ構文が分かりにくい、あるいは記述が長すぎると感じる場合は、代わりに[フィルタ](#フィルタ)を使うこともできます。

## シンプルなプロンプト検索

基本的な検索方法は、プロンプトに含まれるテキストを入力することです。多くの場合、期待通りに動作します。ただし、プロンプト中でのカンマの扱いとは異なります。クエリ構文では、カンマは**検索語**の区切りとして使われます。

例えば次のクエリを見てください:

```
A man staring into a starry night sky, by Van Gogh
```

これには2つの検索語が含まれます:

* `A man staring into a starry night sky`
* `by Van Gogh`

上記のクエリは、`A man staring into a starry night sky` と `by Van Gogh` の両方を、任意の順序・位置で含むプロンプトに一致します。

次のプロンプトすべてに一致します:

```
A man staring into a starry night sky, by Van Gogh
A man staring into a starry night sky, pencil sketch, by Van Gogh
A man staring into a starry night sky, oil painting, by Van Gogh
by Van Gogh, a man staring into a starry night sky
```

カンマを含む完全に一致する語を検索したい場合は、その語をダブルクォートで囲みます:

```
"A man staring into a starry night sky, by Van Gogh"
```

上記のクエリは、`A man staring into a starry night sky, by Van Gogh` という語をその表現（スペース含む）で正確に含むプロンプトのみに一致します。

上記のクエリに一致するプロンプトの例:

```
A man staring into a starry night sky, by Van Gogh
A man staring into a starry night sky, by Van Gogh, pencil sketch
oil painting, A man staring into a starry night sky, by Van Gogh
```

スペースは重要であることに注意してください。次のクエリは

```
"A man staring into a starry night sky , by Van Gogh"
```

先ほどの例とは同じ結果になりません。

## パラメータ検索

他のパラメータで絞り込むには、特別な*クエリトークン*を使う必要があります。通常はパラメータを示す単語にコロンを続けたものです。例えば `seed: 12345` は、シード値 `12345` で画像を絞り込むパラメータクエリを追加します。

プロンプトクエリ（ある場合）は、クエリトークンの引数として解釈されないよう、常に最初に記述する必要があります。例:

```
A man staring into a starry night sky, by Van Gogh steps: 20 cfg:12
```

パラメータクエリは AND 結合されます。つまり、パラメータを追加するほど、より絞り込まれた結果が少なくなります。

上記のクエリは、次の条件すべてに一致する画像を表示します:

* プロンプトが `A man staring into a starry night sky` を含む
* かつ `by Van Gogh` を含む
* かつ `steps` が `20`
* かつ `cfg` が `12`

## 対応パラメータ

### ネガティブプロンプト

* `negative prompt: <term> [,<term>]`
* `negative_prompt: <term> [,<term>]`
* `negative: <term> [,<term>]`

### ステップ数

* `steps: <number>`
* `steps: <start>-<end>`

### サンプラー

* `sampler: <name>`

   サンプラー名は AI ジェネレータによって異なります。スペースを含む名前を使うものもあれば、小文字＋アンダースコアを使うものもあります。画像に保存されている名前を確認してください。スペースを含むサンプラー名の場合は、名前をクォートで囲んでください。

   以下は A1111 や他のツールで使われる既知のサンプラーのリストです。

   * "Euler a" または `euler_a`
   * Euler または `euler`
   * LMS または `lms`
   * Heun または `heun`
   * DPM2 または `dpm2`
   * "DPM2 a" または `dpm2_a`
   * "DPM++ 2S" a または `dpm++_2s_a`
   * "DPM++ 2M" または `dpm++_2m`
   * "DPM++ SDE" または `dpm++_sde`
   * "DPM fast" または `dpm_fast`
   * "DPM adaptive" または `dpm_adaptive`
   * "LMS Karras" または `lms_karras`
   * "DPM2 Karras" または `dpm2_karras`
   * "DPM2 a Karras" または `dpm2_a_karras`
   * "DPM++ 2S a Karras" または `dpm++_2s_a_karras`
   * "DPM++ 2M Karras" または `dpm++_2s_karras`
   * "DPM++ SDE Karras" または `dpm++_sde_karras`
   * DDIM または `ddim`
   * PLMS または `plms`

### Classifier-Free Guidance Scale (CFG/Scale)

* `cfg: <number>`
* `cfg_scale: <number>`
* `cfg scale: <number>`

### シード

`seed` は数値、範囲、またはワイルドカードで検索できます。

* `seed: <number>`
* `seed: <start>-<end>`
* `seed: 123*`
   * `123` で始まるシードを持つすべての画像を表示します
* `seed: 123456???000`
   * `123456` で始まり、任意の3桁を挟み、`000` で終わるシードを持つすべての画像を表示します

### サイズ

* `size: <width>x<height>`

  `width` と `height` は数値、または任意の値に一致させるクエスチョンマーク（`?`）を指定できます。例: `size:512x?` は幅 `512`・任意の高さの画像に一致します。

* `size: <width>:<height>` （比率、例: 16:9）

* `size: <orientation>`

   `orientation` には次のいずれかを指定します:

   * `landscape`
   * `portrait`
   * `square`

### モデルハッシュ

* `model_hash: <hash>`

### モデル名

* `model: <model name>`

モデル名ではワイルドカード（`?`, `*`）がサポートされています。

*注意: 以下の情報は古い可能性があります:*

一部の画像ジェネレータはメタデータにモデル名を保存せず、代わりにモデルハッシュを保存します。

Diffusion Toolkit は、AUTOMATIC1111 の `cache.json` ファイルが存在すれば、そこに保存されている情報を使ってハッシュ検索を試みます。名前（部分一致可）で検索し、一致するモデルのハッシュを取得して、そのハスクエリで画像を検索します。旧ハッシュアルゴリズムと新しい SHA256 ハッシュの両方がサポートされています。

`cache.json` ファイルは AUTOMATIC1111 によって随時更新されます。初めて読み込む新規モデルのモデルハッシュを計算します。モデルハッシュは UI でそのモデルに切り替えた際に計算されます。

アプリケーションが最新の json ファイルを保持していることを確認するには、**編集 > モデルを再読み込み**をクリックしてください。

### 美しさスコア

美しさスコア（Aesthetic score）は、AUTOMATIC1111 Web UI 向けの [Aesthetic Image Scorer Extension](https://github.com/tsngo/stable-diffusion-webui-aesthetic-image-scorer) によって追加されるタグです。

Chad Scorer をベースにした CLIP+MLP Aesthetic Score Predictor を用いて生成画像の美しさスコアを計算し、メタデータに保存します。

* `aesthetic_score: [<|>|<=|>=|<>] <number>`

厳密な数値（例: `aesthetic_score: 0.6`）でも検索できますが、多くの場合は `aesthetic_score: < 0.6` のような比較検索を行うでしょう。

### Hyper Networks

Hypernetwork を使用した画像を検索し、使用された強さ（AUTOMATIC1111）も指定できます。

* `hypernet: <name>`

* `hypernet strength: [<|>|<=|>=|<>] <number>`

### お気に入り

お気に入り（Favorite）は true または false の値を持つ Diffusion Toolkit のメタデータです。お気に入りとしてタグ付けした画像（true）、またはお気に入りにタグ付けしていない画像（false）を検索できます。

* `favorite: [true|false]`

### 評価

評価（Rating）は 1〜10 の値を持つ Diffusion Toolkit のメタデータです。

* `rating: [<|>|<=|>=|<>] <number>`

省略可能な比較演算子を使えます。

### NSFW

NSFW は true または false の値を持つ Diffusion Toolkit のメタデータです。NSFW としてタグ付けした画像（true）、または NSFW にタグ付けしていない画像（false）を検索できます。

この条件を明示的に指定すると、**結果から NSFW を非表示**オプションより優先されます。

* `nsfw: [true|false]`

### メタデータなし

このフィルタはメタデータを持たない画像を表示します。

* `nometa: [true|false]`
* `nometadata: [true|false]`

### 削除対象

削除（Delete）は true または false の値を持つ Diffusion Toolkit のメタデータです。削除対象としてタグ付けした画像（true）、または削除対象にタグ付けしていない画像（false）を検索できます。

* `delete: [true|false]` - 削除対象としてマークされたファイルで絞り込みます

### 作成日

**作成日**（Date Created）は、スキャン時に画像ファイルの属性から取得される Diffusion Toolkit のメタデータです。

ファイルの作成日で検索できます。

* `date: <criteria>`

   * `date: today` - 当日のファイルを含める
   * `date: yesterday` - 前日のファイルを含める
   * `date: between 11-11-2022 and yesterday` - 2022年11月11日から前日までのファイルを含める
   * `date: from 10-10-2022 to 11-11-2022` - 別の書式
   * `date: before 11-11-2022` - 最初から2022年11月11日までのファイルを含める
   * `date: since 01-01-2022` - 2022年1月1日から今日までに作成されたファイルを含める

   注意:

   * `YYYY-MM-DD` 書式がサポートされています
   * `XX-XX-XXXX` の日付は、お使いのコンピュータの日付書式で解析されます。米国等では `MM-DD-YYYY`、欧州の地域では `DD-MM-YYYY` となります。

### パス

**パス**（Path）は、スキャン時のファイルの完全なパスを使う Diffusion Toolkit のメタデータです。

ワイルドカード（`?`, `*`）、または `starts with`、`contains`、`ends with` などの条件を使えます。

パスは完全なパス（ファイル名を含む）に一致するため、通常はワイルドカードを使うことになります。

ワイルドカードを使ったパス検索はサブフォルダ内の一致も返します。特定のフォルダだけを検索したい場合は[フォルダ](#フォルダ)を使ってください。

* `path: [criteria] <search-term>`

   * ワイルドカードを使う場合:
      * `path: D:\diffusion\images*`
      * `path: *img2img*`
      * `path: *.jpg`

   * 条件を使う場合:
      * `path: starts with D:\diffusion\images`
      * `path: contains img2img`
      * `path: ends with .jpg`

   パスにスペースが含まれる場合は、パスをダブルクォートで囲んでください。

   * glob を使う場合:
      * `path: "D:\My pics\images**"`
      * `path: "**funny cats**"`

   * 条件を使う場合:
      * `path: starts with "D:\My pics\images"`
      * `path: contains "funny cats"`

### フォルダ

**フォルダ**（Folder）は、スキャン時に画像ファイルの属性から取得される Diffusion Toolkit のメタデータです。フォルダで検索すると、結果が特定のフォルダに制限されます（パス検索はサブフォルダ内の画像も含む点が異なります）。

* `folder: <folder>`

## 検索に関する注意

* `steps:`、`sampler:` などのパラメータは大文字小文字を区別しません。プロンプトからコピーできるよう、`Steps:`、`Sampler:` などと記述できます。
* コロン（`:`）の*後ろ*には0個以上のスペースを入れられますが、前には入れられません
    * 例: `steps:20`、`steps: 20`、`steps:   20` は OK
    * ただし `steps  :20`、`steps :20` は不可

## 複数値での検索

ほとんどのパラメータで複数の値を指定して検索できます。結果は OR 結合され、値を追加するほどより多くの結果が得られます。

* seed は範囲指定ができます: `seed: <start>-<end>`
  * 例: `seed: 10000-20000`
* 他のパラメータではパイプ（`|`）を使って複数の値を指定できます
  * 例: `sampler: euler a | ddim | plms`
  * 例: `cfg: 4.5|7|9|12`
  * 例: `model_hash: aabbccdd | deadbeef | 12345678`

## ワークフロープロパティと生メタデータの検索

Diffusion Toolkit で ComfyUI ワークフローまたは生のメタデータを検索対象にできます。

まず設定でワークフローと生メタデータのスキャンを有効にし、画像を再スキャンする必要があります。

次に、クエリバーの設定アイコンをクリックして、検索対象のプロパティを設定します。

プロパティ名を探すには、メタデータペインのワークフロータブを開き、各プロパティ右側の ... ボタンから「プロパティ名をコピー」を選びます。

# フィルタ

フィルタボタンを押すと、**メタデータ**タブと**ワークフロー**タブを持つフィルタダイアログが表示されます。

## メタデータタブ

ここでは絞り込みたいパラメータを選択できます。プロンプトはクエリと[同じ方法](#プロンプトのクエリ)で解析されます。

検索対象のパラメータの横にあるチェックボックスをオンにしてください。オフの場合、そのパラメータは無視されます。

タブの下部付近には **True** と **False** のオプションを持つパラメータが並んでいます。これらは画像が*タグ付け*されている（True）か*タグ付けされていない*（False）かで検索するために使います。

## ワークフロータブ

ワークフロータブでは ComfyUI メタデータを持つ画像を絞り込みます。ここでは検索対象のプロパティと、値の検索方法を選択できます。テキストプロパティでは通常 *contains* を使いますが、*starts with* など他の方法も役立つことがあります。

*and*、*or*、*not* 演算子でフィルタを組み合わせられます。演算子の順序は重要です。あるフィルタの結果が次のフィルタで変更されるため、フィルタの順序を計画的に組み立ててください。
