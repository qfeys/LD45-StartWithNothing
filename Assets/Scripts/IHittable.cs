internal interface IHittable
{
    void GetHit(int attackRating, bool ArmorPiercing = false, bool evasive = false);
}