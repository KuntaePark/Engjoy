using DataForm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartUI1 : GameStartUI
{
    public Text scoreText;
    public Text rankingText;

    public override void setGameStartUI(UserGameData userGameData)
    {
        scoreText.text = userGameData.game1Score.ToString() + "점";
        rankingText.text = userGameData.ranking.ToString() + "위";
    }
}
