using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{

    float walkingAnimationClock;
    float animationClockReset = .3f;
    bool animationPhase;
    Vector3 currentVelocity;
    public Vector3 CurrentVelocity { get => currentVelocity; set => currentVelocity = value; }

    float attackTimer;
    float attackCooldown = .6f;
    public GameObject SwordArm;
    Vector3 swordNeutralPos = new Vector3(.6f, 1.2f, .4f);
    Vector3 swordNeutralRot = new Vector3(60, 0, 0);
    Vector3 swordFwdPos = new Vector3(.4f, 1f, 1.4f);
    Vector3 swordFwdRot = new Vector3(90, -10, 0);

    float maxSpeed;


    // Start is called before the first frame update
    void Start()
    {
        maxSpeed = transform.parent.GetComponent<Player>().MaxSpeed;
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
        SwordArm.transform.localPosition = Vector3.Lerp(swordNeutralPos, swordFwdPos, p);
        SwordArm.transform.localRotation = Quaternion.Euler(Vector3.Lerp(swordNeutralRot, swordFwdRot, p));
        attackTimer -= Time.deltaTime;
        if (attackTimer < 0)
            attackTimer = 0;
    }

    private Vector3 AnimationPos(Vector3 currentVelocity)
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

    public void StartAttack() => attackTimer = attackCooldown;
}
