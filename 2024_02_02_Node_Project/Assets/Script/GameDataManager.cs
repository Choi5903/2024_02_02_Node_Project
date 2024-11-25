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

    //데이터 리스트 
    public List<InventoryItem> inventoryItems = new List<InventoryItem>();
    public List<Quest> playerQuests = new List<Quest>();

    //로그인 성공 시 실행될 이벤트 
    public delegate void OnLoginSuccessHandler(Player player);
    public event OnLoginSuccessHandler OnLoginSuccess;

    //데이터 업데이트시 실행될 이벤트
    public delegate void OnInventoryUpdateHandler(List<InventoryItem> Items);
    public event OnInventoryUpdateHandler OnInventoryUpdate;

    //퀘스트 업데이트시 실행될 이벤트
    public delegate void OnQuestsUpdateHandler(List<Quest> quests);
    public event OnQuestsUpdateHandler OnQuestsUpdate;

    public IEnumerator Login(string username, string passwordHash)
    {
        var loginData = new Dictionary<string, string>
        {
            { "username", username },
            { "password_hash", passwordHash }
        };

        string jsonData = JsonConvert.SerializeObject(loginData);
        var www = new UnityWebRequest($"{serverUrl}/login", "POST");
        Debug.Log($"{serverUrl}/login");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
            if ((bool)response["success"])
            {
                currentPlayer = JsonConvert.DeserializeObject<Player>(response["player"].ToString());
                Debug.Log($"로그인 성공: {currentPlayer.username}");

                OnLoginSuccess?.Invoke(currentPlayer);

                //로그인 성공후 데이터 조회 
                yield return StartCoroutine(GetInventory());
                yield return StartCoroutine(GetQuests());
            }
        }
        else
        {
            Debug.LogError("로그인 실패: " + www.error);
        }

        www.Dispose();
    }

    private IEnumerator GetInventory()
    {
        if (currentPlayer == null) yield break;

        using (UnityWebRequest www = UnityWebRequest.Get($"{serverUrl}/inventory/{currentPlayer.player_id}"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                inventoryItems = JsonConvert.DeserializeObject<List<InventoryItem>>(www.downloadHandler.text);
                Debug.Log("인벤토리 아이템:");
                foreach (var item in inventoryItems)
                {
                    Debug.Log($" - {item.name} x {item.quantity} (가치 : {item.value})");
                }

                OnInventoryUpdate?.Invoke(inventoryItems);
            }
            else
            {
                Debug.LogError("인벤토리 조회 실패 : " + www.error);
            }
        }
    }

    private IEnumerator GetQuests()
    {
        if (currentPlayer == null) yield break;

        using (UnityWebRequest www = UnityWebRequest.Get($"{serverUrl}/quests/{currentPlayer.player_id}"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                playerQuests = JsonConvert.DeserializeObject<List<Quest>>(www.downloadHandler.text);
                Debug.Log("진행 중인 퀘스트 :");
                foreach (var quest in playerQuests)
                {
                    Debug.Log($" - {quest.title}  {quest.status})");
                }

                OnQuestsUpdate?.Invoke(playerQuests);
            }
            else
            {
                Debug.LogError("퀘스트 조회 실패 : " + www.error);
            }
        }
    }

    //인벤토리 아이템 찾기
    public InventoryItem GetInventoryItem(int itemId)
    {
        return inventoryItems.Find(item => item.item_id == itemId);
    }

    //퀘스트 찾기
    public Quest GetQuest(int questId)
    {
        return playerQuests.Find(quest => quest.quest_id == questId);
    }

    //현재 플레이어 정보 가저오기 
    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }

    void Start()
    {
        StartCoroutine(Login("hero123", "hashed_password1"));
    }

}