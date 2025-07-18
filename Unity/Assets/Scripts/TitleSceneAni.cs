using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; //DOTween 사용
using System.IO; 

public class TitleSceneAni : MonoBehaviour
{
    [Header("배경 요소")]
    public Image ground;
    public Image trees;
    public Image water;
    public Image cloud;
    public Image cloudShadow;

    [Header("타이틀 및 버튼")]
    public RectTransform title;
    public RectTransform gameStartButton;

    [Header("캐릭터")]
    public RectTransform characterSprite;
    public Vector2 characterEndPosition; //캐릭터가 고정되는 위치(중앙)

    [Header("타이밍 설정")]
    public float beatInterval = 0.1f;
    public float popDuration = 0.1f;


    private void Start()
    {
        //애니메이션 시작 전 초기 상태
        PrepareAnimation();

        //애니메이션 시퀀스 생성 및 실행
        PlayTitleSequence();
        
    }

    void PrepareAnimation()
    {
        SetAlpha(ground, 0);
        SetAlpha(trees, 0);
        SetAlpha(water, 0);
        SetAlpha(cloud, 0);
        SetAlpha(cloudShadow, 0);

        // 타이틀과 버튼은 이전 방식(크기 0)을 유지합니다.
        title.localScale = Vector3.zero;
        gameStartButton.localScale = Vector3.zero;
    }

    void PlayTitleSequence()
    {
        Sequence mySequence = DOTween.Sequence();

        //첫짠(구름&물)
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


        //둘짠(땅)
        mySequence.AppendInterval(beatInterval);
        mySequence.AppendCallback(() => SetAlpha(ground, 1)); 
        mySequence.Join(CreateSquashAndPop(ground.transform, 0.1f));



        //셋짠(나무)
        mySequence.AppendInterval(beatInterval);
        mySequence.AppendCallback(() => SetAlpha(trees, 1));
        mySequence.Join(CreateSquashAndPop(trees.transform, 0.2f));


        //잠시 대기
        mySequence.AppendInterval(beatInterval * 5);

        //막짠(타이틀, 버튼, 캐릭터 동시 등장)
        mySequence.Append(title.DOScale(1f,popDuration).SetEase(Ease.OutBack));
        mySequence.Join(gameStartButton.DOScale(1f, popDuration).SetEase(Ease.OutBack));
        mySequence.Join(characterSprite.DOJumpAnchorPos(characterEndPosition, 50f, 1, popDuration + 0.2f));
        mySequence.Join(characterSprite.DORotate(new Vector3(0, 0, -20), popDuration + 0.2f));

    }

    Sequence CreateSquashAndPop(Transform target, float intensity)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(target.DOScaleY(0.85f, popDuration * 0.3f)) // 꾹 찌그러짐
                .Append(target.DOScaleY(1f, popDuration * 0.3f).SetEase(Ease.OutBack, intensity)); // 뿅 하고 원래 크기로
        return sequence;
    }


    void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

}
