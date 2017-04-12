using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class FeedViewModel
    {
        public List<AggregateFeedItem> Feed { get; set; }
        public DateTime LastPollDate { get; set; }
        public int LastLogId { get; set; }
        public int SingleUserId { get; set; }
        public List<EventTypes> EventFilterOptions { get; set; }
        public List<EventTypes> UserEventFilterOptions { get; set; }
        public List<ErrorType> ErrorTypes { get; set; }
        public List<Course> Courses { get; set; }
        public List<CourseRole> CourseRoles { get; set; }
        public ErrorType SelectedErrorType { get; set; }
        public CourseRole SelectedCourseRole { get; set; }
        public int SelectedCourseId { get; set; }
        public List<string> RecentUserErrors { get; set; }
        public List<UserBuildErrorsByType> RecentClassErrors { get; set; }
        public string Keyword { get; set; }

        public FeedViewModel()
        {
            Feed = new List<AggregateFeedItem>();
            SingleUserId = -1;
            LastLogId = -1;
            LastPollDate = DateTime.UtcNow;
            EventFilterOptions = new List<EventTypes>();
            UserEventFilterOptions = new List<EventTypes>();
            SelectedErrorType = new ErrorType();
            RecentUserErrors = new List<string>();
            RecentClassErrors = new List<UserBuildErrorsByType>();
            CourseRoles = new List<CourseRole>();
            Courses = new List<Course>();
        }
    }
}