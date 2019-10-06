using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAnimator : MonoBehaviour
{

    float walkingAnimationClock;
    float animationClockReset = .4f;
    bool animationPhase;
    Vector3 currentVelocity;
    public Vector3 CurrentVelocity { get => currentVelocity; set => currentVelocity = value; }

    float attackTimer;
    float attackCooldown = .6f;
    public GameObject body;
    Vector3 NeutralPos = new Vector3(0, .6f, 0);
    Vector3 NeutralRot = new Vector3(90, 0, 0);
    Vector3 FwdPos = new Vector3(0, 1f, 2f);
    Vector3 FwdRot = new Vector3(120, 0, 0);

    float maxSpeed;


    // Start is called before the first frame update
    void Start()
    {
        maxSpeed = GetComponentInParent<Dog>().MaxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        walkingAnimationClock += Time.deltaTime;
        if (walkingAnimationClock > animationClockReset)
        {
            walkingAnimationClock -= animationClockReset;
            animationPhase = !animationPhase;
        }
        transform.localPosition = AnimationPos(currentVelocity);

        float p = Mathf.Sin(Mathf.Pow(attackTimer / attackCooldown, 2) * Mathf.PI);
        body.transform.localPosition = Vector3.Lerp(NeutralPos, FwdPos, p);
        body.transform.localRotation = Quaternion.Euler(Vector3.Lerp(NeutralRot, FwdRot, p));
        attackTimer -= Time.deltaTime;
        if (attackTimer < 0)
            attackTimer = 0;
    }
    
    private Vector3 AnimationPos(Vector3 currentVelocity)
    {
        float x, y;
        float bounceHeight = .15f * currentVelocity.magnitude / maxSpeed;
        float px = Mathf.Cos(walkingAnimationClock / (animationClockReset * 0.7f) * Mathf.PI);
        float py = Mathf.Sin(walkingAnimationClock / (animationClockReset * 0.7f) * Mathf.PI);
        y = py > 0 ? py * bounceHeight : 0;
        x = py > 0 ? px : -1;
        x *= bounceHeight / 2;
        return new Vector3(animationPhase ? x : -x, y, 0);
    }

    public void StartAttack() => attackTimer = attackCooldown;
}
