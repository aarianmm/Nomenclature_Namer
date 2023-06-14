using System;
using System.Xml.Linq;

namespace Nomenclature_Namer
{

    public class ElementGraph
    {

        public List<Element> chain;

        public ElementGraph(string SF)
        {
            //chain = new Element[SF.Length];
            compileElementGraph(SF);
        }
        public void addFullBond(int index1, int index2, int bondorder)
        {
            chain[index1].addHalfBond(index2, bondorder);
            chain[index2].addHalfBond(index1, bondorder);
        }
        public void SpellOutElements()
        {
            //Console.WriteLine("How many atoms are in this organic molecule?");
            //chain = new Element[int.Parse(Console.ReadLine())];
            chain = new List<Element>();
            chain[0] = new Carbon();
            for (int i = 0; i < chain[0].BondsFree; i++)
            {
                SpellOutBranch(chain[0], i + 1, 0); //?
            }
        }
        private void SpellOutBranch(Element rootElement, int bond, int chainIndex)
        {
            Element elementToAdd;
            Console.WriteLine($"What is bonded to this {rootElement.Name}?\nBond {bond}.");
            string elementChar = Console.ReadLine();
            Console.WriteLine($"How many bonds do these two atoms form?");
            int bondOrder = int.Parse(Console.ReadLine());
            switch (Console.ReadLine())
            {
                case "C":
                    elementToAdd = new Carbon(); //idk
                    break;
                case "O":
                    elementToAdd = new Oxygen(chainIndex, bondOrder);
                    break;
                case "N":
                    elementToAdd = new Nitrogen(chainIndex, bondOrder);
                    break;

            }
        }

        private void compileElementGraph(string SF)
        {

        }
    }
}

