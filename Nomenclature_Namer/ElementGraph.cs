using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;

namespace Nomenclature_Namer
{

    public class ElementGraph
    {
        private List<Element> chain;
        public List<Element> Chain { get { return chain; } set { chain = value; } }
        private int[] ends;
        public int[] Ends { get { return ends; } }
        public ElementGraph()
        {
            chain = new List<Element>();
            //compileElementGraph(SF);
            Carbon root = new Carbon();
            SpellOutAtoms(root);
            ends = findEnds();
            foreach(Element atom in chain)
            {
                if(atom.Name == "Carbon")
                {
                    ((Carbon)atom).provideEndsCount(ends.Length);
                }
            }
            for (int i = 0; i < ends.Length; i++) // for(int i=0; i<ends.Count;i++) // - pass i to numberCarbons so final 'if else' can check that all carbon numbers are filled if error occurs - 'else if (chain.Count(x => x.Name == "Carbon" && ((Carbon)x).CarbonNumber.Count != i+1 ) == 0) // no incomplete lists
            {
                numberCarbons(ends[i], i);
            }
            //chain.Count(x=>x.)
            Console.WriteLine("Done");
        }
        public void addFullBond(int index1, int index2, int bondorder)
        {
            chain[index1].addHalfBond(index2, bondorder);
            chain[index2].addHalfBond(index1, bondorder);
        }
        public int previousSimilarAtoms(int atomIndex)
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
            //Console.WriteLine(chain[rootAtomIndex].Name);     //debug
            //foreach(int i in chain[rootAtomIndex].bondIndexes) 
            //{
            //    Console.WriteLine(i);
            //}
            //Console.WriteLine(chain[rootAtomIndex].BondsFree);
            do
            {
                while (chain[rootAtomIndex].BondsFree > 0)
                {
                    int bondOrder = 1;
                    Console.WriteLine($"What {(rootAtom.MaxBonds > rootAtom.BondsFree ? "else " : "")}is bonded to {rootAtom.Name} number {previousSimilarAtoms(rootAtomIndex)}?\n{chain[rootAtomIndex].BondsFree} bonds left.");
                    switch (Console.ReadLine().ToUpper())
                    {
                        case "C":
                            chain.Add(new Carbon());
                            break;
                        case "O":
                            chain.Add(new Oxygen());
                            break;
                        case "N":
                            chain.Add(new Nitrogen());
                            break;
                        default:
                            chain.Add(new Hydrogen());
                            break;
                    }
                    int MaximumBonds = Math.Min(rootAtom.BondsFree, chain.Last().BondsFree);
                    bool validBondOrder = false;
                    if (MaximumBonds > 1)
                    {
                        Console.WriteLine($"How many bonds do these two atoms form?\nMaximum {MaximumBonds}.");
                        validBondOrder = int.TryParse(Console.ReadLine(), out bondOrder);
                    }
                    if(MaximumBonds == 1 || bondOrder>MaximumBonds || !validBondOrder)
                    {
                        bondOrder = 1;
                    }

                    addFullBond(rootAtomIndex, currentAtomIndex, bondOrder);
                    currentAtomIndex++;
                }

                rootAtomIndex++;
                rootAtom = chain[rootAtomIndex];

            } while (chain.Count(x => x.BondsFree > 0) > 0); //do while there are bonds to be made
            Console.WriteLine("All done.");
            Console.ReadLine();
        }
        private int alkylCounter(int index)
        {
            return chain[index].bondIndexes.Distinct().Count(x => x > 0 && chain[x].Name == "Carbon"); //hashset as double bonds were counted twice
        }
        private int alkylCounter(int index, HashSet<int> exclude)
        {
            return chain[index].bondIndexes.Distinct().Count(x => x > 0 && chain[x].Name == "Carbon" && !exclude.Contains(x));
        }
        private int nextCarbon(int index, HashSet<int> exclude)
        {
            return chain[index].bondIndexes.First(x => x > 0 && chain[x].Name == "Carbon" && !exclude.Contains(x));
        }
        private int[] findEnds()
        {
            List<int> ends = new List<int>();
            for (int j = 0; j < 2; j++)
            {
                for (int chainIndex = 0; chainIndex < chain.Count; chainIndex++)
                {
                    if (chain[chainIndex].Name == "Carbon" && !ends.Contains(chainIndex))
                    {
                        //Carbon currentCarbon = (Carbon)chain[chainIndex];
                        int alkylCount = alkylCounter(chainIndex);
                        //Console.WriteLine($"{chainIndex} has {alkylCount} alkyl groups");
                        if (alkylCount == 0) // isolated carbon ie R-O-C-O-R
                        {
                            ends.Add(chainIndex);
                        }
                        if (alkylCount == 1) //unique end
                        {
                            ends.Add(chainIndex);
                        }
                    }
                }
            }
            if (ends.Count == 0)
            {
                throw new Exception("No ends?");
            }
            return ends.ToArray();
        }
        private void numberCarbons(int startIndex, int end) // using a stack of 'toVisit' treats the tree as if its in one line, which makes keeping track of carbon number hard, so i would need a tuple to keep track of carbon number
        { // R-O-R' works as R and R' treated as ends
            int chainIndex = startIndex;
            int carbonNumber = 1;
            Queue<int> indexToReturnTo = new Queue<int>(); // tuple would have been here
            HashSet<int> visited = new HashSet<int>();
            while (true)
            {
                Console.WriteLine("Numbering carbon - "+carbonNumber);
                visited.Add(chainIndex);
                if (chain[chainIndex].Name == "Carbon")
                {
                    ((Carbon)chain[chainIndex]).CarbonNumber[end] = carbonNumber; // set carbon number -- realised i needed to enclose the whole thing in brackets
                    int unvisitedAdjacentCarbons = alkylCounter(chainIndex, visited);
                    if (unvisitedAdjacentCarbons >= 2) //unvisited branch exists here
                    {
                        indexToReturnTo.Enqueue(chainIndex);
                        chainIndex = nextCarbon(chainIndex, visited);
                    }
                    else if (unvisitedAdjacentCarbons == 1) //middle of branch
                    {
                        chainIndex = nextCarbon(chainIndex, visited);
                    }
                    else if (unvisitedAdjacentCarbons == 0 && indexToReturnTo.Count > 0) //dead end, return to branch
                    {
                        chainIndex = indexToReturnTo.Dequeue();
                        carbonNumber = ((Carbon)chain[chainIndex]).CarbonNumber[end]; //return the carbon number to what it was at the carbon returning to
                        chainIndex = nextCarbon(chainIndex, visited);
                    }
                    else //all carbon numbers in this chain filled - this condition is met immedietly if it is a lone carbon ie R-O-C-O-R
                    {
                        return;
                    }
                    carbonNumber++;
                }
            }
        }
        //private void displayGraph //cannot do as winforms does not work here
    }


}

