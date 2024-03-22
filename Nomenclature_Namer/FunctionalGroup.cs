using System;
namespace Nomenclature_Namer
{
    public class FunctionalGroup
    {
        protected string elementSymbol;
        public string ElementSymbol { get { return elementSymbol; } }
        protected char bondSymbol;
        public virtual string GroupFormula { get { return "C" + bondSymbol + elementSymbol; } }
        protected int mainIndex;
        public int MainIndex { get { return mainIndex; } }
        public FunctionalGroup(string elementSymbol, int bondOrder, int mainIndex)
        {
            this.elementSymbol = elementSymbol;
            this.mainIndex = mainIndex;
            switch (bondOrder)
            {
                case 1:
                    bondSymbol = '-';
                    break;
                case 2:
                    bondSymbol = '=';
                    break;
                case 3:
                    bondSymbol = '≡';
                    break;
                case 4:
                    bondSymbol = '≣';
                    break;
                default:
                    throw new Exception("Unknown bond order. Bonds are between order 1 and 4");
                    break;
            }
        }
        virtual public bool Involves(int index)
        {
            return mainIndex == index;
        }
    }
    public class CarbonCarbonGroup : FunctionalGroup
    {
        int otherCarbonIndex;
        public int OtherCarbonIndex { get { return otherCarbonIndex; } }
        public CarbonCarbonGroup(int bondOrder, int carbonIndex, int otherCarbonIndex) : base("C", bondOrder, carbonIndex)
        {
            this.otherCarbonIndex = otherCarbonIndex;
        }
        public bool IsSame(CarbonCarbonGroup otherGroup)
        {
            return (otherGroup.mainIndex == mainIndex && otherGroup.otherCarbonIndex == otherCarbonIndex) || (otherGroup.mainIndex == otherCarbonIndex && otherGroup.otherCarbonIndex == mainIndex);
        }
        override public bool Involves(int index)
        {
            return mainIndex == index || otherCarbonIndex == index;
        }
    }
    public class MergedGroup : FunctionalGroup
    {
        string formula;
        public override string GroupFormula { get { return formula; } }
        public MergedGroup(string formula, int carbonIndex) : base("", 1, carbonIndex)
        {
            this.formula = formula;
        }
    }
    public class CarbonOtherCarbonGroup : FunctionalGroup //future proofing
    {
        int[] carbonIndexes;
        public override string GroupFormula { get { return "C" + bondSymbol + elementSymbol + bondSymbol + "C"; } }
        public CarbonOtherCarbonGroup(string centralElementSymbol, int centralAtomIndex, int[] carbonIndexes) : base(centralElementSymbol, 1, centralAtomIndex)
        {
            this.carbonIndexes = carbonIndexes;
        }
        override public bool Involves(int index)
        {
            return carbonIndexes.Contains(index);
        }
    }
}