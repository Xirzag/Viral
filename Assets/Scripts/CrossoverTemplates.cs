using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Chromosome = System.Collections.Generic.List<float>;

public class CrossoverTemplates
{
    
    public static Chromosome None(Chromosome a, Chromosome b)
    {
        return a;
    }

    public static Chromosome InAPoint(Chromosome a, Chromosome b)
    {
        Chromosome child = new Chromosome();
        int point = Random.Range(0, a.Count);
        for(int i = 0; i < a.Count; i++)
        {
            child.Add( point < i? a[i] : b[i]);
        }

        return child;
    }

    public static Chromosome Uniform(Chromosome a, Chromosome b)
    {
        Chromosome child = new Chromosome();
        for (int i = 0; i < a.Count; i++)
        {
            child.Add(Random.value < .5f ? a[i] : b[i]);
        }

        return child;
    }

    public static Chromosome Plain(Chromosome a, Chromosome b)
    {
        Chromosome child = new Chromosome();
        for (int i = 0; i < a.Count; i++)
        {
            child.Add(Random.Range(a[i], b[i]));
        }

        return child;
    }

    public static Chromosome Lineal(Chromosome a, Chromosome b)
    {
        Chromosome child = new Chromosome();
        for (int i = 0; i < a.Count; i++)
        {
            child.Add( (a[i] + b[i]) /2.0f);
        }

        return child;
    }

    public static Chromosome LinealMulti(Chromosome a, Chromosome b)
    {
        Chromosome child = new Chromosome();
        for (int i = 0; i < a.Count; i++)
        {
            int r = Random.Range(0, 3);
            if(r == 0)       child.Add((a[i] + b[i]) / 2.0f);
            else if (r == 1) child.Add(1.5f * a[i] - 0.5f * b[i]);
            else/* r == 2 */ child.Add(1.5f * b[i] - 0.5f * a[i]);
        }

        return child;
    }

    public static Chromosome Combinated50(Chromosome a, Chromosome b)
    {
        return Combinated(a, b, .5f);
    }

    public static Chromosome Combinated25(Chromosome a, Chromosome b)
    {
        return Combinated(a, b, .25f);
    }

    public static Chromosome Combinated10(Chromosome a, Chromosome b)
    {
        return Combinated(a, b, .10f);
    }

    public static Chromosome CombinatedMinus10(Chromosome a, Chromosome b)
    {
        return Combinated(a, b, -.10f);
    }

    public static Chromosome Combinated(Chromosome a, Chromosome b, float frac)
    {
        Chromosome child = new Chromosome();
        for (int i = 0; i < a.Count; i++)
        {
            float margin = Mathf.Abs(a[i] - b[i]) * frac;
            float max = Mathf.Max(a[i], b[i]) + margin;
            float min = Mathf.Min(a[i], b[i]) - margin;
            child.Add(Random.Range(min, max));
        }

        return child;
    }

}
