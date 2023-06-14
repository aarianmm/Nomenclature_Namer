using System;
namespace Nomenclature_Namer
{
    public abstract class Element
    {
        protected int[] bondIndexes;
        protected string name;
        public string Name { get { return name; } }
        public int BondsFree { get { return Array.FindAll(bondIndexes, i => i != -1).Length; } }
        public void addHalfBond(int bondIndex, int bondOrder) //bond order; 1 = single, 2 = double etc
        {
            for (int i = 0; i < bondOrder; i++)
            {
                bondIndexes[Array.FindIndex(bondIndexes, i => i == -1)] = bondIndex;        //https://stackoverflow.com/questions/5400895/using-c-sharp-linq-to-return-first-index-of-null-empty-occurrence-in-an-array            }

            }
        }
        public void addHalfBond(int bondIndex)
        {
            addHalfBond(bondIndex, 1);
        }
        //protected int[] createBondIndexesArray(int size)
        //{
        //    bondIndexes = new int[size];
        //    Array.Fill(bondIndexes, -1);
        //}
    }
    public class Carbon : Element
    {
        public Carbon()
        {
            bondIndexes = new int[4];
            Array.Fill(bondIndexes, -1);
            name = "Carbon";
        }
    }
    public class Hydrogen : Element
    {
        public Hydrogen(int bondIndex)
        {
            bondIndexes = new int[4];
            Array.Fill(bondIndexes, -1);
            addHalfBond(bondIndex);
            name = "Hydrogen";
        }
    }
    public class Oxygen : Element
    {
        public Oxygen(int bondIndex, int bondOrder)
        {
            bondIndexes = new int[2];
            Array.Fill(bondIndexes, -1);
            addHalfBond(bondIndex, bondOrder);
            name = "Oxygen";
        }
    }
    public class Nitrogen : Element
    {
        public Nitrogen(int bondIndex, int bondOrder)
        {
            bondIndexes = new int[3];
            Array.Fill(bondIndexes, -1);
            addHalfBond(bondIndex, bondOrder);
            name = "Nitrogen";
        }

    }


}

