
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoidEngine;

public static class Lerp
{
    public static float Towards(float a, float b, float speed, float deltaTime)
    {
        float distance = b - a;
        float stepDistance = speed * deltaTime;

        if (stepDistance >= MathF.Abs(distance))
        {
            return b;
        }

        return a + MathF.Sign(distance) * stepDistance;
    }
    public static double Towards(double a, double b, float speed, float deltaTime)
    {
        double distance = b - a;
        float stepDistance = speed * deltaTime;

        if (stepDistance >= Math.Abs(distance))
        {
            return b;
        }

        return a + Math.Sign(distance) * stepDistance;
    }
    public static double TowardsUnclipped(double a, double b, float speed)
    {
        return a + (b - a) * speed;
    }
    public static Vector2 Towards(Vector2 a, Vector2 b, float speed, float deltaTime)
    {
        Vector2 distance = b - a;
        float stepDistance = speed * deltaTime;

        if (stepDistance >= distance.Length())
        {
            return b;
        }

        distance.Normalize();
        return a + distance * stepDistance;
    }
    public static float TowardsSmooth(float a, float b, float decay, float deltaTime) => b + (a - b) * MathF.Exp(-decay*deltaTime);
    public static Vector2 TowardsSmooth(Vector2 a, Vector2 b, float decay, float deltaTime) => b + (a - b) * MathF.Exp(-decay*deltaTime);
}
