using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Fighter
{
    int hp;
    int attack;
    int defenceSkill;
    int armor;
    int shield;

    bool isAlive = true;
    public bool IsAlive { get => isAlive;}
    public int Attack { get => attack; }

    public Fighter(int hp, int attack, int defenceSkill, int armor, int shield)
    {
        this.hp = hp;
        this.attack = attack;
        this.defenceSkill = defenceSkill;
        this.armor = armor;
        this.shield = shield;
    }
    
    internal void GetHit(int attackRating, bool armorPiercing, bool evasive)
    {
        int defence = defenceSkill + (armorPiercing ? 0 : armor) + (evasive ? 0 : shield);
        int defenceRoll = UnityEngine.Random.Range(0, defence) + 1;
        int attackRoll = UnityEngine.Random.Range(0, attackRating) + 1;
        if (attackRoll > defenceRoll)
        {
            UnityEngine.Debug.Log("Successfull hit");
            hp--;
            if (hp <= 0)
                isAlive = false;
        } else UnityEngine.Debug.Log("Deflection");
    }
}
