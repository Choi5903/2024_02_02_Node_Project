using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

[System.Serializable]
public class Player
{
    public int player_id;
    public string username;
    public int level;
}

[System.Serializable]
public class InventoryItem
{
    public int item_id;
    public string name;
    public string description;
    public int value;
    public int quantity;
}

[System.Serializable]
public class Quest
{
    public int quest_id;
    public string title;
    public string description;
    public int reward_exp;
    public int reward_item_id;
    public string status;
}

public class GameDataManager : MonoBehaviour
{
    private string serverUrl = "http://localhost:3000";
    private Player currentPlayer;

    public List<InventoryItem> inventoryItems = new List<InventoryItem>();
    public List<Quest> playerQuests = new List<Quest>();

    public delegate void OnLoginSuccessHandler(Player player);
    public event OnLoginSuccessHandler OnLoginSuccess;

    public delegate void OnInventoryUpdateHandler(List<InventoryItem> items);
    public event OnInventoryUpdateHandler OnInventoryUpdate;

    public delegate void OnQuestsUpdateHandler(List<Quest> quests);
    public event OnQuestsUpdateHandler OnQuestsUpdate;

    public IEnumerator Login(string username, string passwordHash)
    {
        Debug.Log($"[Login] Attempting login for user: {username}");

        var loginData = new Dictionary<string, string>
    {
        { "username", username },
        { "password_hash", passwordHash }
    };

        string jsonData = JsonConvert.SerializeObject(loginData);
        Debug.Log($"[Login] JSON Payload: {jsonData}");

        var www = new UnityWebRequest($"{serverUrl}/login", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest(); // `yield return`을 try 블록 외부로 이동

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[Login] Server Response: {www.downloadHandler.text}");
            bool parseError = false; // 응답 파싱 에러 추적용 플래그
            Dictionary<string, object> response = null;

            try
            {
                response = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
            }
            catch (System.Exception ex)
            {
                parseError = true;
                Debug.LogError($"[Login] Exception while parsing response: {ex.Message}");
            }

            if (!parseError && response != null && (bool)response["success"])
            {
                currentPlayer = JsonConvert.DeserializeObject<Player>(response["player"].ToString());
                Debug.Log($"[Login] Login successful! Player: {currentPlayer.username}");

                OnLoginSuccess?.Invoke(currentPlayer);

                yield return StartCoroutine(GetInventory());
                yield return StartCoroutine(GetQuest());
            }
            else
            {
                Debug.LogError("[Login] Login failed: Invalid credentials or response parsing issue.");
            }
        }
        else
        {
            Debug.LogError($"[Login] HTTP Error: {www.error}");
        }

        www.Dispose();
    }


    private IEnumerator GetInventory()
    {
        if (currentPlayer == null)
        {
            Debug.LogError("[GetInventory] Current player is null.");
            yield break;
        }

        Debug.Log($"[GetInventory] Fetching inventory for player ID: {currentPlayer.player_id}");

        using (UnityWebRequest www = UnityWebRequest.Get($"{serverUrl}/inventory/{currentPlayer.player_id}"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[GetInventory] Server Response: {www.downloadHandler.text}");
                inventoryItems = JsonConvert.DeserializeObject<List<InventoryItem>>(www.downloadHandler.text);
                OnInventoryUpdate?.Invoke(inventoryItems);
            }
            else
            {
                Debug.LogError($"[GetInventory] HTTP Error: {www.error}");
            }
        }
    }

    private IEnumerator GetQuest()
    {
        if (currentPlayer == null)
        {
            Debug.LogError("[GetQuest] Current player is null.");
            yield break;
        }

        Debug.Log($"[GetQuest] Fetching quests for player ID: {currentPlayer.player_id}");

        using (UnityWebRequest www = UnityWebRequest.Get($"{serverUrl}/quests/{currentPlayer.player_id}"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[GetQuest] Server Response: {www.downloadHandler.text}");
                playerQuests = JsonConvert.DeserializeObject<List<Quest>>(www.downloadHandler.text);
                OnQuestsUpdate?.Invoke(playerQuests);
            }
            else
            {
                Debug.LogError($"[GetQuest] HTTP Error: {www.error}");
            }
        }
    }

    void Start()
    {
        StartCoroutine(Login("hero1", "hashed_password1"));
    }
}
