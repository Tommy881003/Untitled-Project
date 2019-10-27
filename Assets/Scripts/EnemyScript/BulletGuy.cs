﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BulletGuy : Enemy
{
    protected float rotateSeed;
    protected float speedSeed;
    protected float originalSpeed;
    protected Color originalColor;
    protected bool hitTarget = false;
    protected bool endChasing = false;
    private BulletManager manager;
    public BulletPattern pattern1;
    private ParticleSystem shockwave;
    protected override void Start()
    {
        base.Start();
        manager = BulletManager.instance;
        Attacks.Add(attack1);
        atkPatternLen = Attacks.Count;
        shockwave = enemyTransform.Find("Main").gameObject.GetComponent<ParticleSystem>();
        sr = enemyTransform.Find("Main").gameObject.GetComponent<SpriteRenderer>();
        speedSeed = UnityEngine.Random.Range(0.8f, 1.2f);
        rotateSeed = UnityEngine.Random.Range(0.8f, 1.2f);
        originalColor = sr.color;
        originalSpeed = speed * speedSeed;
        current = 0;
        pattern = new int[atkPatternLen];
        PatternShuffle();
    }

    public void attack1()
    {
        StartCoroutine(forAttack1());
    }

    IEnumerator forAttack1()
    {
        float chaseTime = 0;
        isAttacking = true;
        sr.DOColor(Color.red, 2.5f);
        while(chaseTime <= 2.5f && hitTarget == false)
        {
            speed = originalSpeed * 2;
            Vector2 ray = playerTransform.transform.position - enemyTransform.transform.position;
            chaseTime += Time.deltaTime;
            yield return null;
        }
        if (chaseTime <= 2.5f)
        {
            sr.DOKill();
            sr.color = originalColor;
        }
        speed = originalSpeed;
        endChasing = true;
        rb.velocity = Vector2.zero;
        if(hitTarget == false)
        {
            sr.color = Color.white;
            sr.DOColor(Color.red, 0.5f);
            enemyTransform.DOShakePosition(0.5f,0.5f);
            float toAngle = Vector2.SignedAngle(Vector2.right, playerTransform.transform.position - enemyTransform.transform.position);
            enemyTransform.DORotateQuaternion(Quaternion.Euler(0, 0, toAngle - 90), 0.5f);
            yield return new WaitForSeconds(0.5f);
            rb.velocity = originalSpeed * 4 * new Vector2(Mathf.Cos(toAngle * Mathf.Deg2Rad), Mathf.Sin(toAngle * Mathf.Deg2Rad));
            while (hitTarget == false)
                yield return null;
        }
        sr.color = originalColor;
        hitTarget = false;
        isAttacking = false;
        endChasing = false;
        yield break;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);
            hp -= 10;
            if (isAttacking)
            {
                hitTarget = true;
                shockwave.Play();
            }
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            if (isAttacking)
            {
                hitTarget = true;
                shockwave.Play();
            }
        }
        else if (collision.gameObject.CompareTag("Wall") && isAttacking)
        {
            hitTarget = true;
            shockwave.Play();
            manager.StartCoroutine(manager.SpawnPattern(pattern1, this.transform.position, Quaternion.identity));
        }
    }

    protected override bool CanFollow()
    {
        if(endChasing)
        {
            if (destination.isActiveAndEnabled)
                destination.enabled = false;
            if (iPath.isActiveAndEnabled)
                iPath.enabled = false;
        }
        else
        {
            destination.enabled = true;
            iPath.enabled = true;
        }
        return !endChasing;
    }

    protected override void Follow()
    {
        RaycastHit2D raycast = Physics2D.CircleCast(enemyTransform.position, 1f, playerTransform.position - enemyTransform.position, float.PositiveInfinity, 1 << 8 | 1 << 10);
        if (raycast.collider.gameObject.layer != 10)
            base.Follow();
        else
        {
            if (destination.isActiveAndEnabled)
            {
                iPath.enabled = false;
                destination.enabled = false;
            }
            rb.velocity = (playerTransform.position - enemyTransform.position).normalized * speed;
            Vector3 from = enemyTransform.transform.up;
            Vector3 to = playerTransform.transform.position - enemyTransform.transform.position;
            enemyTransform.transform.up = Vector3.Lerp(from, to, 0.1f * rotateSeed);
        }
    }
}
