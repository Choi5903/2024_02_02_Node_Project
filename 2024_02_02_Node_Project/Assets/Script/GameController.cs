using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameView gameView;
    private PlayerModel playerModel;
    private GameAPI gameAPI;

    // Start is called before the first frame update
    void Start()
    {
        gameAPI = gameObject.AddComponent<GameAPI>();
        gameView.SetRegisterButtonListener(OnRegisterButtonClicked);
        gameView.SetLoginButtonListener(OnLoginButtonClicked);
        gameView.SetCollectButtonListener(OnCollectButtonClicked);
    }

    public void OnRegisterButtonClicked()
    {
        string playerName = gameView.playerNameInput.text;
        StartCoroutine(gameAPI.RegisterPlayer(playerName, "1234"));     //예시 비밀번호
    }

    public void OnLoginButtonClicked()
    {
        string playerName = gameView.playerNameInput.text;
        StartCoroutine(LoginPlayerCoroutine(playerName, "1234"));     //예시 비밀번호
    }

    public void OnCollectButtonClicked()
    {
        if (playerModel != null)
        {
            Debug.Log($"Collecting resources for : {playerModel.playerName}");
            StartCoroutine(CollectCoroutine(playerModel.playerName));   //PlayerModel.name 사용            
        }
        else
        {
            Debug.LogError("Player model is null");
        }
    }

    private IEnumerator CollectCoroutine(string playerName)
    {
        yield return gameAPI.CollectResources(playerName, player =>
        {
            playerModel.metal = player.metal;
            playerModel.crystal = player.crystal;
            playerModel.deuterium = player.deuterium;
            UpdateResourcesDisplay();
        });
    }

    private IEnumerator LoginPlayerCoroutine(string playerName, string password)
    {
        yield return gameAPI.LoginPlayer(playerName, password, player =>
        {
            playerModel = player;   //로그인 성공 시 playerModel 업데이트
            UpdateResourcesDisplay();
        });
    }

    private void UpdateResourcesDisplay()
    {
        if (playerModel != null) //playerModel 이 null이 아닐 때만 UI 업데이트
        {
            gameView.SetPlayerName(playerModel.playerName);
            gameView.UpdateResources(playerModel.metal, playerModel.crystal, playerModel.deuterium);
        }
    }
}