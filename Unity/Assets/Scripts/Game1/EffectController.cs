using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public float projectileScaleFactor = 0.5f; //투사체 크기 조정 인자

    public Transform[] MagicPlaceholder = new Transform[2];
    public Transform[] ChargePlaceholder = new Transform[2];

    public GameObject MagicEffectPrefab;
    public GameObject ChargeEffectPrefab;
    public GameObject ProjectilePrefab;
    public GameObject HealPrefab;

    private GameObject[] MagicEffects = new GameObject[2];
    private GameObject[] ChargeEffects = new GameObject[2];

    private GameObject[] Projectiles = { null, null };
    private float[] projectileElapsed = new float[2]; //투사체 시간 경과
    private float projectileDuration = 0.5f; //투사체 지속 시간

    public Game1Manager game1Manager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        try {
            if (game1Manager == null || game1Manager.gameState.state != "start")
            {
                //게임 매니저나 게임 상태가 초기화되지 않았으면 업데이트 중지
                return;
            }
            foreach (var player in game1Manager.gameState.players)
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
                        //강도에 따라 이펙트 크기 조정
                        MagicEffectController magicEffectController = MagicEffects[player.idx].GetComponent<MagicEffectController>();
                        magicEffectController.strengthLevel = player.strengthLevel;
                    }
                }

                if (player.castEnd && Projectiles[player.idx] == null)
                {
                    switch (player.currentAction)
                    {
                        case "ATTACK":
                            if (Projectiles[player.idx] == null)
                            {
                                //투사체 발사
                                Debug.Log("Player " + player.idx + " is casting attack action.");
                                shootProjectile(player.idx, player.strengthLevel);
                            }
                            break;
                        case "DEFENSE":
                            //방어 액션은 투사체를 발사하지 않음
                            Debug.Log("Player " + player.idx + " is casting defense action.");
                            break;
                        case "SPECIAL":
                            Debug.Log("Player " + player.idx + " is casting special action type of" + player.skillId);
                            showSpecialEffect(player.skillId, player.idx);
                            break;
                        default:
                            Debug.LogWarning("Unknown action: " + player.currentAction);
                            break;
                    }

                }
            }

            for (int i = 0; i < 2; i++)
            {
                if (Projectiles[i] != null)
                {
                    //투사체 날아가는 효과
                    projectileElapsed[i] += Time.deltaTime;
                    float t = Mathf.Clamp01(projectileElapsed[i] / projectileDuration);
                    float easeIn = t * t; // Ease-in effect
                    Projectiles[i].transform.position = Vector3.Lerp(MagicPlaceholder[i].position, ChargePlaceholder[1 - i].position, easeIn);
                    if (projectileElapsed[i] >= projectileDuration)
                    {
                        Debug.Log("Destroying projectile for player " + i + " after duration: " + projectileElapsed[i]);
                        Destroy(Projectiles[i]);
                        Projectiles[i] = null; //투사체 제거
                    }
                }
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in EffectController Update: " + e.Message);
        }
    }

    private void shootProjectile(int idx, int strengthLevel)
    {
        GameObject projectile = Instantiate(ProjectilePrefab, MagicPlaceholder[idx].position, Quaternion.identity);
        Debug.Log("Projectile created for player " + idx + " at position: " + MagicPlaceholder[idx].position);
        //0이면 정방향, 1이면 역방향
        projectile.transform.localScale = new Vector3(1.0f * Mathf.Sign(0.5f - idx), 1.0f, 1.0f);
        Debug.Log("Projectile scale set to: " + projectile.transform.localScale);
        Projectiles[idx] = projectile;
        projectileElapsed[idx] = 0.0f; //투사체 시간 초기화
    }

    private void showSpecialEffect(string skillId, int idx)
    {
        //특수 효과를 보여주는 로직을 여기에 추가
        Debug.Log("Showing special effect for skillId: " + skillId + " for player " + idx);
        switch(skillId)
        {
            case "heal":
                //Heal effect logic
                Debug.Log("Player " + idx + " is healing.");
                Instantiate(HealPrefab, ChargePlaceholder[idx].position, Quaternion.identity);
                break;
            default:
              Debug.LogWarning("Unknown skillId: " + skillId + " for player " + idx);
                break;
        }
    }
}
