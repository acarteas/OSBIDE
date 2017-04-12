using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class OsbideContextSeeder
    {
        public static void Seed(OsbideContext context)
        {
            //add in some sample schools
            School wsu = new School() { Name = "Washington State University" };
            context.Schools.Add(wsu);
            context.Schools.Add(new School() { Name = "Other Institution" });
            context.SaveChanges();

            //add in default chat rooms
            context.ChatRooms.Add(new ChatRoom() { Name = "General Chat", SchoolId = wsu.Id, IsDefaultRoom = true });
            context.ChatRooms.Add(new ChatRoom() { Name = "CptS 121 Chat", SchoolId = wsu.Id, IsDefaultRoom = false });
            context.ChatRooms.Add(new ChatRoom() { Name = "CptS 122 Chat", SchoolId = wsu.Id, IsDefaultRoom = false });
            context.ChatRooms.Add(new ChatRoom() { Name = "CptS 223 Chat", SchoolId = wsu.Id, IsDefaultRoom = false });

            //[obsolete]
            //add in some default subscriptions
            //context.UserSubscriptions.Add(new UserSubscription() { ObserverInstitutionId = 123, ObserverSchoolId = 1, SubjectSchoolId = 1, SubjectInstitutionId = 456 });
            //context.UserSubscriptions.Add(new UserSubscription() { ObserverInstitutionId = 123, ObserverSchoolId = 1, SubjectSchoolId = 1, SubjectInstitutionId = 789, IsRequiredSubscription = true });
            //context.UserSubscriptions.Add(new UserSubscription() { ObserverInstitutionId = 456, ObserverSchoolId = 1, SubjectSchoolId = 1, SubjectInstitutionId = 789, IsRequiredSubscription = true });

            //also set up some courses
            context.Courses.Add(new Course()
            {
                Name = "OSBIDE 101",
                Year = 2014,
                Season = "Spring",
                SchoolId = 1,
                Description = "Everything you ever wanted to know about OSBIDE."
            }
                );

            context.Courses.Add(new Course()
            {
                Name = "CptS 121",
                Year = 2014,
                Season = "Spring",
                SchoolId = 1,
                Description = "Formulation of problems and top-down design of programs in a modern structured language for their solution on a digital computer."
            }
                );

            context.Courses.Add(new Course()
            {
                Name = "CptS 122",
                Year = 2014,
                Season = "Spring",
                SchoolId = 1,
                Description = "This course is about advanced programming techniques, data structures, recursion, sorting, searching, and basic algorithm analysis."
            }
            );

            context.Courses.Add(new Course()
            {
                Name = "CptS 223",
                Year = 2014,
                Season = "Spring",
                SchoolId = 1,
                Description = "Advanced data structures, object oriented programming concepts, concurrency, and program design principles."
            }
            );

            context.Courses.Add(new Course()
            {
                Name = "CptS 483",
                Year = 2014,
                Season = "Spring",
                SchoolId = 1,
                Description = "Web development"
            }
            );

            context.SaveChanges();

            //add some test users
            IdenticonRenderer renderer = new IdenticonRenderer();
            OsbideUser joe = new OsbideUser()
                {
                    FirstName = "Joe",
                    LastName = "User",
                    Email = "joe@user.com",
                    InstitutionId = 123,
                    SchoolId = wsu.Id,
                    Role = SystemRole.Student,
                    Gender = Gender.Male,
                    DefaultCourseId = 1
                };
            joe.SetProfileImage(renderer.Render(joe.Email.GetHashCode(), 128));
            context.Users.Add(joe);

            OsbideUser betty = new OsbideUser()
            {
                FirstName = "Betty",
                LastName = "Rogers",
                Email = "betty@rogers.com",
                InstitutionId = 456,
                SchoolId = wsu.Id,
                Role = SystemRole.Student,
                Gender = Gender.Female,
                DefaultCourseId = 1
            };
            betty.SetProfileImage(renderer.Render(betty.Email.GetHashCode(), 128));
            context.Users.Add(betty);
            context.SaveChanges();

            OsbideUser adam = new OsbideUser()
            {
                FirstName = "Adam",
                LastName = "Carter",
                Email = "cartera@wsu.edu",
                InstitutionId = 789,
                SchoolId = wsu.Id,
                Role = SystemRole.Instructor,
                Gender = Gender.Male,
                DefaultCourseId = 1
            };
            adam.SetProfileImage(renderer.Render(adam.Email.GetHashCode(), 128));
            context.Users.Add(adam);
            context.SaveChanges();

            //...and set their passwords
            UserPassword up = new UserPassword();
            up.UserId = joe.Id;
            up.Password = UserPassword.EncryptPassword("123123", joe);
            context.UserPasswords.Add(up);

            up = new UserPassword();
            up.UserId = betty.Id;
            up.Password = UserPassword.EncryptPassword("123123", betty);
            context.UserPasswords.Add(up);

            up = new UserPassword();
            up.UserId = adam.Id;
            up.Password = UserPassword.EncryptPassword("123123", adam);
            context.UserPasswords.Add(up);
            context.SaveChanges();

            //add students to the courses
            context.Courses.Find(1).CourseUserRelationships.Add(new CourseUserRelationship() { UserId = 3, CourseId = 1, Role = CourseRole.Coordinator });
            context.Courses.Find(1).CourseUserRelationships.Add(new CourseUserRelationship() { UserId = 1, CourseId = 1, Role = CourseRole.Student });
            context.Courses.Find(1).CourseUserRelationships.Add(new CourseUserRelationship() { UserId = 2, CourseId = 1, Role = CourseRole.Assistant });

            context.Courses.Find(2).CourseUserRelationships.Add(new CourseUserRelationship() { UserId = 3, CourseId = 2, Role = CourseRole.Coordinator});
            context.Courses.Find(2).CourseUserRelationships.Add(new CourseUserRelationship() { UserId = 1, CourseId = 2, Role = CourseRole.Student });
            context.Courses.Find(2).CourseUserRelationships.Add(new CourseUserRelationship() { UserId = 2, CourseId = 2, Role = CourseRole.Assistant });
            context.SaveChanges();
        }
    }
}
