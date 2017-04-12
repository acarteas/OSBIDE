using System.ComponentModel.DataAnnotations;

namespace OSBIDE.Library.Events
{
/*
    public class EventType
    {
        [Key]
        [Required]
        public int EventTypeId { get; set; }
        [Required]
        public string EventTypeName { get; set; }
        [Required]
        public int IsSocialEvent { get; set; }
        [Required]
        public int IsIDEEvent { get; set; }
        [Required]
        public int IsFeedEvent { get; set; }
        [Required]
        public int IsEditEvent { get; set; }
        [Required]
        public int EventTypeCategoryId { get; set; }
    }
*/

    public enum EventTypes
    {
        Unknown = 0,
        AskForHelpEvent = 1,
        BuildEvent = 2,
        CutCopyPasteEvent = 3,
        DebugEvent = 4,
        EditorActivityEvent = 5,
        ExceptionEvent = 6,
        FeedPostEvent = 7,
        HelpfulMarkGivenEvent = 8,
        LogCommentEvent = 9,
        SaveEvent = 10,
        SubmitEvent = 11,
    }
}
