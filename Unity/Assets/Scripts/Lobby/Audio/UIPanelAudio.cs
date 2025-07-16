using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UIRandomAudio : MonoBehaviour
{
    // 여러 개의 오디오 클립을 담을 '배열'
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // UI가 켜질 때
    void OnEnable()
    {
        // openSounds 배열에 클립이 하나라도 있다면
        if (openSounds != null && openSounds.Length > 0)
        {
            // 0부터 배열의 길이-1 사이에서 랜덤한 숫자(인덱스)를 뽑음
            int randomIndex = Random.Range(0, openSounds.Length);

            // 랜덤하게 선택된 오디오 클립을 재생
            audioSource.PlayOneShot(openSounds[randomIndex]);
        }
    }

    // UI가 꺼질 때
    void OnDisable()
    {
        // closeSounds 배열에 클립이 하나라도 있다면
        if (closeSounds != null && closeSounds.Length > 0)
        {
            // 랜덤 인덱스 뽑기
            int randomIndex = Random.Range(0, closeSounds.Length);

            // 랜덤하게 선택된 오디오 클립을 재생
            audioSource.PlayOneShot(closeSounds[randomIndex]);
        }
    }
}
