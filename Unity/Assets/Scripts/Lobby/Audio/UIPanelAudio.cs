using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UIRandomAudio : MonoBehaviour
{
    // ���� ���� ����� Ŭ���� ���� '�迭'
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // UI�� ���� ��
    void OnEnable()
    {
        // openSounds �迭�� Ŭ���� �ϳ��� �ִٸ�
        if (openSounds != null && openSounds.Length > 0)
        {
            // 0���� �迭�� ����-1 ���̿��� ������ ����(�ε���)�� ����
            int randomIndex = Random.Range(0, openSounds.Length);

            // �����ϰ� ���õ� ����� Ŭ���� ���
            audioSource.PlayOneShot(openSounds[randomIndex]);
        }
    }

    // UI�� ���� ��
    void OnDisable()
    {
        // closeSounds �迭�� Ŭ���� �ϳ��� �ִٸ�
        if (closeSounds != null && closeSounds.Length > 0)
        {
            // ���� �ε��� �̱�
            int randomIndex = Random.Range(0, closeSounds.Length);

            // �����ϰ� ���õ� ����� Ŭ���� ���
            audioSource.PlayOneShot(closeSounds[randomIndex]);
        }
    }
}
