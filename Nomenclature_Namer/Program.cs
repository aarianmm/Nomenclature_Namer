using Nomenclature_Namer;

IUPAC.RestoreDefaultSpecifications();
ElementGraph.RestoreDefaultPeriodicTable();

ElementGraph graph = new ElementGraph("default");
IUPAC namer = new IUPAC("AllGroups", graph);

Console.WriteLine("quit");
Console.ReadLine();