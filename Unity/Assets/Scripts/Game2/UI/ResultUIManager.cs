using System.Collections;
using DataForm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultUIManager : MonoBehaviour
{

    public static ResultUIManager Instance { get; private set; }

    [Header("Effect & Main Panels")]
    public Image screenEffectImage; //���ӿ��� ����Ʈ�� �̹���
    public GameObject gameOverText; //���ӿ��� �ؽ�Ʈ
    public GameObject resultPanel;

    [Header("Page 1 UI")]
    public GameObject page1;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI goldText;
    public Button nextPageButton;

    [Header("Page 2 UI")]
    public GameObject page2;
    public GameObject sentenceItemPrefab;
    public Transform sentenceListContent; //����� �� Content
    //public Button backToLobbyButton; //�κ�� ��ư
    //public Button againButton; //���Ǵ� ��ư



    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if(resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private void Start()
    {
        //��ư �̺�Ʈ ����
        if (nextPageButton != null) nextPageButton.onClick.AddListener(ShowNextPage);
        
        //�κ� ��ư, ���Ǵ� ��ư �̺�Ʈ ����

    }

    public void StartGameOverSequence(GameState finalState)
    {
        //���� �����Ǹ� �ΰ��� UI �� ����
        if(UIManager.Instance != null)
        {
            UIManager.Instance.matchingRoomPanel.SetActive(false);
            UIManager.Instance.playingPanel.SetActive(false);
        }


        //���� �� ��� ���� UI�� �� ���·� �ʱ�ȭ
        if(resultPanel != null) resultPanel.SetActive(false);
        if(gameOverText != null) gameOverText.SetActive(false);
        if (screenEffectImage != null) screenEffectImage.color = new Color(0, 0, 0, 0);

        StartCoroutine(GameOverSequenceCoroutine(finalState));
    }

    private IEnumerator GameOverSequenceCoroutine(GameState finalState)
    {

        if (screenEffectImage != null) screenEffectImage.gameObject.SetActive(true);

        //ȭ�� ����
        screenEffectImage.color = new Color(1, 1, 1, 0.5f);
        yield return new WaitForSeconds(0.1f);

        float fadeOutDuration = 0.5f;
        float timer = 0f;
        Color startColor = screenEffectImage.color;
        Color targetColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);

        if (gameOverText != null) gameOverText.SetActive(false);

        while (timer < fadeOutDuration)
            {
            timer += Time.deltaTime;
            screenEffectImage.color = Color.Lerp(startColor, targetColor, timer/fadeOutDuration);
            yield return null;

            }
        screenEffectImage.color = targetColor; //ȭ�� �������

        yield return new WaitForSeconds(1.0f); //1�� ��� �� Game Over �ؽ�Ʈ ǥ��

        //Game Over �ؽ�Ʈ ����
        if (gameOverText != null) gameOverText.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        //������ ���� ȭ������(2.7�ʵ���)
        timer = 0f;
        startColor = screenEffectImage.color;

        fadeOutDuration = 2.7f;

        if(gameOverText != null) gameOverText.SetActive(false);

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            screenEffectImage.color = Color.Lerp(startColor, Color.black, timer / fadeOutDuration);
            yield return null;
        }
        screenEffectImage.color = Color.black;
        yield return new WaitForSeconds(1.0f);

        //���â �����ֱ�
        gameOverText.SetActive(false);
        resultPanel.SetActive(true);
        UpdateResultUI(finalState);
        GameManager.Instance.NotifyResultVisible(); //���â ��!
    }

    //���â ���� ����ֱ�
    void UpdateResultUI(GameState finalState)
    {
        //������1 ���� ä���
        if (stageText != null) stageText.text = $"CLEARED STAGE: {finalState.gameLevel - 1}";
        if (scoreText != null) scoreText.text = $"TOTAL SCORE: {finalState.score}";
        if (goldText != null) goldText.text = $"EARNED GOLD: {finalState.gold}";

        //������2_���� ����Ʈ ���� ä���
        //������ �ִ� ����Ʈ ����
        if(sentenceListContent != null)
        {
            foreach (Transform child in sentenceListContent)
            {
                Destroy(child.gameObject);
            }
        }

        //�������� ���� ���� ������� ���� ����
        if(sentenceItemPrefab != null && finalState.completedSentences != null)
        {
            foreach (SentenceData sentence in finalState.completedSentences)
            {
                GameObject itemGO = Instantiate(sentenceItemPrefab, sentenceListContent);
                //�����տ� �ִ� Text ������Ʈ ã�Ƽ� ���� ä���ֱ� (������ �ؽ�Ʈ, �ؼ��� �־��ֱ�)
                TextMeshProUGUI[] texts = itemGO.GetComponentsInChildren<TextMeshProUGUI>();
                if(texts.Length >= 2)
                {
                    texts[0].text = sentence.text;
                    texts[1].text = sentence.meaning;
                }
            }
        }

        //�ʱ⿣ 1�������� �����ֱ�
        if(page1 != null) page1.SetActive(true);
        if(page2 != null) page2.SetActive(false);
    }

    //���� ������ ���� ��ư
    void ShowNextPage()
    {
        if(page1 != null) page1.SetActive(false);
        if(page2 != null) page2.SetActive(true);
    }

    //�κ�� ���ư��� ��ư

    //�� �� �� ��ư
}
