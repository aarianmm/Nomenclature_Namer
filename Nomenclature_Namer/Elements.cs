using System;
using System.Xml.Serialization;
namespace Nomenclature_Namer
{
    public class Element
    {
        private int[] bondIndexes;
        public int[] BondIndexes { get { return bondIndexes; } }
        private string name;
        public string Name { get { return name; } }
        private string symbol;
        public string Symbol { get { return symbol; } }
        public int MaxBonds { get { return bondIndexes.Length; } }
        public int BondsFree { get { return Array.FindAll(bondIndexes, i => i == -1).Length; } }
        public Element(string symbol, string name, int valency)
        {
            this.symbol = symbol;
            this.name = name;
            bondIndexes = new int[valency];
            Array.Fill(bondIndexes, -1);
        }
        public void AddHalfBond(int bondIndex, int bondOrder) //bond order; 1 = single, 2 = double etc
        {
            for (int i = 0; i < bondOrder; i++)
            {
                bondIndexes[Array.FindIndex(bondIndexes, i => i == -1)] = bondIndex;        //https://stackoverflow.com/questions/5400895/using-c-sharp-linq-to-return-first-index-of-null-empty-occurrence-in-an-array            }
            }
        }
    }
    /*
    public class Carbon : Element
    {
        //public string Symbol { get { return name.Substring(0,1); } }
        private int[] carbonNumbers;
        public int[] CarbonNumbers
        {
            get
            {
                return carbonNumbers;
            }
            set
            {
                carbonNumbers = value;
            }
        }
        private int definiteCarbonNumber;
        public int DefiniteCarbonNumber
        {
            get
            {
                if (!CarbonNumberDecided)
                {
                    throw new Exception("Cannot retrieve the definite carbon number before it has been decided");
                }
                return definiteCarbonNumber;
            }
        }
        private bool EndCountsProvided { get { return carbonNumbers.Length != 0; } }
        private bool CarbonNumberDecided { get { return definiteCarbonNumber != -1; } }

        public Carbon()
        {
            bondIndexes = new int[4];
            Array.Fill(bondIndexes, -1);
            name = "Carbon";
            symbol = "C";
            carbonNumbers = new int[0];
            definiteCarbonNumber = -1;
        }
        public void ProvideEndsCount(int endsCount)
        {
            if (EndCountsProvided)
            {
                throw new Exception("End counts have already been provided for this carbon");
            }
            carbonNumbers = new int[endsCount];
        }
        public void DecideDefiniteCarbonNumber(int endIndex)
        {
            if (!EndCountsProvided)
            {
                throw new Exception("Trying to decide definite carbon number before end counts have been provided");
            }
            definiteCarbonNumber = carbonNumbers[endIndex];
        }
        public void NarrowDownCarbonNumber(List<int> endsIndexes)
        {
            List<int> newNums = new List<int>();
            foreach(int n in endsIndexes)
            {
                newNums.Add(carbonNumbers[n]);
            }
            carbonNumbers = newNums.ToArray();
        }
    }
    public class Hydrogen : Element
    {
        public Hydrogen()
        {
            bondIndexes = new int[1];
            Array.Fill(bondIndexes, -1);
            name = "Hydrogen";
            symbol = "H";
        }
    }
    public class Oxygen : Element
    {
        public Oxygen()
        {
            bondIndexes = new int[2];
            Array.Fill(bondIndexes, -1);
            name = "Oxygen";
            symbol = "O";
        }
    }
    public class Nitrogen : Element
    {
        public Nitrogen()
        {
            bondIndexes = new int[3];
            Array.Fill(bondIndexes, -1);
            name = "Nitrogen";
            symbol = "N";
        }

    }
    */
}