using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour, IHittable
{
    Fighter fighter;

    // Start is called before the first frame update
    void Start()
    {
        fighter = new Fighter(1, 5, 5, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GetHit(int attackRating, bool ArmorPiercing = false, bool evasive = false)
    {
        fighter.GetHit(attackRating, ArmorPiercing, evasive);
        if (fighter.IsAlive == false)
            Destroy(gameObject);
    }
}
