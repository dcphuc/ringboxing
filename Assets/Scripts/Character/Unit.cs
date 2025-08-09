using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;
using System;

public class Unit : MonoBehaviour
{
    [SerializeField] private float health;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private string _attackTag = null;
    private NavMeshAgent agent;
    private Animator animator;
    private WalkArea area;
    private CharacterState state = CharacterState.Idle;
    private int maxAlliesPerEnemy;
    private float startLives;
    private float defaultStoppingDistance;
    private bool dead;
    private float inStateTimeCounter = 0f;

    [HideInInspector]
    public bool Spread;

    [HideInInspector]
    public Transform CurrentTarget;

    private void Start()
    {
        //if this enemy, don't use the spread option
        if (gameObject.tag == Defines.ENEMY_TAG)
            Spread = false;

        maxAlliesPerEnemy = Defines.MAX_ALLIES_PER_ENEMY;

        agent = GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponent<Animator>();

        startLives = health;

        //get default stopping distance
        defaultStoppingDistance = agent.stoppingDistance;

        //find the area so the character can walk around
        area = GameObject.FindObjectOfType<WalkArea>();

        GameManager.Instance.OnMovement += MovementByJoystick;
    }

    private void FixedUpdate()
    {
        // if (CurrentTarget == null && GameObject.FindGameObjectsWithTag(_attackTag).Length > 0)
        //     CurrentTarget = FindCurrentTarget();

        // if (health < 1 && !dead)
        // {
        //     dead = true;
        //     UpdateState(CharacterState.Dead);
        // }

        // if (CurrentTarget != null)
        // {
        //     if (Vector3.Distance(CurrentTarget.position, transform.position) <= agent.stoppingDistance)
        //     {
        //         UpdateState(CharacterState.Attacking);
        //         Vector3 currentTargetPosition = CurrentTarget.position;
        //         currentTargetPosition.y = transform.position.y;
        //         transform.LookAt(currentTargetPosition);
        //     }
        //     else
        //     {
        //         UpdateState(CharacterState.Walking);
        //         if (agent.stoppingDistance != defaultStoppingDistance)
        //             agent.stoppingDistance = defaultStoppingDistance;

        //         agent.isStopped = false;
        //         agent.destination = CurrentTarget.position;
        //     }
        // }
    }

    private void MovementByJoystick(Vector2 amount)
    {
        var formatAmount = RotateVector2WithCamera(amount);
        Vector3 pos = transform.position;
        pos.x += (formatAmount.x * speed * Time.fixedDeltaTime);
        pos.z += (formatAmount.y * speed * Time.fixedDeltaTime);
        transform.position = pos;
    }

    Vector2 RotateVector2WithCamera(Vector2 vector)
    {
        float cameraYRotation = Camera.main.transform.eulerAngles.y;

        float angleInRadians = cameraYRotation * Mathf.Deg2Rad;
        float angle = Mathf.Atan2(vector.y, vector.x) - Mathf.Abs(angleInRadians);
        float radius = vector.magnitude;
        return new Vector2(
            radius * Mathf.Cos(angle),
            radius * Mathf.Sin(angle)
        );
    }

    private void UpdateState(CharacterState newState)
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
        //find all potential targets (enemies of this character)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(_attackTag);
        Transform target = null;

        //if we want this character to communicate with his allies
        if (Spread)
        {
            //get all enemies
            List<GameObject> availableEnemies = enemies.ToList();
            int count = 0;

            //make sure it doesn't get stuck in an infinite loop
            while (count < 300)
            {
                //for all enemies
                for (int i = 0; i < enemies.Length; i++)
                {
                    //distance between character and its nearest enemy
                    float closestDistance = Mathf.Infinity;

                    foreach (GameObject potentialTarget in availableEnemies)
                    {
                        //check if there are enemies left to attack and check per enemy if its closest to this character
                        if (Vector3.Distance(transform.position, potentialTarget.transform.position) < closestDistance && potentialTarget != null)
                        {
                            //if this enemy is closest to character, set closest distance to distance between character and enemy
                            closestDistance = Vector3.Distance(transform.position, potentialTarget.transform.position);
                            target = potentialTarget.transform;
                        }
                    }

                    //if it is valid, return this target
                    if (target && CanAttack(target))
                    {
                        return target;
                    }
                    else
                    {
                        //if it's not, remove it from the list and try again
                        availableEnemies.Remove(target.gameObject);
                    }
                }

                //after checking all enemies, allow one more ally to also attack the same enemy and try again
                maxAlliesPerEnemy++;
                availableEnemies.Clear();
                availableEnemies = enemies.ToList();

                count++;
            }

            //show a loop error
            Debug.LogError("Infinite loop");
        }
        else
        {
            //if we're using the simple method:
            float closestDistance = Mathf.Infinity;

            foreach (GameObject potentialTarget in enemies)
            {
                //check if there are enemies left to attack and check per enemy if its closest to this character
                if (Vector3.Distance(transform.position, potentialTarget.transform.position) < closestDistance && potentialTarget != null)
                {
                    //if this enemy is closest to character, set closest distance to distance between character and enemy
                    closestDistance = Vector3.Distance(transform.position, potentialTarget.transform.position);
                    target = potentialTarget.transform;
                }
            }

            //check if there's a target and return it
            if (target)
                return target;
        }

        //otherwise return null
        return null;
    }


    //check if there's not too much allies attacking this same enemy already
    public bool CanAttack(Transform target)
    {
        //get the number of allies that are already attacking this enemy
        int numberOfUnitsAttackingThisEnemy = 0;

        //foreach ally that's attacking the same enemy, increase the number of allies
        foreach (GameObject ally in GameObject.FindGameObjectsWithTag(gameObject.tag))
        {
            if (ally.GetComponent<Unit>().CurrentTarget == target)
                numberOfUnitsAttackingThisEnemy++;
        }

        //check if we may attack this target
        if (numberOfUnitsAttackingThisEnemy < maxAlliesPerEnemy)
            return true;

        //return false if there's too much allies attacking this enemy already
        return false;
    }

    private void UpdateAttack(float dt)
    {

    }
}
