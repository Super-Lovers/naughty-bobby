﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    public int EnemyHitPoints = 3;
    [Range(0, 10)]
    public float EnemySpeed = 5f;
    [Range(0, 1)]
    public float EnemyRunningSpeed = 0.03f;
    public GameObject Snowball;
    public Sprite EnemyMeleeSprite;
    public Sprite DamagedSprite;
    public LayerMask PlayerLayerMask;
    public RuntimeAnimatorController _animatorMelee;

    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody2d;
    private bool _canPickNewDirection = true;
    private bool _goLeft = false;
    private bool _canShoot = true;
    private GameObject _player;
    private Animator _animator;
    private bool _canHitPlayer = true;
    private bool _startedBossAttacks = false;
    private ConstantForce2D _constantForce2D;
    public Sprite BossSprite;

    public GameObject[] SplashAttacks;
    public AudioClip WeaponSlashSound;
    public AudioClip SplashAttackSound;
    public AudioClip[] EnemyHitSounds;

    void Start () {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _animator = GetComponent<Animator>();
        _constantForce2D = GetComponent<ConstantForce2D>();

        if (StageController.CurrentStage == 1)
        {
            Invoke("StopConstantForce", 4f);
        } else if ((StageController.CurrentStage == 2 &&
            StageController.CurrentWave != 3) ||
            StageController.CurrentStage == 3)
        {
            StopConstantForce();
            gameObject.transform.Rotate(0, 0, -90);
        } else if (StageController.CurrentStage == 2 &&
            StageController.CurrentWave == 3)
        {
            StopConstantForce();
            gameObject.transform.Rotate(0, 0, 90);
        }
    }

    // This stops the enemy cart from getting closer a few
    // seconds after it spawns.
    private void StopConstantForce()
    {
        Vector2 newForce = _constantForce2D.force;
        newForce.y = 0;
        _constantForce2D.force = newForce;
    }

    private void Update()
    {
        if (StageController.CurrentStage == 1)
        {
            if (_canShoot)
            {
                GameObject snowball = Instantiate(Snowball,
                    transform.position, Quaternion.identity);
                snowball.tag = "Enemy Snowball";

                _canShoot = false;
                Invoke("EnableShooting", 1f);
            }

            if (_canPickNewDirection)
            {
                if (Random.Range(0, 101) > 50)
                {
                    _goLeft = true;
                }
                else
                {
                    _goLeft = false;
                }

                _canPickNewDirection = false;
                Invoke("EnablePickingDirection", Random.Range(0.5f, 0.8f));
            }

            Vector3 currentPosition = transform.position;
            if (_goLeft)
            {
                if (currentPosition.x > -5)
                {
                    _rigidbody2d.AddForce(new Vector2(-EnemySpeed * Time.deltaTime, 0));
                }
                else
                {
                    _rigidbody2d.AddForce(new Vector2(EnemySpeed * Time.deltaTime, 0));
                }
            }
            else
            {
                if (currentPosition.x < 5)
                {
                    _rigidbody2d.AddForce(new Vector2(EnemySpeed, 0));
                }
                else
                {
                    _rigidbody2d.AddForce(new Vector2(-EnemySpeed, 0));
                }
            }
        }
        else if ((StageController.CurrentStage == 2 ||
          StageController.CurrentStage == 3) && StageController.CurrentWave != 3)
        {
            _animator.runtimeAnimatorController = _animatorMelee;
            GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.5f);
            
            if (Vector2.Distance(transform.position, _player.transform.position) < 5)
            {
                Vector3 dir = _player.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                transform.position =
                    Vector2.MoveTowards(transform.position,
                    _player.transform.position, EnemyRunningSpeed);
            }
        }
        else if (_startedBossAttacks == false && gameObject.name == "Boss(Clone)")
        {
            _animator.runtimeAnimatorController = _animatorMelee;
            _animator.enabled = false;
            GetComponent<SpriteRenderer>().sprite = BossSprite;
            GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.5f);

            Vector2 newScale = transform.localScale;
            newScale.x = 5;
            newScale.y = 4;
            transform.localScale = newScale;

            transform.Rotate(0, 0, 90);
            
            InvokeRepeating("WarnPlayerOfAttack", 3f, 4f);

            _startedBossAttacks = true;
        } else if (gameObject.name != "Boss(Clone)")
        {
            _animator.runtimeAnimatorController = _animatorMelee;
            GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.5f);
            
            if (Vector2.Distance(transform.position, _player.transform.position) < 5)
            {
                Vector3 dir = _player.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                transform.position =
                    Vector2.MoveTowards(transform.position,
                    _player.transform.position, EnemyRunningSpeed);
            }
        }
    }

    private void WarnPlayerOfAttack()
    {
        PlayerController.SoundsSource.volume = 0.3f;
        PlayerController.SoundsSource.PlayOneShot(SplashAttackSound);

        StartCoroutine(InitiateBossAttackPattern());
    }

    private IEnumerator InitiateBossAttackPattern()
    {
        yield return new WaitForSeconds(2f);
        PlayerController.SoundsSource.volume = 1f;

        Instantiate(SplashAttacks[Random.Range(0, SplashAttacks.Length)],
            transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        Instantiate(SplashAttacks[Random.Range(0, SplashAttacks.Length)],
            transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        Instantiate(SplashAttacks[Random.Range(0, SplashAttacks.Length)],
            transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
    }

    private void EnableShooting()
    {
        _canShoot = true;
    }

    private void EnablePickingDirection()
    {
        _canPickNewDirection = true;
    }

    /*private IEnumerator FlashEnemy()
    {
        for (int i = 0; i < 3; i++)
        {
            _spriteRenderer.sprite = DamagedSprite;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.sprite = DefaultSprite;
            yield return new WaitForSeconds(0.1f);
        }
    }*/

    public void DamageEnemy()
    {
        PlayerController.SoundsSource.PlayOneShot(
            EnemyHitSounds[Random.Range(0, EnemyHitSounds.Length)]);

        _animator.SetBool("isEnemyHit", true);
        EnemyHitPoints--;

        if (EnemyHitPoints <= 0) {
            StageController.EnemiesCurrentlyAlive--;
            //Debug.Log(GameObject.FindGameObjectsWithTag("Enemy").Length);
            // Once all opponents are defeated, the next wave
            // can commence (and the break between each wave).
            if (StageController.EnemiesCurrentlyAlive <= 0)
            {
                StageController.IsEnemyDefeated = true;
                StageController.CurrentWave++;

                if (StageController.CurrentWave >= 4)
                {
                    StageController.CurrentStage++;
                    StageController.CurrentWave = 1;

                    CutscenesManager.IsCutsceneOver = false;
                    CutscenesManager.CurrentDialogueIndex = 0;

                    if (StageController.CurrentStage == 2)
                    {
                        CutscenesManager.Stage = "Stage 2";
                    }
                    else if (StageController.CurrentStage == 3)
                    {
                        CutscenesManager.Stage = "Stage 3";
                    }
                    else if (StageController.CurrentStage == 4)
                    {
                        CutscenesManager.Stage = "Ending Scene";
                    }

                    GameObject.Find("Cutscenes Manager")
                    .GetComponent<CutscenesManager>().FadeIn();
                }
            }
            Destroy(gameObject);
        }

        Invoke("StopDamageAnimation", 1f);
    }

    private void StopDamageAnimation()
    {
        _animator.SetBool("isEnemyHit", false);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            _animator.SetBool("isAttacking", true);
            if (_canHitPlayer)
            {
                //PlayerController.SoundsSource.PlayOneShot(WeaponSlashSound);

                _player.GetComponent<PlayerController>().DamagePlayer();

                _canHitPlayer = false;
                Invoke("EnableHittingPlayer", 1f);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            _animator.SetBool("isAttacking", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Sword"))
        {
            DamageEnemy();
        }
    }

    private void EnableHittingPlayer()
    {
        _canHitPlayer = true;
    }
}
