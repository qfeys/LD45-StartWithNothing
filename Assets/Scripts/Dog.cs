using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour, IHittable
{
    Fighter fighter;
    FiniteStateMachine<DogState> stateMachine;
    private enum DogState { Wandering, Searching, Attacking}

    Vector3 currentVelocity = Vector3.zero;
    private float maxSpeed = 8;
    private float minRotSpeed = 60;
    private float maxRotSpeed = 180;
    private float acceleration = 20;
    public float MaxSpeed { get => maxSpeed; }

    float reach = 3;


    Vector3 NextWanderPoint;
    float wanderTimer;
    float WanderCooldown = 8;

    float searchTimer;
    float searchCooldown = 1.2f;
    float nextSearchDirection;


    Transform attackTarget;
    float attackTimer;
    float attackCooldown = 1.5f;

    public DogAnimator animator;

    // Start is called before the first frame update
    void Start()
    {
        fighter = new Fighter(1, 5, 5, 1, 0);
        stateMachine = InitFSM();
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Tick(Time.deltaTime);
        transform.position += transform.rotation * currentVelocity * Time.fixedDeltaTime;
        animator.CurrentVelocity = currentVelocity;
    }

    private FiniteStateMachine<DogState> InitFSM()
    {
        FiniteStateMachine<DogState>.StateAction[] stateActions = new FiniteStateMachine<DogState>.StateAction[3];
        stateActions[(int)DogState.Wandering] = (handle, dt) =>
        {
            Vector3 movementDelta = TurnTowardTarget(NextWanderPoint);
            if (movementDelta.sqrMagnitude > 1)
            {
                float z = currentVelocity.z + acceleration * dt;
                currentVelocity.z = Mathf.Clamp(z, -maxSpeed, maxSpeed);
            } else if (movementDelta.sqrMagnitude < .3f)
                handle.ChangeState(DogState.Searching);
            else
            {
                float z = currentVelocity.z;
                currentVelocity.z = z + acceleration * Time.fixedDeltaTime * Mathf.Clamp(-z / maxSpeed * 2, -.5f, .5f);
            }

            wanderTimer += dt;
            if (wanderTimer > WanderCooldown)
            {
                wanderTimer = 0;
                handle.ChangeState(DogState.Searching);
            }
        };
        stateActions[(int)DogState.Searching] = (handle, dt) =>
        {
            // Slow down
            float z = currentVelocity.z;
            currentVelocity.z = z + acceleration * Time.fixedDeltaTime * Mathf.Clamp(-z / maxSpeed * 2, -1f, 1f);

            searchTimer += dt;
            if (searchTimer > searchCooldown)
            {
                searchTimer = 0;
                GameObject scanResult = ScanForward();
                if (scanResult != null)
                {
                    attackTarget = scanResult.transform;
                    handle.ChangeState(DogState.Attacking);
                }
                if (UnityEngine.Random.value > .8f)
                    handle.ChangeState(DogState.Wandering);
                nextSearchDirection = UnityEngine.Random.Range(0f, 360);
            }
            TurnTowardTarget(nextSearchDirection);
        };
        stateActions[(int)DogState.Attacking] = (handle, dt) =>
        {
            Vector3 movementDelta = TurnTowardTarget(attackTarget.position);
            float distance = movementDelta.magnitude;
            if (distance > reach*2)
            {
                float z = currentVelocity.z + acceleration * dt;
                currentVelocity.z = Mathf.Clamp(z, -maxSpeed, maxSpeed);
            } else
            {
                float targetDist = reach * .75f;
                float targetDistDiff = distance - targetDist; // positive is to far away, negative too close.
                float targetDistDiffValue = targetDistDiff / targetDist;
                float targetVel = Mathf.Lerp(-1, 1, targetDistDiffValue / 2 + .5f) * maxSpeed;

                float z = currentVelocity.z;
                float targetVelDiff = z - targetVel; // positive is too fast, negative too slow
                currentVelocity.z = z + acceleration * Time.fixedDeltaTime * Mathf.Clamp(-targetVelDiff / maxSpeed * 2, -1, 1);
            } 
            if(distance < reach)
            {
                // Attack
                attackTimer -= dt;
                if(attackTimer < 0)
                {
                    Attack();
                    attackTimer = attackCooldown;
                    animator.StartAttack();
                    Debug.Log("Attack");
                }
            }
        };
        FiniteStateMachine<DogState>.TransientAction[] openingActions = new FiniteStateMachine<DogState>.TransientAction[3];
        openingActions[(int)DogState.Wandering] = (handle) =>
        {
            NextWanderPoint = FindNewWanderPoint();
            wanderTimer = 0;
        };
        openingActions[(int)DogState.Searching] = (handle) =>
        {
            searchTimer = searchCooldown / 2;
            nextSearchDirection = -transform.rotation.eulerAngles.y + 90;
        };
        openingActions[(int)DogState.Attacking] = (handle) =>
        {

        };
        FiniteStateMachine<DogState>.TransientAction[] closingActions = new FiniteStateMachine<DogState>.TransientAction[3];
        closingActions[(int)DogState.Wandering] = (handle) =>
        {

        };
        closingActions[(int)DogState.Searching] = (handle) =>
        {

        };
        closingActions[(int)DogState.Attacking] = (handle) =>
        {

        };
        return new FiniteStateMachine<DogState>(stateActions, openingActions, closingActions, DogState.Searching);
    }

    private Vector3 TurnTowardTarget(Vector3 target)
    {
        Vector3 movementDelta = target - transform.position;
        float direction = Mathf.Atan2(movementDelta.z, movementDelta.x) * Mathf.Rad2Deg;
        TurnTowardTarget(direction);
        return movementDelta;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction">In angles</param>
    private void TurnTowardTarget(float direction)
    {
        float dirDiff = Helpers.CircularDifference(-transform.rotation.eulerAngles.y + 90, direction, 360);
        if (Mathf.Abs(dirDiff) > 2)
        {
            float rotSpeed = Mathf.Lerp(maxRotSpeed, minRotSpeed, currentVelocity.magnitude / maxSpeed);
            float rotAmount = (dirDiff > 0 ? -1 : 1) * rotSpeed * Time.fixedDeltaTime;
            rotAmount = dirDiff > 0 ? Mathf.Min(rotAmount, dirDiff) : Mathf.Max(rotAmount, dirDiff);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, -rotAmount, 0));
        }
    }

        private Vector3 FindNewWanderPoint()
    {
        int i = 0;
        while(true){
            float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            float distance = UnityEngine.Random.Range(Mathf.Clamp(5 - i / 2f, 0, float.MaxValue), 40 - i);
            Ray ray = new Ray(transform.position + Vector3.up, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.green, 5);
            if (Physics.Raycast(ray, distance) == false) // Nothing was hit
            {
                return transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
            }
            i++;
            if (i > 35)
                return transform.position;
        }
    }

    private GameObject ScanForward()
    {
        float[] headings = new float[17] { 0, -.5f, .5f, -1, 1, -1.5f, 1.5f, -2, 2, -2.5f, 2.5f, -3, 3, -5, 5, -7, 7 };
        float ownHeading = (-transform.rotation.eulerAngles.y + 90) * Mathf.Deg2Rad;
        for (int i = 0; i < headings.Length; i++)
        {
            RaycastHit hit = new RaycastHit();
            float heading = ownHeading + headings[i] / 8;
            Ray ray = new Ray(transform.position + new Vector3(0, 1, 0), new Vector3(Mathf.Cos(heading), 0, Mathf.Sin(heading)));
            Debug.DrawRay(ray.origin, ray.direction * 60, Color.red, 5);
            if (Physics.Raycast(ray, out hit, 60, LayerMask.GetMask("Allies")))
                return hit.collider.gameObject;
        }
        return null;
    }

    private void Attack()
    {
        Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
        int mask = LayerMask.GetMask("Allies");
        RaycastHit[] hits = Physics.RaycastAll(ray, reach, mask);
        Debug.DrawRay(ray.origin, ray.direction * reach, Color.blue, 10);
        if (hits.Length != 0)
        {
            IHittable enemy = hits[0].collider.GetComponentInParent<IHittable>();
            if (enemy != null)
            {
                enemy.GetHit(fighter.Attack);
            }

        }
    }

    public void GetHit(int attackRating, bool ArmorPiercing = false, bool evasive = false)
    {
        fighter.GetHit(attackRating, ArmorPiercing, evasive);
        if (fighter.IsAlive == false)
            Destroy(gameObject);
    }
}
