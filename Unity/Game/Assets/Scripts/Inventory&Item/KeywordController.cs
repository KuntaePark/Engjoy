using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeywordController : MonoBehaviour
{

    public string keywordId;

    private bool isCarried = false; //�÷��̾����� ��ιٴ��ϰ� �ִ��� ����
    private Transform carrier; //��ι����ִ� �÷��̾��� transform

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
        this.isCarried = (newCarrier != null); //newCarrier�� null�� �ƴ϶�� true! �ƴϸ� false! 

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
            transform.localPosition = new Vector3(0, 0.7f, 0); //�Ӹ� �� ��ġ ����
            }

        }

    }

