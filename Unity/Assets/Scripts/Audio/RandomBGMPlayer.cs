using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomBGMPlayer : MonoBehaviour
{
    // �ν����Ϳ��� BGM ����� ������ �迭
    public AudioClip[] bgmList;

    private AudioSource audioSource;
    private int currentBgmIndex = -1; // ���� ��� ���� BGM �ε���

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayRandomBGM(); // ���� �� ù BGM ���
    }

    void Update()
    {
        // ���� BGM ����� ������, ��Ͽ� �뷡�� �ִٸ� ���� �� ���
        if (!audioSource.isPlaying && bgmList.Length > 0)
        {
            PlayRandomBGM();
        }
    }

    void PlayRandomBGM()
    {
        // BGM ����� ��������� �ƹ��͵� ���� ����
        if (bgmList.Length == 0) return;

        int nextBgmIndex;

        // ��Ͽ� �뷡�� 2�� �̻��� ���� �ߺ� ���� ���� ����
        if (bgmList.Length > 1)
        {
            // ������ ����� ��� �ٸ� ���� ���� ������ �ٽ� �̱�
            do
            {
                nextBgmIndex = Random.Range(0, bgmList.Length);
            } while (nextBgmIndex == currentBgmIndex);
        }
        else
        {
            nextBgmIndex = 0;
        }

        // ���� ����� ���� �ε����� Ŭ���� ����
        currentBgmIndex = nextBgmIndex;
        AudioClip clipToPlay = bgmList[currentBgmIndex];

        // ����� �ҽ��� Ŭ���� �Ҵ��ϰ� ���
        audioSource.clip = clipToPlay;
        audioSource.Play();
    }
}