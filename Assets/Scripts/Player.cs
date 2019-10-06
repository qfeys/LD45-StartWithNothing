using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    float maxSpeed = 5;
    float acceleration = 30;
    float rotSpeed = 120; // deg per second
    float reach = 4;

    Vector3 currentVelocity = Vector3.zero;
    float walkingAnimationClock;
    float animationClockReset = .3f;
    bool animationPhase;

    public new CameraScript camera;
    public GameObject AttackPoint;

    Fighter fighter;

    // Start is called before the first frame update
    void Start()
    {
        fighter = new Fighter(10, 10, 10, 2, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Movement
        float direction = camera.GetDirection();
        float dirDiff = Helpers.CircularDifference(transform.rotation.eulerAngles.y, direction, 360);
        if (Mathf.Abs(dirDiff) > 30 || (Mathf.Abs(dirDiff) > 2 && currentVelocity.magnitude > maxSpeed / 3))
        {
            float rotAmount = (dirDiff > 0 ? -1 : 1) * rotSpeed * Time.fixedDeltaTime;
            rotAmount = dirDiff > 0 ? Mathf.Min(rotAmount, dirDiff) : Mathf.Max(rotAmount, dirDiff);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotAmount, 0));
        }


        if (Input.GetButton("Horizontal"))
        {
            float x = currentVelocity.x + acceleration * Input.GetAxis("Horizontal") * Time.fixedDeltaTime;
            currentVelocity.x = Mathf.Clamp(x, -maxSpeed - .8f, maxSpeed * .8f);
        } else
        {
            float x = currentVelocity.x;
            x += acceleration * Time.fixedDeltaTime * (x == 0 ? 0 : x > 0 ? -.5f : .5f);
            currentVelocity.x = Mathf.Clamp(x, -maxSpeed, maxSpeed);
        }
        if (Input.GetButton("Vertical"))
        {
            float z = currentVelocity.z + acceleration * Input.GetAxis("Vertical") * Time.fixedDeltaTime;
            currentVelocity.z = Mathf.Clamp(z, -maxSpeed / 2, maxSpeed);
        } else
        {
            float z = currentVelocity.z;
            z += acceleration * Time.fixedDeltaTime * (z == 0 ? 0 : z > 0 ? -.5f : 1);
            currentVelocity.z = Mathf.Clamp(z, -maxSpeed, maxSpeed);
        }

        if (currentVelocity.magnitude > 0.1 * maxSpeed)
        {
            walkingAnimationClock += Time.fixedDeltaTime;
            if (walkingAnimationClock > animationClockReset)
            {
                walkingAnimationClock -= animationClockReset;
                animationPhase = !animationPhase;
            }
        } else
            walkingAnimationClock = 0;

        transform.position += transform.rotation * currentVelocity * Time.fixedDeltaTime;
        transform.GetChild(0).localPosition = AnimationPos();

        // Attacking

        if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = new Ray(AttackPoint.transform.position, AttackPoint.transform.forward);
            int mask = LayerMask.GetMask("Enemies");
            RaycastHit[] hits = Physics.RaycastAll(ray, reach, mask);
            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 10);
            Debug.Log("Raycasting");
            if (hits.Length !=0)
            {
                IHittable enemy = hits[0].collider.GetComponentInParent<IHittable>();
                if (enemy != null)
                {
                    Debug.Log("Enemy type: " + enemy.GetType());
                    enemy.GetHit(fighter.Attack);
                }

            }

        }
    }

    private Vector3 AnimationPos()
    {
        float x, y;
        float bounceHeight = .1f * currentVelocity.magnitude / maxSpeed;
        float px = Mathf.Cos(walkingAnimationClock / (animationClockReset * 0.7f) * Mathf.PI);
        float py = Mathf.Sin(walkingAnimationClock / (animationClockReset * 0.7f) * Mathf.PI);
        y = py > 0 ? py * bounceHeight : 0;
        x = py > 0 ? px : -1;
        x *= bounceHeight / 2;
        return new Vector3(animationPhase ? x : -x, y, 0);
    }
}
