using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class AnalyticsDbContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<QuestionResponseCount> QuestionResponseCounts { get; set; }
        public DbSet<QuestionCoding> QuestionCodings { get; set; }
        public DbSet<ContentCoding> ContentCodings { get; set; }
        public DbSet<TimelineState> TimelineStates { get; set; }
        public DbSet<AnswerCoding> AnswerCodings { get; set; }
        public DbSet<CommentTimeline> CommentTimelines { get; set; }
        public DbSet<TimelineQuestionCoding> TimelineQuestionCodings { get; set; }
        public DbSet<TimelineAnswerCoding> TimelineAnswerCodings { get; set; }
        public DbSet<TimelineCodeDocument> TimelineCodeDocuments { get; set; }
        public AnalyticsDbContext()
            : base()
        {
        }

        public AnalyticsDbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public AnalyticsDbContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public AnalyticsDbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext)
            : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        public AnalyticsDbContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public AnalyticsDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public AnalyticsDbContext(DbCompiledModel model)
            : base(model)
        {
        }

        public static AnalyticsDbContext DefaultWebConnection
        {
            get
            {
                AnalyticsDbContext _db;
#if DEBUG
                try
                {
                    _db = new AnalyticsDbContext("AnalyticsDebugContext");
                    Database.SetInitializer<AnalyticsDbContext>(new AnalyticsDbContextIfNotExistsInitializer());
                    //Database.SetInitializer<OsbideContext>(new OsbideContextModelChangeInitializer());

                    //uncomment this line (and comment out the one above) when VS is acting stupid and won't
                    //recreate the database on model change.
                    //Database.SetInitializer<OsbideContext>(new OsbideContextAlwaysCreateInitializer());
                }
                catch
                {
                    _db = new AnalyticsDbContext("AnalyticsDebugContext");
                }
#else
                _db = new AnalyticsDbContext("AnalyticsReleaseContext");
#endif
                return _db;
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //load in any model builder extensions (usually foreign key relationships)
            //from the models
            List<Type> componentObjects = (from type in Assembly.GetExecutingAssembly().GetTypes()
                                           where
                                           type.GetInterface("IModelBuilderExtender") != null
                                           &&
                                           type.IsInterface == false
                                           &&
                                           type.IsAbstract == false
                                           select type).ToList();
            foreach (Type component in componentObjects)
            {
                IModelBuilderExtender builder = Activator.CreateInstance(component) as IModelBuilderExtender;
                builder.BuildRelationship(modelBuilder);
            }
        }
    }
}
