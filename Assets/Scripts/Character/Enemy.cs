using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    public override void Attack()
    {

    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(60);
        }
    }
}
