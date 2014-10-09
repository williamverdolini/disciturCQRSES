using Discitur.Infrastructure;
using Discitur.Infrastructure.Api;
using Discitur.QueryStack.Model;
using Discitur.QueryStack.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;

namespace Discitur.QueryStack.Worker
{
    public class LessonQueryWorker : ILessonQueryWorker
    {
        private readonly IDatabase database;

        public LessonQueryWorker(IDatabase db)
        {
            Contract.Requires<ArgumentNullException>(db != null, "db");
            database = db;
        }

        /// <summary>
        /// Searches for lessons based on input parameters. Lessons are paginated and a single paged is returned
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<PagedList<Lesson>> Search(LessonViewModel model) 
        {
            PagedList<Lesson> page = new PagedList<Lesson>();

            IQueryable<Lesson> lessons = SearchedLesson(
                model.Keyword,
                model.InContent,
                model.Discipline,
                model.School,
                model.Classroom,
                model.Rate,
                model.Tags,
                model.PublishedOn,
                model.PublishedBy
            );

            page.Count = lessons.Count();
            page.StartRow = model.StartRow;
            page.PageSize = model.PageSize;

            lessons = lessons.OrderBy(model.OrderBy + " " + model.OrderDir).Skip(model.StartRow).Take(model.PageSize);
            page.Records = await lessons.ToListAsync<Lesson>();

            return page;
        }

        /// <summary>
        /// Finds Discipline name based on string fragment in input
        /// </summary>
        /// <param name="disciplineQ"></param>
        /// <returns></returns>
        public async Task<List<string>> FindDiscipline(string disciplineQ)
        {
            IQueryable<string> disciplines = database.Lessons
                                                .Where(l => l.Discipline.Contains(disciplineQ))
                                                .Select(l => l.Discipline).Distinct();
            return await disciplines.ToListAsync();
        }

        /// <summary>
        /// Finds School name based on string fragment in input
        /// </summary>
        /// <param name="schoolQ"></param>
        /// <returns></returns>
        public async Task<List<string>> FindSchool(string schoolQ)
        {
            IQueryable<string> schools = database.Lessons
                                                .Where(l => l.School.Contains(schoolQ))
                                                .Select(l => l.School).Distinct();
            return await schools.ToListAsync();
        }

        /// <summary>
        /// Finds ClassRoom name based on string fragment in input
        /// </summary>
        /// <param name="classRoomQ"></param>
        /// <returns></returns>
        public async Task<List<string>> FindClassRoom(string classRoomQ)
        {
            IQueryable<string> classRooms = database.Lessons
                                                .Where(l => l.Classroom.Contains(classRoomQ))
                                                .Select(l => l.Classroom).Distinct();
            return await classRooms.ToListAsync();
        }

        /// <summary>
        /// Finds Tag name based on string fragment in input
        /// </summary>
        /// <param name="tagQ"></param>
        /// <returns></returns>
        public async Task<List<string>> FindTags(string tagQ)
        {
            IQueryable<string> tags = database.LessonTags
                                                .Where(l => l.LessonTagName.Contains(tagQ))
                                                .Select(l => l.LessonTagName).Distinct();
            return await tags.ToListAsync();
        }

        /// <summary>
        /// Retrieves a Key-Value pairs list with the last lesson's Id-Title
        /// </summary>
        /// <param name="lastNum"></param>
        /// <returns></returns>
        public List<KeyValuePair<int, string>> LastLessonsId(int lastNum)
        {
            List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();
            IQueryable<Lesson> lessons = database.Lessons;
            lessons = lessons.Where(
                l => l.Published.Equals(Constants.LESSON_PUBLISHED) &&
                l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            lessons = lessons.OrderBy(Constants.LESSON_SEARCH_ORDER_FIELD + " " + Constants.LESSON_SEARCH_ORDER_DIR).Take(lastNum);
            var results = lessons
                .ToList()
                .Select(l => new KeyValuePair<int, string>(l.LessonId, l.Title));

            return results.ToList<KeyValuePair<int, string>>();
        }

        /// <summary>
        /// Gets a Lesson by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Lesson> GetLesson(int id)
        {
            // Only lesson not (logically) deleted are returned
            return await database.Lessons.FirstAsync(l => l.LessonId.Equals(id) && l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
        }

        /// <summary>
        /// Gets the lesson's comments by lesson's Id
        /// </summary>
        /// <param name="lessonId"></param>
        /// <returns></returns>
        public async Task<List<LessonComment>> GetCommentsByLessonId(int lessonId)
        {
            IQueryable<LessonComment> comments = database.LessonComments
                                        .Where(c => c.LessonId.Equals(lessonId) && c.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            return await comments.ToListAsync();
        }

        /// <summary>
        /// Gets a Comment by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<LessonComment> GetCommentById(int id)
        {
            return await database.LessonComments.FirstAsync<LessonComment>(c => c.Id.Equals(id));
        }

        /// <summary>
        /// Gets the lesson's rating by lesson's Id
        /// </summary>
        /// <param name="lessonId"></param>
        /// <returns></returns>
        public async Task<List<LessonRating>> GetRatingsByLessonId(int lessonId)
        {
            IQueryable<LessonRating> ratings = database.LessonRatings
                                        .Where(r => r.LessonId.Equals(lessonId) && r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            return await ratings.ToListAsync();
        }

        /// <summary>
        /// Gets a Rating by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<LessonRating> GetRatingById(int id)
        {
            return await database.LessonRatings.FirstAsync<LessonRating>(c => c.Id.Equals(id));
        }

        public async Task<IList<Lesson>> GetLessons()
        {
            return await database.Lessons.ToListAsync();
        }

        #region Private methods
        private IQueryable<Lesson> SearchedLesson(
            string keyword = null,
            bool inContent = false,
            string discipline = null,
            string school = null,
            string classroom = null,
            int? rate = -1,
            string tags = null,
            string publishedOn = null,
            string publishedBy = null
            )
        {
            IQueryable<Lesson> lessons = database.Lessons;

            if (!String.IsNullOrEmpty(keyword))
            {
                if (inContent)
                    lessons = lessons.Where(l => l.Content.Contains(keyword) || l.Conclusion.Contains(keyword));
                else
                    lessons = lessons.Where(l => l.Title.Contains(keyword));

            }
            if (!String.IsNullOrEmpty(discipline))
                lessons = lessons.Where(l => l.Discipline.Equals(discipline));
            if (!String.IsNullOrEmpty(school))
                lessons = lessons.Where(l => l.School.Equals(school));
            if (!String.IsNullOrEmpty(classroom))
                lessons = lessons.Where(l => l.Classroom.Equals(classroom));
            if (rate > -1)
                lessons = lessons.Where(l => l.Rate.Equals(rate.Value));
            if (!String.IsNullOrEmpty(tags))
            {
                foreach (string tag in GetQueryArrayParameters(tags))
                {
                    lessons = lessons.Where(l => l.Tags.Any(t => t.LessonTagName.Equals(tag)));
                }
            }
            if (!String.IsNullOrEmpty(publishedOn))
                lessons = lessons.Where(l => l.PublishDate.Equals(DateTime.Parse(publishedOn)));
            if (!String.IsNullOrEmpty(publishedBy))
                lessons = lessons.Where(l => l.Author.UserName.Equals(publishedBy));

            // Only published lessons are returned or private lessons (not published for user)
            lessons = lessons.Where(l =>
                l.Published.Equals(Constants.LESSON_PUBLISHED) ||
                (l.Published.Equals(Constants.LESSON_NOT_PUBLISHED) && l.Author.UserName.Equals(publishedBy))
                );
            // Only active lessons are returned
            lessons = lessons.Where(l => l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));

            return lessons;
        }

        private List<string> GetQueryArrayParameters(string arrayValuesQueryString)
        {
            var retval = new List<string>();
            foreach (var item in arrayValuesQueryString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                retval.Add(item);
            }
            return retval;
        }

        #endregion
    }
}
