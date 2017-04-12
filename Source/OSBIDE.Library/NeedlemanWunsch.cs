using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library
{
    /// <summary>
    /// Provides methods to align and score two strings.  Source code adapted from 
    /// http://www.codeproject.com/Tips/638377/Needleman-Wunsch-Algorithm-in-Csharp
    /// under the CPOL license (http://www.codeproject.com/info/cpol10.aspx)
    /// </summary>
    public class NeedlemanWunsch
    {
        /// <summary>
        /// Scores two aligned NPSM sequences.  For use in Analytics tools.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static int ScoreNpsmSequence(string s1, string s2)
        {
            int score = 0;

            for (int i = 0; i < s1.Length; i++)
            {
                //don't count underscores
                if (s1[i].CompareTo('_') == 0 || s2[i].CompareTo('_') == 0)
                {
                    continue;
                }

                //mismatch / indel?
                if (s1[i].CompareTo(s2[i]) != 0)
                {
                    //mismatch gets a penalty of 2
                    if (s1[i].CompareTo('-') != 0 && s2[i].CompareTo('-') != 0)
                    {
                        score += 2;
                    }
                    else
                    {
                        //indel gets a penalty of 1
                        score += 1;
                    }
                }
            }

            return score;
        }

        /// <summary>
        /// Aligns two strings
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static Tuple<string, string> Align(string s1, string s2)
        {
            int s1Length = s1.Length + 1;
            int s2Length = s2.Length + 1;

            int[,] scoringMatrix = new int[s2Length, s1Length];

            //Initailization Step - filled with 0 for the first row and the first column of matrix
            for (int i = 0; i < s2Length; i++)
            {
                for (int j = 0; j < s1Length; j++)
                {
                    scoringMatrix[i, j] = 0;
                }
                // scoringMatrix[i, 0] = 0; 
            }

            //Matrix Fill Step
            for (int i = 1; i < s2Length; i++)
            {
                for (int j = 1; j < s1Length; j++)
                {
                    int scroeDiag = 0;
                    if (s1.Substring(j - 1, 1) == s2.Substring(i - 1, 1))
                        scroeDiag = scoringMatrix[i - 1, j - 1] + 2;
                    else
                        scroeDiag = scoringMatrix[i - 1, j - 1] + -1;

                    int scroeLeft = scoringMatrix[i, j - 1] - 2;
                    int scroeUp = scoringMatrix[i - 1, j] - 2;

                    int maxScore = Math.Max(Math.Max(scroeDiag, scroeLeft), scroeUp);

                    scoringMatrix[i, j] = maxScore;
                }
            }

            //Display the matrix
            /*
            for (int i = 0; i < s2Length; i++)
            {
                for (int j = 0; j < s1Length; j++)
                {
                    if (scoringMatrix[i, j] >= 0)
                        Console.Write(" ");
                    Console.Write(scoringMatrix[i, j]);
                }
                Console.Write(Environment.NewLine);
            }
            */
            //Console.ReadLine();

            //Traceback Step
            char[] alineSeqArray = s2.ToCharArray();
            char[] refSeqArray = s1.ToCharArray();

            string AlignmentA = string.Empty;
            string AlignmentB = string.Empty;
            int m = s2Length - 1;
            int n = s1Length - 1;
            while (m > 0 || n > 0)
            {
                int scroeDiag = 0;

                if (m == 0 && n > 0)
                {
                    AlignmentA = refSeqArray[n - 1] + AlignmentA;
                    AlignmentB = "-" + AlignmentB;
                    n = n - 1;
                }
                else if (n == 0 && m > 0)
                {
                    AlignmentA = "-" + AlignmentA;
                    AlignmentB = alineSeqArray[m - 1] + AlignmentB;
                    m = m - 1;
                }
                else
                {
                    //Remembering that the scoring scheme is +2 for a match, -1 for a mismatch, and -2 for a gap
                    if (alineSeqArray[m - 1] == refSeqArray[n - 1])
                        scroeDiag = 2;
                    else
                        scroeDiag = -1;

                    if (m > 0 && n > 0 && scoringMatrix[m, n] == scoringMatrix[m - 1, n - 1] + scroeDiag)
                    {
                        AlignmentA = refSeqArray[n - 1] + AlignmentA;
                        AlignmentB = alineSeqArray[m - 1] + AlignmentB;
                        m = m - 1;
                        n = n - 1;
                    }
                    else if (n > 0 && scoringMatrix[m, n] == scoringMatrix[m, n - 1] - 2)
                    {
                        AlignmentA = refSeqArray[n - 1] + AlignmentA;
                        AlignmentB = "-" + AlignmentB;
                        n = n - 1;
                    }
                    else //if (m > 0 && scoringMatrix[m, n] == scoringMatrix[m - 1, n] + -2)
                    {
                        AlignmentA = "-" + AlignmentA;
                        AlignmentB = alineSeqArray[m - 1] + AlignmentB;
                        m = m - 1;
                    }
                }
            }

            //return aligned strings
            return new Tuple<string, string>(AlignmentA, AlignmentB);
        }
    }
}
