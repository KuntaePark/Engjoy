using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExitUIManager : MonoBehaviour
{
  public static ExitUIManager Instance {  get; private set; }

    [Header("Exit UI Panel")]
    [SerializeField] private GameObject exitUIPanel; //문장, 번역 등을 포함
    [SerializeField] private TextMeshProUGUI sentenceText; //문장
    [SerializeField] private TextMeshProUGUI translationText; //해설문

    private void Awake()
    {
        Debug.Log($"UIManager Awake() called on GameObject: {gameObject.name}");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
            Debug.Log($"<color=green>UIManager Instance SET by: {gameObject.name}. Canvas will be preserved.</color>");
        }
        else
        {
            Debug.LogError($"<color=red>Duplicate UIManager found on: {gameObject.name}! This is trying to destroy the Canvas!</color>");
            Destroy(transform.root.gameObject);
        }
    }

    private void Start()
    {
        HideExitUI();
    }

    public void UpdateExitUI(string sentence, string translation)
    {
        if (sentence != null) sentenceText.text = sentence; //영문장 세팅
        if (translation != null) translationText.text = translation; //해설문 세팅
    }

    public void ShowExitUI()
    {
        //UI 표시
        if (exitUIPanel != null) exitUIPanel.SetActive(true);
    }

    public void HideExitUI()
    {
        //UI 숨기기
        if (exitUIPanel != null) exitUIPanel.SetActive(false);
    }
}
