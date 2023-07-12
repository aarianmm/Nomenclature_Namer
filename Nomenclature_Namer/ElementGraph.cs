using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;

namespace Nomenclature_Namer
{

    public class ElementGraph
    {
        public List<Element> chain;
        private int startIndex;

        public ElementGraph()
        {
            chain = new List<Element>();
            //compileElementGraph(SF);
            Carbon root = new Carbon();
            SpellOutAtoms(root);
            startIndex = findStart();
            numberCarbons();
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
            while (Console.ReadLine() != "!")
            {
                while (chain[rootAtomIndex].BondsFree > 0)
                {
                    int bondOrder;
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
                    if (MaximumBonds > 1)
                    {
                        Console.WriteLine($"How many bonds do these two atoms form?\nMaximum {MaximumBonds}.");
                        bondOrder = int.Parse(Console.ReadLine());
                    }
                    else
                    {
                        bondOrder = 1;
                    }

                    addFullBond(rootAtomIndex, currentAtomIndex, bondOrder);
                    currentAtomIndex++;
                }
                if (chain.Count(x => x.BondsFree > 0) == 0) //all full
                {
                    Console.WriteLine("All done.");
                    Console.ReadLine();
                    return;
                }

                rootAtomIndex++;
                rootAtom = chain[rootAtomIndex];
            }
        }
        private int alkylCounter(int index)
        {
            return chain[index].bondIndexes.ToHashSet().Count(x => x > 0 && chain[x].Name == "Carbon"); //hashset as double bonds were counted twice
        }
        private int alkylCounter(int index, HashSet<int> exclude)
        {
            return chain[index].bondIndexes.ToHashSet().Count(x => x > 0 && chain[x].Name == "Carbon" && !exclude.Contains(x));
        }
        private int nextCarbon(int index, HashSet<int> exclude)
        {
            return chain[index].bondIndexes.First(x => x > 0 && chain[x].Name == "Carbon" && !exclude.Contains(x));
        }
        private int findStart()
        {
            for (int chainIndex = 0; chainIndex < chain.Count; chainIndex++)
            {
                if (chain[chainIndex].Name == "Carbon")
                {
                    //Carbon currentCarbon = (Carbon)chain[chainIndex];
                    int alkylCount = alkylCounter(chainIndex);
                    //Console.WriteLine($"{chainIndex} has {alkylCount} alkyl groups");
                    if (alkylCount == 0 || alkylCount == 1) // isolated carbon or at the start of the chain
                    {
                        return chainIndex;
                    }
                }
            }
            return -1; //no start?
        }
        private void numberCarbons()
        {
            Queue<int> indexToReturnTo = new Queue<int>(); //might need tuple idk
            int chainIndex = startIndex;
            int carbonNumber = 1;
            HashSet<int> visited = new HashSet<int>();
            while (true)
            {
                //Console.WriteLine(chainIndex);
                if (chain[chainIndex].Name == "Carbon")
                {
                    ((Carbon)chain[chainIndex]).CarbonNumber = carbonNumber; // set carbon number -- reealised i needed to enclose the whole thing in brackets
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
                        carbonNumber = ((Carbon)chain[chainIndex]).CarbonNumber; //return the carbon number to before
                        chainIndex = nextCarbon(chainIndex, visited);
                    }
                    else if (chain.Count(x => x.Name == "Carbon" && ((Carbon)x).CarbonNumber == -1) == 0) //all carbon numbers filled
                    {
                        return;
                    }
                    else //dead end but all arent filled
                    {
                        throw new Exception("Error"); //fix this
                    }
                    carbonNumber++;
                }
            }
        }

        //private void compileElementGraph(string SF)
        //{

        //}
    }
}

