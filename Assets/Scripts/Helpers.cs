using UnityEngine;


static class Helpers
{
    static public float CircularDifference(float a, float b, float modulo)
    {
        float diff = (a % modulo) - (b % modulo);
        if (diff < -modulo / 2)
            return diff + modulo;
        else if (diff > modulo / 2)
            return diff - modulo;
        else
            return diff;
    }

    const float goldenRatio = 1.6180f;

    static public Vector2 PlaceInSpiral(int index, float distance)
    {
        float angleIncrement = Mathf.PI * 2 * goldenRatio;
        float radius = (float)index * distance;
        float angle = angleIncrement * index;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}

