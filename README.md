# UlrtraAquarius 

NIDAQから音作成・US波形・磁気刺激波形を作るアプリケーション

## 説明

* NIDAQから音信号・US波形・磁気刺激波形を出力するためのGUIアプリケーション
* 超音波出力用にファンクションジェネレータも制御可能
* 主に神経活動計測に使用する目的

## 刺激種類

### 音刺激

* PureTone(窓なし)
* Tonepip(50%長の線形窓付き)
* ToneBurst(10%長の線形窓付き)
* AM Tone
* Click

### 超音波

ファンクションジェネレータ経由での出力

* Sine
* Square
* Sine with window(Sine, Liner)

### 磁気刺激波形

ファンクションジェネレータ経由での出力

* SquarePulse：矩形波を用いたPulse
* Pulse




## 必須環境

* NIDAQmxドライバー16.0以上
* .NET Framework 4.5以上
* NIDAQ（Analog outputが4ポート以上あること）
* ファンクションジェネレータ（WF1947）

## セットアップ

* Releaseから最新版をダウンロードしてください
