using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameView gameView;
    public GameController gameController;

    void Start()
    {
        //GameView와 GameController 초기화
        gameController = gameView.gameObject.AddComponent<GameController>();    
        gameController.gameView = gameView;
    }
}
