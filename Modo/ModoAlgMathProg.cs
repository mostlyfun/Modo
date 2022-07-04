﻿namespace Modo;

public static class ModoAlgMathProg<M, C, V, B, X> where B : ModelBuilder<M, C, V>
{
    // Method
    public static ModoAlg<X> Create(Model model, B builder, Var1 objectives,
                    Func<B, X> extractSolution,
                    Opt<Rect> boundingRectangle,
                    Opt<Func<double[], double[], double>> getPriority)

    {
        var boundingRect = boundingRectangle.IsSome ? boundingRectangle.Unwrap()
                                                    : GetBoundingRect(model, builder, objectives);
        var solveForEpsilon = GetSolveForEpsilon(model, builder, objectives, extractSolution);
        return new(boundingRect, solveForEpsilon, getPriority);
    }


    // helper - bounding rect
    static Rect GetBoundingRect(Model model, B builder, Var1 objectives)
        => new(GetBoundingRectVertex(model, builder, objectives, ObjType.Minimize),
                GetBoundingRectVertex(model, builder, objectives, ObjType.Maximize));
    static double[] GetBoundingRectVertex(Model model, B builder, Var1 objectives, ObjType objType)
    {
        int dim = objectives.Len1 - 1;
        var vertex = new double[dim];
        for (int j = 0; j < dim; j++)
        {
            model.Obj(objType, objectives[j]);
            var (_, isFeas) = model.BuildAndSolve(builder);
            if (!isFeas)
                throw new ArgumentException("Failed to compute lower bound for objective " + j);
            vertex[j] = builder.GetVal1(objectives)[j];
        }
        Console.WriteLine("\n\n\n" + string.Join('\n', vertex) + "\n\n\n");
        return vertex;
    }


    // helper - solver
    static Func<Rect, Opt<(X X, double[] Fx)>> GetSolveForEpsilon(Model model, B builder, Var1 objectives, Func<B, X> extractSolution)
    {
        var j = model.Set(objectives.Len1);
        var sumOfObjectives = Sum(j | objectives[j]);
        var dummyVar = model.Cont0("dummy", double.MinValue, double.MaxValue);
        var boundConstraintNames = Enumerable.Range(0, objectives.Len1).Select(i => string.Format("__bb{0}__", i)).ToArray();
        
        return rect =>
        {
            // first stage: Pk(epsilon)
            Debug.Assert(rect.Upper.Length == objectives.Len1 - 1);
            int dim = objectives.Len1 - 1;
            model.Obj(ObjType.Minimize, objectives[dim]);
            var upper = rect.Upper;
            for (int j = 0; j < dim; j++)
                model[boundConstraintNames[j]] = objectives[j] <= (upper[j] - 1.0);
            model[boundConstraintNames[upper.Length]] = dummyVar >= 0;

            var (_, isFeas) = model.BuildAndSolve(builder);
            if (!isFeas)
                return None<(X, double[])>();
            var objValsP = builder.GetVal1(objectives);
            double zStar = objValsP[upper.Length];

            // second stage: Qk(epsilon)
            model.Obj(ObjType.Minimize, sumOfObjectives);
            model[boundConstraintNames[upper.Length]] = objectives[dim] == zStar;

            (_, isFeas) = model.BuildAndSolve(builder);
            if (!isFeas)
                return None<(X, double[])>();

            // extract solution
            var x = extractSolution(builder);
            var objValsQ = builder.GetVal1(objectives).ToArray();
            return (x, objValsQ);
        };
    }
}
