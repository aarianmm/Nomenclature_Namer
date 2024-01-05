using System;
namespace Nomenclature_Namer
{
	//public class FunctionalGroupsList
	//{
 //       private List<FunctionalGroup> groups;
 //       public List<FunctionalGroup> Groups { get { return groups; } }
 //       //private List<CarbonCarbonGroup> carbonCarbonGroups;
 //       //public List<CarbonCarbonGroup> CarbonCarbonGroups { get; }
 //       //private List<CarbonOtherCarbonGroup> carbonOtherCarbonGroups;
 //       //public List<CarbonOtherCarbonGroup> CarbonOtherCarbonGroups { get; } //make into one list prob
 //       //private HashSet<string, (int, )> sufffixPriority = { "C≡N","C=O", "C-O", } //maybe make new class and take from file!!!

 //       public FunctionalGroupsList(ElementGraph atoms)
 //       {
 //           groups = FindGroups(atoms);
 //           removeDuplicateGroups();
 //       }
 //       private List<FunctionalGroup> FindGroups(ElementGraph atoms)
 //       {
 //           List<FunctionalGroup> newGroups = new List<FunctionalGroup>();
 //           //carbonCarbonGroups = new List<CarbonCarbonGroup>();
 //           //carbonOtherCarbonGroups = new List<CarbonOtherCarbonGroup>();
 //           for (int atomIndex = 0; atomIndex < atoms.Chain.Count; atomIndex++)
 //           {
 //               Element atom = atoms.Chain[atomIndex];
 //               if (atom.Name == "Carbon")
 //               {
 //                   int[] unvisitedBonds = atoms.AdjacentAtoms(atomIndex); //no carbons too
 //                   foreach (int bondIndex in unvisitedBonds)
 //                   {
 //                       Element bondedAtom = atoms.Chain[bondIndex];
 //                       int order = atoms.BondOrder(atomIndex, bondIndex);
 //                       if (bondedAtom.Name == "Carbon" && order != 1) // no functional group
 //                       {
 //                           newGroups.Add(new CarbonCarbonGroup(order, atomIndex, bondIndex));
 //                       }
 //                       else if (bondedAtom.Name != "Carbon")// normal group ie C=O, C-N etc
 //                       {
 //                           newGroups.Add(new FunctionalGroup(bondedAtom.Symbol, order, atomIndex));
 //                       }

 //                   }
 //               }
 //               else if (atoms.AlkylCounter(atomIndex) > 1) //C-O-C
 //               {
 //                   List<int> carbonIndexes = new List<int>();
 //                   foreach (int bond in atom.BondIndexes)
 //                   {
 //                       if (atoms.Chain[bond].Name == "Carbon")
 //                       {
 //                           if (atoms.BondOrder(atomIndex, bond) > 1)
 //                           {
 //                               throw new Exception(); //cannot have esthers or amines where the atom makes double/triple bonds with carbon
 //                           }
 //                           carbonIndexes.Add(bond);
 //                       }
 //                       else if (atoms.Chain[bond].Name != "Hydrogen")
 //                       {
 //                           throw new Exception(); //cannot have esthers or amines where the atom makes multiple non-carbon bonds ? already CHECKED??
 //                       }
 //                   }
 //                   groups.Add(new CarbonOtherCarbonGroup(atom.Symbol, atomIndex, carbonIndexes.ToArray()));
 //               }
 //               else if (atom.Name != "Hydrogen" && atoms.AdjacentAtoms(atomIndex, "C").Count() > 0)  // bonded to something else - not in alevel,## except for esthers (C-O-C). can identify esthers by multiple chains
 //               {
 //                   //Console.WriteLine(atom.Symbol);
 //                   //Console.WriteLine(atoms.AdjacentAtoms(atomIndex, new string[] { "C", "H" }).Count());
 //                   //Console.ReadLine();

 //                   throw new Exception("Unrecognised molecule - Multiple non-carbon atoms are bonded together"); //can expand to include no2 etc in future
 //               }
 //           }
 //           return newGroups;
 //       }
 //       private void removeDuplicateGroups()
 //       {
 //           List<int> rm = new List<int>();
 //           for (int i = 0; i < groups.Count(); i++)
 //           {
 //               if (groups[i] is CarbonCarbonGroup)
 //               {
 //                   CarbonCarbonGroup cgroup = ((CarbonCarbonGroup)groups[i]);
 //                   for(int j=0; i<groups.Count; j++)
 //                   {
 //                       CarbonCarbonGroup cgroupTwo = ((CarbonCarbonGroup)groups[j]);
 //                       if (cgroup.IsSame(cgroupTwo))
 //                       {
 //                           rm.Add(i);
 //                       }
 //                   }
 //               }
 //           }
 //           foreach(int i in rm)
 //           {
 //               groups.RemoveAt(i);
 //           }
 //       }
 //       private void DisplayGroupsDebug() //debug
 //       {
 //           foreach(FunctionalGroup fg in groups)
 //           {
 //               Console.WriteLine(fg.GetType().Name);
 //               Console.WriteLine(fg.GroupFormula);
 //           }
 //       }

 //       //public void removeDuplicateGroups()
 //       //{
 //       //    //List<funcionalGroup> doubles = groups.DistinctBy(x => new int[] { x.carbonIndex, x.bondIndex }.Order()).ToList();
 //       //    for (int i = 0; i < groups.Count(); i++)
 //       //    {
 //       //        (int, int) bondIndexes = (groups[i].bondIndex, groups[i].carbonIndex);
 //       //        for (int j = 0; j < groups.Count(); j++)
 //       //        {
 //       //            if (bondIndexes)
 //       //        }
 //       //    }
 //       //}
 //   }
}
