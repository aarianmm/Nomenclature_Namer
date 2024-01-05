using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Nomenclature_Namer
{
    public class ElementGraph
    {
        private List<Element> chain;
        public List<Element> Chain { get { return chain; } }
        private int[] ends;
        private List<FunctionalGroup> groups;
        public List<FunctionalGroup> Groups { get { return groups; } }
        private Dictionary<string, (string name, int valency)> periodicTable;

        public ElementGraph(string periodicTableFileName)
        {
            periodicTable = LoadPeriodicTable(periodicTableFileName);
            Construct();
            DisplayGroupsDebug();
            //DisplayPeriodicTableDebug();
            //SpellOutGraphDebug(true);
        }
        public void Construct()
        {
            chain = new List<Element>();
            Element root = new Element("C", "Carbon", 4);
            SpellOutAtoms(root);
            ends = FindEnds();
            /*
            foreach (Element atom in chain)
            {
                if (atom.Name == "Carbon")
                {
                    ((Carbon)atom).ProvideEndsCount(ends.Length);
                }
            }
            for (int i = 0; i < ends.Length; i++) // for(int i=0; i<ends.Count;i++) // - pass i to numberCarbons so final 'if else' can check that all carbon numbers are filled if error occurs - 'else if (chain.Count(x => x.Name == "Carbon" && ((Carbon)x).CarbonNumber.Count != i+1 ) == 0) // no incomplete lists
            { 
                NumberCarbons(ends[i], i);
            }*/

            groups = FindGroups();
            removeDuplicateGroups();

            Console.WriteLine("Done");
        }
        public bool IsAnEnd(int index)
        {
            return ends.Contains(index);
        }
        private void AddFullBond(int indexOne, int indexTwo, int bondOrder)
        {
            chain[indexOne].AddHalfBond(indexTwo, bondOrder);
            chain[indexTwo].AddHalfBond(indexOne, bondOrder);
        }
        private int PreviousSimilarAtoms(int atomIndex)
        {
            int count = 1;
            for (int i = 0; i < atomIndex; i++)
            {
                if (chain[i].Name == chain[atomIndex].Name)
                {
                    count++;
                }
            }
            return count;
        }
        private void SpellOutAtoms(Element rootAtom)
        {
            chain.Add(rootAtom);
            int rootAtomIndex = 0;
            int currentAtomIndex = 1;
            do
            {
                while (chain[rootAtomIndex].BondsFree > 0)
                {
                    int bondOrder = 1;
                    Console.WriteLine($"What {(rootAtom.MaxBonds > rootAtom.BondsFree ? "else " : "")}is bonded to {rootAtom.Name} number {PreviousSimilarAtoms(rootAtomIndex)}?\n{chain[rootAtomIndex].BondsFree} bonds left.");
                    string symbolInput = Console.ReadLine().ToUpper();
                    if (periodicTable.ContainsKey(symbolInput))
                    {
                        chain.Add(new Element(symbolInput, periodicTable[symbolInput].name, periodicTable[symbolInput].valency));
                    }
                    else if (symbolInput == "")
                    {
                        symbolInput = "H";
                        chain.Add(new Element(symbolInput, periodicTable[symbolInput].name, periodicTable[symbolInput].valency));
                    }
                    else
                    {
                        Console.WriteLine("Invalid... Please try again.");
                        continue;
                    }
                    int maximumBonds = Math.Min(rootAtom.BondsFree, chain.Last().BondsFree);
                    bool validBondOrder = false;
                    while (!validBondOrder)
                    {
                        if (maximumBonds > 1)
                        {
                            Console.WriteLine($"How many bonds do these two atoms form?\nMaximum {maximumBonds}.");
                            validBondOrder = int.TryParse(Console.ReadLine(), out bondOrder);
                            validBondOrder = validBondOrder && bondOrder <= maximumBonds && bondOrder > 0;
                        }
                        else
                        {
                            bondOrder = 1;
                            validBondOrder = true;
                        }
                        if (!validBondOrder)
                        {
                            Console.WriteLine("Invalid... Please try again.");
                        }
                    }
                    AddFullBond(rootAtomIndex, currentAtomIndex, bondOrder);
                    currentAtomIndex++;
                }

                rootAtomIndex++;
                rootAtom = chain[rootAtomIndex];

            } while (chain.Count(x => x.BondsFree > 0) > 0); //do while there are bonds to be made
            Console.WriteLine("All done.");
        }
        //public int[] AdjacentAtoms(int index, string specifyElement, string ignoreElement)
        //{

        //}
        public int AlkylCounter(int index)
        {
            return chain[index].BondIndexes.Distinct().Count(x => chain[x].Name == "Carbon"); //hashset as double bonds were counted twice
        }
        private int AlkylCounter(int index, HashSet<int> exclude)
        {
            return chain[index].BondIndexes.Distinct().Count(x => chain[x].Name == "Carbon" && !exclude.Contains(x));
        }
        public int[] AdjacentAtoms(int index, string ignoreSymbol) // adjacent atoms, H is to be ignored as it is dead end
        {
            return chain[index].BondIndexes.Distinct().Where(x => chain[x].Symbol != ignoreSymbol && chain[x].Symbol != "H").ToArray();
        }
        public int[] AdjacentAtoms(int index)
        {
            return AdjacentAtoms(index, "");
        }
        public int BondOrder(int indexOne, int indexTwo) // not distinct as it returns bond order
        {
            return chain[indexOne].BondIndexes.Count(x => x == indexTwo);
        }
        private int[] FindEnds()
        {
            List<int> ends = new List<int>();
            for (int chainIndex = 0; chainIndex < chain.Count; chainIndex++)
            {
                if (chain[chainIndex].Name == "Carbon" && !ends.Contains(chainIndex))
                {
                    int alkylCount = AlkylCounter(chainIndex);
                    if (alkylCount == 0 || alkylCount == 1) // isolated carbon ie R-O-C-O-R, or unique end
                    {
                        ends.Add(chainIndex);
                    }
                }
            }
            if (ends.Count == 0)
            {
                throw new Exception("No ends?");
            }
            Console.WriteLine(ends.Count);
            return ends.ToArray();
        }
        public List<List<int>> FindBranches(List<int> path)
        {
            List<List<int>> branches = new List<List<int>>();
            foreach (int i in path)
            {
                if (AlkylCounter(i) > 2)
                {
                    foreach (int end in ends)
                    {
                        if (!path.Contains(end))
                        {
                            List<int> branch = FindPath(i, end);
                            //Console.WriteLine("finding branch... union is " + branch.Union(path).Count());
                            if (branch.Intersect(path).Count() == 1) //new branch
                            {
                                //Console.WriteLine("succesful branch");
                                branches.Add(branch);
                            }
                        }
                    }
                }
            }
            return branches;
        }
        public List<List<int>> FindEveryLongestPath()
        {
            if (ends.Length == 1)
            {
                return new List<List<int>> { new List<int> { ends[0] } };
            }
            List<List<int>> paths = new List<List<int>>();
            for (int i = 0; i < ends.Length; i++)
            {
                for (int j = 0; j < ends.Length; j++)
                {
                    if (i != j)
                    {
                        paths.Add(FindPath(ends[i], ends[j]));
                    }

                }
            }
            return paths;
        }
        public List<int> FindPath(int start, int end)
        {
            List<int> path = new List<int>();
            HashSet<int> visited = new HashSet<int>();
            FindPathRecursive(start, end, ref visited, ref path);
            return path;
        }
        private bool FindPathRecursive(int current, int end, ref HashSet<int> visited, ref List<int> path)
        {
            visited.Add(current);
            path.Add(current);

            if (current == end)
            {
                return true;
            }
            int[] bonds = AdjacentAtoms(current);
            foreach (int bond in bonds)
            {
                if (!visited.Contains(bond))
                {
                    if (FindPathRecursive(bond, end, ref visited, ref path))
                    {
                        return true;
                    }
                }
            }

            path.Remove(current);
            return false;
        }
        /*private void NumberCarbons(int startIndex, int end) //? broken
        {
            //Console.WriteLine("NUMBERING end "+end);
            int chainIndex = startIndex;
            int carbonNumber = 1;
            Queue<int> indexToReturnTo = new Queue<int>();
            HashSet<int> visited = new HashSet<int>();
            bool allCarbonsNumbered = false;
            while (!allCarbonsNumbered)
            {
                visited.Add(chainIndex);
                if (chain[chainIndex].Name == "Carbon")
                {
                    int[] bondIndexes = AdjacentAtoms(chainIndex);
                    //foreach(int i in bondIndexes)
                    //{
                    //    Console.WriteLine(i);
                    //}
                    ((Carbon)chain[chainIndex]).CarbonNumbers[end] = carbonNumber;
                    int unvisitedAdjacentCarbons = AlkylCounter(chainIndex, visited);
                    //Console.WriteLine($"alyklcounter = {unvisitedAdjacentCarbons}");
                    if (unvisitedAdjacentCarbons >= 2)
                    {
                        indexToReturnTo.Enqueue(chainIndex);
                        chainIndex = NextCarbon(bondIndexes, visited);
                        carbonNumber++;
                    }
                    else if (unvisitedAdjacentCarbons == 1)
                    {
                        chainIndex = NextCarbon(bondIndexes, visited);
                        carbonNumber++;
                    }
                    else if (unvisitedAdjacentCarbons == 0 && indexToReturnTo.Count > 0)
                    {
                        chainIndex = indexToReturnTo.Dequeue();
                        carbonNumber = ((Carbon)chain[chainIndex]).CarbonNumbers[end];
                    }
                    else
                    {
                        allCarbonsNumbered = true;
                    }
                }
            }
            foreach (Element e in chain)
            {
                if (e.Name == "Carbon")
                {
                    if (((Carbon)e).CarbonNumbers[end] == 0)
                    {

                        throw new Exception("Carbon isolation from end " + end);
                    }
                }
            }
        }
        public void NarrowDownCarbonsNumbers(List<int> endsIndexes) //overclimplicated to put in this layer/class ??
        {
            foreach(Element c in chain)
            {
                if(c.Name == "Carbon")
                {
                    ((Carbon) c).NarrowDownCarbonNumber(endsIndexes);
                }
            }
            int[] newEnds = new int[endsIndexes.Count];
            for(int i = 0; i < endsIndexes.Count; i++)
            {
                newEnds[i] = ends[endsIndexes[i]]; //order matters but hashset unordered
            }
        }

        //private int NextCarbon(int[] bonds, HashSet<int> exclude)
        //{
        //    return bonds.First(x => x > 0 && chain[x].Name == "Carbon" && !exclude.Contains(x));
        //}
        private int NextCarbon(int[] bonds, HashSet<int> exclude) //recursion
        {
            if (bonds.Length == 0)
            {
                return -1; //ERROR
            }
            int first = bonds[0];
            bonds = bonds.Skip(1).ToArray();
            if (chain[first].Name == "Carbon" && !exclude.Contains(first)) //found
            {
                return first;
            }
            return NextCarbon(bonds, exclude);
        }
        public List<int> FindLowestEndIndexes(int importantCarbonIndex) //minimise the carbon number of this index
        {
            List<int> indexes = new List<int>();
            Carbon importantCarbon = (Carbon)chain[importantCarbonIndex];
            int lowest = importantCarbon.CarbonNumbers.Min();
            for (int i = 0; i < ends.Count(); i++)
            {
                if (importantCarbon.CarbonNumbers[i] == lowest)
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }
        */
        private List<FunctionalGroup> FindGroups()
        {
            List<FunctionalGroup> newGroups = new List<FunctionalGroup>();
            //carbonCarbonGroups = new List<CarbonCarbonGroup>();
            //carbonOtherCarbonGroups = new List<CarbonOtherCarbonGroup>();
            for (int atomIndex = 0; atomIndex < chain.Count; atomIndex++)
            {
                Element atom = chain[atomIndex];
                if (atom.Name == "Carbon")
                {
                    int[] unvisitedBonds = AdjacentAtoms(atomIndex); //no carbons too
                    foreach (int bondIndex in unvisitedBonds)
                    {
                        Element bondedAtom = chain[bondIndex];
                        int order = BondOrder(atomIndex, bondIndex);
                        if (bondedAtom.Name == "Carbon" && order != 1) // no functional group
                        {
                            newGroups.Add(new CarbonCarbonGroup(order, atomIndex, bondIndex));
                        }
                        else if (bondedAtom.Name != "Carbon")// normal group ie C=O, C-N etc
                        {
                            newGroups.Add(new FunctionalGroup(bondedAtom.Symbol, order, atomIndex));
                        }

                    }
                }
                else if (AlkylCounter(atomIndex) > 1) //C-O-C
                {
                    List<int> carbonIndexes = new List<int>();
                    foreach (int bond in atom.BondIndexes)
                    {
                        if (chain[bond].Name == "Carbon")
                        {
                            if (BondOrder(atomIndex, bond) > 1)
                            {
                                throw new Exception(); //cannot have esthers or amines where the atom makes double/triple bonds with carbon
                            }
                            carbonIndexes.Add(bond);
                        }
                        else if (chain[bond].Name != "Hydrogen")
                        {
                            throw new Exception(); //cannot have esthers or amines where the atom makes multiple non-carbon bonds ? already CHECKED??
                        }
                    }
                    newGroups.Add(new CarbonOtherCarbonGroup(atom.Symbol, atomIndex, carbonIndexes.ToArray()));
                }
                else if (atom.Name != "Hydrogen" && AdjacentAtoms(atomIndex, "C").Count() > 0)  // bonded to something else - not in alevel,## except for esthers (C-O-C). can identify esthers by multiple chains
                {
                    //Console.WriteLine(atom.Symbol);
                    //Console.WriteLine(atoms.AdjacentAtoms(atomIndex, new string[] { "C", "H" }).Count());
                    //Console.ReadLine();

                    throw new Exception("Unrecognised molecule - Multiple non-carbon atoms are bonded together"); //can expand to include no2 etc in future
                }
            }
            return newGroups;
        }
        private void removeDuplicateGroups()
        {
            List<int> rm = new List<int>();
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                if (groups[i] is CarbonCarbonGroup)
                {
                    CarbonCarbonGroup cgroupOne = (CarbonCarbonGroup)groups[i];
                    for (int j = groups.Count - 1; j >= 0; j--)
                    {
                        if (groups[j] is CarbonCarbonGroup && i != j)
                        {
                            CarbonCarbonGroup cgroupTwo = (CarbonCarbonGroup)groups[j];
                            if (cgroupOne.IsSame(cgroupTwo))
                            {
                                groups.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }

        public static void SavePeriodicTable(string fileName, Dictionary<string, (string name, int valency)> newPeriodicTable)
        { 
            fileName += ".PeriodicTable";
            
            using (BinaryWriter writefile = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writefile.Write(Convert.ToInt16(newPeriodicTable.Count));
                foreach(KeyValuePair<string, (string name, int valency)> element in newPeriodicTable)
                {
                    writefile.Write(element.Key);
                    writefile.Write(element.Value.name);
                    writefile.Write(Convert.ToInt16(element.Value.valency));
                }
                writefile.Close();
            }
            
        }
        private Dictionary<string, (string name, int valency)> LoadPeriodicTable(string fileName)
        {
            if (!fileName.EndsWith(".PeriodicTable"))
            {
                fileName += ".PeriodicTable";
            }
            Dictionary<string, (string name, int valency)> newPeriodicTable = new Dictionary<string, (string name, int valency)>();
            using (BinaryReader readfile = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                int elementCount = readfile.ReadInt16();
                for(int i=0; i<elementCount; i++)
                {
                    string symbol = readfile.ReadString();
                    string name = readfile.ReadString();
                    int valency = readfile.ReadInt16();
                    newPeriodicTable.Add(symbol, (name, valency));
                }
                readfile.Close();
            }
            return newPeriodicTable;
        }
        private void SpellOutGraphDebug(bool carbonOnly)
        {
            for (int i = 0; i < chain.Count; i++)
            {
                if (carbonOnly && chain[i].Name == "Carbon" || !carbonOnly)
                {
                    Console.WriteLine($"{chain[i].Name} has index {i}");
                }
            }
        }
        private void DisplayGroupsDebug() //debug
        {
            foreach (FunctionalGroup fg in groups)
            {
                Console.WriteLine(fg.GetType().Name);
                Console.WriteLine(fg.GroupFormula);
            }
        }
        private void DisplayPeriodicTableDebug()
        {
            foreach(KeyValuePair<string, (string name, int valency)> kvp in periodicTable)
            {
                Console.WriteLine(kvp.Key + " | " + kvp.Value.name + " | " + kvp.Value.valency);
            }
        }
    }


}

