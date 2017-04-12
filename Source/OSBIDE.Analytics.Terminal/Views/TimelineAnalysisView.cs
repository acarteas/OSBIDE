using OSBIDE.Analytics.Library.Models;
using OSBIDE.Analytics.Terminal.Models;
using OSBIDE.Analytics.Terminal.ViewModels;
using OSBIDE.Library;
using OSBIDE.Library.CSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Views
{
    class TimelineAnalysisView
    {
        private enum MenuOption
        {
            ChangeDirectory = 1,
            ListFiles,
            LoadFile,
            AppendFile,
            LoadGrades,
            FilterGrades,
            NormalizeProgrammingStates,
            CountTransitions,
            BuildTransitionFrequencyCounts,
            BuildAggregateTransitionCounts,
            LocateTransitionCycles,
            OrderTransitionCyclesByDate,
            WriteAggregateToFile,
            WriteTransitionsToFile,
            WriteDataToCsv,
            Exit
        }

        private TimelineAnalysisViewModel Vm { get; set; }

        public TimelineAnalysisView()
        {
            Vm = new TimelineAnalysisViewModel();
        }

        /// <summary>
        /// Similar to <see cref="BuildTransitionFrequencyCounts"/>, but instead creates a single file
        /// per transition length for all students and all loaded files.
        /// </summary>
        /// <param name="vm"></param>
        private void AggregateTransitionFrequencyCounts(TimelineAnalysisViewModel vm)
        {
            //step 1: get list of files to process
            List<string> filesToProcess = new List<string>();
            string fileName = "a";
            Console.WriteLine("Enter files to process (-1 to stop)");
            while ((fileName = GetFile()).Length > 0)
            {
                filesToProcess.Add(fileName);
            }

            //load all data into VM
            vm.LoadTimeline(filesToProcess[0]);
            for (int i = 1; i < filesToProcess.Count; i++)
            {
                vm.AppendTimeline(filesToProcess[i]);
            }

            //step2: get sequence boundaries.  Again, hard coded for now
            int startingSequenceLength = 2;
            int endingSequenceLength = 25;

            //this produces a lot of files, so create a separate directory for the output
            string outputDirectory = "AggregateTransitionFrequencyCounts";
            if (Directory.Exists(outputDirectory) == false)
            {
                Directory.CreateDirectory(outputDirectory);
            }

            /*
             * What I need to do:
             * Get all sequences.
             * For each sequence:
             *      Determine if similar to other known sequences.  If so, combine into same set. (disjoint set?)
             * */

            Dictionary<int, Dictionary<string, int>> allTransitions = new Dictionary<int, Dictionary<string, int>>();

            //begin file processing
            for (int sequenceLength = startingSequenceLength; sequenceLength <= endingSequenceLength; sequenceLength++)
            {
                //get grade data
                vm.AttachGrades();

                //build markov transitions
                vm.BuildDefaultMarkovStates();

                //figure out sequence distribution for entire data set and for individual students
                Dictionary<string, int> transitions = vm.GetAllTransitionCombinations(sequenceLength);

                //filter out singletons
                var smallKeys = transitions.Where(t => t.Value < 5).Select(t => t.Key).ToList();
                foreach (string key in smallKeys)
                {
                    transitions.Remove(key);
                }

                //save for future use
                allTransitions.Add(sequenceLength, transitions);

                Console.WriteLine("Loaded transitions of length {0}.", sequenceLength);
            }

            //use Needleman-Wunsch algorithm and disjoint sets to combine similar sequences
            DisjointSet<string> matches = new DisjointSet<string>();
            int matchCount = 0;

            //start with large sequences as it will make it more likely that these will be the "top" of the disjoint set
            int startingNumber = (int)'a';
            for (int sequenceLength = endingSequenceLength; sequenceLength >= startingSequenceLength; sequenceLength--)
            {
                Console.WriteLine("Matching sequences of length {0}", sequenceLength);

                //Needleman-Wunsch works on single characters, so we need to transform Markov-like numbers to letters
                Dictionary<string, int> originalSequences = allTransitions[sequenceLength];
                Dictionary<string, int> modifiedSequences = new Dictionary<string, int>();
                foreach (var kvp in originalSequences)
                {
                    //convert into numbers
                    int[] pieces = kvp.Key.Split('_').Select(k => Convert.ToInt32(k) + startingNumber).ToArray();

                    //then, convert back to characters
                    char[] sequence = pieces.Select(p => Convert.ToChar(p)).ToArray();

                    //and finally into a string
                    string charSequence = string.Join("_", sequence);

                    //lastly, remember this sequence
                    modifiedSequences.Add(charSequence, kvp.Value);
                }

                //prime the disjoint set
                foreach (string key in modifiedSequences.Keys)
                {
                    matches.Find(key);
                }

                //having converted to character state representations, now run the Needleman-Wunsch algorithm
                List<string> sequences = modifiedSequences.Keys.ToList();
                for (int i = 0; i < sequences.Count; i++)
                {
                    for (int j = i + 1; j < sequences.Count; j++)
                    {
                        string first = matches.Find(sequences[i]);
                        string second = matches.Find(sequences[j]);

                        //automatically count sequences as the same when one sequence is a complete substring of another sequence
                        string firstSequence = sequences[i];
                        string secondSequence = sequences[j];
                        if (firstSequence.Replace(secondSequence, "").Length == 0
                        || secondSequence.Replace(firstSequence, "").Length == 0
                            )
                        {
                            matches.UnionWith(first, second);
                            matchCount++;
                        }
                        else
                        {
                            //Use NW to check for alignment
                            //align the two sequences
                            var result = NeedlemanWunsch.Align(first, second);

                            //if score is similar, then count the sequences as the same (union)
                            if ((double)NeedlemanWunsch.ScoreNpsmSequence(result.Item1, result.Item2) < 3)
                            {
                                matches.UnionWith(first, second);
                                matchCount++;
                            }
                        }
                    }
                }
            }

            //now, get all sets and figure out popularity of each set
            Console.WriteLine("{0} unions performed.", matchCount);
            List<List<string>> allSets = matches.AllSets();
            List<List<string>> smallerSets = allSets.Where(s => s.Count > 1).ToList();
            Dictionary<string, int> popularityDict = new Dictionary<string, int>();
            Console.WriteLine("Calculating popularity of {0} sets...", allSets.Count);
            foreach (List<string> set in allSets)
            {
                foreach (string item in set)
                {
                    //convert back to Markov-style transitions
                    int[] pieces = item.Split('_').Select(c => Convert.ToChar(c)).Select(c => (int)c - startingNumber).ToArray();
                    string key = string.Join("_", pieces);

                    if (popularityDict.ContainsKey(key) == false)
                    {
                        popularityDict.Add(key, 0);
                    }

                    //add in counts to the popularity dictionary
                    popularityDict[key] += allTransitions[pieces.Length][key];
                }
            }

            //write this information to a file
            CsvWriter writer = new CsvWriter();

            //aggregate class results
            Console.WriteLine("Writing most popular sequences to file.");
            foreach (KeyValuePair<string, int> kvp in popularityDict.OrderByDescending(p => p.Value))
            {
                int[] pieces = kvp.Key.Split('_').Select(c => Convert.ToInt32(c)).ToArray();
                string npsmKey = string.Join("_", pieces.Select(p => vm.StateNumberToNpsmString(p)).ToArray());

                writer.AddToCurrentLine(npsmKey);
                writer.AddToCurrentLine(kvp.Value.ToString());
                writer.CreateNewRow();
            }
            using (TextWriter tw = File.CreateText(string.Format("popular_sequences.csv")))
            {
                tw.Write(writer.ToString());
            }

        }

        /*
         * What I want to do:
         *  For each assignment:
         *      figure out common sequences of length m to n
         *      For each student, for each grade band (A-F), again determine frequences of length m to n
         *      Build a frequency distribution for each grade band by sequence
         * */
        private void BuildTransitionFrequencyCounts(TimelineAnalysisViewModel vm)
        {
            //step 1: get list of files to process
            List<string> filesToProcess = new List<string>();
            string fileName = "a";
            Console.WriteLine("Enter files to process (-1 to stop)");
            while ((fileName = GetFile()).Length > 0)
            {
                filesToProcess.Add(fileName);
            }

            //step 2: setup grade-bands (e.g. A, B, C, etc.)  Hard coded for now as this is just for a
            //single class
            double maxScore = 200;
            double[] gradeRanges = { 90, 78, 69, 60, 0 };
            string[] gradeMap = { "A", "B", "C", "D", "F" };

            //step 3: get sequence boundaries.  Again, hard coded for now
            int startingSequenceLength = 2;
            int endingSequenceLength = 25;

            //step 4: get assignments.  
            string[] assignments = { "Assignment #1",
                                     "Assignment #2",
                                     "Assignment #3",
                                     "Assignment #4",
                                     "Assignment #5",
                                     "Assignment #6",
                                     "Assignment #7"
                                   };
            int assignmentCounter = 0;

            //this produces a lot of files, so create a separate directory for the output
            string outputDirectory = "TransitionFrequencyCounts";
            if (Directory.Exists(outputDirectory) == false)
            {
                Directory.CreateDirectory(outputDirectory);
            }


            //finally, begin processing
            foreach (string fileToProcess in filesToProcess)
            {
                string folderName = fileToProcess.Replace("#", "");
                string outputPath = Path.Combine(outputDirectory, folderName);
                if (Directory.Exists(outputPath) == false)
                {
                    Directory.CreateDirectory(outputPath);
                }
                for (int sequenceLength = startingSequenceLength; sequenceLength <= endingSequenceLength; sequenceLength++)
                {
                    //reset max score for A students
                    maxScore = 200;

                    //based on currently existing code, it is easier to reopen the file for
                    //each grade range
                    for (int i = 0; i < gradeRanges.Length; i++)
                    {
                        double bound = gradeRanges[i];

                        //reload the file
                        LoadFile(fileToProcess);

                        //get grade data
                        vm.AttachGrades();

                        //filter based on grade data
                        vm.FilterByGrade(assignments[assignmentCounter], bound, maxScore);

                        //update scores for next grade boundary
                        maxScore = bound - 0.01;

                        //build markov transitions
                        vm.BuildDefaultMarkovStates();

                        //figure out sequence distribution for entire data set and for individual students
                        Dictionary<string, int> transitions = vm.GetAllTransitionCombinations(sequenceLength);

                        //interesting transitions are those in which we have at least 5 occurrances 
                        var interestingTransitions = transitions.Where(t => t.Value > 5).OrderBy(t => t.Value).ToList();

                        //write this information to a file
                        CsvWriter writer = new CsvWriter();

                        //aggregate class results
                        Console.WriteLine("Processing transition sequences of length {0}...", sequenceLength);
                        foreach (KeyValuePair<string, int> kvp in interestingTransitions)
                        {
                            writer.AddToCurrentLine(kvp.Key);
                            writer.AddToCurrentLine(kvp.Value.ToString());
                            writer.CreateNewRow();
                        }
                        using (TextWriter tw = File.CreateText(string.Format("{0}/aggregate_{1}_{2}.csv", outputPath, sequenceLength, gradeMap[i])))
                        {
                            tw.Write(writer.ToString());
                        }

                        //individual students
                        //add header data
                        writer = new CsvWriter();
                        writer.AddToCurrentLine("UserId");
                        writer.AddToCurrentLine("Grade");
                        foreach (var kvp in interestingTransitions)
                        {
                            writer.AddToCurrentLine(kvp.Key);
                        }
                        writer.CreateNewRow();

                        foreach (var user in vm.Timeline.Values)
                        {
                            //first row for users is raw values
                            writer.AddToCurrentLine(user.OsbideId);
                            writer.AddToCurrentLine(gradeMap[i]);

                            //only use the interesting states as columns as identified in the aggregate analysis
                            foreach (KeyValuePair<string, int> kvp in interestingTransitions)
                            {
                                if (user.TransitionCounts.ContainsKey(kvp.Key) == true)
                                {
                                    writer.AddToCurrentLine(user.TransitionCounts[kvp.Key]);
                                }
                                else
                                {
                                    writer.AddToCurrentLine("0");
                                }
                            }
                            writer.CreateNewRow();

                            //2nd row contains normalized values
                            writer.AddToCurrentLine(user.OsbideId);
                            writer.AddToCurrentLine(gradeMap[i]);
                            int totalTransitions = user.TransitionCounts.Values.Sum();

                            //only use the interesting states as columns as identified in the aggregate analysis
                            foreach (KeyValuePair<string, int> kvp in interestingTransitions)
                            {
                                if (user.TransitionCounts.ContainsKey(kvp.Key) == true)
                                {
                                    writer.AddToCurrentLine(user.TransitionCounts[kvp.Key] / (double)totalTransitions);
                                }
                                else
                                {
                                    writer.AddToCurrentLine("0");
                                }
                            }
                            writer.CreateNewRow();
                        }
                        using (TextWriter tw = File.CreateText(string.Format("{0}/individual_{1}_{2}.csv", outputPath, sequenceLength, gradeMap[i])))
                        {
                            tw.Write(writer.ToString());
                        }
                    }
                }

                //move to the next assignment
                assignmentCounter++;
            }
        }

        /// <summary>
        /// Locates common transition sequence cycles
        /// </summary>
        /// <param name="vm"></param>
        private void LocateCommonTransitionCycles(TimelineAnalysisViewModel vm)
        {


            //write results to a file
            vm.NormalizeProgrammingStates();
            vm.AttachGrades();
            CsvWriter writer = new CsvWriter();

            //build header
            writer.AddToCurrentLine("UserId");
            writer.AddToCurrentLine("TotalTimeProgramming");
            writer.AddToCurrentLine("NumberOfCycles");
            foreach (string[] sequence in vm.InterestingSequences)
            {
                string sequenceHeader = String.Join("_", sequence);
                writer.AddToCurrentLine("Time_" + sequenceHeader);
                writer.AddToCurrentLine("NormalizedTime_" + sequenceHeader);
                writer.AddToCurrentLine("Count_" + sequenceHeader);
                writer.AddToCurrentLine("NormalizedCount_" + sequenceHeader);
            }
            writer.AddToCurrentLine("CycleTime");
            writer.AddToCurrentLine("PercentTimeAccountedFor");

            //add grades if loaded
            List<string> grades = vm.GetAllGrades();
            foreach (string grade in grades)
            {
                writer.AddToCurrentLine(grade);
            }

            writer.CreateNewRow();

            //write data cells
            foreach (int userId in vm.StatesByUser.Keys)
            {

                writer.AddToCurrentLine(userId);

                //get total time spent programming
                StudentTimeline userTimeline = vm.Timeline[userId];
                TimelineState timeState = userTimeline.GetAggregateState("normalized_total_time");
                TimeSpan totalTime = timeState.EndTime - timeState.StartTime;
                writer.AddToCurrentLine(totalTime.TotalMinutes);
                TimeSpan cycleTime = new TimeSpan();

                //and total number of cycles
                int totalCycles = 0;
                foreach (string[] sequence in vm.InterestingSequences)
                {
                    string sequenceKey = String.Join("_", sequence);
                    totalCycles += vm.StatesByUser[userId][sequenceKey].Count;
                }
                writer.AddToCurrentLine(totalCycles);

                //write total time spent in cycle
                foreach (string[] sequence in vm.InterestingSequences)
                {
                    string sequenceKey = String.Join("_", sequence);
                    TimeSpan sequenceTime = new TimeSpan();
                    foreach (PatternParserResult pattern in vm.StatesByUser[userId][sequenceKey])
                    {
                        foreach (TimelineState state in pattern.StateSequence)
                        {
                            if (state.EndTime < state.StartTime)
                            {
                                throw new Exception("EndTime must be larger than StartTime");
                            }
                            sequenceTime += state.EndTime - state.StartTime;
                        }
                    }
                    cycleTime += sequenceTime;

                    //total time
                    writer.AddToCurrentLine(sequenceTime.TotalMinutes);

                    //normalized time
                    writer.AddToCurrentLine(Math.Round((sequenceTime.TotalMinutes / totalTime.TotalMinutes) * 100, 2));

                    //count
                    writer.AddToCurrentLine(vm.StatesByUser[userId][sequenceKey].Count);

                    //normalized count
                    double normalizedCount = ((vm.StatesByUser[userId][sequenceKey].Count) / (double)totalCycles) * 100;
                    writer.AddToCurrentLine(Math.Round(normalizedCount, 2));
                }
                writer.AddToCurrentLine(cycleTime.TotalMinutes);
                writer.AddToCurrentLine(Math.Round((cycleTime.TotalMinutes / totalTime.TotalMinutes) * 100, 2));

                //add grade information
                foreach (string grade in grades)
                {
                    if (userTimeline.Grades.ContainsKey(grade) == true)
                    {
                        writer.AddToCurrentLine(userTimeline.Grades[grade].ToString());
                    }
                    else
                    {
                        writer.AddToCurrentLine("0");
                    }
                }

                writer.CreateNewRow();
            }

            using (TextWriter tw = File.CreateText("sequence_cycles.csv"))
            {
                tw.Write(writer.ToString());
            }

            Console.WriteLine("Finished locating sequences.");
        }

        /// <summary>
        /// Creates a matrix of cycle activity for each student 
        /// </summary>
        /// <param name="vm"></param>
        private void OrderTransitionsByDate(TimelineAnalysisViewModel vm)
        {

            //step 1: get list of files to process
            List<string> filesToProcess = new List<string>();
            string fileName = "a";
            Console.WriteLine("Enter files to process (-1 to stop)");
            while ((fileName = GetFile()).Length > 0)
            {
                filesToProcess.Add(fileName);
            }

            //step 2: setup grade-bands (e.g. A, B, C, etc.)  Hard coded for now as this is just for a
            //single class
            double maxScore = 200;
            double[] gradeRanges = { 90, 78, 69, 60, 0 };
            string[] gradeMap = { "A", "B", "C", "D", "F" };

            //this produces a lot of files, so create a separate directory for the output
            string outputDirectory = "TransitionsByDate";
            if (Directory.Exists(outputDirectory) == false)
            {
                Directory.CreateDirectory(outputDirectory);
            }

            //finally, begin processing
            //reset max score for A students
            maxScore = 200;

            //based on currently existing code, it is easier to reopen the file for
            //each grade range
            for (int i = 0; i < gradeRanges.Length; i++)
            {
                double bound = gradeRanges[i];

                //reload the files
                LoadFile(filesToProcess[0]);
                for (int j = 1; j < filesToProcess.Count; j++)
                {
                    vm.AppendTimeline(filesToProcess[j]);
                }

                //get grade data
                vm.AttachGrades();

                //filter based on grade data
                vm.FilterByGrade("Assignment AVG", bound, maxScore);

                //get transitions for this grade level
                var result = vm.OrderTransitionsByDate();
                var byDate = result.Item1;

                //update scores for next grade boundary
                maxScore = bound - 0.01;

                //hold all day keys for easier access
                int[] keys = byDate.Keys.OrderBy(k => k).ToArray();

                //figure out all transitions
                Dictionary<string, string> transitionsDict =  new Dictionary<string, string>();
                foreach(int key in keys)
                {
                    foreach (string transition in byDate[key].Keys)
                    {
                        transitionsDict[transition] = transition;
                    }
                }
                string[] transitions = transitionsDict.Keys.ToArray();

                //write aggregate information to a file
                CsvWriter writer = new CsvWriter();

                //blank line for transitions
                writer.AddToCurrentLine("Transition");

                //add in header row
                foreach(int key in keys)
                {
                    writer.AddToCurrentLine(key);
                }
                writer.CreateNewRow();

                //add in data
                foreach(string transition in transitions)
                {
                    //data for given transition
                    writer.AddToCurrentLine("T: " + transition);
                    foreach(int key in keys)
                    {
                        if(byDate[key].ContainsKey(transition))
                        {
                            //add in data for given transition
                            writer.AddToCurrentLine(byDate[key][transition].Count);
                        }
                        else
                        {
                            //no data, add a 0
                            writer.AddToCurrentLine(0);
                        }
                    }
                    writer.CreateNewRow();
                }

                //aggregate class results
                using (TextWriter tw = File.CreateText(string.Format("{0}/aggregate_{1}.csv", outputDirectory, gradeMap[i])))
                {
                    tw.Write(writer.ToString());
                    Console.WriteLine("Created file aggregate_{0}.csv", gradeMap[i]);
                }

                //write individual student information to file
                writer = new CsvWriter();

                writer.AddToCurrentLine("UserID");

                //blank line for transitions
                writer.AddToCurrentLine("Transition");

                //add in header row
                foreach (int key in keys)
                {
                    writer.AddToCurrentLine(key);
                }
                writer.CreateNewRow();

                //add in data
                var userData = result.Item2;
                foreach(int userId in userData.Keys)
                {
                    foreach (string transition in transitions)
                    {
                        //user id
                        writer.AddToCurrentLine(userId);

                        //data for given transition
                        writer.AddToCurrentLine("T: " + transition);
                        foreach (int key in keys)
                        {
                            if (userData[userId][key].ContainsKey(transition))
                            {
                                //add in data for given transition
                                writer.AddToCurrentLine(userData[userId][key][transition].Count);
                            }
                            else
                            {
                                //no data, add a 0
                                writer.AddToCurrentLine(0);
                            }
                        }
                        writer.CreateNewRow();
                    }
                }

                using (TextWriter tw = File.CreateText(string.Format("{0}/students_{1}.csv", outputDirectory, gradeMap[i])))
                {
                    tw.Write(writer.ToString());
                    Console.WriteLine("Created file students_{0}.csv", gradeMap[i]);
                }
            }
        }

        /// <summary>
        /// Filters loaded data based on a given criteria set
        /// </summary>
        /// <param name="vm"></param>
        private void FilterStudentsByGrade(TimelineAnalysisViewModel vm)
        {
            //present options to filter by
            List<string> gradingCriteria = vm.GetAllGrades();
            Console.WriteLine("***Grading Categories***");
            for (int i = 0; i < gradingCriteria.Count; i++)
            {
                Console.WriteLine("{0}: {1}", i, gradingCriteria[i]);
            }
            Console.Write("Select category: ");
            string categoryStr = Console.ReadLine();

            //ask for min/max filter
            Console.Write("Enter max score [100%]: ");
            string maxScoreStr = Console.ReadLine();
            Console.Write("Enter min score [0%]: ");
            string minScoreStr = Console.ReadLine();

            //parse strings, send off to VM for processing
            int category = -1;
            double maxScore = 200;
            double minScore = 0;
            if (Int32.TryParse(categoryStr, out category) && category > -1)
            {
                if (Double.TryParse(maxScoreStr, out maxScore) == false)
                {
                    maxScore = 200;
                }
                if (Double.TryParse(minScoreStr, out minScore) == false)
                {
                    minScore = 0;
                }

                int result = vm.FilterByGrade(gradingCriteria[category], minScore, maxScore);
                Console.WriteLine("{0} entries removed from dataset.", result);
            }
            else
            {
                Console.WriteLine("Error parsing user input.");
            }
        }

        private string GetFile()
        {
            int counter = 1;

            //enumerate files in working directory
            List<string> files = Directory.EnumerateFiles(".", "*.csv").ToList();
            foreach (string file in files)
            {
                Console.WriteLine("{0}. {1}", counter, file);
                counter++;
            }
            Console.Write(">> ");
            int fileToRead = -1;
            string user_input = Console.ReadLine();
            Int32.TryParse(user_input, out fileToRead);

            //ensure valid selection
            if (fileToRead > 0 && fileToRead < counter)
            {
                return files[fileToRead - 1];
            }
            return "";
        }

        private void LoadFile()
        {
            string file = GetFile();
            if (file.Length > 0)
            {
                Vm.LoadTimeline(file);
            }
        }

        private void LoadFile(string file)
        {
            if (file.Length > 0)
            {
                Vm.LoadTimeline(file);
            }
        }

        private void AppendFile()
        {
            Console.WriteLine("Enter files to process (-1 to stop)");
            string fileName = "";
            while ((fileName = GetFile()).Length > 0)
            {
                Vm.AppendTimeline(fileName);
            };

        }

        public void Run()
        {
            int userChoice = 0;
            while (userChoice != (int)MenuOption.Exit)
            {
                Console.WriteLine((int)MenuOption.ChangeDirectory + ". Change working directory");
                Console.WriteLine((int)MenuOption.ListFiles + ". List all files in current directory");
                Console.WriteLine((int)MenuOption.LoadFile + ". Load file (clears existing timeline data)");
                Console.WriteLine((int)MenuOption.AppendFile + ". Append data to existing timeline data");
                Console.WriteLine((int)MenuOption.LoadGrades + ". Attach grade data to students");
                Console.WriteLine((int)MenuOption.FilterGrades + ". Filter students by grade");
                Console.WriteLine((int)MenuOption.NormalizeProgrammingStates + ". Normalize programming states");
                Console.WriteLine((int)MenuOption.CountTransitions + ". Count transitions");
                Console.WriteLine((int)MenuOption.BuildTransitionFrequencyCounts + ". Build transition frequency counts");
                Console.WriteLine((int)MenuOption.BuildAggregateTransitionCounts + ". Build aggregate transition frequency counts");
                Console.WriteLine((int)MenuOption.LocateTransitionCycles + ". Locate transition cycles");
                Console.WriteLine((int)MenuOption.OrderTransitionCyclesByDate + ". Order transition cycles by date");
                Console.WriteLine((int)MenuOption.WriteAggregateToFile + ". Write aggregate results to CSV");
                Console.WriteLine((int)MenuOption.WriteTransitionsToFile + ". Write transition results to CSV");
                Console.WriteLine((int)MenuOption.WriteDataToCsv + ". Write data back to CSV");
                Console.WriteLine((int)MenuOption.Exit + ". Exit");
                Console.Write(">> ");
                string rawInput = Console.ReadLine();
                if (Int32.TryParse(rawInput, out userChoice) == false)
                {
                    Console.WriteLine("Invalid input.");
                }
                else
                {
                    MenuOption selection = (MenuOption)userChoice;
                    switch (selection)
                    {
                        case MenuOption.ChangeDirectory:
                            Console.WriteLine("Feature not implemented.");
                            break;
                        case MenuOption.ListFiles:
                            Console.WriteLine("Feature not implemented.");
                            break;
                        case MenuOption.LoadFile:
                            LoadFile();
                            Console.WriteLine("File load complete.");
                            break;
                        case MenuOption.AppendFile:
                            AppendFile();
                            Console.WriteLine("File appended.");
                            break;
                        case MenuOption.LoadGrades:
                            Vm.AttachGrades();
                            Console.WriteLine("Grades attached.");
                            break;
                        case MenuOption.FilterGrades:
                            FilterStudentsByGrade(Vm);
                            break;
                        case MenuOption.NormalizeProgrammingStates:
                            Vm.NormalizeProgrammingStates();
                            Console.WriteLine("States normalized.");
                            break;
                        case MenuOption.CountTransitions:
                            Vm.ProcessTransitions();
                            Console.WriteLine("Transitions counted.");
                            break;
                        case MenuOption.BuildTransitionFrequencyCounts:
                            BuildTransitionFrequencyCounts(Vm);
                            break;
                        case MenuOption.BuildAggregateTransitionCounts:
                            AggregateTransitionFrequencyCounts(Vm);
                            break;
                        case MenuOption.LocateTransitionCycles:
                            LocateCommonTransitionCycles(Vm);
                            break;
                        case MenuOption.OrderTransitionCyclesByDate:
                            OrderTransitionsByDate(Vm);
                            break;
                        case MenuOption.WriteAggregateToFile:
                            Console.Write("Enter destination file: ");
                            rawInput = Console.ReadLine();
                            Vm.WriteTimeInStateToCsv(rawInput);
                            break;
                        case MenuOption.WriteTransitionsToFile:
                            Console.Write("Enter destination file: ");
                            rawInput = Console.ReadLine();
                            Vm.WriteTransitionsToCsv(rawInput);
                            break;
                        case MenuOption.WriteDataToCsv:
                            Console.Write("Enter destination file: ");
                            rawInput = Console.ReadLine();
                            Vm.WriteLoadedDataToCsv(rawInput);
                            break;
                        case MenuOption.Exit:
                            Console.WriteLine("Returning to main menu.");
                            break;
                        default:
                            break;
                    }
                }
                Console.WriteLine("");
            }
        }


        #region unused / incomplete methods

        ///// <summary>
        ///// Constructs a new Markov model using currently loaded data
        ///// </summary>
        ///// <param name="vm"></param>
        //private void ConstructMarkovModel(TimelineAnalysisViewModel vm)
        //{
        //    //step 1: build states
        //    vm.BuildDefaultMarkovStates();

        //    //construct idle sequences for markov learning
        //    Dictionary<string, int> idleSequences = vm.GetIdleTransitionSequence();

        //    //get learning sequence
        //    List<List<int>> trainingSequence = new List<List<int>>();
        //    foreach(string sequence in idleSequences.Keys)
        //    {
        //        //remove all singletons from consideration
        //        if(idleSequences[sequence] > 1)
        //        {
        //            List<int> toAdd = sequence.Split('_').Select(c => Convert.ToInt32(c)).ToList();
        //            trainingSequence.Add(toAdd);
        //        }
        //    }

        //    //step 2: construct markov model
        //    //vm.BuildMarkovModel(trainingSequence);
        //}

        ///// <summary>
        ///// Processes currently loaded data through the currently loaded markov model
        ///// </summary>
        ///// <param name="vm"></param>
        //private void ProcessMarkovModel(TimelineAnalysisViewModel vm)
        //{

        //    CsvWriter writer = new CsvWriter();

        //    Dictionary<string, int> idleSequences = vm.GetIdleTransitionSequence();
        //    Dictionary<string, double> probabilities = new Dictionary<string, double>();// vm.GetMarkovProbabilities(idleSequences.Keys.ToList());
        //    writer = new CsvWriter();
        //    foreach (KeyValuePair<string, double> kvp in probabilities)
        //    {
        //        writer.AddToCurrentLine(kvp.Key);
        //        writer.AddToCurrentLine(kvp.Value.ToString());
        //        writer.CreateNewRow();
        //    }
        //    using (TextWriter tw = File.CreateText("probabilites.csv"))
        //    {
        //        tw.Write(writer.ToString());
        //    }


        //    Console.WriteLine("Processing idle sequences...");
        //    foreach (KeyValuePair<string, int> kvp in idleSequences)
        //    {
        //        writer.AddToCurrentLine(kvp.Key);
        //        writer.AddToCurrentLine(kvp.Value.ToString());
        //        writer.CreateNewRow();
        //    }
        //    using (TextWriter tw = File.CreateText(string.Format("idle_sequences.csv")))
        //    {
        //        tw.Write(writer.ToString());
        //    }

        //    /*

        //    //step 3: get and determine probability of all possible transitions from the loaded data
        //    for (int i = 2; i < 50; i++)
        //    {
        //        Dictionary<string, int> transitions = vm.GetAllTransitionCombinations(i);
        //        Dictionary<string, double> probabilities = vm.GetMarkovProbabilities(transitions.Keys.ToList());

        //        //write raw counts to CSV
        //        writer = new CsvWriter();
        //        foreach(KeyValuePair<string, int> kvp in transitions)
        //        {
        //            writer.AddToCurrentLine(kvp.Key);
        //            writer.AddToCurrentLine(kvp.Value.ToString());
        //            writer.CreateNewRow();
        //        }
        //        using (TextWriter tw = File.CreateText(string.Format("counts_{0}.csv", i)))
        //        {
        //            tw.Write(writer.ToString());
        //        }

        //        //write probabilities to CSV
        //        writer = new CsvWriter();
        //        foreach(KeyValuePair<string, double> kvp in probabilities)
        //        {
        //            writer.AddToCurrentLine(kvp.Key);
        //            writer.AddToCurrentLine(kvp.Value.ToString());
        //            writer.CreateNewRow();
        //        }
        //        using(TextWriter tw = File.CreateText(string.Format("probabilites_{0}.csv", i)))
        //        {
        //            tw.Write(writer.ToString());
        //        }
        //     }
        //     * */
        //}

        #endregion
    }
}
