using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
}

