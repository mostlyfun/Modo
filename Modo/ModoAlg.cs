namespace Modo;

public class ModoAlg<X>
{
    // ctor
    public ModoAlg(Rect boundingRect,
                    Func<Rect, Opt<(X X, double[] Fx)>> solveForEpsilon,
                    Opt<Func<double[], double[], double>> getPriority)
    {
        BoundingRect = boundingRect;
        SolveForEpsilon = solveForEpsilon;
        GetPriority = getPriority.IsSome ? getPriority.Unwrap() : GetDefaultCalcVolume(boundingRect.Lower);
    }


    // data
    readonly Rect BoundingRect;
    readonly Func<Rect, Opt<(X X, double[] Fx)>> SolveForEpsilon;
    readonly Func<double[], double[], double> GetPriority;


    // method
    public int Dim
        => BoundingRect.Dim;
    public IEnumerable<(X x, double[] fx)> CreateNondominatedSet(double[] disturbanceRadius)
    {
        Debug.Assert(disturbanceRadius.Length >= BoundingRect.Lower.Length);
        List<Rect> T = new(), Tpr = new();
        var yN = new Dictionary<int, (X x, double[] fx)>();
        var L = new List<(Rect Rect, double Prio)> { (BoundingRect, 0) };
        while (L.Count > 0)
        {
            //Console.WriteLine(yN.Count);
            var first = L.OrderByDescending(x => x.Prio).First();
            var Li = first.Rect;
            var ui = Li.Upper;
            var maybeSoln = SolveForEpsilon(Li);
            if (maybeSoln.IsNone)
                // infeasible
                RemoveRect(L, new(BoundingRect.Lower, ui), disturbanceRadius);
            else
            {
                var (x, fx) = maybeSoln.Unwrap();
                var fxbar = fx.Take(fx.Length - 1).ToArray();
                int key = HashArray(fxbar);
                if (yN.ContainsKey(key))
                {
                    RemoveRect(L, new(fxbar, ui), disturbanceRadius);
                }
                else
                {
                    // new nondominated solution is found
                    yN.Add(key, (x, fx));
                    L = UpdateList(GetPriority, T, Tpr, L, fxbar);
                    RemoveRect(L, new(fxbar, ui), disturbanceRadius);
                }
            }
        }
        return yN.Values;
    }
    
    
    // helpers
    static List<(Rect Rect, double Prio)> UpdateList(
                                Func<double[], double[], double> GetPriority,
                                List<Rect> T, List<Rect> Tpr,
                                List<(Rect Rect, double Prio)> Lold,
                                double[] fx)
    {
        var L = new List<(Rect Rect, double Prio)>();
        foreach (var (Ri, _) in Lold)
        {
            T.Clear();
            T.Add(Ri);
            foreach (var (objInd, newObjVal) in Ri.GetIntersectingObjectiveIndex(fx))
            {
                Tpr = new List<Rect>();
                foreach (var Rt in T)
                {
                    var (lower, upper) = Rt.SplitRectangle(objInd, newObjVal);
                    Tpr.Add(lower);
                    Tpr.Add(upper);
                }
                T = Tpr;
            }
            L.AddRange(T.Select(x => (x, GetPriority(x.Lower, x.Upper))));
        }
        return L;
    }
    static void RemoveRect(List<(Rect Rect, double Prio)> L, Rect other, double[] disturbanceRadius)
    {
        for (int i = 0; i < L.Count; i++)
            if (L[i].Rect.IsSubsetOf(other))
            {
                var removedRect = L[i].Rect;
                L.RemoveAt(i);
                int decrement = 1;

                for (int i2 = 0; i2 < L.Count; i2++)
                {
                    // exclude potentially degenerate point
                    if (i != i2 && L[i2].Rect.Upper.SequenceEqual(removedRect.Lower))
                    {
                        for (int j = 0; j < L[i2].Rect.Upper.Length; j++)
                            L[i2].Rect.Upper[j] = L[i2].Rect.Upper[j] - disturbanceRadius[j];
                        for (int j = 0; j < L[i2].Rect.Upper.Length; j++)
                            if (L[i2].Rect.Upper[j] < L[i2].Rect.Lower[j])
                            {
                                if (i2 < i)
                                    decrement++;
                                L.RemoveAt(i2);
                                i2--;
                                break;
                            }
                    }
                }
                i -= decrement;
            }
    }
    static int HashArray(double[] arr)
        => ((IStructuralEquatable)arr).GetHashCode(EqualityComparer<double>.Default);
    // defaults
    static double CalcVolume(double[] lower, double[] upper)
    {
        Debug.Assert(lower.Length == upper.Length, nameof(CalcVolume));
        double vol = 1;
        for (int j = 0; j < lower.Length; j++)
            vol *= (upper[j] - lower[j]);
        Debug.Assert(vol >= 0);
        return vol;
    }
    static Func<double[], double[], double> GetDefaultCalcVolume(double[] boundingRectLower)
        => (_, upper) => CalcVolume(boundingRectLower, upper);
}
