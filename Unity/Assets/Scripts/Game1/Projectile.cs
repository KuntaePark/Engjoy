using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    [SerializeField] private AudioClip fireHitSound;
    private AudioSource audioSource;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            audioSource.PlayOneShot(fireHitSound);

            //run player hit animation
            CharacterRenderer characterRenderer = collision.gameObject.GetComponent<CharacterRenderer>();
            characterRenderer.bodyAnimator.SetTrigger("hit");

        }
    }
}
