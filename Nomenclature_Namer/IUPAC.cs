using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Nomenclature_Namer
{
    public class IUPAC
    {
        private readonly List<FunctionalGroup> groups;
        private readonly ElementGraph atoms;
        public struct namingSpec
        {
            public string[] alkylNames;
            public string[] numericalPrefixes;
            public Dictionary<HashSet<string>, string> merging; //C-O and C=O to COOH
            public Dictionary<string, string> prefixOnly;
            public Dictionary<string, (string name, int priority)> middle;
            public Dictionary<string, (string prefix, string suffix, int priority)> prefixOrSuffix; //pref, suff, priority
            public Dictionary<string, ((string prefix, string suffix, int priority) middle, (string prefix, string suffix, int priority) end)> endDependentPrefixOrSuffix;
        }
        private readonly namingSpec spec;
        public string[] names;
        private List<(List<int> path, List<List<int>> branches)> allPathsAndBranches;
        private string suffixFormula;
        private string suffixRoot;
        private bool suffixIsEnd;
        private bool suffixIsMiddle;

        public IUPAC(string specFileName, ElementGraph atoms)
        {
            spec = LoadSpecification(specFileName);
            this.atoms = atoms;
            atoms.MergeFunctionalGroups(spec.merging);
            groups = atoms.Groups;
            //atoms.DisplayGroupsDebug();
            //Console.WriteLine("suffix is end" + suffixIsEnd.ToString());
            //Console.WriteLine("suffix is middle" + suffixIsMiddle.ToString());
            CheckGroups(groups);
            //naming
            List<List<int>> longestPaths = atoms.FindEveryLongestPath();
            allPathsAndBranches = NarrowDownPathsByBranches(longestPaths);
            suffixFormula = "";
            suffixRoot = "";
            FindHighestPrioritySuffix();
            if (longestPaths.Count == 0)
            {
                throw new Exception("Impossibe to name, as branches must be empty.");
            }
            if (longestPaths.Count != 1)
            {
                NarrowDownPathsByLength();
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
                    }
                }
            }
            names = ConstructName();
            names = names.Distinct().ToArray();
            foreach (string name in names)
            {
                Console.WriteLine(name);
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
                names[i] = FormatName(prefixName + longestAlkylName + middleName + suffixName);
            }
            return names;
        }
        private string FormatName(string name)
        {
            name = name.ToLower();
            Regex vowels = new Regex(@"\|(.)(?=[^a-z]*[aeiouy])"); //optional vowel followed by vowel is removed
            Regex startDashes = new Regex(@"([a-z])(\d)");
            Regex endDashes = new Regex(@"(\d)([a-z])");
            name = vowels.Replace(name, "");
            name = name.Replace("|", "");
            name = startDashes.Replace(name, (m) => m.Groups[1].Value + "-" + m.Groups[2].Value);
            name = endDashes.Replace(name, (m) => m.Groups[1].Value + "-" + m.Groups[2].Value);
            return name;
        }
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
                    allPathsAndBranches.RemoveAt(i); // branch invalid, so whole path invalid
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
            for (int i = allPathsAndBranches.Count - 1; i >= 0; i--)  //negative iter. as mutating
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
            for (int i = allPathsAndBranches.Count - 1; i >= 0; i--)
            {
                if (middleCarbonSums[i] != lowestSum)
                {
                    allPathsAndBranches.RemoveAt(i);
                }
            }
        }
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
            for (int i = allPathsAndBranches.Count - 1; i >= 0; i--)
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
            for (int i = allPathsAndBranches.Count - 1; i >= 0; i--)
            {
                if (allPathsAndBranches[i].path.Count < longest)
                {
                    allPathsAndBranches.RemoveAt(i);
                }
            }
        }
        private Dictionary<string, List<int>> FindPrefixCarbons(List<int> path, List<List<int>> branches)
        {
            Dictionary<string, List<int>> prefixesAndIndexes = new Dictionary<string, List<int>>();
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
                if (spec.endDependentPrefixOrSuffix.ContainsKey(group.GroupFormula)) //testing, was 'else if' so didnt work as prefix ketones and aldehydes did not name ie oxo!!!
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
                string name = spec.alkylNames[branch.Count - 1] + "yl";
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
        private Dictionary<string, List<int>> FindMiddleCarbons(List<int> path)
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
                    numbers += carbonNumbers[i];
                    if (i != carbonNumbers.Count - 1)
                    {
                        numbers += ",";
                    }
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
                for (int i = 1; i < branch.Count; i++)
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
        public static void DisplaySpecDebug(string fileName)
        {
            namingSpec spec = LoadSpecification(fileName);
            Console.WriteLine("alkylNames");
            ConsoleColor c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("----------");
            Console.ForegroundColor = c;
            foreach (string s in spec.alkylNames)
            {
                Console.Write("'" + s + "', ");
            }
            Console.WriteLine();
            Title("numericalPrefixes");
            foreach (string s in spec.numericalPrefixes)
            {
                Console.Write("'" + s + "', ");
            }
            Console.WriteLine();
            Title("merging");
            foreach (var v in spec.merging)
            {
                foreach (string s in v.Key)
                {
                    Console.Write("'" + s + "', ");
                }
                Console.Write(" : '" + v.Value + "'");
            }
            Console.WriteLine();
            Title("prefixOnly");
            foreach (var v in spec.prefixOnly)
            {
                Console.WriteLine("'" + v.Key + "' : '" + v.Value + "'");
            }
            Title("middle");
            foreach (var v in spec.middle)
            {
                Console.WriteLine("'" + v.Key + "' : '" + v.Value.name + "', '" + v.Value.priority + "'");
            }
            Title("prefixOrSuffix");
            foreach (var v in spec.prefixOrSuffix)
            {
                Console.WriteLine("'" + v.Key + "' : '" + v.Value.prefix + "', '" + v.Value.suffix + "', '" + v.Value.priority + "'");
            }
            Title("endDependentPrefixOrSuffix");
            foreach (var v in spec.endDependentPrefixOrSuffix)
            {
                Console.WriteLine("'" + v.Key + "' (MIDDLE) : '" + v.Value.middle.prefix + "', '" + v.Value.middle.suffix + "', '" + v.Value.middle.priority + "'");
                Console.WriteLine("'" + v.Key + "' (END) : '" + v.Value.end.prefix + "', '" + v.Value.end.suffix + "', '" + v.Value.end.priority + "'");
            }

            void Title(string s)
            {
                ConsoleColor c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("----------");
                Console.ForegroundColor = c;
                Console.WriteLine(s);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("----------");
                Console.ForegroundColor = c;
            }
        }
        public static void SaveSpecification(string fileName, namingSpec spec)
        {
            fileName += ".ExamSpec";
            using (BinaryWriter writeFile = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writeFile.Write(((short)spec.alkylNames.Length)); //less than 32,000
                foreach (string name in spec.alkylNames)
                {
                    writeFile.Write(name);
                }
                writeFile.Write((short)spec.numericalPrefixes.Length);
                foreach (string pref in spec.numericalPrefixes)
                {
                    writeFile.Write(pref);
                }
                writeFile.Write(((short)spec.merging.Count));
                foreach (KeyValuePair<HashSet<string>, string> v in spec.merging)
                {
                    writeFile.Write(((short)v.Key.Count));
                    foreach (string s in v.Key)
                    {
                        writeFile.Write(s);
                    }
                    writeFile.Write(v.Value);
                }
                writeFile.Write(((short)spec.prefixOnly.Count));
                foreach (KeyValuePair<string, string> v in spec.prefixOnly)
                {
                    writeFile.Write(v.Key);
                    writeFile.Write(v.Value);
                }
                List<(string key, string name)> sortedMiddle = new List<(string key, string name)>();
                sortedMiddle = spec.middle.Select(kv => (kv.Key, kv.Value.name)).ToList();
                sortedMiddle = sortedMiddle.OrderBy(x => spec.middle[x.key].priority).ToList();
                writeFile.Write(((short)sortedMiddle.Count));
                for (int i = sortedMiddle.Count - 1; i >= 0; i--)
                {
                    writeFile.Write(sortedMiddle[i].key);
                    writeFile.Write(sortedMiddle[i].name);
                }
                List<(string key, bool middleOnly, bool endOnly, string prefix, string suffix)> sortedSuffixes = spec.prefixOrSuffix.Select(kv => (kv.Key, false, false, kv.Value.prefix, kv.Value.suffix)).ToList();
                sortedSuffixes.AddRange(spec.endDependentPrefixOrSuffix.Select(kv => (kv.Key, true, false, kv.Value.middle.prefix, kv.Value.middle.suffix)).ToList()); //middle only, end only
                sortedSuffixes.AddRange(spec.endDependentPrefixOrSuffix.Select(kv => (kv.Key, false, true, kv.Value.end.prefix, kv.Value.end.suffix)).ToList());
                sortedSuffixes = sortedSuffixes.OrderBy(x => getPriorityForSuffixSort(x)).ToList();
                writeFile.Write(((short)sortedSuffixes.Count));
                for (int i = sortedSuffixes.Count - 1; i >= 0; i--)
                {
                    writeFile.Write(sortedSuffixes[i].key);
                    writeFile.Write(sortedSuffixes[i].middleOnly);
                    writeFile.Write(sortedSuffixes[i].endOnly);
                    writeFile.Write(sortedSuffixes[i].prefix);
                    writeFile.Write(sortedSuffixes[i].suffix);
                }
                writeFile.Close();

                int getPriorityForSuffixSort((string key, bool middleOnly, bool endOnly, string prefix, string suffix) x)
                {
                    if (x.middleOnly)
                    {
                        return spec.endDependentPrefixOrSuffix[x.key].middle.priority;
                    }
                    else if (x.endOnly)
                    {
                        return spec.endDependentPrefixOrSuffix[x.key].end.priority;
                    }
                    else
                    {
                        return spec.prefixOrSuffix[x.key].priority;
                    }
                }
            }
        }
        private static namingSpec LoadSpecification(string fileName)
        {
            fileName += ".ExamSpec";
            namingSpec spec = new namingSpec();
            using (BinaryReader readFile = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                spec.alkylNames = new string[readFile.ReadInt16()];
                for (int i = 0; i < spec.alkylNames.Length; i++)
                {
                    spec.alkylNames[i] = readFile.ReadString();
                }
                spec.numericalPrefixes = new string[readFile.ReadInt16()];
                for (int i = 0; i < spec.numericalPrefixes.Length; i++)
                {
                    spec.numericalPrefixes[i] = readFile.ReadString();
                }
                spec.merging = new Dictionary<HashSet<string>, string>();
                int mergingCount = readFile.ReadInt16();
                for (int i = 0; i < mergingCount; i++)
                {
                    HashSet<string> toMerge = new HashSet<string>();
                    int toMergeCount = readFile.ReadInt16();
                    for (int j = 0; j < toMergeCount; j++)
                    {
                        toMerge.Add(readFile.ReadString());
                    }
                    spec.merging.Add(toMerge, readFile.ReadString());
                }
                spec.prefixOnly = new Dictionary<string, string>();
                int prefixOnlyCount = readFile.ReadInt16();
                for (int i = 0; i < prefixOnlyCount; i++)
                {
                    string key = readFile.ReadString();
                    string name = readFile.ReadString();
                    spec.prefixOnly.Add(key, name);
                }
                spec.middle = new Dictionary<string, (string name, int priority)>();
                int middleOnlyCount = readFile.ReadInt16();
                for (int i = middleOnlyCount - 1; i >= 0; i--)
                {
                    string key = readFile.ReadString();
                    string name = readFile.ReadString();
                    spec.middle.Add(key, (name, i + 1));
                }
                spec.prefixOrSuffix = new Dictionary<string, (string prefix, string suffix, int priority)>();
                spec.endDependentPrefixOrSuffix = new Dictionary<string, ((string prefix, string suffix, int priority) middle, (string prefix, string suffix, int priority) end)>();
                int suffixCount = readFile.ReadInt16();
                for (int i = suffixCount - 1; i >= 0; i--)
                {
                    string key = readFile.ReadString();
                    bool middleOnly = readFile.ReadBoolean();
                    bool endOnly = readFile.ReadBoolean();
                    string prefix = readFile.ReadString();
                    string suffix = readFile.ReadString();
                    //List<((string prefix, string suffix, int priority) middle, (string prefix, string suffix, int priority) end)> entires = new List<((string prefix, string suffix, int priority) middle, (string prefix, string suffix, int priority) end)>();
                    if (middleOnly)
                    {
                        if (spec.endDependentPrefixOrSuffix.ContainsKey(key)) //hard to form back - does this work??
                        {
                            ((string prefix, string suffix, int priority) middle, (string prefix, string suffix, int priority) end) v = spec.endDependentPrefixOrSuffix[key];
                            v.middle.prefix = prefix;
                            v.middle.suffix = suffix;
                            v.middle.priority = i + 1;
                            spec.endDependentPrefixOrSuffix[key] = v; //cannot mutate dictionary complex return values while inside dictionary
                        }
                        else
                        {
                            spec.endDependentPrefixOrSuffix.Add(key, ((prefix, suffix, i + 1), ("", "", -1)));
                        }
                    }
                    else if (endOnly)
                    {
                        if (spec.endDependentPrefixOrSuffix.ContainsKey(key))
                        {
                            ((string prefix, string suffix, int priority) middle, (string prefix, string suffix, int priority) end) v = spec.endDependentPrefixOrSuffix[key];
                            v.end.prefix = prefix;
                            v.end.suffix = suffix;
                            v.end.priority = i + 1;
                            spec.endDependentPrefixOrSuffix[key] = v;
                        }
                        else
                        {
                            spec.endDependentPrefixOrSuffix.Add(key, (("", "", -1), (prefix, suffix, i + 1)));
                        }
                    }
                    else
                    {
                        if (spec.prefixOrSuffix.ContainsKey(key))
                        {
                            (string prefix, string suffix, int priority) v = spec.prefixOrSuffix[key];
                            v.prefix = prefix;
                            v.suffix = suffix;
                            v.priority = i + 1;
                            spec.prefixOrSuffix[key] = v;
                        }
                        else
                        {
                            spec.prefixOrSuffix.Add(key, (prefix, suffix, i + 1));
                        }
                    }
                }
                readFile.Close();
            }
            return spec;
        }
        private void CheckGroups(List<FunctionalGroup> groups)
        {

            foreach (FunctionalGroup g in groups)
            {
                if (!spec.prefixOnly.ContainsKey(g.GroupFormula) && !spec.prefixOrSuffix.ContainsKey(g.GroupFormula) && !spec.endDependentPrefixOrSuffix.ContainsKey(g.GroupFormula) && !spec.middle.ContainsKey(g.GroupFormula))
                {

                    throw new Exception($"{g.GroupFormula} is unrecognised");
                }
            }
        }
        /*private void FindHighestPrioritySuffix()
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
        }*/
        private void FindHighestPrioritySuffix()
        {
            //return groups.MaxBy(x => spec.prefixOrSuffix[x.GroupFormula].priority).GroupFormula;
            int maxPriority = -1;
            foreach (FunctionalGroup fg in groups)
            {
                int currentPriority;
                if (spec.prefixOrSuffix.ContainsKey(fg.GroupFormula))
                {
                    currentPriority = spec.prefixOrSuffix[fg.GroupFormula].priority;
                    if (currentPriority > maxPriority)
                    {
                        maxPriority = currentPriority;
                        suffixIsMiddle = false;
                        suffixIsEnd = false;
                        suffixFormula = fg.GroupFormula;
                        suffixRoot = spec.prefixOrSuffix[fg.GroupFormula].suffix;
                    }
                }
                else if (spec.endDependentPrefixOrSuffix.ContainsKey(fg.GroupFormula))
                {

                    if (atoms.IsAnEnd(fg.MainIndex))
                    {
                        currentPriority = spec.endDependentPrefixOrSuffix[fg.GroupFormula].end.priority;
                        if (currentPriority > maxPriority)
                        {
                            maxPriority = currentPriority;
                            suffixIsMiddle = false;
                            suffixIsEnd = true;
                            suffixFormula = fg.GroupFormula;
                            suffixRoot = spec.endDependentPrefixOrSuffix[fg.GroupFormula].end.suffix;
                        }
                    }
                    else
                    {
                        currentPriority = spec.endDependentPrefixOrSuffix[fg.GroupFormula].middle.priority;
                        if (currentPriority > maxPriority)
                        {
                            maxPriority = currentPriority;
                            suffixIsMiddle = true;
                            suffixIsEnd = false;
                            suffixFormula = fg.GroupFormula;
                            suffixRoot = spec.endDependentPrefixOrSuffix[fg.GroupFormula].middle.suffix;
                        }
                    }
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
        public static void RestoreDefaultSpecifications()
        {
            namingSpec fullSpec = new namingSpec();
            fullSpec.merging = new Dictionary<HashSet<string>, string>();
            fullSpec.prefixOnly = new Dictionary<string, string>();
            fullSpec.middle = new Dictionary<string, (string, int)>();
            fullSpec.prefixOrSuffix = new Dictionary<string, (string, string, int)>();
            fullSpec.endDependentPrefixOrSuffix = new Dictionary<string, ((string, string, int), (string, string, int))>();
            fullSpec.alkylNames = new string[] { "", "meth|a", "eth|a", "prop|a", "but|a", "pent|a", "hex|a", "hept|a", "oct|a", "non|a", "dec|a" }; // symbol to denote no double vowel here
            fullSpec.numericalPrefixes = new string[] { "", "", "di", "tri", "tetra", "penta", "hexa", "hepta", "octa", "nona" };
            fullSpec.merging.Add(new HashSet<string> { "C-O", "C=O" }, "COOH");
            fullSpec.merging.Add(new HashSet<string> { "C-Cl", "C=O" }, "COCl");
            fullSpec.merging.Add(new HashSet<string> { "C-N", "C=O" }, "CON");
            fullSpec.prefixOnly.Add("C-F", "fluoro");
            fullSpec.prefixOnly.Add("C-Cl", "chloro");
            fullSpec.prefixOnly.Add("C-Br", "bromo");
            fullSpec.prefixOnly.Add("C-I", "iodo");
            fullSpec.middle.Add("C=C", ("en|e", 2));
            fullSpec.middle.Add("C≡C", ("yn|e", 1));
            fullSpec.middle.Add("", ("an|e", 0));
            fullSpec.prefixOrSuffix.Add("COOH", ("carboxy", "oic acid", 10));
            fullSpec.prefixOrSuffix.Add("COCl", ("chlorocarbonyl", "oyl chloride", 9));
            fullSpec.prefixOrSuffix.Add("CON", ("carbamoyl", "amide", 8));
            fullSpec.prefixOrSuffix.Add("C≡N", ("cyano", "nitrile", 7));
            fullSpec.endDependentPrefixOrSuffix.Add("C=O", (("oxo", "one", 5), ("formyl", "al", 6)));
            fullSpec.prefixOrSuffix.Add("C-O", ("hydroxy", "ol", 4));
            fullSpec.prefixOrSuffix.Add("C-S", ("sulfanyl", "thiol", 3));
            fullSpec.prefixOrSuffix.Add("C-N", ("amino", "amine", 2));
            fullSpec.prefixOrSuffix.Add("C=N", ("imino", "imine", 1));
            // using this resource https://iupac.org/wp-content/uploads/2021/06/Organic-Brief-Guide-brochure_v1.1_June2021.pdf
            SaveSpecification("AllGroups", fullSpec);

            namingSpec hcarbonSpec = new namingSpec();
            hcarbonSpec.merging = new Dictionary<HashSet<string>, string>();
            hcarbonSpec.prefixOnly = new Dictionary<string, string>();
            hcarbonSpec.middle = new Dictionary<string, (string, int)>();
            hcarbonSpec.prefixOrSuffix = new Dictionary<string, (string, string, int)>();
            hcarbonSpec.endDependentPrefixOrSuffix = new Dictionary<string, ((string, string, int), (string, string, int))>();
            hcarbonSpec.alkylNames = new string[] { "", "meth|a", "eth|a", "prop|a", "but|a", "pent|a", "hex|a", "hept|a", "oct|a", "non|a", "dec|a" };
            hcarbonSpec.numericalPrefixes = new string[] { "", "", "di", "tri", "tetra", "penta", "hexa", "hepta", "octa", "nona" };
            hcarbonSpec.middle.Add("C=C", ("en|e", 2));
            hcarbonSpec.middle.Add("C≡C", ("yn|e", 1));
            hcarbonSpec.middle.Add("", ("an|e", 0));
            SaveSpecification("Hydrocarbons", hcarbonSpec);

            namingSpec alkaneSpec = new namingSpec();
            alkaneSpec.merging = new Dictionary<HashSet<string>, string>();
            alkaneSpec.prefixOnly = new Dictionary<string, string>();
            alkaneSpec.middle = new Dictionary<string, (string, int)>();
            alkaneSpec.prefixOrSuffix = new Dictionary<string, (string, string, int)>();
            alkaneSpec.endDependentPrefixOrSuffix = new Dictionary<string, ((string, string, int), (string, string, int))>();
            alkaneSpec.alkylNames = new string[] { "", "meth|a", "eth|a", "prop|a", "but|a", "pent|a", "hex|a", "hept|a", "oct|a", "non|a", "dec|a" };
            alkaneSpec.numericalPrefixes = new string[] { "", "", "di", "tri", "tetra", "penta", "hexa", "hepta", "octa", "nona" };
            alkaneSpec.middle.Add("", ("an", 0));
            SaveSpecification("Alkanes", alkaneSpec);
        }
    }
}

