using Nomenclature_Namer;

IUPAC.RestoreDefaultSpecifications();
ElementGraph.RestoreDefaultPeriodicTable();
ElementGraph graph = new ElementGraph("default");
Console.WriteLine("----------");
IUPAC namer = new IUPAC("AllGroups", graph);
//Middle does not need a priority- fix if possible
Console.ReadLine();