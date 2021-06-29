using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

//#define WINDOWS_UWP // コーディングの際にシンタックスハイライトをつけたいため。保存する際はコメントアウトすること

#if WINDOWS_UWP
// Web Socket https://docs.microsoft.com/ja-jp/windows/uwp/networking/websockets
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Foundation;
using System;
#endif

public class ConnectToServer : MonoBehaviour
{
#if WINDOWS_UWP
    private MessageWebSocket websocket;
#endif
    public uint user_id = 0;
    public string ipaddr = "192.168.11.4";
    public string port = "10090";
    public TextMeshProUGUI recvText;
    public GameObject audioController;
    private AudioController acScript;
    private HumanState humanState = null;
    private string preMessage = "";

    // Start is called before the first frame update
    void Start()
    {
        // アプリケーション起動時に必ずサーバーとの接続処理を入れる
        OnConnect();
        // audioControllerのスクリプトを取得し、音声再生を可能にする。
        acScript = audioController.GetComponent<AudioController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ParseMessage(string jsonString)
    {
        // JsonUtility.FromJson<クラス名>(string); でstringをクラス名にパースできる
        this.humanState = JsonUtility.FromJson<HumanState>(jsonString);
        if (humanState.message != "" && humanState.message != preMessage)
        {
            // 前回のメッセージと違っていたら読み上げと表示を行う。
            this.acScript.SynthesizeAudio(humanState.message);
            this.preMessage = humanState.message; // 同じメッセージが何度も表示されるのを防ぐ
            RecvMessage(this.preMessage);
        }
    }

    // バッチ確認の時に音声を流すための関数
    public void CheckBatch(AudioClip clip)
    {
        if (this.humanState == null) return;
        if (this.humanState.currenttask == "")
        {
            // 何もないことを示すメッセージ(clip)を送信
            this.acScript.playWav(clip);
        }
        else
        {
            // 現在のタスクをacScriptを使って音声合成する。
            this.acScript.SynthesizeAudio(this.humanState.currenttask);
            RecvMessage("現在のタスク: " + this.humanState.currenttask);
        }
    }

#if WINDOWS_UWP

    // サーバーと接続する。すでに接続しているなら、何もしない
    public void OnConnect()
    {
        if (websocket != null) return;

        // 本来なら変数はStartで初期化したいが、後々の事を考えて、こちらで初期化している。
        // つまり、コネクションを再接続する可能性があることを意味している。
        websocket = new MessageWebSocket();
        websocket.Control.MessageType = SocketMessageType.Utf8;
        websocket.MessageReceived += WebSocket_MessageReceived;
        websocket.Closed += WebSocket_Closed;

        Debug.Log("OnConnect");
        try {
            Task.Run(async () =>
            {
                await websocket.ConnectAsync(new Uri("ws://" + ipaddr + ":" + port + "/w"));
                await WebSocket_SendMessage(user_id.ToString());
            });
        } catch (Exception e)
        {
            Debug.Log("Error: " + e.ToString());
        }
    }

    // メッセージを受け取った際に表示データを更新する関数。
    private void RecvMessage(string s)
    {
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            this.recvText.SetText(s);
            Invoke("ClearText", 5.0f); // 5sec後にClearTextを呼び出す。
        }, false);
    }

    // Unity側で認識させるためのラッパー
    public void SendToServer(string s)
    {
        try
        {
            Task.Run(async () => {
                await WebSocket_SendMessage(s);
            });
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.ToString());
        }
    }

    // 追加情報欄を空にする
    public void ClearText()
    {
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            this.recvText.SetText("");
        }, false);
    }

    // 実際にサーバーに送信を行う。外部からは呼び出されない
    private async Task WebSocket_SendMessage(string message)
    {
        using (var dataWriter = new DataWriter(this.websocket.OutputStream))
        {
            dataWriter.WriteString(message);
            await dataWriter.StoreAsync();
            dataWriter.DetachStream();
        }
    }

    // メッセージを受信した際に自動的に呼び出される。
    // ハンドラのため、必要のないsenderを受け取っている。
    // senderを使えばいろいろできるのだろうか...?
    private void WebSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
    {
        try
        {
            using (DataReader dataReader = args.GetDataReader())
            {
                // DataReaderを用いて受信したデータを解読
                dataReader.UnicodeEncoding = UnicodeEncoding.Utf8;
                string message = dataReader.ReadString(dataReader.UnconsumedBufferLength);

                ParseMessage(message);

                // 表示データを更新
                //Debug.Log("Received: " + message);
                //RecvMessage(message);

                /*
                Task.Run(async () =>
                {
                    // serverにログを残す
                    await WebSocket_SendMessage("Holo:" + message);
                    // Do Something; ex) Debug.Log();
                    //Task.Delay(100);
                });
                */
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.ToString());
        }

    }

    // 実は、公式ページの丸パクリだったり...
    private void WebSocket_Closed(IWebSocket webSock, WebSocketClosedEventArgs args)
    {
        Debug.Log("WebSocket_Closed; Code: " + args.Code + ", Reason: \"" + args.Reason + "\"");
    }
    
    // サーバーとの接続を切る。これをやらないと、毎回不正に切られてしまう。それはよくない
    public void CloseConnection()
    {
        if (websocket == null) return;
        this.websocket.Dispose(); // 接続を切る
        websocket = null; // nullを入れることで、接続の有無を明確に
    }

    // 音声コマンドでアプリケーションを終了させる
    public void FinishApplication()
    {
        if (websocket != null) CloseConnection(); // 接続が切れてないなら切る
        Application.Quit();
    }

#else
    public void OnConnect()
    {
        this.recvText.SetText("Start Connection " + user_id.ToString());
    }
    public void CloseConnection()
    {
        this.recvText.SetText("Close Connection");
    }
    public void SendToServer(string s)
    {
        this.recvText.SetText("send ->" + s);
    }
    private void RecvMessage(string s)
    {
        this.recvText.SetText(s);
    }
    public void ClearText()
    {
        this.recvText.SetText("");
    }
    public void FinishApplication()
    {
        this.recvText.SetText("Finish Application.");
        Application.Quit();
    }
#endif
}

/* json用のシリアライズクラス */
[System.Serializable]
public class HumanState
{
    public string currenttask;
    public string nextposition;
    public string workingtime;
    public string movedistance;
    public string message;
    public Position currentpostion;
    public Position targetposition;
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
}