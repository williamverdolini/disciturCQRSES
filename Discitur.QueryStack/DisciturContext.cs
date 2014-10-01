using Discitur.QueryStack.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Discitur.QueryStack
{
    public class DisciturContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public DisciturContext() : base("name=DisciturContext")
        {
        }

        public DbSet<Lesson> Lessons { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserActivation> UserActivations { get; set; }

        public DbSet<LessonFeedback> LessonFeedbacks { get; set; }

        public DbSet<LessonTag> LessonTags { get; set; }

        public DbSet<LessonComment> LessonComments { get; set; }

        public DbSet<LessonRating> LessonRatings { get; set; }

        public virtual DbSet<IdentityMap> IdMap { get; set; }
    }
}
