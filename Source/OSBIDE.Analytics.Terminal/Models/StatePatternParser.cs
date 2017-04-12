using OSBIDE.Analytics.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Models
{
    public class StatePatternParser
    {
        public List<TimelineState> Sequence { get; set; }
        
        public StatePatternParser()
        {
            Sequence = new List<TimelineState>();
        }

        public List<PatternParserResult> ParseTimelineSequence(List<TimelineState> inputStates)
        {
            /*
             * Things to consider:
             *  * The input state could start in the middle of a sequence.  As such, we need to
             *    be flexible enough to detect when this occurs.
             *  
             * */

            //keeps track of last state seen in our sequence list
            int lastKnownState = -1;

            //keeps track if we've made a complete cycle through our Sequence
            List<TimelineState> currentCycleList = new List<TimelineState>();

            //captures all matches, will be returned to caller
            List<PatternParserResult> matchedSequences = new List<PatternParserResult>();

            //Loop through the entire input sequence, figuring out where our pattern
            //exists.
            foreach(TimelineState currentState in inputStates)
            {
                //did our state reset last iteration?  If so, see if the current state matches
                //one of the states in the sequence we're looking for
                if(lastKnownState == -1)
                {
                    for (int i = 0; i < Sequence.Count; i++)
                    {
                        if (Sequence[i].State == currentState.State)
                        {
                            lastKnownState = i;
                            currentCycleList.Add(currentState);
                            break;
                        }
                    }
                }
                else
                {
                    //ELSE: we're already in the middle of a sequence chain
                    //Does the current state match the next excpected state in our sequence?
                    int nextSequenceIndex = (lastKnownState + 1) % Sequence.Count;
                    if(Sequence[nextSequenceIndex].State == currentState.State)
                    {
                        //match found
                        lastKnownState = nextSequenceIndex;
                        currentCycleList.Add(currentState);
                    }
                    else
                    {
                        //match not found, reset
                        
                        //do we have at least one cycle?
                        if(currentCycleList.Count >= Sequence.Count)
                        {
                            //remember that we saw this seqeunce
                            matchedSequences.Add(new PatternParserResult() { 
                                StateSequence = currentCycleList,
                                TerminatingState = currentState
                            });
                        }

                        //reset current cycle list
                        currentCycleList = new List<TimelineState>();

                        //reset known state marker
                        lastKnownState = -1;
                    }
                }
            }
            return matchedSequences;
        }
    }
}
