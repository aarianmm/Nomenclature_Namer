namespace Nomenclature_Namer
{
    public class ElementGraph
    {
        private List<Element> atoms; //list of nodes
        public List<Element> Atoms { get { return atoms; } }
        private int[] ends;
        private List<FunctionalGroup> groups;
        public List<FunctionalGroup> Groups { get { return groups; } }
        private Dictionary<string, (string name, int valency)> periodicTable;

        public ElementGraph(string periodicTableFileName)
        {
            periodicTable = LoadPeriodicTable(periodicTableFileName);
            Construct();
        }
        private void Construct()
        {
            atoms = new List<Element>();
            Element root = new Element("C", "Carbon", 4); //always starts with a carbon atom to build off
            SpellOutAtoms(root);
            ends = FindEnds();
            groups = FindGroups();
            RemoveDuplicateGroups();
        }
        public bool IsAnEnd(int index)
        {
            return ends.Contains(index);
        }
        private void AddFullBond(int indexOne, int indexTwo, int bondOrder)
        {
            atoms[indexOne].AddHalfBond(indexTwo, bondOrder);
            atoms[indexTwo].AddHalfBond(indexOne, bondOrder);
        }
        private int PreviousSimilarAtoms(int atomIndex) //how many atoms of same element have already been added
        {
            int count = 1;
            for (int i = 0; i < atomIndex; i++)
            {
                if (atoms[i].Name == atoms[atomIndex].Name)
                {
                    count++;
                }
            }
            return count;
        }
        private void SpellOutAtoms(Element rootAtom)
        {
            atoms.Add(rootAtom);
            int rootAtomIndex = 0;
            int currentAtomIndex = 1;
            do
            {
                while (atoms[rootAtomIndex].BondsFree > 0)
                {
                    int bondOrder = 1;
                    Console.WriteLine($"What {(rootAtom.MaxBonds > rootAtom.BondsFree ? "else " : "")}is bonded to {rootAtom.Name} number {PreviousSimilarAtoms(rootAtomIndex)}?\n{atoms[rootAtomIndex].BondsFree} bonds left.");
                    string symbolInput = Console.ReadLine();
                    if (symbolInput.Length > 0)
                    {
                        symbolInput = symbolInput[0].ToString().ToUpper() + symbolInput.Substring(1);
                    }
                    if (periodicTable.ContainsKey(symbolInput))
                    {
                        atoms.Add(new Element(symbolInput, periodicTable[symbolInput].name, periodicTable[symbolInput].valency));
                    }
                    else if (symbolInput == "")
                    {
                        symbolInput = "H";
                        atoms.Add(new Element(symbolInput, periodicTable[symbolInput].name, periodicTable[symbolInput].valency));
                    }
                    else
                    {
                        Console.WriteLine("Invalid... Please try again.");
                        continue;
                    }
                    int maximumBonds = Math.Min(rootAtom.BondsFree, atoms.Last().BondsFree);
                    bool validBondOrder = false;
                    while (!validBondOrder)
                    {
                        if (maximumBonds > 1)
                        {
                            Console.WriteLine($"How many bonds do these two atoms form?\nMaximum {maximumBonds}.");
                            validBondOrder = int.TryParse(Console.ReadLine(), out bondOrder);
                            validBondOrder = validBondOrder && bondOrder <= maximumBonds && bondOrder > 0; //bondOrder is an integer between 1 and the maximum
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
                rootAtom = atoms[rootAtomIndex];

            } while (atoms.Count(x => x.BondsFree > 0) > 0); //do while there are bonds to be made
        }
        public int AlkylCounter(int index)
        {
            return atoms[index].BondIndexes.Distinct().Count(x => atoms[x].Name == "Carbon"); //hashset as double bonds are counted once
        }
        public int[] AdjacentAtoms(int index, string ignoreSymbol) //adjacent atoms, H is to be ignored as it is dead end
        {
            return atoms[index].BondIndexes.Distinct().Where(x => atoms[x].Symbol != ignoreSymbol && atoms[x].Symbol != "H").ToArray();
        }
        public int[] AdjacentAtoms(int index)
        {
            return AdjacentAtoms(index, "");
        }
        public int BondOrder(int indexOne, int indexTwo) //not distinct as it returns bond order/weighting
        {
            return atoms[indexOne].BondIndexes.Count(x => x == indexTwo);
        }
        private int[] FindEnds()
        {
            List<int> ends = new List<int>();
            for (int chainIndex = 0; chainIndex < atoms.Count; chainIndex++)
            {
                if (atoms[chainIndex].Name == "Carbon" && !ends.Contains(chainIndex))
                {
                    int alkylCount = AlkylCounter(chainIndex);
                    if (alkylCount == 0 || alkylCount == 1) //isolated carbon ie O-C-O, or unique end
                    {
                        ends.Add(chainIndex);
                    }
                }
            }
            if (ends.Count == 0)
            {
                throw new Exception("The molecule seems to have no ends");
            }
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
                            if (branch.Intersect(path).Count() == 1) //new branch
                            {
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
            if (ends.Length == 1) //only one carbon atom
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
            FindPathRecursive(start, end, ref visited, ref path); //ref to keep track through the whole stack
            return path;
        }
        private bool FindPathRecursive(int current, int end, ref HashSet<int> visited, ref List<int> path) //recurisve DFS
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
                    if (FindPathRecursive(bond, end, ref visited, ref path)) //found end somewhere down stack
                    {
                        return true;
                    }
                }
            }
            path.Remove(current); //hit deadend somewhere down stack. backtrack
            return false;
        }
        private List<FunctionalGroup> FindGroups()
        {
            List<FunctionalGroup> newGroups = new List<FunctionalGroup>();
            for (int atomIndex = 0; atomIndex < atoms.Count; atomIndex++)
            {
                Element atom = atoms[atomIndex];
                if (atom.Name == "Carbon")
                {
                    int[] unvisitedBonds = AdjacentAtoms(atomIndex);
                    foreach (int bondIndex in unvisitedBonds)
                    {
                        Element bondedAtom = atoms[bondIndex];
                        int order = BondOrder(atomIndex, bondIndex);
                        if (bondedAtom.Name == "Carbon" && order != 1) //two carbons forming double / triple bond
                        {
                            newGroups.Add(new CarbonCarbonGroup(order, atomIndex, bondIndex)); //
                        }
                        else if (bondedAtom.Name != "Carbon") //normal group eg C=O, C-N
                        {
                            newGroups.Add(new FunctionalGroup(bondedAtom.Symbol, order, atomIndex));
                        }
                    }
                }
                else if (atom.Name != "Hydrogen" && AdjacentAtoms(atomIndex, "C").Count() > 0)  //non-carbon atom bonded to some non-carbon atoms
                {
                    throw new Exception("Multiple non-carbon atoms are bonded together");
                }
                else if (AlkylCounter(atomIndex) > 1) //C-O-C
                {
                    List<int> carbonIndexes = new List<int>();
                    foreach (int bond in atom.BondIndexes)
                    {
                        if (atoms[bond].Name == "Carbon")
                        {
                            if (BondOrder(atomIndex, bond) > 1)
                            {
                                throw new Exception("An atom makes non-single bonds with multiple carbons"); //cannot have CarbonOthercarbon groups where the atom makes double/triple bonds with carbon
                            }
                            carbonIndexes.Add(bond);
                        }
                    }
                    newGroups.Add(new CarbonOtherCarbonGroup(atom.Symbol, atomIndex, carbonIndexes.ToArray()));
                }
            }
            return newGroups;
        }
        private void RemoveDuplicateGroups() //CarbonCarbonGroups are recorded twice (once for each carbon)
        {
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
        public void MergeFunctionalGroups(Dictionary<HashSet<string>, string> toMerge)
        {
            for (int i = 0; i < atoms.Count; i++)
            {
                HashSet<string> formulae = groups.Where(x => x.Involves(i)).Select(x => x.GroupFormula).ToHashSet();
                foreach (HashSet<string> collect in toMerge.Keys)
                {
                    if (collect.IsSubsetOf(formulae)) //enough groups to merge
                    {
                        formulae = new HashSet<string>(collect); //new set, otherwise it is passsed by ref. and collect is mutated
                        groups.Add(new MergedGroup(toMerge[collect], i));
                        for (int j = groups.Count - 1; j >= 0; j--) //negative iteration as mutating data structure
                        {
                            if (groups[j].Involves(i) && formulae.Contains(groups[j].GroupFormula))
                            {
                                formulae.Remove(groups[j].GroupFormula);
                                groups.RemoveAt(j);
                            }
                        }
                    }
                }
            }
        }
        public static void SavePeriodicTable(string fileName, Dictionary<string, (string name, int valency)> newPeriodicTable)
        {
            if (!fileName.EndsWith(".PeriodicTable"))
            {
                fileName += ".PeriodicTable";
            }
            using (BinaryWriter writefile = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writefile.Write(Convert.ToInt16(newPeriodicTable.Count));
                foreach (KeyValuePair<string, (string name, int valency)> element in newPeriodicTable)
                {
                    writefile.Write(element.Key); //must contain a "Carbon" "C" and "Hydrogen" "H"
                    writefile.Write(element.Value.name);
                    writefile.Write(Convert.ToInt16(element.Value.valency));
                }
                writefile.Close();
            }
        }
        public static void RestoreDefaultPeriodicTable()
        {
            Dictionary<string, (string name, int valency)> newPeriodicTable = new Dictionary<string, (string name, int valency)> {
                { "C", ("Carbon", 4) },
                { "H", ("Hydrogen", 1) },
                { "O", ("Oxygen", 2) },
                { "N", ("Nitrogen", 3) },
                { "F", ("Fluorine", 1) },
                { "Cl", ("Chlorine", 1) },
                { "Br", ("Bromine", 1) },
                { "I", ("Iodine", 1) },
                { "S", ("Sulphur", 2) } };
            SavePeriodicTable("Default", newPeriodicTable);
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
                for (int i = 0; i < elementCount; i++)
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
            for (int i = 0; i < atoms.Count; i++)
            {
                if (carbonOnly && atoms[i].Name == "Carbon" || !carbonOnly)
                {
                    Console.WriteLine($"{atoms[i].Name} has index {i}");
                }
            }
        }
        public void DisplayGroupsDebug()
        {
            foreach (FunctionalGroup fg in groups)
            {
                Console.WriteLine(fg.GetType().Name);
                Console.WriteLine(fg.GroupFormula);
            }
        }
        private void DisplayPeriodicTableDebug()
        {
            foreach (KeyValuePair<string, (string name, int valency)> kvp in periodicTable)
            {
                Console.WriteLine(kvp.Key + " | " + kvp.Value.name + " | " + kvp.Value.valency);
            }
        }
    }
}