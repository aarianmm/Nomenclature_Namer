using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace Nomenclature_Namer
{
    public class IUPAC //check functional group lowest number, then total lowest number
    {

        private readonly List<FunctionalGroup> groups;
        private readonly ElementGraph atoms;
        public struct namingSpec //make elements readonly and add loadspec to here after
        {
            public Dictionary<string, (string prefix, string suffix, int priority)> prefixOrSuffix; //pref, suff, priority
            public Dictionary<string, ((string prefix, string suffix, int priority) middle, (string prefix, string suffix, int priority) end)> endDependentPrefixOrSuffix; //pref, suff, priority (MIDDLE), same for END
            public Dictionary<string, string> prefixOnly;
            public Dictionary<string, (string name, int priority)> middle;
            public string[] numericalPrefixes; //INCLUDE ZERO, AS NEEDED FOR DEFAULTS. SO, "", "", "di', "tri"
            public string[] alkylNames;
            /*public Dictionary<string, string> Prefix
            {
                get
                {
                    Dictionary<string, string> prefix = prefixOnly;
                    foreach (KeyValuePair<string, (string, string, int)> pref in prefixOrSuffix)
                    {
                        prefix.Add(pref.Key, pref.Value.Item1);
                    }
                    return prefix;
                }
            }
            public HashSet<string> AllGroups //broken
            {
                get
                {
                    HashSet<string> all = prefixOrSuffix.Keys.ToHashSet();
                    all.UnionWith(prefixOnly.Keys.ToHashSet());
                    all.UnionWith(middle.Keys.ToHashSet());
                    return all;
                }
            }
            public bool includePrefixForOne;
            public string[] NumericalPrefixes
            {
                get
                {
                    if (includePrefixForOne)
                    {
                        string newone = numericalPrefixes.
                    }
                }
            }*/
        }
        private namingSpec spec;
        public string[] names;
        private List<(List<int> path, List<List<int>> branches)> allPathsAndBranches;
        private string suffixFormula;
        private string suffixRoot;
        private bool suffixIsEnd;
        private bool suffixIsMiddle;
        //private string highestPrioritySuffixFormula;

        public IUPAC(ElementGraph atoms)
        {
            this.atoms = atoms;
            this.groups = atoms.Groups;

            spec.prefixOrSuffix = new Dictionary<string, (string, string, int)>();
            spec.prefixOrSuffix.Add("C≡N", ("cyano", "nitrile", 9));
            spec.prefixOrSuffix.Add("C-O", ("hydroxy", "ol", 2));
            spec.prefixOrSuffix.Add("", ("", "e", 0));
            spec.prefixOnly = new Dictionary<string, string>();
            spec.prefixOnly.Add("C-F", "fluoro");
            spec.prefixOnly.Add("C-Br", "bromo");
            spec.prefixOnly.Add("C-I", "iodo");
            spec.endDependentPrefixOrSuffix = new Dictionary<string, ((string prefix, string suffix, int priority) middle, (string prefix, string suffix, int priority) end)>();
            spec.endDependentPrefixOrSuffix.Add("C=O", (("oxo", "one", 5), ("formyl", "al", 6)));
            spec.middle = new Dictionary<string, (string, int)>();
            spec.middle.Add("C=C", ("en", 2));
            spec.middle.Add("C≡C", ("yn", 1));
            spec.middle.Add("", ("an", 0));
            spec.numericalPrefixes = new string[] { "", "", "di", "tri" };
            spec.alkylNames = new string[] { "", "meth", "eth", "prop", "but", "pent", "hex", "hept", "octo", "non", "dec" };
            //DisplaySpecDebug();
            CheckGroups(this.groups);

            //naming
            List<List<int>> longestPaths = atoms.FindEveryLongestPath();
            //Console.WriteLine($" at start {allPathsAndBranches.Count} paths");
            allPathsAndBranches = NarrowDownPathsByBranches(longestPaths);
            suffixFormula = "";
            suffixRoot = "";
            FindHighestPrioritySuffix();
            Console.WriteLine("suffix formula is " + suffixFormula);
            Console.WriteLine("suffix root is " + suffixRoot);
            if (suffixIsEnd)
            {
                Console.WriteLine("it is end");
            }
            if (suffixIsMiddle)
            {
                Console.WriteLine("it is middle");
            }
            
            //showpathdebug("start");
            if (longestPaths.Count == 0)
            {
                throw new Exception("Impossibe to name, as branches must be empty.");
            }
            if (longestPaths.Count != 1)
            {
                if (longestPaths.Count != 1)
                {
                    NarrowDownPathsBySuffix();
                    if (longestPaths.Count != 1)
                    {
                        NarrowDownPathsByMiddle();
                        if (longestPaths.Count != 1)
                        {
                            NarrowDownPathsByPrefixes();
                        }
                        if (longestPaths.Count != 1)
                        {
                            NarrowDownPathsByLength();
                        }
                    }
                }
            }
            names = ConstructName();
            names = names.Distinct().ToArray();
            foreach (string name in names)
            {
                Console.WriteLine(name);
            }
            showpathdebug("end");
        }
        private void showpathdebug(string s)
        {
            Console.WriteLine("at " + s);
            foreach (var i in allPathsAndBranches)
            {
                foreach (int j in i.path)
                {
                    Console.WriteLine(j);
                }
                Console.WriteLine("-----");
            }
        }
        private string[] ConstructName()
        {
            string[] names = new string[allPathsAndBranches.Count];
            for (int i = 0; i < allPathsAndBranches.Count; i++)
            {
                List<int> path = allPathsAndBranches[i].path;
                List<List<int>> branches = allPathsAndBranches[i].branches;
                string longestAlkylName = spec.alkylNames[path.Count];
                Dictionary<string, List<int>> prefixCarbons = FindPrefixCarbons(path, branches);
                Dictionary<string, List<int>> middleCarbons = FindMiddleCarbons(path);
                List<int> suffixCarbonsList = FindSuffixCarbons(path);
                Dictionary<string, List<int>> suffixCarbons = new Dictionary<string, List<int>> { { suffixRoot, suffixCarbonsList } };
                string prefixName = NameSegment(prefixCarbons);
                string middleName = NameSegment(middleCarbons);
                string suffixName = NameSegment(suffixCarbons);
                names[i] = prefixName + longestAlkylName + middleName + suffixName;
            }
            return names;
        }
        /*private void NarrowDownPathsByGroups() //brocken
        {
            int groupsCount = groups.Count;
            int[] counts = new int[allPathsAndBranches.Count];
            foreach(FunctionalGroup group in groups)
            {
                for (int i = 0; i < allPathsAndBranches.Count; i++)
                {
                    if (group is CarbonCarbonGroup)
                    {
                        CarbonCarbonGroup cgroup = (CarbonCarbonGroup)group;
                        if (cgroup.Involves(i))
                        {
                            counts[i]++;
                        }
                    }
                    else
                    {
                        if (group.Involves(i))
                        {
                            counts[i]++;
                        }
                    }
                }
            }
            for (int i = allPathsAndBranches.Count - 1; i >= 0; i--)
            {
                if (counts[i] != groupsCount)
                {
                    allPathsAndBranches.RemoveAt(i);
                }
            }
        }*/
        private List<(List<int> path, List<List<int>> branches)> NarrowDownPathsByBranches(List<List<int>> paths)
        {
            List<(List<int> path, List<List<int>> branches)> allPathsAndBranches = new List<(List<int> path, List<List<int>> branches)>();
            foreach (List<int> path in paths)
            {
                List<List<int>> branches = atoms.FindBranches(path);
                allPathsAndBranches.Add((path, branches));
            }
            for (int i = allPathsAndBranches.Count - 1; i >= 0; i--)
            {
                List<List<int>> branches = allPathsAndBranches[i].branches;
                if (!CheckBranchValidity(branches))
                {
                    allPathsAndBranches.RemoveAt(i);
                }
            }
            return allPathsAndBranches;
        }
        private void NarrowDownPathsByPrefixes()
        {
            List<int> prefixesCarbonSums = new List<int>();
            for (int i = 0; i < allPathsAndBranches.Count; i++)
            {
                List<int> path = allPathsAndBranches[i].path;
                List<List<int>> branches = allPathsAndBranches[i].branches;
                List<List<int>> prefixCarbons = FindPrefixCarbons(path, branches).Values.ToList();
                int prefixCarbonSum = 0;
                foreach (List<int> a in prefixCarbons)
                {
                    prefixCarbonSum += a.Sum();
                }
                prefixesCarbonSums.Add(prefixCarbonSum);
            }
            int lowestSum = prefixesCarbonSums.Min();
            for (int i = allPathsAndBranches.Count -1 ; i >= 0; i--)
            {
                if (prefixesCarbonSums[i] != lowestSum)
                {
                    allPathsAndBranches.RemoveAt(i);
                }
            }
        }
        private void NarrowDownPathsByMiddle()
        {
            string middlePriorityFormula = FindHighestPriorityMiddleFormula();
            string middlePriorityName = spec.middle[middlePriorityFormula].name;
            List<int> middleCarbonSums = new List<int>();
            for (int i = 0; i < allPathsAndBranches.Count; i++)
            {
                List<int> path = allPathsAndBranches[i].path;
                List<int> middleCarbons = FindMiddleCarbons(path)[middlePriorityName];
                int middleCarbonSum = middleCarbons.Sum();
                middleCarbonSums.Add(middleCarbonSum);
            }
            int lowestSum = middleCarbonSums.Min();
            for (int i = allPathsAndBranches.Count -1; i >= 0; i--)
            {
                if (middleCarbonSums[i] != lowestSum)
                {
                    allPathsAndBranches.RemoveAt(i);
                }
            }
        } //PROBLEM. narrowing down doesnt account for the multiple carbon indexes
        private void NarrowDownPathsBySuffix()
        {
            List<int> suffixCarbonSums = new List<int>();
            for (int i = 0; i < allPathsAndBranches.Count; i++)
            {
                List<int> path = allPathsAndBranches[i].path;
                List<int> suffixCarbons = FindSuffixCarbons(path);
                int suffixCarbonSum = suffixCarbons.Sum();
                suffixCarbonSums.Add(suffixCarbonSum);
            }
            int lowestSum = suffixCarbonSums.Min();
            for (int i = allPathsAndBranches.Count -1; i >= 0; i--) //negative due to issues changing the list size as removed
            {
                if (suffixCarbonSums[i] != lowestSum)
                {
                    allPathsAndBranches.RemoveAt(i);
                }
            }
        }
        private void NarrowDownPathsByLength()
        {
            int longest = allPathsAndBranches.MaxBy(x => x.path.Count).path.Count;
            //int longest = 0;
            //foreach((List<int> path, List<List<int>> branches) a in allPathsAndBranches)
            //{
            //    if(a.path.Count > longest)
            //    {
            //        longest = a.path.Count;
            //    }
            //}
            for (int i = allPathsAndBranches.Count -1; i >= 0; i--)
            {
                if (allPathsAndBranches[i].path.Count < longest)
                {
                    allPathsAndBranches.RemoveAt(i);
                }
            }
        }
        private Dictionary<string, List<int>> FindPrefixCarbons(List<int> path, List<List<int>> branches) //carbon numbers are REDUNDANT
        {
            Dictionary<string, List<int>> prefixesAndIndexes = new Dictionary<string, List<int>>(); //DO FOR SUFFIX!!!
            foreach (FunctionalGroup group in groups)
            {
                string name = "";
                int carbonNumber = path.IndexOf(group.MainIndex) + 1;
                if (group.GroupFormula != suffixFormula)
                {
                    if (spec.prefixOnly.ContainsKey(group.GroupFormula))
                    {
                        name = spec.prefixOnly[group.GroupFormula];
                    }
                    else if (spec.prefixOrSuffix.ContainsKey(group.GroupFormula))
                    {
                        name = spec.prefixOrSuffix[group.GroupFormula].prefix;
                    }
                }
                else if (spec.endDependentPrefixOrSuffix.ContainsKey(group.GroupFormula))
                {
                    if (atoms.IsAnEnd(group.MainIndex) && !(suffixIsEnd && group.GroupFormula == suffixFormula)) //group is on the end and isnt the same as the suffix one
                    {
                        name = spec.endDependentPrefixOrSuffix[group.GroupFormula].end.prefix;
                    }
                    else if (!atoms.IsAnEnd(group.MainIndex) && !(suffixIsMiddle && group.GroupFormula == suffixFormula)) //group is in the middle and isnt the same as the suffix one
                    {
                        name = spec.endDependentPrefixOrSuffix[group.GroupFormula].middle.prefix;
                    }
                }
                if (name != "")
                {
                    if (prefixesAndIndexes.ContainsKey(name))
                    {
                        prefixesAndIndexes[name].Add(carbonNumber);
                    }
                    else
                    {
                        prefixesAndIndexes.Add(name, new List<int> { carbonNumber });
                    }
                }
            }
            foreach (List<int> branch in branches)
            {
                int carbonNumber = path.IndexOf(branch[0]) + 1;
                string name = spec.alkylNames[branch.Count - 1];
                if (prefixesAndIndexes.ContainsKey(name))
                {
                    prefixesAndIndexes[name].Add(carbonNumber);
                }
                else
                {
                    prefixesAndIndexes.Add(name, new List<int> { carbonNumber });
                }
            }
            return prefixesAndIndexes;
        }
        private Dictionary<string, List<int>> FindMiddleCarbons(List<int> path) //carbon numbers are REDUNDANT
        {
            Dictionary<string, List<int>> middleAndIndexes = new Dictionary<string, List<int>>();
            foreach (FunctionalGroup group in groups)
            {
                if (group is CarbonCarbonGroup)
                {
                    CarbonCarbonGroup cgroup = (CarbonCarbonGroup)group;
                    int carbonNumber = -1;
                    int carbonNumberOne = path.IndexOf(cgroup.MainIndex) + 1;
                    int carbonNumberTwo = path.IndexOf(cgroup.OtherCarbonIndex) + 1;
                    if (carbonNumberOne < carbonNumberTwo)
                    {
                        carbonNumber = carbonNumberOne;
                    }
                    else
                    {
                        carbonNumber = carbonNumberTwo;
                    }
                    string name = spec.middle[group.GroupFormula].name;
                    if (middleAndIndexes.ContainsKey(name))
                    {
                        middleAndIndexes[name].Add(carbonNumber);
                    }
                    else
                    {
                        middleAndIndexes.Add(name, new List<int> { carbonNumber });
                    }
                }
            }
            if (middleAndIndexes.Count == 0)
            {
                middleAndIndexes.Add(spec.middle[""].name, new List<int>());
            }
            return middleAndIndexes;
        }
        private List<int> FindSuffixCarbons(List<int> path)
        {
            List<int> indexes = new List<int>();
            foreach (FunctionalGroup group in groups)
            {
                bool endsAreInvolved = suffixIsMiddle || suffixIsEnd;
                bool groupAndSuffixAreMiddle = suffixIsMiddle && !atoms.IsAnEnd(group.MainIndex);
                bool groupAndSuffixAreEnd = suffixIsEnd && atoms.IsAnEnd(group.MainIndex);
                if (group.GroupFormula == suffixFormula && (!endsAreInvolved || groupAndSuffixAreMiddle || groupAndSuffixAreEnd))
                { 
                    indexes.Add(path.IndexOf(group.MainIndex) + 1);
                }
            }
            return indexes;
        }
        private string NameSegment(Dictionary<string, List<int>> namesAndCarbonNumbers)
        {
            List<(string numbers, string name)> names = new List<(string, string)>();
            foreach (KeyValuePair<string, List<int>> groupData in namesAndCarbonNumbers)
            {
                string name = groupData.Key;
                List<int> carbonNumbers = groupData.Value;
                carbonNumbers.Sort();
                string numericalPrefix = spec.numericalPrefixes[carbonNumbers.Count];
                string numbers = "";
                for (int i = 0; i < carbonNumbers.Count; i++)
                {
                    numbers += "," + carbonNumbers[i];
                }
                numbers += numericalPrefix;
                names.Add((numbers, name));
            }
            names = names.OrderBy(x => x.name).ToList();
            string nameSegment = "";
            for (int i = 0; i < names.Count; i++)
            {
                nameSegment += names[i].numbers + names[i].name;
            }
            return nameSegment;
        }
        private bool CheckBranchValidity(List<List<int>> branches)
        {
            foreach (List<int> branch in branches)
            {
                for(int i=1;i<branch.Count; i++)
                {
                    foreach (FunctionalGroup group in groups)
                    {
                        if (group.Involves(branch[i]) || atoms.AlkylCounter(branch[i]) > 2)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private void DisplaySpecDebug()
        {
            Console.WriteLine("suffix");
            foreach (string s in spec.prefixOrSuffix.Keys)
            {
                Console.WriteLine(s);
            }
            Console.WriteLine("-----");
            Console.WriteLine("prefix");
            //foreach (string s in spec.prefixOnly.Keys)
            //{
            //    Console.WriteLine(s);
            //}
            Console.WriteLine("-----");
            Console.WriteLine("middle");
            foreach (string s in spec.middle.Keys)
            {
                Console.WriteLine(s);
            }
        }
        //public static void SaveSpecification(string fileName,namingSpec spec)
        //{
        //    fileName += ".ExamSpec";
        //    short prefixCount = Convert.ToInt16(prioritySuffix.Length); //less than 32,000
        //    short prefixLength = Convert.ToInt16(priorityPrefix.Length);
        //    using (BinaryWriter writefile = new BinaryWriter(File.Open(fileName, FileMode.Create)))
        //    {
        //        writefile.Write(suffixLength);
        //        writefile.Write(prefixLength);
        //        for (int i = 0; i < suffixLength; i++)
        //        {
        //            writefile.Write(prioritySuffix[i]);
        //        }
        //        for (int i = 0; i < prefixLength; i++)
        //        {
        //            writefile.Write(priorityPrefix[i]);
        //        }
        //        writefile.Close();
        //    }
        //}
        //public static void SaveSpecification(string fileName, string[] prioritySuffix, string[] priorityPrefix)
        //{
        //    fileName += ".ExamSpec";
        //    short suffixLength = Convert.ToInt16(prioritySuffix.Length);
        //    short prefixLength = Convert.ToInt16(priorityPrefix.Length);
        //    using (BinaryWriter writefile = new BinaryWriter(File.Open(fileName, FileMode.Create)))
        //    {
        //        writefile.Write(suffixLength);
        //        writefile.Write(prefixLength);
        //        for(int i=0; i<suffixLength; i++)
        //        {
        //            writefile.Write(prioritySuffix[i]);
        //        }
        //        for (int i = 0; i < prefixLength; i++)
        //        {
        //            writefile.Write(priorityPrefix[i]);
        //        }
        //        writefile.Close();
        //    }
        //}
        private (string[], string[]) LoadSpecification(string fileName)
        {
            fileName += ".ExamSpec";
            string[] suffix;
            string[] prefix;
            using (BinaryReader writefile = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                short suffixLength = writefile.ReadInt16();
                short prefixLength = writefile.ReadInt16();
                suffix = new string[suffixLength];
                prefix = new string[prefixLength];
                for (int i = 0; i < suffixLength; i++)
                {
                    suffix[i] = writefile.ReadString();
                }
                for (int i = 0; i < prefixLength; i++)
                {
                    prefix[i] = writefile.ReadString();
                }
                writefile.Close();
            }
            return (suffix, prefix);
        }
        private void CheckGroups(List<FunctionalGroup> groups)
        {

            foreach (FunctionalGroup g in groups) //non c-o-c or c==c
            {
                if (!spec.prefixOnly.ContainsKey(g.GroupFormula) && !spec.prefixOrSuffix.ContainsKey(g.GroupFormula) && !spec.endDependentPrefixOrSuffix.ContainsKey(g.GroupFormula) && !spec.middle.ContainsKey(g.GroupFormula))
                {
                    throw new Exception($"{g.GroupFormula} is unrecognised");
                }
            }
        }
        private void FindHighestPrioritySuffix()
        {
            //return groups.MaxBy(x => spec.prefixOrSuffix[x.GroupFormula].priority).GroupFormula;
            int priority = -1;
            foreach (FunctionalGroup fg in groups)
            {
                int currentPriority;
                if (spec.prefixOrSuffix.ContainsKey(fg.GroupFormula))
                {
                    currentPriority = spec.prefixOrSuffix[fg.GroupFormula].priority;
                }
                else if (spec.endDependentPrefixOrSuffix.ContainsKey(fg.GroupFormula))
                {
                    
                    if (atoms.IsAnEnd(fg.MainIndex))
                    {
                        currentPriority = spec.endDependentPrefixOrSuffix[fg.GroupFormula].end.priority;
                    }
                    else
                    {
                        currentPriority = spec.endDependentPrefixOrSuffix[fg.GroupFormula].middle.priority;
                    }
                }
                else
                {
                    continue;
                }
                if (currentPriority > priority)
                {
                    priority = currentPriority;
                }
            }
            foreach (KeyValuePair<string, (string, string, int priority)> v in spec.prefixOrSuffix)
            {
                if (v.Value.priority == priority)
                {
                    suffixFormula = v.Key;
                    suffixRoot = spec.prefixOrSuffix[v.Key].suffix;
                }
            }
            foreach (KeyValuePair<string, ((string, string, int priority) middle, (string, string, int priority) end)> v in spec.endDependentPrefixOrSuffix)
            {
                if (v.Value.middle.priority == priority)
                {
                    suffixIsMiddle = true;
                    suffixIsEnd = false;
                    suffixFormula = v.Key;
                    suffixRoot = spec.endDependentPrefixOrSuffix[v.Key].middle.suffix;
                }
                if (v.Value.end.priority == priority)
                {
                    suffixIsMiddle = false;
                    suffixIsEnd = true;
                    suffixFormula = v.Key;
                    suffixRoot = spec.endDependentPrefixOrSuffix[v.Key].end.suffix;
                }
            }
        }
        private string FindHighestPriorityMiddleFormula()
        {
            //return groups.MaxBy(x => spec.middle[x.GroupFormula].priority).GroupFormula;
            int priority = -1;
            foreach (FunctionalGroup fg in groups)
            {
                if (spec.middle.ContainsKey(fg.GroupFormula))
                {
                    int currentPriority = spec.middle[fg.GroupFormula].priority;
                    if (currentPriority > priority)
                    {
                        priority = currentPriority;
                    }
                }
            }
            string formula = "";
            foreach (KeyValuePair<string, (string, int)> v in spec.middle)
            {
                if (v.Value.Item2 == priority)
                {
                    formula = v.Key;
                }
            }
            return formula;
        }
    }
}

