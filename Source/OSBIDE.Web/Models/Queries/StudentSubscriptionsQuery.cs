using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Library.Models;

namespace OSBIDE.Web.Models.Queries
{
    /// <summary>
    /// Will retrieve a list of users to which the current user (observer) is subscribed.
    /// </summary>
    public class StudentSubscriptionsQuery : IOsbideQuery<OsbideUser>
    {
        private readonly OsbideContext _db;
        private readonly OsbideUser _observer;

        public StudentSubscriptionsQuery(OsbideContext db, OsbideUser observer)
        {
            if (db == null || observer == null)
            {
                throw new Exception("Parameters cannot be null");
            }
            _db = db;
            _observer = observer;
        }

        public IEnumerable<OsbideUser> Execute()
        {
            return (from subscription in _db.UserSubscriptions
                        join user in _db.Users on
                                          new { InstitutionId = subscription.SubjectInstitutionId, SchoolId = subscription.SubjectSchoolId }
                                          equals new { InstitutionId = user.InstitutionId, SchoolId = user.SchoolId }
                        where subscription.ObserverSchoolId == _observer.SchoolId
                           && subscription.ObserverInstitutionId == _observer.InstitutionId
                        select user).ToList();
        }
    }
}