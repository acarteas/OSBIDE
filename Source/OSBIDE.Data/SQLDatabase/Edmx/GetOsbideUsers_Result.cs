//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OSBIDE.Data.SQLDatabase.Edmx
{
    using System;
    
    public partial class GetOsbideUsers_Result
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int SchoolId { get; set; }
        public int InstitutionId { get; set; }
        public bool ReceiveEmailOnNewAskForHelp { get; set; }
        public bool ReceiveEmailOnNewFeedPost { get; set; }
        public bool ReceiveNotificationEmails { get; set; }
        public int DefaultCourseId { get; set; }
        public System.DateTime LastVsActivity { get; set; }
        public string DefaultCourseNumber { get; set; }
        public string DefaultCourseNamePrefix { get; set; }
    }
}
