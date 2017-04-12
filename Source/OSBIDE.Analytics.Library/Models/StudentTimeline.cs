using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class StudentTimeline
    {
        public int OsbideId { get; set; }
        public int StudentId { get; set; }
        public Dictionary<KeyValuePair<string, string>, int> Transitions { get; set; }
        public Dictionary<string, int> TransitionCounts { get; set; }
        public List<TimelineState> RawStates { get; set; }
        public List<int> MarkovSequence { get; set; }
        public List<TimelineState> MarkovStates { get; set; }
        public List<List<int>> FilteredMarkovSequence { get; set; }

        public Dictionary<string, double> Grades { get; set; }
        private Dictionary<string, TimelineState> _aggregateStates { get; set; }

        public StudentTimeline()
        {
            FilteredMarkovSequence = new List<List<int>>();
            MarkovStates = new List<TimelineState>();
            TransitionCounts = new Dictionary<string, int>();
            RawStates = new List<TimelineState>();
            MarkovSequence = new List<int>();
            Grades = new Dictionary<string, double>();
            _aggregateStates = new Dictionary<string, TimelineState>();
            Transitions = new Dictionary<KeyValuePair<string, string>, int>();
        }

        //returns aggregate information for a given state
        public TimelineState GetAggregateState(string key)
        {
            //check to see if we need to load the state
            if (_aggregateStates.Count == 0)
            {
                foreach (TimelineState state in RawStates)
                {
                    if (_aggregateStates.ContainsKey(state.State) == false)
                    {
                        _aggregateStates.Add(state.State, new TimelineState()
                        {
                            State = state.State,
                            IsSocialEvent = state.IsSocialEvent,
                            StartTime = DateTime.MinValue,
                            EndTime = DateTime.MinValue
                        });
                    }

                    //social states are instant, so we'll just increment a single tick to mark an individual event
                    if (state.IsSocialEvent == false)
                    {
                        TimeSpan difference = state.EndTime - state.StartTime;

                        //currently, there's a bug that causes some events to create negaitve time.  Ignore these events.
                        if (difference.TotalSeconds > 0)
                        {
                            _aggregateStates[state.State].EndTime += difference;
                        }
                    }
                    else
                    {
                        _aggregateStates[state.State].EndTime += new TimeSpan(0, 0, 1);
                    }
                }
            }
            if (_aggregateStates.ContainsKey(key))
            {
                return _aggregateStates[key];
            }
            else
            {
                _aggregateStates[key] = new TimelineState() { };
                return _aggregateStates[key];
            }
        }


        public void AggregateTransitions()
        {
            //clear existing transitions
            Transitions.Clear();

            for (int i = 0; i < RawStates.Count - 1; i++)
            {
                //starting state
                TimelineState currentState = RawStates[i];

                //non-social state?
                if (currentState.IsSocialEvent == true)
                {
                    //try again
                    continue;
                }

                //pointer to next index
                int nextIndex = i + 1;

                //and the next one
                TimelineState nextState = RawStates[nextIndex];

                //handle social events differently.  Like normal, place them in a bucket that is of the form
                //State -> Social Event.  However, do not increment currentState until we find a matching
                //non-social event.
                while (nextIndex < RawStates.Count && nextState.IsSocialEvent == true)
                {
                    KeyValuePair<string, string> transition = new KeyValuePair<string, string>(currentState.State, currentState.State);
                    if (Transitions.ContainsKey(transition) == false)
                    {
                        Transitions.Add(transition, 0);
                    }
                    Transitions[transition]++;
                    nextIndex++;
                }

                //at this point, currentState and nextState should both be non-social events.  Record transition.
                KeyValuePair<string, string> key = new KeyValuePair<string, string>(currentState.State, nextState.State);
                if (Transitions.ContainsKey(key) == false)
                {
                    Transitions.Add(key, 0);
                }
                Transitions[key]++;
            }
        }

        //resets aggregate state information 
        public void ClearAggregateStates()
        {
            _aggregateStates.Clear();
        }
    }
}
