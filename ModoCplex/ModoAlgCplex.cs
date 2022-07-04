using ILOG.Concert;
using ILOG.CPLEX;
namespace ModoCplex;

public static class ModoAlgCplex
{
    // Method
    public static ModoAlg<X> Create<X>(Model model, CplexBuilder builder, Var1 objectives,
                    Func<CplexBuilder, X> extractSolution,
                    Opt<Rect> boundingRectangle,
                    Opt<Func<double[], double[], double>> getPriority)
        => ModoAlgMathProg<Cplex, ILinearNumExpr, INumVar, CplexBuilder, X>
            .Create(model, builder, objectives, extractSolution, boundingRectangle, getPriority);
}
