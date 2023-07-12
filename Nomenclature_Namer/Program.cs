using Nomenclature_Namer;

ElementGraph bob = new ElementGraph();


//Element[] chain = new Element[6];  test revieled that carbon number stays but is hidden in array
//chain[1] = new Carbon();
//Carbon cheeky = (Carbon)chain[1];
//cheeky.CarbonNumber = 2;
//chain[1] = cheeky;
//Carbon bob = (Carbon)chain[1];
//Console.WriteLine(bob.CarbonNumber);


Console.ReadLine();
////while (true) { Console.WriteLine(longestChainLength(Console.ReadLine())); }

//using static System.Net.WebRequestMethods;


//Console.WriteLine("≡");

//string SF = Console.ReadLine() + "!";   //tried removing all hydrogens for efficiency, but they are needed for identifying hydroxyl group



//int branches = countBranches(SF);


////List<funcionalGroup> endHydroxylGroups = stemData.compound.Where(group => group.Priority == 2 && group.Carbon == 1 || group.Carbon == longestChainLength).ToList();   //https://stackoverflow.com/questions/28725800/filter-an-immutable-list-recursively
////List<funcionalGroup> aldehydeGroups = stemData.compound.Where(group => group.Priority == 4).ToList();


////foreach (funcionalGroup group in stemData.compound)
////{
////      depends if deriving carboxylic acid from OH and =O or if checking for COOH in compiling function
////}

//compound userCompound = new compound(SF);

//string core = SkeletonName(longestChainLength);
//funcionalGroup highestPriority = findHighestPriority(stemData.compound);
//if (highestPriority.Carbon > longestChainLength / 2)
//{
//    foreach (funcionalGroup group in stemData.compound)
//    {
//        group.Carbon = longestChainLength - group.Carbon;   //counts chain from opposite side
//    }
//}

//string iupac = core + "-" + highestPriority.Carbon + highestPriority.Suffix;

//Console.WriteLine(highestPriority.suffix);
//Console.ReadLine();

////----------------------END OF CODE---------------------------------


//static funcionalGroup findHighestPriority(List<funcionalGroup> compound)
//{

//    funcionalGroup highestPriority = compound[0];
//    foreach (funcionalGroup group in compound)
//    {
//        if (group.Priority > highestPriority.Priority)   //can be optimised if longest chain is given as functionalgroup in the future maybe
//        {
//            highestPriority = group;
//        }
//    }
//    return highestPriority;
//}