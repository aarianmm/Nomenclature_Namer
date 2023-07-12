//using System;
//namespace Nomenclature_Namer
//{
//    //while (true) { Console.WriteLine(longestChainLength(Console.ReadLine())); }

//    public class compound
//    {
//        int chainLength;
//        int[] branchLengths;
//        public int[] BranchLengths { get { return branchLengths; } set { branchLengths = value; } }
//        funcionalGroup[] stem;
//        public funcionalGroup[] Stem { get { return stem; } set { stem = value; } }

//        public compound(string SF)
//        {
//            chainLength = countChar(SF, 'C');
//            branchLengths = new int[countChar(SF, '(')];
//            stem = new funcionalGroup[chainLength];
//            //compileMainStem(SF);
//        }

//        private int countChar(string SF, char c)  //recursion hehe
//        {
//            if (SF.Length == 0)
//            {
//                return 0;
//            }
//            if (SF[0] == c)
//            {
//                return 1 + countChar(SF.Substring(1), c);
//            }
//            return countChar(SF.Substring(1), c);
//        }

//        private string SkeletonName(int chainLength)
//        {
//            string core = "ERR";
//            switch (chainLength)
//            {
//                case 1:
//                    core = "meth";
//                    break;
//                case 2:
//                    core = "eth";
//                    break;
//                case 3:
//                    core = "prop";
//                    break;
//                case 4:
//                    core = "but";
//                    break;
//                case 5:
//                    core = "pent";
//                    break;
//                case 6:
//                    core = "hex";
//                    break;
//                case 7:
//                    core = "hept";
//                    break;
//                case 8:
//                    core = "oct";
//                    break;
//                case 9:
//                    core = "non";
//                    break;
//                case 10:
//                    core = "pent";
//                    break;
//                default:
//                    throw new Exception("Carbon skeleton too long");
//                    break;
//            }
//            return core;
//        }

//    //    private void compileMainStem(string SF)  //branches with functional groups not implimented yet
//    //    {

//    //        //compound.Add(new funcionalGroup(00000, -1, 0, "ane", false));
//    //        int chainLength = 0;
//    //        int currentBranch = 0;  //useless atm?
//    //        int quantity = 1;  //NEED TO DEAL WITH CF4 - NUMBERS AFTER HALOGENS, ALSO, WHAT IF MULTIPLE THINGS ON SAME CARBON??
//    //        bool mainBranch = true;
//    //        for (int i = 0; i < SF.Length; i++)
//    //        {
//    //            if (SF[i] == '(' && SF[i + 1] == 'C' && mainBranch)
//    //            {
//    //                currentBranch++;
//    //                BranchLengths[currentBranch] = i;
//    //                mainBranch = false;
//    //            }
//    //            else if (SF[i] == ')' && SF[i + 1] == 'C' && !mainBranch)
//    //            {
//    //                BranchLengths[currentBranch] = i - BranchLengths[currentBranch];
//    //                mainBranch = true;
//    //            }
//    //            if (SF[i] == 'B')
//    //            {
//    //                int.TryParse(SF[i + 1].ToString(), out quantity);
//    //                for (int j = 0; j < quantity; j++)
//    //                {
//    //                    Stem[chainLength, i] = (new funcionalGroup(00000, -1, "bromo", true));   //branches with functional groups not implimented yet
//    //                }
//    //            }
//    //            else if (SF[i] == 'l')
//    //            {
//    //                int.TryParse(SF[i + 1].ToString(), out quantity);
//    //                for (int j = 0; j < quantity; j++)
//    //                {
//    //                    Stem[chainLength, i] = (new funcionalGroup(00000, -1, "chloro", true));
//    //                }
//    //            }
//    //            else if (SF[i] == 'F')
//    //            {
//    //                int.TryParse(SF[i + 1].ToString(), out quantity);
//    //                for (int j = 0; j < quantity; j++)
//    //                {
//    //                    Stem[chainLength, j] = (new funcionalGroup(00000, -1, "fluoro", true));
//    //                }
//    //            }
//    //            else if (SF[i] == 'I')
//    //            {
//    //                int.TryParse(SF[i + 1].ToString(), out quantity);
//    //                for (int j = 0; j < quantity; j++)
//    //                {
//    //                    Stem[chainLength, j] = (new funcionalGroup(00000, -1, "iodo", true));
//    //                }
//    //            }
//    //            else if (SF.Substring(i, 3) == "OOH")
//    //            {
//    //                Stem[chainLength, 1] = (new funcionalGroup(00000, 5, "oic acid", false));
//    //            }
//    //            else if (SF.Substring(i, 3) == "OH")
//    //            {
//    //                Stem[chainLength, 1] = (new funcionalGroup(00000, 2, "hydroxy", "ol"));
//    //            }
//    //            else if (SF[i] == '=')
//    //            {
//    //                if (SF[i + 1] == 'O')
//    //                {
//    //                    if (chainLength == 1 || SF[i + 1] == '!' || SF[i + 2] == '!') //at the end of the chain - can optimise if carbon chain length is calculated beforehand
//    //                    {
//    //                        Stem[chainLength, i] = (new funcionalGroup(00000, 4, "formyl", "al"));
//    //                    }
//    //                    else
//    //                    {
//    //                        Stem[chainLength, i] = (new funcionalGroup(00000, 3, "oxo", "one"));
//    //                    }
//    //                }
//    //                else
//    //                {
//    //                    Stem[chainLength, i] = (new funcionalGroup(00000, 1, "ene", false));
//    //                }
//    //            }
//    //            else if ((SF[i] == 'C' || SF[i] == 'c') && mainBranch)
//    //            {
//    //                chainLength++;
//    //            }
//    //        }

//    //        BranchLengths[0] = chainLength - BranchLengths.Sum();
//    //    }
//    }
//}

