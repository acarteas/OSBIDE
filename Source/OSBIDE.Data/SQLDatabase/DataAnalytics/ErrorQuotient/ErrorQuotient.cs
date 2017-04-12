using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Data.SQLDatabase.DataAnalytics
{
    public class ErrorQuotient
    {
        public static decimal Calculate(ErrorQuotientParams eparams, IEnumerable<BuildErrorEvent> sessionEvents)
        {
            var orderedEvents = sessionEvents.OrderBy(e => e.EventDate).ToArray();
            var score = 0m;

            // process session events
            for (var i = 0; i < orderedEvents.Length - 1; i++)
            {
                var currentEvent = orderedEvents[i];
                var nextEvent = orderedEvents[i + 1];

                // do both events end in errors?
                if (currentEvent.ErrorTypes != null && nextEvent.ErrorTypes != null)
                {
                    // yes
                    score += 2;
                }

                // same error type?
                if (currentEvent.ErrorTypes != null
                    && nextEvent.ErrorTypes != null
                    && currentEvent.ErrorTypes.Select(e => e.ErrorTypeId).Intersect(nextEvent.ErrorTypes.Select(e => e.ErrorTypeId)).Count() > 0)
                {
                    // yes
                    score += 3;

                    if (eparams.EtypeSamePenalty.HasValue)
                    {
                        // apply etype-same-penalty when the error type reported is the same as the previous compilation result
                        score += eparams.EtypeSamePenalty.Value;
                    }
                }
                else if (eparams.EtypeDiffPenalty.HasValue)
                {
                    // apply etype-diff-penalty when the error type is different
                    score += eparams.EtypeDiffPenalty.Value;
                }

                // same error or edit locations?
                if (currentEvent.Documents != null
                    && nextEvent.Documents != null
                    && currentEvent.Documents
                                   .Select(d => d.FileName.ToLower())
                                   .Intersect(nextEvent.Documents.Select(d => d.FileName.ToLower())).Count() > 0)
                {
                    // same error location?
                    // eline-range defines what constituted an error on the “same line”
                    // a 0 would literally mean that two subsequent errors would need to be on exactly the same line
                    // while the range [-3,3] would indicate that any error within three lines
                    // in either direction would constitute being “on the same line.”
                    if (currentEvent.Documents
                                    .Any(d => nextEvent.Documents
                                                       .Any(nd => string.Compare(nd.FileName, d.FileName, true) == 0
                                                               && nd.Line > d.Line - (eparams.ElineRange.HasValue ? eparams.ElineRange.Value : 1)
                                                               && nd.Line < d.Line + (eparams.ElineRange.HasValue ? eparams.ElineRange.Value : 1)
                                                               && nd.Column == d.Column)))
                    {
                        // yes
                        score += 3;

                        if (eparams.ElinePenalty.HasValue)
                        {
                            // eline-penalty is applied when an error occurs on the same line
                            score += eparams.ElinePenalty.Value;
                        }
                    }

                    // same edit location?
                    if (nextEvent.Documents.Any(nd => nd.NumberOfModified > 0))
                    {
                        // if next build event has modifications
                        foreach (var nd in nextEvent.Documents.Where(d=>d.NumberOfModified > 0))
                        {
                            // check if the previous build event also has modifications
                            // same file couild appear multiple times due to the multi listings in ErrorListItems table
                            var pd = currentEvent.Documents.FirstOrDefault(d => string.Compare(d.FileName, nd.FileName, true) == 0
                                                                              && d.NumberOfModified > 0);

                            // are they on the same lines or in touched-line range?
                            // tlinerange is introduced to mimic elinerage
                            // which defines what constituted an edit or modification on the “same line”
                            if (pd != null && pd.ModifiedLines
                                                .Any(pl => nd.ModifiedLines
                                                             .Any(nl => nl > pl - (eparams.TlineRange.HasValue ? eparams.TlineRange.Value : 1)
                                                                     && nl < pl + (eparams.TlineRange.HasValue ? eparams.TlineRange.Value : 1))))
                            {
                                // yes
                                score += 1;

                                if (eparams.TouchedMultiplier.HasValue)
                                {
                                    // touched-multiplier is a multiplicative factor that applied to the scoring
                                    // when students touched the same line of code from one compilation to the next
                                    score *= eparams.TouchedMultiplier.Value;
                                }

                                // only penalize once per build batch
                                // i.e. when more than one file-locations in the batch being repeatedly modified
                                // the penalty is applied once
                                break;
                            }
                        }
                    }
                }
            }

            return score > 0 ? decimal.Round(score / (9 * ( orderedEvents.Length - 1 ) ), 2) : 0m;
        }
    }
}
