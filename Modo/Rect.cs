namespace Modo;

public readonly struct Rect : IEquatable<Rect>
{
    // ctor
    public Rect(double[] lowerVertex, double[] upperVertex)
    {
        Debug.Assert(lowerVertex.Length == upperVertex.Length, nameof(Rect));
        for (int i = 0; i < lowerVertex.Length; i++)
            Debug.Assert(lowerVertex[i] <= upperVertex[i]);
        Lower = lowerVertex;
        Upper = upperVertex;
    }


    // data
    public readonly double[] Lower;
    public readonly double[] Upper;


    // method
    public int Dim
        => Lower.Length;
    public (double[] lower, double[] upper) CloneVectors()
    {
        double[] lower = new double[Lower.Length], upper = new double[Upper.Length];
        Array.Copy(Lower, lower, lower.Length);
        Array.Copy(Upper, upper, upper.Length);
        return (lower, upper);
    }
    public IEnumerable<(int ObjInd, double NewObjVal)> GetIntersectingObjectiveIndex(double[] newNondominatedSoln)
    {
        Debug.Assert(newNondominatedSoln.Length == Dim, nameof(GetIntersectingObjectiveIndex));
        for (int j = 0; j < newNondominatedSoln.Length; j++)
        {
            double newObjVal = newNondominatedSoln[j];
            if (Lower[j] < newObjVal && newObjVal < Upper[j])
                yield return (j, newObjVal);
        }
    }
    public (Rect Lower, Rect Upper) SplitRectangle(int objInd, double newObjVal)
    {
        Debug.Assert(objInd < Dim, nameof(SplitRectangle));

        var (lowerLower, lowerUpper) = CloneVectors();
        lowerUpper[objInd] = newObjVal; // push upper bound vertex to newObjVal on objInd-th dim

        // misuse this rectangle as the upper
        var (upperLower, upperUpper) = CloneVectors();
        upperLower[objInd] = newObjVal; // push lower vertex to newObjVal on objInd-th dim

        return (new(lowerLower, lowerUpper), new(upperLower, upperUpper));
    }
    public bool IsSubsetOf(Rect other)
    {
        Debug.Assert(other.Lower.Length == Dim, nameof(IsSubsetOf));
        for (int j = 0; j < other.Lower.Length; j++)
            if (Lower[j] < other.Lower[j])
                return false; // lower[j] < other.Lower[j]; this is not a subset of the other
            else if (Upper[j] > other.Upper[j])
                return false; // upper[j] > other.Upper[j]; this is not a subset of the other
        return true;
    }


    // common
    public override string ToString()
        => string.Format("{0};{1}", string.Join(',', Lower), string.Join(',', Upper));
    public static bool operator ==(Rect left, Rect right)
        => left.Lower.Length == right.Lower.Length
        && left.Lower.SequenceEqual(right.Lower)
        && left.Upper.SequenceEqual(right.Upper);
    public static bool operator !=(Rect left, Rect right)
        => left.Lower.Length != right.Lower.Length
        || !left.Lower.SequenceEqual(right.Lower)
        || !left.Upper.SequenceEqual(right.Upper);
    public bool Equals(Rect other)
        => this == other;
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        else if (obj is Rect)
            return Equals((Rect)obj);
        return false;
    }
    public override int GetHashCode()
        => HashCode.Combine(
            ((IStructuralEquatable)Lower).GetHashCode(EqualityComparer<double>.Default),
            ((IStructuralEquatable)Upper).GetHashCode(EqualityComparer<double>.Default));
}
