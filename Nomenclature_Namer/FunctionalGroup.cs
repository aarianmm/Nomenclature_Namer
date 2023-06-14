using System;
namespace Nomenclature_Namer
{
    public class funcionalGroup
    {
        protected int alphebatism;
        protected int priority;
        public int Priority { get { return priority; } set { priority = value; } }
        protected int branch;   //branches with functional groups not added yet
        protected string prefix;
        public string Prefix { get { return prefix; } }
        public string suffix;
        public string Suffix { get { return suffix; } }
        public funcionalGroup(int branch, int priority, string prefix, string suffix)
        {
            alphebatism = prefix[1] - 96;
        }

        public funcionalGroup(int branch, int priority, string name, bool prefixOnly)
        {
            if (prefixOnly)
            {
                prefix = name;
                suffix = "!";
            }
            else
            {
                prefix = "!";
                suffix = name;
            }

            this.priority = priority;
            this.alphebatism = name[1] - 96;
        }

        public void addBond(int index)
        {
            for (int i = 0; i < bondIndexes.Length; i++)
            {
                if (bondIndexes[i] == 0)
                {
                    bondIndexes[i] = index;
                }
            }
        }
    }

}

