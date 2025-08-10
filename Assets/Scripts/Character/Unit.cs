using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;
using System;

public abstract class Unit : MonoBehaviour
{
    [SerializeField] protected float health = 100f;
    [SerializeField] protected float attackDamage = 50f;
    [SerializeField] protected float attackSpeed = 1f;
    [SerializeField] protected float movementSpeed = 1f;
    [SerializeField] protected float rotationSpeed = 10f;
    [SerializeField] private string attackTag = null;
    [SerializeField] private float hitTextOffsetY = 1f;
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterState state = CharacterState.Idle;
    private int maxAlliesPerEnemy;
    private float startLives;
    private float defaultStoppingDistance;
    private bool dead;
    private float inStateTimeCounter = 0f;
    private float lastFloatingTextTime = 0f;
    private bool isControlManual = false;

    [HideInInspector]
    public bool Spread;

    [HideInInspector]
    public Transform CurrentTarget;

    private void Start()
    {
        if (gameObject.tag == Defines.ENEMY_TAG)
            Spread = false;

        maxAlliesPerEnemy = Defines.MAX_ALLIES_PER_ENEMY;
        startLives = health;
        defaultStoppingDistance = agent.stoppingDistance;

        agent = GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (CurrentTarget == null && GameObject.FindGameObjectsWithTag(attackTag).Length > 0)
            CurrentTarget = FindCurrentTarget();

        if (CurrentTarget != null)
        {
            if (!isControlManual)
            {
                if (Vector3.Distance(CurrentTarget.position, transform.position) <= agent.stoppingDistance)
                {
                    UpdateState(CharacterState.Attacking);
                    Vector3 currentTargetPosition = CurrentTarget.position;
                    currentTargetPosition.y = transform.position.y;
                    transform.LookAt(currentTargetPosition);

                }
                else
                {
                    UpdateState(CharacterState.Walking);
                    if (agent.stoppingDistance != defaultStoppingDistance)
                        agent.stoppingDistance = defaultStoppingDistance;

                    agent.isStopped = false;
                    agent.destination = CurrentTarget.position;
                }
            }
        }
    }

    public void UpdateState(CharacterState newState)
    {
        if (this.state == newState)
        {
            return;
        }
        this.state = newState;
        switch (newState)
        {
            case CharacterState.Idle:

                break;
            case CharacterState.Walking:
                animator.SetBool(AnimationVariables.IsWalking.ToString(), true);
                break;
            case CharacterState.Attacking:
                animator.SetBool(AnimationVariables.IsWalking.ToString(), false);
                animator.SetBool(AnimationVariables.IsStomachPunch.ToString(), true);
                break;
            case CharacterState.Dead:
                animator.SetBool(AnimationVariables.IsKnockedOut.ToString(), true);
                break;
        }
    }

    private Transform FindCurrentTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(attackTag);
        Transform target = null;

        if (Spread)
        {
            List<GameObject> availableEnemies = enemies.ToList();
            int count = 0;

            while (count < 300)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    float closestDistance = Mathf.Infinity;

                    foreach (GameObject potentialTarget in availableEnemies)
                    {
                        if (Vector3.Distance(transform.position, potentialTarget.transform.position) < closestDistance && potentialTarget != null)
                        {
                            closestDistance = Vector3.Distance(transform.position, potentialTarget.transform.position);
                            target = potentialTarget.transform;
                        }
                    }

                    if (target && CanAttack(target))
                    {
                        return target;
                    }
                    else
                    {
                        availableEnemies.Remove(target.gameObject);
                    }
                }

                maxAlliesPerEnemy++;
                availableEnemies.Clear();
                availableEnemies = enemies.ToList();

                count++;
            }

            Debug.LogError("Infinite loop");
        }
        else
        {
            float closestDistance = Mathf.Infinity;

            foreach (GameObject potentialTarget in enemies)
            {
                if (Vector3.Distance(transform.position, potentialTarget.transform.position) < closestDistance && potentialTarget != null)
                {
                    closestDistance = Vector3.Distance(transform.position, potentialTarget.transform.position);
                    target = potentialTarget.transform;
                }
            }

            if (target)
                return target;
        }

        return null;
    }

    private bool CanAttack(Transform target)
    {
        int numberOfUnitsAttackingThisEnemy = 0;

        foreach (GameObject ally in GameObject.FindGameObjectsWithTag(gameObject.tag))
        {
            if (ally.GetComponent<Unit>().CurrentTarget == target)
                numberOfUnitsAttackingThisEnemy++;
        }

        if (numberOfUnitsAttackingThisEnemy < maxAlliesPerEnemy)
            return true;

        return false;
    }

    public abstract void Attack();

    public virtual void TakeDamage(float damage)
    {
        // health -= damage;
        // if (health <= 0)
        // {
        //     Die();
        // }

        if (lastFloatingTextTime + 0.18f <= Time.realtimeSinceStartup)
        {
            lastFloatingTextTime = Time.realtimeSinceStartup;
            FloatingTextController.Instance.SpawnFloatingText("-" + damage.ToString("F0"),
                transform.position + Vector3.up * hitTextOffsetY + new Vector3(0, UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.3f, 0.3f)),
                Quaternion.identity, 1f);
        }
    }

    private void Die()
    {
        dead = true;
        UpdateState(CharacterState.Dead);
        agent.isStopped = true;
        agent.ResetPath();
    }
}
