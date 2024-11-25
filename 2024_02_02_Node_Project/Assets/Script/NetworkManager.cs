using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;                              //WebSocket 활용
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

//메세지 타입 정의
[Serializable]
public class NetworkMessage
{
    public string type;
    public string playerId;
    public string message;
    public Vector3Data position;
}
//Vector3 직렬화를 위한 클래스
[Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVectior3()
    {
        return new Vector3(x, y, z);
    }
}

public class NetworkManager : MonoBehaviour
{
    private WebSocket webSocket;
    [SerializeField] private string serverUrl = "ws://localhost:3000";

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button SendButton;
    [SerializeField] private TextMeshProUGUI chatLog;
    [SerializeField] private TextMeshProUGUI statusText;


    void Start()
    {
        
    }


    void Update()
    {
        
    }

    private async void ConnectToServer()
    {
        webSocket = new WebSocket(serverUrl);

        webSocket.OnOpen += () =>
        {
            Debug.Log("연결 성공!");
            UpdateStatusText("연결됨", Color.green);
        };

        webSocket.OnError += (e) =>
        {
            Debug.Log($"에러 : {e}");
            UpdateStatusText("에러 발생", Color.red);
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log("연결 종료");
            UpdateStatusText("연결 끊김", Color.red);
        };

        webSocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };
        await webSocket.Connect();
    }

    private void HandleMessage(string json)
    {
        try
        {
            NetworkMessage message = JsonConvert.DeserializeObject<NetworkMessage>(json);

            switch (message.type)
            {
                case "connection":
                    break;
                case "chat":
                    AddToChatLog(message.message);
                    break;
            }    
        }
        catch (Exception e)
        {
            Debug.LogError($"메세지 처리 중 에러 : {e.Message}");
        }
    }

    private async void SendChatMessage()
    {
        if (string.IsNullOrEmpty(messageInput.text)) return;

        if(webSocket.State == WebSocketState.Open)
        {
            NetworkMessage message = new NetworkMessage
            {
                type = "chat",
                message = messageInput.text
            };

            await webSocket.SendText(JsonConvert.SerializeObject(message));
            messageInput.text = "";
        }
    }

    private void UpdateStatusText(string status, Color color)
    {
        if(statusText != null)
        {
            statusText.text = status;
            statusText.color = color;
        }
    }

    private void AddToChatLog(string message)
    {
        if(chatLog != null)
        {
            chatLog.text += $"\n{message}";
        }
    }

    private async void OnApplicationQuit()
    {
        if(webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.Close();
        }
    }    
}
