using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public Transform[] MagicPlaceholder = new Transform[2];
    public Transform[] ChargePlaceholder = new Transform[2];

    public GameObject MagicEffectPrefab;
    public GameObject ChargeEffectPrefab;
    public GameObject ProjectilePrefab;

    private GameObject[] MagicEffects = new GameObject[2];
    private GameObject[] ChargeEffects = new GameObject[2];

    public Game1Manager game1Manager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var player in game1Manager.gameState.players)
        {
            //charge effect
            if (player.isCharging && ChargeEffects[player.idx] == null)
            {
                // Instantiate charge effect if not already instantiated
                ChargeEffects[player.idx] = Instantiate(ChargeEffectPrefab, ChargePlaceholder[player.idx].position, Quaternion.identity);
                ChargeEffects[player.idx].transform.SetParent(ChargePlaceholder[player.idx]);
            }
            else if (!player.isCharging && ChargeEffects[player.idx] != null)
            {
                // Destroy charge effect if it exists and player is not charging
                Destroy(ChargeEffects[player.idx]);
                ChargeEffects[player.idx] = null;
            } 

            //magic effect
            if (player.isCasting && MagicEffects[player.idx] == null)
            {
                // Instantiate magic effect if not already instantiated
                MagicEffects[player.idx] = Instantiate(MagicEffectPrefab, MagicPlaceholder[player.idx].position, Quaternion.identity);
                MagicEffects[player.idx].transform.SetParent(MagicPlaceholder[player.idx]);
            }
            else if (!player.isCasting && MagicEffects[player.idx] != null)
            {
                // Destroy magic effect if it exists and player is not casting
                Destroy(MagicEffects[player.idx]);
                MagicEffects[player.idx] = null;
            }
            else
            {
                // Ensure charge effect is not null if player is not charging
                if (MagicEffects[player.idx] != null)
                {
                    float strengthLevel = player.strengthLevel;
                    MagicEffects[player.idx].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * (strengthLevel * 0.5f + 0.5f);
                }
            }

        }
    }
}
