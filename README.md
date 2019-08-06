# UltraAquarius

NIDAQとFunction Generator(WF1947)から音・超音波・磁気刺激用の波形を作成するアプリケーション

## 概要

* NIDAQとWF1947を制御して音・超音波・磁気刺激用を出力するためのGUIアプリ
* 刺激オーダーと刺激パラメータの保存・読み込みが可能
* 刺激終了時に刺激パラメータ・オーダーをjsonとして出力する

### 刺激種類

* 音刺激(DAQ)
* 超音波刺激(DAQ, Function Generator)
* 磁気刺激(DAQ, Function Generator)

### 必須環境

* NIDAQmxドライバー16.0以上
* .NET Framework 4.5以上
* NIDAQ（Analog outputが4ポート以上あること）
* ファンクションジェネレータ（WF1947）

## MenuBar

* ファイル
  * 刺激パラメータを保存する: 刺激テーブルに入力したデータを出力する(デフォルト名: Mixer.json)
  * 刺激パラメータを読み込みする: 出力された刺激テーブルデータを読み込みする(デフォルト名: Mixer.json)
  * 刺激オーダーを保存する: OutputListに入力したデータを出力する(デフォルト名: OutputList.json)
  * 刺激オーダー読み込みする: 出力されたOutputListデータを読み込みする(デフォルト名: OutputList.json)
* ヘルプ
  * 現状ハリボテ

## 刺激説明とパラメータ

### 共通パラメータ

#### OutputList

* Valid: CheckMarkがついている場合には出力する
* Type: 右刺激テーブルのどの種類の刺激かを選択する(`Pure Tone`, `Click Tone`, `AM Tone`, `Ultrasound`, `USMod`, `Magnetic`)
* Signal Name: 種類内でユニークな名前で指定する
* Signal Number: 出力するDigitalOutputを指定する(1:P0, 2:P1, ... , 6:P5)

#### StimulationTable

* Signal Name: 種類内でユニークな名前で指定する

#### Trial(右上タブ)

* Interval-Duration[ms]: OutputListで指定した刺激と刺激の感覚
* Interval-Waggle[ms]: 上記のDurationに 0~Waggle[ms] の値をランダム加える
* Trial Count: OutputListに指定した刺激を何回繰り返すかを指定する
* Random:`Not`, `Partial Random`, `Full Random`を指定する
  * Not:OutputListの順番通りに刺激を行う
  * Partial random: Micam用Random OutputListの順番をランダムにしてそれをTrial Count分繰り返す
  * Full Random: OutputListに指定した刺激をTrial Count分，完全にランダムに出力する

#### Trigger(右上タブ)

* TriggerLevel[V]: ALL Trigger Channelから出力されるTriggerの電圧
* FunGene Trigger Level[V]: Ex device Channelから出力される電圧

#### DAQ（右上タブ）

* Device Identifer: PCが認識しているDAQの固有識別子
* SamplingRate[Hz]: DAQ出力のSamplingRate
* Sound Channel: 音刺激が出力されるChannel
* Trigger Channel: DigitalTriggerが出力されるPort
  * For OmniPlex Trigger
* Ex device Channel: Function GeneratorへのTriggerが出力されるChannel
* Ex device Channel2: (`USMod Only`) Function GeneratorのModulationPortへの波形が出力されるChannel
* ALL Trigger Channel: 刺激が出力される場合にSignalNumberによらずTriggerが出力されるChannel
  * For Micam Trigger

#### FunGene（右上タブ）

* Function Generatorを使用したい場合にアプリにFunction Generatorの識別子を認識させるために使用する．


1. Get FunGene IDを押す
2. ComboBoxから使用したいFunction GeneratorのIdentiferを選択する
3. FunGeneIDの下部Function Generatorの名称が出てくれば認識が完了している


### PureTone

* ToneType: 下記`Pure`, `Tonepip`, `ToneBurst`の選択
* Decibel[dB]: 刺激音圧
* Frequency[Hz]: 正弦波の周波数
* Duration[ms]: 刺激波形の長さ

![PureTone](https://tt-lab.ist.hokudai.ac.jp/gitbucket/toda/UltraAquarius/_attached/1565072296807FjYQr14RM1)

#### Pure

* 窓なしの正弦波

#### TonePip

* バートレット窓付きの正弦波
  * 線形に刺激中心を1，両端を0とした三角形状の窓

#### ToneBurst

* 台形状(両端10％が斜辺)の窓関数付き正弦波
  * 線形に刺激開始を0，刺激長の10～90%を1，刺激終了点を0とした台形状の窓

### ClickTone

* Decibel[dB]: 刺激音圧
* Duration[ms]: 刺激波形の長さ

![Click](https://tt-lab.ist.hokudai.ac.jp/gitbucket/toda/UltraAquarius/_attached/15650723130518zMGlqevdJ)

#### Click

* Pulse波形

### AM(Amplitude Modulation)Tone

* Decibel[dB]: 刺激音圧
* Frequency[Hz]: 輸送波の周波数
* Modulation[Hz]: 変調波の周波数
* Duration[ms]: 刺激波形の長さ

![AMTone](https://tt-lab.ist.hokudai.ac.jp/gitbucket/toda/UltraAquarius/_attached/1565072333037EF5R5AP5M4)

#### AM

* 振幅変調させた正弦波

### Ultrasound

* WaveForm: 出力する波形(`Sine`, `Square`)
* Voltage[V]: Function Generatorから出力する電圧値
* Frequency[Hz]: 出力波形の周波数
* Waves: Burst一つに含まれる波の数
* Duty[%] (`Square Only`): Squareの場合のDuty比
* PRF[Hz]: PulseRepeatFrequencyの略, Burstの繰り返し頻度
* Pulses: Burstの数

![Ultrasound](https://tt-lab.ist.hokudai.ac.jp/gitbucket/toda/UltraAquarius/_attached/1565072346842FsLlhzNHnO)

#### Burst波(Sin, Square)

* 一定波数の刺激をPRFの周波数でBurst状に照射する

### USMod

* WindowForm: 窓の形(`Liner`, `Sine`)
* Voltage[V]: Function Generatorから出力する電圧値
* Frequency[Hz]: 出力波形の周波数
* Waves: Burst一つに含まれる波の数
* WindowWaves: 窓の長さを波数(周期)で指定する

![USMod](https://tt-lab.ist.hokudai.ac.jp/gitbucket/toda/UltraAquarius/_attached/1565072354945ekIFmtze17)

#### 窓付き連続波

* 出力にFunction Generatorの変調モードを使用しているのでModがついている
* 正弦波に線形・Sine窓(窓長は0-50％まで可変)をつけたもの

### Magnetic

* WaveForm: 出力に使用する波形(`Sine`, `Pulse`, `Square`)
* Freq[Hz] (`Sine Only`): 出力波形の周波数(FreqはFrequencyの略記)
* Voltage[V]: Function Generatorから出力する電圧値
* Duration[us] (`Pulse, Square Only`): 刺激長
* Interval[us] (`Pulse, Square Only`): 刺激-刺激間の長さ
* Raise[us] (`Pulse Only`): 刺激の立ち上がり時間
* Fall[us] (`Pulse Only`): 刺激の立ち下がり時間
* Waves: 刺激波形の数

![Magnetic](https://tt-lab.ist.hokudai.ac.jp/gitbucket/toda/UltraAquarius/_attached/1565072364952ijbHQGYyUi)

#### Sine

* 窓なしの正弦波

#### Pulse

* 立ち上がり，立ち下がりの時間を指定できる

#### Square(Pulse)

* Function Generatorの矩形波の機能を使ったPulse波形，立ち上がり，立ち下がり時間が最小
