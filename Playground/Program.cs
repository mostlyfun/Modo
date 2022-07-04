Console.WriteLine("hello");


var rnd = new Random(0);
int nbTasks = 10;
int nbMachines = 5;
var taskTime = Enumerable.Range(0, nbTasks).Select(i => (double)rnd.Next(10, 20)).ToArray();
var machineCap = Enumerable.Range(0, nbMachines).Select(i => (double)rnd.Next(20, 30)).ToArray();
var objCoef1 = Enumerable.Range(0, nbMachines).Select(m => Enumerable.Range(0, nbTasks).Select(t => (double)rnd.Next(1, 10)).ToArray()).ToArray();
var objCoef2 = Enumerable.Range(0, nbMachines).Select(m => Enumerable.Range(0, nbTasks).Select(t => (double)rnd.Next(1, 10)).ToArray()).ToArray();


Model model = new();
var x = model.Bin2("x", nbMachines, nbTasks);
Set t = model.Set("t", nbTasks);
Set m = model.Set("m", nbMachines);
Par1 time = new("time", taskTime);
Par1 cap = new("caop", machineCap);
Par2 coef1 = new("coef1", objCoef1);
Par2 coef2 = new("coef2", objCoef2);

model += t | Sum(m | x[m, t]) <= 1;
model += m | Sum(t | x[m, t] * time[t]) <= cap[m];


var objLst = new List<Var0>()
{
    model.Cont0("f0", double.MinValue, double.MaxValue),
    model.Cont0("f1", double.MinValue, double.MaxValue),
};
model += objLst[0] == Sum((m, t) | coef1[m, t] * x[m, t]);
model += objLst[1] == Sum((m, t) | coef2[m, t] * x[m, t]);

/*/model += 2 * x + y >= 4;
model += objLst[0] == 2 * x - y;
model += objLst[1] == 3 * y - 2 * x;
model += objLst[2] == - 4 * y - x;
*/
Var1 objVec = model.Cont1("objVec", objLst.Count, double.MinValue, double.MaxValue);
for (int i = 0; i < objLst.Count; i++)
    model += objVec[i] == -objLst[i];


var disturbances = new double[]
{
    0.0001,
    0.0001,
};

Console.WriteLine($"\n\n\n{model.BuildAndSolve(new LpBuilder())}\n\n\n");


var modoAlg = ModoAlgCplex.Create(model, new(true), objVec, _ => true, default, default);
var result = modoAlg.CreateNondominatedSet(disturbances);
Console.WriteLine("nb-solns = " + result.Count());
foreach (var (_, fx) in result)
{
    fx.ToList().ForEach(f => Console.Write(-f + "\t"));
    Console.WriteLine();
}
