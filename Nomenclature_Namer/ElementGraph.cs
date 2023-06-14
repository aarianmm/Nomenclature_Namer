using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Nomenclature_Namer
{

    public class ElementGraph
    {

        public List<Element> chain;

        public ElementGraph()
        {
            chain = new List<Element>();
            //compileElementGraph(SF);
            Carbon root = new Carbon();
            SpellOutAtoms(root);
        }
        public void addFullBond(int index1, int index2, int bondorder)
        {
            chain[index1].addHalfBond(index2, bondorder);
            chain[index2].addHalfBond(index1, bondorder);
        }
        public int previousSimilarAtoms(int atomIndex)
        {
            int count = 1;
            for(int i =0; i<atomIndex; i++)
            {
                if (chain[i].Name == chain[atomIndex].Name)
                {
                    count++;
                }
            }
            return count;
        }
        public void SpellOutElements()
        {
            
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
                    Console.WriteLine($"What is bonded to {rootAtom.Name} number {previousSimilarAtoms(rootAtomIndex)}?\n{chain[rootAtomIndex].BondsFree} bonds left.");
                    switch (Console.ReadLine())
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
                        case "H":
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
                rootAtomIndex++;
                rootAtom = chain[rootAtomIndex];
            }
        }

        //private void compileElementGraph(string SF)
        //{

        //}
    }
}

