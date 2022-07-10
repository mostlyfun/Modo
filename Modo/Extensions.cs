using System.Text;

namespace Modo;

public static class Extensions
{
    public static ModoAlg<X> CreateLpSolver<X>(Model model, LpSolver builder, Var1 objectives,
                    Func<LpSolver, X> extractSolution,
                    Opt<Rect> boundingRectangle,
                    Opt<Func<double[], double[], double>> getPriority)
        => ModoAlgMathProg<StringBuilder, StringBuilder, string, LpSolver, X>
            .Create(model, builder, objectives, extractSolution, boundingRectangle, getPriority);
}
