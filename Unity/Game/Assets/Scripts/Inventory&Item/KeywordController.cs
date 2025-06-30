using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeywordController : MonoBehaviour
{

    public string keywordId;

    private bool isCarried = false; //플레이어한테 어부바당하고 있는지 여부
    private Transform carrier; //어부바해주는 플레이어의 transform

    private TextMeshPro keywordText;

    private void Awake()
    {
        keywordText = GetComponentInChildren<TextMeshPro>();
    }

    public void Initialize(string id, string text)
    {
        this.keywordId = id;
       if(keywordText  != null )
        {
            keywordText.text = text;
        }
    }

   public void SetCarrier(Transform newCarrier)
    {
        this.carrier = newCarrier;
        this.isCarried = (newCarrier != null); //newCarrier가 null이 아니라면 true! 아니면 false! 

        if (!isCarried)
        {
            transform.SetParent(null);
        }
    }

    private void Update()
    {
        if (isCarried && carrier != null)
        {

            if (transform.parent != carrier)
            {
                transform.SetParent(carrier);

            }
            transform.localPosition = new Vector3(0, 0.7f, 0); //머리 위 위치 세팅
            }

        }

    }

