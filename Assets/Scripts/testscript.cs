﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : Enemy
{
    private BulletManager manager;
    public BulletPattern pattern1;
    protected override void Start()
    {
        base.Start();
        manager = BulletManager.instance;
        Attacks.Add(attack1);
        atkPatternLen = Attacks.Count;
        current = 0;
        pattern = new int[atkPatternLen];
        PatternShuffle();
    }

    public void attack1()
    {
        float angle = Vector2.SignedAngle(Vector2.right, playerTransform.transform.position - enemyTransform.transform.position);
        manager.StartCoroutine(manager.SpawnPattern(pattern1, this.transform.position, Quaternion.Euler(0, 0, angle)));
        //manager.StartCoroutine(manager.SpawnOneShot("Enemy_Cicle",5,45,15,true,this.transform.position, angle));
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
            Vector3 to = enemyTransform.transform.position - playerTransform.transform.position;
            enemyTransform.transform.up = Vector3.Lerp(from, to, 0.05f);
        }
    }

    protected override void OnDead()
    {
        Debug.Log("The OnDead method is overrided");
        Destroy(this.gameObject);
    }

}
