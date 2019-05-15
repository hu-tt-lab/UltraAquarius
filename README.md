# UlrtraAquarius 

NIDAQから音作成・US波形・磁気刺激波形を作るアプリケーション

## 説明

* NIDAQから音信号・US波形・磁気刺激波形を出力するためのGUIアプリケーション
* 超音波出力用にファンクションジェネレータも制御可能
* 主に神経活動計測に使用する目的

### 音刺激

* PureTone
* Tonepip
* ToneBurst
* Click

### 超音波

ファンクションジェネレータ経由での出力

* Sine
* Square

### 磁気刺激波形

* FrontEdgeSawPulse：波形の前がEdgeのSawWave
* LastEdgeSawPulse：波形の後がEdgeのSawWave
* SquarePulse：矩形波
* TrianglePulse：三角波

#### パラメータ

* Duration：波形の長さ
* Interval：波形と波形の間の時間
* Waves：波形を何個出力するか


## 必須環境

* NIDAQmxドライバー16.0以上
* .NET Framework 4.5以上
* NIDAQ（Analog outputが3ポート以上あること）
* ファンクションジェネレータ（WF1947）

## セットアップ

* Releaseから最新版をダウンロードしてください
