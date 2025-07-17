using DataForm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartUI2 : GameStartUI
{
    public Text scoreText;

    public override void setGameStartUI(UserGameData userGameData)
    {
        scoreText.text = userGameData.game2Score.ToString() + "점";
        return;
    }
}
