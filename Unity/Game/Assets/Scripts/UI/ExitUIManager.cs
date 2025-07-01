using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExitUIManager : MonoBehaviour
{
  public static ExitUIManager Instance {  get; private set; }

    [Header("Exit UI Panel")]
    [SerializeField] private GameObject exitUIPanel; //����, ���� ���� ����
    [SerializeField] private TextMeshProUGUI sentenceText; //����
    [SerializeField] private TextMeshProUGUI translationText; //�ؼ���

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HideExitUI();
    }

    public void UpdateExitUI(string sentence, string translation)
    {
        if (sentence != null) sentenceText.text = sentence; //������ ����
        if (translation != null) translationText.text = translation; //�ؼ��� ����
    }

    public void ShowExitUI()
    {
        //UI ǥ��
        if (exitUIPanel != null) exitUIPanel.SetActive(true);
    }

    public void HideExitUI()
    {
        //UI �����
        if (exitUIPanel != null) exitUIPanel.SetActive(false);
    }
}
