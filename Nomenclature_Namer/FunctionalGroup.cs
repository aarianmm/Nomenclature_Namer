using System;
namespace Nomenclature_Namer
{
    public class funcionalGroup
    {
        protected int alphebatism;
        protected int priority;
        public int Priority { get { return priority; } set { priority = value; } }
        //protected int branch;   //branches with functional groups not added yet
        protected string prefix;
        public string Prefix { get { return prefix; } }
        public string suffix;
        public string Suffix { get { return suffix; } }
        public funcionalGroup(int priority, string prefix, string suffix)
        {
            alphebatism = prefix[0] - 96;
        }

        public funcionalGroup(int priority, string name, bool prefixOnly)
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
            this.alphebatism = name[0] - 96;
        }

    }

}

