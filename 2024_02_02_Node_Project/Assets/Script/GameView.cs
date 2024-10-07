using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameView : MonoBehaviour
{

    //UI 요소 선언
    public Text playerNameText;
    public Text metalText;
    public Text crystalText;
    public Text deuteriumText;
    public InputField playerNameInput;

    public Button registerButton;
    public Button loginButton;
    public Button collectButton;
    public Button developButton;
    public Slider progressBar;

    //UI 업데이트 메서드
    public void SetPlayerName(string name)
    {
        playerNameText.text = name;
    }

    public void UpdateResources(int metal, int crystal, int deuterium)
    {
        metalText.text = $"Metal : {metal}";
        crystalText.text = $"Crystal : {crystal}";
        deuteriumText.text = $"Deuterium : {deuterium}";
    }

    public void UpdateProgressBar(float value)
    {
        progressBar.value = value;
    }

    //버튼 클릭 리스너 설정 메서드
    public void SetRegisterButtonListener(UnityEngine.Events.UnityAction action)
    {
        registerButton.onClick.RemoveAllListeners();
        registerButton.onClick.AddListener(action);
    }
    public void SetLoginButtonListener(UnityEngine.Events.UnityAction action)
    {
        loginButton.onClick.RemoveAllListeners();
        loginButton.onClick.AddListener(action);
    }
    public void SetCollectButtonListener(UnityEngine.Events.UnityAction action)
    {
        collectButton.onClick.RemoveAllListeners();
        collectButton.onClick.AddListener(action);
    }
    public void SetDevelopButtonListener(UnityEngine.Events.UnityAction action)
    {
        developButton.onClick.RemoveAllListeners();
        developButton.onClick.AddListener(action);
    }
}
