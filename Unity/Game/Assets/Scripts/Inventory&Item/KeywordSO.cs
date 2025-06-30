using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "키워드 규칙", menuName = "Item/Keyword Data")]
public class KeywordSO : ScriptableObject
{

    public string keyword; //키워드 형태?

    public bool isCorrect; //정답인지 아닌지를 표시하는 플래그


}
