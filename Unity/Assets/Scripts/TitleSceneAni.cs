using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; //DOTween ���
using System.IO; 

public class TitleSceneAni : MonoBehaviour
{
    [Header("��� ���")]
    public Image ground;
    public Image trees;
    public Image water;
    public Image cloud;
    public Image cloudShadow;

    [Header("Ÿ��Ʋ �� ��ư")]
    public RectTransform title;
    public RectTransform gameStartButton;

    [Header("ĳ����")]
    public RectTransform characterSprite;
    public Vector2 characterEndPosition; //ĳ���Ͱ� �����Ǵ� ��ġ(�߾�)

    [Header("Ÿ�̹� ����")]
    public float beatInterval = 0.1f;
    public float popDuration = 0.1f;


    private void Start()
    {
        //�ִϸ��̼� ���� �� �ʱ� ����
        PrepareAnimation();

        //�ִϸ��̼� ������ ���� �� ����
        PlayTitleSequence();
        
    }

    void PrepareAnimation()
    {
        SetAlpha(ground, 0);
        SetAlpha(trees, 0);
        SetAlpha(water, 0);
        SetAlpha(cloud, 0);
        SetAlpha(cloudShadow, 0);

        // Ÿ��Ʋ�� ��ư�� ���� ���(ũ�� 0)�� �����մϴ�.
        title.localScale = Vector3.zero;
        gameStartButton.localScale = Vector3.zero;
    }

    void PlayTitleSequence()
    {
        Sequence mySequence = DOTween.Sequence();

        //ù§(����&��)
        mySequence.AppendInterval(beatInterval);
        mySequence.AppendCallback(() =>
        {
            SetAlpha(water, 1);
            SetAlpha(cloud, 1);
            SetAlpha(cloudShadow, 1);
        });

        mySequence.Join(CreateSquashAndPop(water.transform, 0.2f));
        mySequence.Join(CreateSquashAndPop(cloud.transform, 0.2f));
        mySequence.Join(CreateSquashAndPop(cloudShadow.transform, 0.05f));


        //��§(��)
        mySequence.AppendInterval(beatInterval);
        mySequence.AppendCallback(() => SetAlpha(ground, 1)); 
        mySequence.Join(CreateSquashAndPop(ground.transform, 0.1f));



        //��§(����)
        mySequence.AppendInterval(beatInterval);
        mySequence.AppendCallback(() => SetAlpha(trees, 1));
        mySequence.Join(CreateSquashAndPop(trees.transform, 0.2f));


        //��� ���
        mySequence.AppendInterval(beatInterval * 5);

        //��§(Ÿ��Ʋ, ��ư, ĳ���� ���� ����)
        mySequence.Append(title.DOScale(1f,popDuration).SetEase(Ease.OutBack));
        mySequence.Join(gameStartButton.DOScale(1f, popDuration).SetEase(Ease.OutBack));
        mySequence.Join(characterSprite.DOJumpAnchorPos(characterEndPosition, 50f, 1, popDuration + 0.2f));
        mySequence.Join(characterSprite.DORotate(new Vector3(0, 0, -20), popDuration + 0.2f));

    }

    Sequence CreateSquashAndPop(Transform target, float intensity)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(target.DOScaleY(0.85f, popDuration * 0.3f)) // �� ��׷���
                .Append(target.DOScaleY(1f, popDuration * 0.3f).SetEase(Ease.OutBack, intensity)); // �� �ϰ� ���� ũ���
        return sequence;
    }


    void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

}
