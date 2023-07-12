using System;
namespace Nomenclature_Namer
{
    public abstract class Element
    {
        public int[] bondIndexes; //make protected
        protected string name;
        public string Name { get { return name; } }
        public int MaxBonds { get { return bondIndexes.Length; } }
        public int BondsFree { get { return Array.FindAll(bondIndexes, i => i == -1).Length; } }
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
        private List<int> carbonNumber;
        public List<int> CarbonNumber { get { return carbonNumber; } set { carbonNumber = value; } }
        public Carbon(int bondIndex, int bondOrder)
        {
            bondIndexes = new int[4];
            Array.Fill(bondIndexes, -1);
            addHalfBond(bondIndex, bondOrder);
            name = "Carbon";

            carbonNumber = new List<int>();
        }
        public Carbon()
        {
            bondIndexes = new int[4];
            Array.Fill(bondIndexes, -1);
            name = "Carbon";

            carbonNumber = new List<int>();
        }
    }
    public class Hydrogen : Element
    {
        public Hydrogen(int bondIndex)
        {
            bondIndexes = new int[1];
            Array.Fill(bondIndexes, -1);
            addHalfBond(bondIndex);
            name = "Hydrogen";
        }
        public Hydrogen()
        {
            bondIndexes = new int[1];
            Array.Fill(bondIndexes, -1);
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
        public Oxygen()
        {
            bondIndexes = new int[2];
            Array.Fill(bondIndexes, -1);
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
        public Nitrogen()
        {
            bondIndexes = new int[3];
            Array.Fill(bondIndexes, -1);
            name = "Nitrogen";
        }

    }


}

