using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private string ipaddr = "192.168.11.4";
#endif

    // Start is called before the first frame update
    void Start()
    {
#if WINDOWS_UWP
        websocket = new MessageWebSocket();
        websocket.Control.MessageType = SocketMessageType.Utf8;
        websocket.MessageReceived += WebSocket_MessageReceived;
        websocket.Closed += WebSocket_Closed;

        OnConnect(new Uri("ws:" + ipaddr + ":10090/w"));
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if WINDOWS_UWP
    void OnConnect(Uri uri)
    {
        Debug.Log("OnConnect");
        try {
            Task.Run(async () =>
            {
                await websocket.ConnectAsync(uri);
                await WebSocket_SendMessage(websocket, "Holo:Connect");
            });
        } catch (Exception e)
        {
            Debug.Log("Error: " + e.ToString());
        }
    }

    // ただのwrapper
    void SendMessage(string s)
    {
        try
        {
            WebSocket_SendMessage(websocket, s);
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.ToString());
        }
    }

    private async Task WebSocket_SendMessage(MessageWebSocket webSock, string message)
    {
        DataWriter mWriter = new DataWriter(webSock.OutputStream);
        mWriter.WriteString(message);
        await mWriter.StoreAsync();
        mWriter.DetachStream(); // ストリームの破棄らしいが、意味がよくわからん。メッセージ送ったらきちんとストリーム切ろうねってことかも
    }

    private void WebSocket_MessageReceived(MessageWebSocket webSock, MessageWebSocketMessageReceivedEventArgs args)
    {
        DataReader mReader = args.GetDataReader();
        mReader.UnicodeEncoding = UnicodeEncoding.Utf8;
        string mString = mReader.ReadString(mReader.UnconsumedBufferLength);

        Task.Run(async () =>
        {
            await WebSocket_SendMessage(webSock, "Holo:" + mString); // serverにログを残す
            // Do Something; ex) Debug.Log;
            Debug.Log("Received: " + mString);
            Task.Delay(100);
        });
    }

    private void WebSocket_Closed(IWebSocket webSock, WebSocketClosedEventArgs args)
    {
        // WebSock_Closed
        //webSock.Close();
    }
#endif
}