using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomBGMPlayer : MonoBehaviour
{
    // 인스펙터에서 BGM 목록을 관리할 배열
    public AudioClip[] bgmList;

    private AudioSource audioSource;
    private int currentBgmIndex = -1; // 현재 재생 중인 BGM 인덱스

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayRandomBGM(); // 시작 시 첫 BGM 재생
    }

    void Update()
    {
        // 현재 BGM 재생이 끝났고, 목록에 노래가 있다면 다음 곡 재생
        if (!audioSource.isPlaying && bgmList.Length > 0)
        {
            PlayRandomBGM();
        }
    }

    void PlayRandomBGM()
    {
        // BGM 목록이 비어있으면 아무것도 하지 않음
        if (bgmList.Length == 0) return;

        int nextBgmIndex;

        // 목록에 노래가 2곡 이상일 때만 중복 방지 로직 실행
        if (bgmList.Length > 1)
        {
            // 이전에 재생된 곡과 다른 곡이 나올 때까지 다시 뽑기
            do
            {
                nextBgmIndex = Random.Range(0, bgmList.Length);
            } while (nextBgmIndex == currentBgmIndex);
        }
        else
        {
            nextBgmIndex = 0;
        }

        // 현재 재생할 곡의 인덱스와 클립을 설정
        currentBgmIndex = nextBgmIndex;
        AudioClip clipToPlay = bgmList[currentBgmIndex];

        // 오디오 소스에 클립을 할당하고 재생
        audioSource.clip = clipToPlay;
        audioSource.Play();
    }
}