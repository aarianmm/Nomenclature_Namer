using Nomenclature_Namer;

//IUPAC.SaveSpecification("ExedelIGCSE", examplePrioritySuffix, examplePriorityPrefix);
Dictionary<string, (string, int)> pt = new Dictionary<string, (string, int)> { { "C", ("Carbon", 4) }, { "O", ("Oxygen", 2) }, { "N", ("Nitrogen", 3) }, { "H", ("Hydrogen", 1) }, { "F", ("Florine", 1) }, { "Cl", ("Chlorine", 1) }, { "Br", ("Bromine", 1) }, { "I", ("Iodine", 1) } };
ElementGraph.SavePeriodicTable("default", pt);

ElementGraph bob = new ElementGraph("default");
//bob.Construct();
//foreach (Element e in bob.Chain)
//{
//    Console.WriteLine(e.Symbol);
//    //Console.WriteLine(e.)
//}
//bob.save("newest");

IUPAC namer = new IUPAC(bob);

//Element[] chain = new Element[6];  test revieled that carbon number 
//chain[1] = new Carbon();
//Carbon cheeky = (Carbon)chain[1];
//cheeky.CarbonNumber = 2;
//chain[1] = cheeky;
//Carbon bob = (Carbon)chain[1];
//Console.WriteLine(bob.CarbonNumber);

Console.WriteLine("quit");
Console.ReadLine();