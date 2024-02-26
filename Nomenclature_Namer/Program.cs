using Nomenclature_Namer;
using System;
//IUPAC.DisplaySpecDebug("AllGroups");

IUPAC.RestoreDefaultSpecifications();
ElementGraph.RestoreDefaultPeriodicTable();
ElementGraph graph = new ElementGraph("default");
Console.WriteLine("----------");
IUPAC namer = new IUPAC("AllGroups", graph);
//Middle does not need a priority- fix if possible
//Console.WriteLine("quit");
Console.ReadLine();
