using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using OSBIDE.Analytics.Library.Models;
using OSBIDE.Analytics.Terminal.Models;
using OSBIDE.Library.CSV;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.ViewModels
{
    public class TimelineAnalysisViewModel
    {
        /*programming states: 
            * ?? - Unknown
            * Y? - SynYSemU
            * YN - SynYSemN
            * N? - SynNSemU
            * NN - SynNSemN
            * D? - Debugging SemU
            * DN - Debugging SemN
            * RN - Run SemU
            * R? - Run SemN
            * R/ - Run SynNSemU
            * 
            */
        //                                       0     1      2    3     4     5     6     7     8     9
        private string[] _intersting_states = { "??", "Y?", "YN", "N?", "NN", "D?", "DN", "RN", "R?", "R/" };

        private HiddenMarkovModel _model;

        public Dictionary<int, StudentTimeline> Timeline { get; set; }

        private OsbideContext _db { get; set; }

        public TimelineAnalysisViewModel()
        {
            _db = OsbideContext.DefaultWebConnection;
            Timeline = new Dictionary<int, StudentTimeline>();
        }

        public string[][] InterestingSequences = { 
                                               //four possible editing / running cycles
                                               new string[]{"Y?", "R?"},
                                               new string[]{"YN", "RN"},
                                               new string[]{"N?", "R/"},
                                               new string[]{"NN", "R/"},

                                               //two possible editing / debugging cycles
                                               new string[]{"Y?", "D?"},
                                               new string[]{"YN", "DN"},

                                               //moving between syntactically correct / incorrect
                                               new string[]{"Y?", "N?"},

                                               //encountering debugging errors
                                               new string[]{"Y?", "D?", "Y?", "YN" },

                                               //fixing debug errors
                                               new string[]{"YN", "DN", "Y?" },

                                               //top sequences as identified by algorithm
                                               new string[]{"--", "YN"},
                                               new string[]{"--", "Y?"},
                                               new string[]{"N?", "R/", "Y?"},
                                               new string[]{"--", "N?"},
                                               new string[]{"--", "??"},
                                               new string[]{"RN", "DN", "YN"},
                                               new string[]{"Y?", "R?", "D?"},
                                               new string[]{"DN", "Y?", "D?"},
                                               new string[]{"YN", "DN", "Y?", "D?"}
                                           };

        private Dictionary<int, Dictionary<string, List<PatternParserResult>>> _statesByUser;

        /// <summary>
        /// GET: gets states by user
        /// SET: recalcuates user states
        /// </summary>
        public Dictionary<int, Dictionary<string, List<PatternParserResult>>> StatesByUser
        {
            get 
            {
                if(_statesByUser == null)
                {
                    _statesByUser = LocateCycles();
                }
                return _statesByUser;
            }
            set
            {
                _statesByUser = LocateCycles();
            }
        }

        /// <summary>
        /// Returns all states present in the currently loaded Timeline
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllStates()
        {
            Dictionary<string, string> allStates = new Dictionary<string, string>();
            foreach (var user in Timeline)
            {
                foreach (var state in user.Value.RawStates)
                {
                    allStates[state.State] = state.State;
                }
            }
            List<string> result = allStates.Keys.ToList();
            result.Sort();
            return result;
        }

        /// <summary>
        /// returns a list of all grades present in the currently loaded timeline
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllGrades()
        {
            Dictionary<string, string> allGrades = new Dictionary<string, string>();
            foreach (var user in Timeline.Values)
            {
                foreach (var grade in user.Grades)
                {
                    allGrades[grade.Key] = grade.Key;
                }
            }
            List<string> result = allGrades.Keys.ToList();
            result.Sort();
            return result;
        }

        private Dictionary<int, Dictionary<string, List<PatternParserResult>>> LocateCycles()
        {

            //build markov states
            BuildDefaultMarkovStates();

            //captures the final output by user and by sequence
            Dictionary<int, Dictionary<string, List<PatternParserResult>>> statesByUser = new Dictionary<int, Dictionary<string, List<PatternParserResult>>>();

            //for each student, locate desired sequences
            foreach (var user in Timeline.Values)
            {
                //add record for user
                statesByUser.Add(user.OsbideId, new Dictionary<string, List<PatternParserResult>>());

                foreach (string[] sequence in InterestingSequences)
                {

                    //create new parser, prime parser with states to locate
                    StatePatternParser parser = new StatePatternParser();
                    foreach (string state in sequence)
                    {
                        parser.Sequence.Add(new TimelineState { State = state });
                    }

                    //find cycles
                    List<PatternParserResult> result = parser.ParseTimelineSequence(user.MarkovStates);

                    //remove all cycles whose lenght is equal to the sequence length as they're not really cycles
                    //(e.g. cycle: Y? -> R? really needs to be Y? -> R? -> Y? to be a cycle)
                    //List<int> indicesToRemove = new List<int>();
                    //for (int i = 0; i < result.Count; i++)
                    //{
                    //    if (result[i].StateSequence.Count == sequence.Length)
                    //    {
                    //        indicesToRemove.Add(i);
                    //    }
                    //}
                    //for (int i = indicesToRemove.Count - 1; i >= 0; i--)
                    //{
                    //    int index = indicesToRemove[i];
                    //    result.RemoveAt(indicesToRemove[i]);
                    //}

                    //add surviving records for particular sequence
                    string sequenceAsString = String.Join("_", sequence);
                    statesByUser[user.OsbideId].Add(sequenceAsString, result);
                }
            }
            return statesByUser;
        }

        /// <summary>
        /// Orders transitions by proximity to due date.  0 = due date, -1 = day before due date, etc.
        /// </summary>
        /// <returns></returns>
        public Tuple<Dictionary<int, Dictionary<string, List<TimelineState>>>, Dictionary<int, Dictionary<int, Dictionary<string, List<TimelineState>>>>> OrderTransitionsByDate()
        {
            //clear out states by user
            _statesByUser = null;

            //get new states for users
            Dictionary<int, Dictionary<string, List<PatternParserResult>>> cycles = StatesByUser;
            Dictionary<int, Dictionary<DateTime, Dictionary<string, List<TimelineState>>>> cyclesByDate = new Dictionary<int, Dictionary<DateTime, Dictionary<string, List<TimelineState>>>>();

            //keeps track of all possible dates in the class
            List<DateTime> validDates = new List<DateTime>();
            DateTime endDate = new DateTime(2014, 5, 14);
            for (DateTime startDate = new DateTime(2014, 1, 15); startDate <= endDate; startDate = startDate.AddDays(1))
            {
                validDates.Add(startDate);
            }

            /*
             * Algorithm:
             *  1.  For each student, record activity on that date
             *  * */

            foreach (int userId in cycles.Keys)
            {
                //1. prime user with a bunch of empty data for all valid dates
                foreach (DateTime currentDate in validDates)
                {
                    if(cyclesByDate.ContainsKey(userId) == false)
                    {
                        cyclesByDate.Add(userId, new Dictionary<DateTime, Dictionary<string, List<TimelineState>>>());
                    }
                    cyclesByDate[userId].Add(currentDate, new Dictionary<string, List<TimelineState>>());
                }

                //2. cycles by date orders information by cycle, so we have to manually go through each pattern to aggregate by date
                Dictionary<DateTime, Dictionary<string, List<TimelineState>>> dailyCycles = new Dictionary<DateTime, Dictionary<string, List<TimelineState>>>();
                foreach (string cycle in cycles[userId].Keys)
                {
                    foreach (PatternParserResult patterns in cycles[userId][cycle])
                    {
                        foreach (TimelineState state in patterns.StateSequence)
                        {
                            //add date if we haven't seen it before
                            if (dailyCycles.ContainsKey(state.StartTime.Date) == false)
                            {
                                dailyCycles.Add(state.StartTime.Date, new Dictionary<string, List<TimelineState>>());
                            }

                            //add particular cycle if we haven't seen it before
                            if (dailyCycles[state.StartTime.Date].ContainsKey(cycle) == false)
                            {
                                dailyCycles[state.StartTime.Date].Add(cycle, new List<TimelineState>());
                            }

                            //record this particular state
                            dailyCycles[state.StartTime.Date][cycle].Add(state);
                        }
                    }
                }

                //3. Update blanks generated in #1 with real data
                foreach (var item in dailyCycles)
                {
                    cyclesByDate[userId][item.Key] = item.Value;
                }
            }

            //break cycles based on assignment due date.  Look at change over time
            DateTime[] dueDates = new DateTime[] { 
                new DateTime(2014, 1, 14)       //1st homework assigned
                , new DateTime(2014, 2, 7)      //1st homework due
                , new DateTime(2014, 2, 21)     //2nd homework due
                , new DateTime(2014, 3, 7)      //hw 3 due
                , new DateTime(2014, 3, 16)     //hw4 due
                , new DateTime(2014, 4, 7)      //hw5 due
                , new DateTime(2014, 4, 18)     //hw6 due
                , new DateTime(2014, 5, 4)      //hw7 due
            };

            Dictionary<int, Dictionary<string, List<TimelineState>>> statesByDueDate = new Dictionary<int, Dictionary<string, List<TimelineState>>>();
            Dictionary<int, Dictionary<int, Dictionary<string, List<TimelineState>>>> statesByUserByDate = new Dictionary<int, Dictionary<int, Dictionary<string, List<TimelineState>>>>();
            foreach (int userId in cyclesByDate.Keys)
            {
                statesByUserByDate.Add(userId, new Dictionary<int, Dictionary<string, List<TimelineState>>>());

                //pull all activity, organize by due date
                for (int i = 0; i < dueDates.Length - 1; i += 2)
                {
                    //figure out assigned / due date
                    DateTime hwAssignedDate = dueDates[i];
                    DateTime hwDueDate = dueDates[i + 1];

                    int daysToDueDate = 0;
                    for (DateTime currentDate = hwDueDate; currentDate > hwAssignedDate; currentDate = currentDate.AddDays(-1))
                    {
                        if(statesByDueDate.ContainsKey(daysToDueDate) == false)
                        {
                            statesByDueDate.Add(daysToDueDate, new Dictionary<string, List<TimelineState>>());
                        }
                        if(statesByUserByDate[userId].ContainsKey(daysToDueDate) == false)
                        {
                            statesByUserByDate[userId].Add(daysToDueDate, new Dictionary<string, List<TimelineState>>());
                        }

                        //activity for this user on this day?
                        if (cyclesByDate[userId].ContainsKey(currentDate))
                        {
                            //transfer over
                            foreach (var cycle in cyclesByDate[userId][currentDate])
                            {
                                if (statesByDueDate[daysToDueDate].ContainsKey(cycle.Key) == false)
                                {
                                    statesByDueDate[daysToDueDate].Add(cycle.Key, new List<TimelineState>());
                                }
                                if(statesByUserByDate[userId][daysToDueDate].ContainsKey(cycle.Key) == false)
                                {
                                    statesByUserByDate[userId][daysToDueDate].Add(cycle.Key, new List<TimelineState>());
                                }
                                foreach (var state in cycle.Value)
                                {
                                    statesByDueDate[daysToDueDate][cycle.Key].Add(state);
                                    statesByUserByDate[userId][daysToDueDate][cycle.Key].Add(state);
                                }
                            }
                        }

                        daysToDueDate--;
                    }
                }
            }
            return new Tuple<Dictionary<int,Dictionary<string,List<TimelineState>>>,Dictionary<int,Dictionary<int,Dictionary<string,List<TimelineState>>>>>(statesByDueDate, statesByUserByDate);
        }

        /// <summary>
        /// Removes students from timeline if they fail to be within a certain range for a given grading cateogry
        /// </summary>
        /// <param name="category"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        public int FilterByGrade(string category, double minScore, double maxScore)
        {
            Dictionary<int, int> keysToRemove = new Dictionary<int, int>();
            foreach(var kvp in Timeline)
            {
                var user = kvp.Value;

                //not having grading category immediately flags user for removal
                if(user.Grades.ContainsKey(category))
                {
                    //is the user's score not within bounds?
                    if(user.Grades[category] < minScore || user.Grades[category] > maxScore)
                    {
                        keysToRemove[kvp.Key] = kvp.Key;
                    }
                }
                else
                {
                    keysToRemove[kvp.Key] = kvp.Key;
                }
            }

            //with keys known, remove from loade users
            foreach(int key in keysToRemove.Keys)
            {
                Timeline.Remove(key);
            }
            return keysToRemove.Count;
        }

        /// <summary>
        /// Attaches grade data to the currently loaded timeline
        /// </summary>
        public void AttachGrades()
        {
            //pull grades from DB
            var query = from user in _db.Users
                        join grade in _db.StudentGrades on user.InstitutionId equals grade.StudentId
                        where Timeline.Keys.Contains(user.Id)
                        && grade.CourseId == 3 //may need to comment out in order to get correct grade data
                        select new { UserId = user.Id, StudentId = user.InstitutionId, Deliverable = grade.Deliverable, Grade = grade.Grade };

            //add to timeline students
            foreach (var item in query)
            {
                if (Timeline.ContainsKey(item.UserId) == true)
                {
                    if (Timeline[item.UserId].Grades.ContainsKey(item.Deliverable) == false)
                    {
                        Timeline[item.UserId].Grades.Add(item.Deliverable, item.Grade);
                    }
                    Timeline[item.UserId].Grades[item.Deliverable] = item.Grade;
                }
            }
        }

        public string StateNumberToNpsmString(int state)
        {
            if(state >= _intersting_states.Length)
            {
                return "--";
            }
            return _intersting_states[state];
        }

        /// <summary>
        /// Helper method used in some of the Markov functions
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, int> GetInterestingStatesAsDictionary()
        {
            Dictionary<string, int> interestingStates = new Dictionary<string, int>();
            for (int i = 0; i < _intersting_states.Length; i++)
            {
                interestingStates.Add(_intersting_states[i], i);
            }

            //for markov models, we use inactivity as an interesting state
            interestingStates.Add("--", interestingStates.Keys.Count);
            return interestingStates;
        }

        /// <summary>
        /// Converts each student timeline into a numerical sequence suitable for machine learning
        /// </summary>
        public void BuildDefaultMarkovStates()
        {
            //convert interesting states array into dictionary for faster lookup in algorithm
            Dictionary<string, int> interestingStates = GetInterestingStatesAsDictionary();

            foreach (StudentTimeline timeline in Timeline.Values)
            {
                //clear existing markov state list
                timeline.MarkovSequence = new List<int>();
                timeline.MarkovStates = new List<TimelineState>();

                //It is possible for other events to interrupt IDE events.  E.g. A social event occurs during
                //the ?? stage.  We use this variable to track whether or not the previous state is connected
                //to the current state, which should prevent self-cycling between the same event.
                int previousState = -1;

                //convert timeline state to integer markov representation
                foreach (TimelineState state in timeline.RawStates)
                {
                    //only add "interesting" states (programming states)
                    if (interestingStates.ContainsKey(state.State) == true)
                    {
                        //prevent transitioning between the same event
                        if(interestingStates[state.State] != previousState) //depending on analysis, might want to turn this off
                        {
                            timeline.MarkovSequence.Add(interestingStates[state.State]);
                            timeline.MarkovStates.Add(state);
                            previousState = interestingStates[state.State];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a dictionary of all transitions of the supplied length for all loaded data
        /// </summary>
        /// <param name="transitionLength"></param>
        public Dictionary<string, int> GetAllTransitionCombinations(int transitionLength)
        {
            Dictionary<string, int> transitions = new Dictionary<string, int>();
            foreach (StudentTimeline timeline in Timeline.Values)
            {
                //starting at the first sequence, figure out all possible sequences for each student
                for (int i = 0; i + (transitionLength - 1) < timeline.MarkovSequence.Count; i++)
                {
                    List<int> transitionList = new List<int>();
                    for (int j = i; j < i + transitionLength; j++)
                    {
                        transitionList.Add(timeline.MarkovSequence[j]);
                    }
                    string transition = String.Join("_", transitionList);

                    //do we have this transition?
                    if(transitions.ContainsKey(transition) == false)
                    {
                        //if not, create a new space for it
                        transitions[transition] = 0;
                    }
                    if(timeline.TransitionCounts.ContainsKey(transition) == false)
                    {
                        //also add data for student
                        timeline.TransitionCounts[transition] = 0;
                    }
                    transitions[transition]++;
                    timeline.TransitionCounts[transition]++;
                }
            }
            return transitions;
        }

        /// <summary>
        /// Returns a dictionary of all transition sequences capped by 
        /// periods of inactivity (idle; --)
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetIdleTransitionSequence()
        {
            Dictionary<string, int> transitions = new Dictionary<string, int>();
            Dictionary<string, int> interestingStates = GetInterestingStatesAsDictionary();
            foreach (StudentTimeline timeline in Timeline.Values)
            {
                //clear out existing filtered markov sequence
                timeline.FilteredMarkovSequence = new List<List<int>>();

                //holds action sequence
                List<int> currentActionSeqeunce = new List<int>();

                //always start action sequence with idle
                currentActionSeqeunce.Add(interestingStates["--"]);

                //run through all markov states
                foreach(int state in timeline.MarkovSequence)
                {
                    //push current state onto current action sequence
                    currentActionSeqeunce.Add(state);

                    //end of sequence detected?
                    if(state == interestingStates["--"])
                    {
                        //convert current action sequence to string, add to final dictionary
                        string transition = String.Join("_", currentActionSeqeunce);
                        if(transitions.ContainsKey(transition) == false)
                        {
                            transitions[transition] = 0;
                        }
                        transitions[transition]++;

                        //also add to current markov filtration set for current user
                        timeline.FilteredMarkovSequence.Add(currentActionSeqeunce);

                        currentActionSeqeunce = new List<int>();
                        currentActionSeqeunce.Add(interestingStates["--"]);
                    }
                }
            }
            return transitions;
        }

        public void BuildMarkovModel(List<List<int>> dataset)
        {
            Dictionary<string, int> interestingStates = GetInterestingStatesAsDictionary();

            //create new model
            _model = new HiddenMarkovModel(states: Timeline.Count, symbols: interestingStates.Count);

            //teach model
            BaumWelchLearning teacher = new BaumWelchLearning(_model);
            
            //convert timeline into 2D int array
            int[][] data = dataset.Select(a => a.ToArray()).ToArray();
            teacher.Run(data);
        }

        /// <summary>
        /// Returns a dictionary of Markov probabilities for all supplied sequences
        /// </summary>
        /// <param name="sequences"></param>
        /// <returns></returns>
        public Dictionary<string, double> GetMarkovProbabilities(List<string> sequences)
        {
            Dictionary<string, double> probabilities = new Dictionary<string, double>();

            //determine probability for each sequence
            foreach(string sequence in sequences)
            {
                //each sequence is in the form "#-#-#...-#".  We need to conver this into an array of
                //integers for use in the markov model
                int[] sequenceNumbers = sequence.Split('_').Select(m => Convert.ToInt32(m)).ToArray();

                //store probability in dictionary for use elsewhere
                probabilities[sequence] = Math.Exp(_model.Evaluate(sequenceNumbers));
            }

            return probabilities;
        }

        //adds normalized programming state metrics to each user
        public void NormalizeProgrammingStates()
        {
            //Algorithm: sum states total time, normalize based on total time spent in each state
            foreach (StudentTimeline timeline in Timeline.Values)
            {
                TimeSpan total_time = new TimeSpan(0, 0, 0);

                //pass #1: find total time
                foreach (string state in _intersting_states)
                {
                    TimelineState aggregate = timeline.GetAggregateState(state);
                    total_time += aggregate.TimeInState;
                }

                //add total time to states
                timeline.GetAggregateState("normalized_total_time").StartTime = DateTime.MinValue;
                timeline.GetAggregateState("normalized_total_time").EndTime = DateTime.MinValue + total_time;
                TimelineState totalState = new TimelineState()
                {
                    State = "normalized_total_time",
                    StartTime = DateTime.MinValue,
                    EndTime = timeline.GetAggregateState("normalized_total_time").EndTime
                };
                timeline.RawStates.Add(totalState);

                //pass #2: normalize
                foreach (string state in _intersting_states)
                {
                    string normilzedKey = string.Format("normalized_{0}", state);
                    TimelineState aggregate = timeline.GetAggregateState(state);
                    TimelineState normalizedState = new TimelineState()
                    {
                        State = normilzedKey,
                        NormalizedTimeInState = (aggregate.TimeInState.TotalSeconds / total_time.TotalSeconds) * 100,
                    };

                    //add back to student
                    timeline.GetAggregateState(normilzedKey).NormalizedTimeInState = normalizedState.NormalizedTimeInState;
                    timeline.RawStates.Add(normalizedState);
                }

            }
        }

        /// <summary>
        /// Builds transition counts for each loaded timeline
        /// </summary>
        public void ProcessTransitions()
        {
            foreach (StudentTimeline student in Timeline.Values)
            {
                student.AggregateTransitions();
            }
        }

        /// <summary>
        /// Returns all transitions present in the loaded timeline
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> GetAllTransitions()
        {
            Dictionary<KeyValuePair<string, string>, int> transitions = new Dictionary<KeyValuePair<string, string>, int>();
            foreach (StudentTimeline timeline in Timeline.Values)
            {
                foreach (var transition in timeline.Transitions)
                {
                    transitions[transition.Key] = 0;
                }
            }
            return transitions.Keys.ToList();
        }

        /// <summary>
        /// Writes transition counts to a CSV file
        /// </summary>
        /// <param name="fileName"></param>
        public void WriteTransitionsToCsv(string fileName)
        {
            //write results to file
            CsvWriter writer = new CsvWriter();

            //add header row
            writer.AddToCurrentLine("User ID");
            var query = from item in GetAllTransitions()
                        select new
                        {
                            Key = item.Key,
                            Value = item.Value,
                            AsString = string.Format("{0};{1}", item.Key, item.Value),
                            Kvp = item
                        };
            var transitions = query.OrderBy(q => q.AsString).ToList();
            foreach (var transition in transitions)
            {
                writer.AddToCurrentLine(transition.AsString);
            }

            //add grades if loaded
            List<string> grades = GetAllGrades();
            foreach (string grade in grades)
            {
                writer.AddToCurrentLine(grade);
            }

            writer.CreateNewRow();

            //add data rows
            foreach (var item in Timeline.Values)
            {
                writer.AddToCurrentLine(item.OsbideId.ToString());

                //add state information
                foreach (var transition in transitions)
                {
                    if (item.Transitions.ContainsKey(transition.Kvp) == true)
                    {
                        writer.AddToCurrentLine(item.Transitions[transition.Kvp].ToString());
                    }
                    else
                    {
                        writer.AddToCurrentLine("0");
                    }
                }

                //add grade information
                foreach (string grade in grades)
                {
                    if (item.Grades.ContainsKey(grade) == true)
                    {
                        writer.AddToCurrentLine(item.Grades[grade].ToString());
                    }
                    else
                    {
                        writer.AddToCurrentLine("0");
                    }
                }
                writer.CreateNewRow();
            }

            //write to file
            using (TextWriter tw = File.CreateText(fileName))
            {
                tw.Write(writer.ToString());
            }
        }

        /// <summary>
        /// Writes time in state information to a CSV file
        /// </summary>
        /// <param name="fileName"></param>
        public void WriteTimeInStateToCsv(string fileName)
        {
            //write results to file
            CsvWriter writer = new CsvWriter();

            //add header row
            writer.AddToCurrentLine("User ID");
            List<string> states = GetAllStates();
            foreach (string state in states)
            {
                writer.AddToCurrentLine(state);
            }

            //add grades if loaded
            List<string> grades = GetAllGrades();
            foreach (string grade in grades)
            {
                writer.AddToCurrentLine(grade);
            }

            writer.CreateNewRow();

            //add data rows
            foreach (var item in Timeline.Values)
            {
                writer.AddToCurrentLine(item.OsbideId.ToString());

                //add state information
                foreach (string state in states)
                {
                    //normalized states will use the "NormalizedTimeInState" property.
                    //non-normalized states will use the "MillisecondsInState" property.
                    if (item.GetAggregateState(state).NormalizedTimeInState > 0)
                    {
                        writer.AddToCurrentLine(item.GetAggregateState(state).NormalizedTimeInState.ToString("0.0000000"));
                    }
                    else
                    {
                        writer.AddToCurrentLine(item.GetAggregateState(state).TimeInState.TotalSeconds.ToString());
                    }

                }

                //add grade information
                foreach (string grade in grades)
                {
                    if (item.Grades.ContainsKey(grade) == true)
                    {
                        writer.AddToCurrentLine(item.Grades[grade].ToString());
                    }
                    else
                    {
                        writer.AddToCurrentLine("0");
                    }
                }
                writer.CreateNewRow();
            }

            //write to file
            using (TextWriter tw = File.CreateText(fileName))
            {
                tw.Write(writer.ToString());
            }
        }

        /// <summary>
        /// Writes the loaded timeline information to a CSV.  The format of the CSV file
        /// should match so that it could be read by ParseTimeline()
        /// </summary>
        /// <param name="fileName"></param>
        public void WriteLoadedDataToCsv(string fileName)
        {
            CsvWriter writer = new CsvWriter();
            foreach(var user in Timeline.Values)
            {
                //first column is user id
                writer.AddToCurrentLine(user.OsbideId);

                //all other rows are state transitions
                foreach(TimelineState state in user.RawStates)
                {
                    string stateAsString = "";
                    if(state.IsSocialEvent)
                    {
                        stateAsString = string.Format("{0};{1}", state.State, state.StartTime);
                    }
                    else
                    {
                        stateAsString = string.Format("{0};{1};{2}", state.State, state.StartTime, state.EndTime);
                    }
                    writer.AddToCurrentLine(stateAsString);
                }
                writer.CreateNewRow();
            }

            //write to file
            using (TextWriter tw = File.CreateText(fileName))
            {
                tw.Write(writer.ToString());
            }
        }

        private Dictionary<int, StudentTimeline> ParseTimeline(string fileName, Dictionary<int, StudentTimeline> userStates)
        {
            //get raw data from CSV file
            List<List<string>> rawData = new List<List<string>>();
            using (FileStream fs = File.OpenRead(fileName))
            {
                CsvReader csv = new CsvReader(fs);
                rawData = csv.Parse();
            }

            //convert raw data into object form
            foreach (List<string> pieces in rawData)
            {
                //pull user ID
                int userId = -1;
                Int32.TryParse(pieces[0], out userId);

                if (userStates.ContainsKey(userId) == false)
                {
                    userStates.Add(userId, new StudentTimeline());
                }

                foreach (string entry in pieces)
                {
                    //split data elements
                    string[] parts = entry.Split(new Char[] { ';' });

                    //ignore first record, which is user ID
                    if (parts.Length < 2)
                    {
                        continue;
                    }

                    //build current state
                    TimelineState currentState = new TimelineState();
                    currentState.State = parts[0];
                    currentState.UserId = userId;
                    DateTime tempDate = DateTime.MinValue;
                    DateTime.TryParse(parts[1], out tempDate);
                    currentState.StartTime = tempDate;

                    //two items = social event
                    if (parts.Length == 2)
                    {
                        currentState.IsSocialEvent = true;

                        //social events do not have an end time
                        currentState.EndTime = currentState.StartTime;
                    }
                    else
                    {
                        tempDate = DateTime.MinValue;
                        DateTime.TryParse(parts[2], out tempDate);
                        currentState.EndTime = tempDate;
                    }

                    //add to dictionary
                    userStates[userId].OsbideId = userId;
                    userStates[userId].RawStates.Add(currentState);
                }
            }
            return userStates;
        }

        /// <summary>
        /// Loads a new timeline from the supplied file
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadTimeline(string fileName)
        {
            Timeline = ParseTimeline(fileName, new Dictionary<int, StudentTimeline>());
        }

        /// <summary>
        /// Appends a new file's timeline onto an already loaded timeline
        /// </summary>
        /// <param name="fileName"></param>
        public void AppendTimeline(string fileName)
        {
            Timeline = ParseTimeline(fileName, Timeline);
        }
    }
}
