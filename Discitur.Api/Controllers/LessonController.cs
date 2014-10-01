using Discitur.Api.Filters;
using Discitur.CommandStack.ViewModel;
using Discitur.CommandStack.Worker;
using Discitur.Infrastructure;
using Discitur.QueryStack.Model;
using Discitur.QueryStack.ViewModel;
using Discitur.QueryStack.Worker;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Discitur.Api.Controllers
{
    public class LessonController : ApiController
    {
        private readonly ILessonQueryWorker QueryWorker;
        private readonly ILessonCommandWorker CommandWorker;

        public LessonController(ILessonQueryWorker queryWorker, ILessonCommandWorker commandWorker)
        {
            Contract.Requires<System.ArgumentNullException>(queryWorker != null, "queryWorker");
            Contract.Requires<System.ArgumentNullException>(commandWorker != null, "commandWorker");
            QueryWorker = queryWorker;
            CommandWorker = commandWorker;
        }

        #region Commands
        // POST api/Lesson
        [Authorize]
        public IHttpActionResult PostLesson(SaveDraftLessonViewModel lesson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                CommandWorker.SaveNewDraftLesson(lesson);
                return RedirectToRoute("GetLessonById", new { lessonId = lesson.LessonId });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [Route("api/lesson/edit/{id}")]
        [HttpPut]
        //[ResponseType(typeof(Lesson))]
        public IHttpActionResult PutLesson(int id, SaveLessonAndPublishViewModel lesson)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                CommandWorker.SaveDraftLesson(lesson);
                return RedirectToRoute("GetLessonById", new { lessonId = id });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [Route("api/lesson/{lessonId}/comment")]
        [HttpPost]
        public IHttpActionResult PostLessonComment(CommentViewModel comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                CommandWorker.AddNewComment(comment);
                return RedirectToRoute("GetCommentById", new { id = comment.Id.Value });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [Route("api/lesson/{lessonId}/comment/{id}")]
        [HttpPut]
        //[ResponseType(typeof(LessonComment))]
        public IHttpActionResult PutLessonComment(int id, CommentViewModel comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                CommandWorker.EditComment(comment);
                return RedirectToRoute("GetCommentById", new { id = comment.Id.Value });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [Route("api/lesson/{lessonId}/comment/{id}/delete")]
        [HttpPut]
        public IHttpActionResult DeleteLessonComment(int id, CommentViewModel comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                CommandWorker.DeleteComment(comment);
                return RedirectToRoute("GetCommentById", new { id = comment.Id.Value });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
                       
            //// Search for child comments, if exists the comment can't be deleted
            //IQueryable<LessonComment> comments = db.LessonComments
            //                            .Where(c => (c.ParentId.HasValue && c.ParentId.Value.Equals(id)) && c.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
            //int count = await comments.CountAsync();
            //if (count == 0)
            //{
            //    LessonComment _comment = await db.LessonComments.FindAsync(id);
            //    if (_comment == null)
            //    {
            //        return NotFound();
            //    }
            //    _comment.LastModifDate = DateTime.Now;
            //    _comment.LastModifUser = comment.Author.UserName;
            //    _comment.RecordState = Constants.RECORD_STATE_DELETED;

            //    db.Entry(_comment).State = EntityState.Modified;
            //    try
            //    {
            //        await db.SaveChangesAsync();
            //    }
            //    catch (Exception e)
            //    {
            //        return BadRequest(e.Message);
            //    }

            //    return Ok(_comment);

            //}
            //else
            //{
            //    return BadRequest("comment linked by other user's comments");
            //}
        }

        [Authorize]
        [Route("api/lesson/{lessonId}/rating")]
        [HttpPost]
        public IHttpActionResult PostLessonRating(RatingViewModel rating)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                CommandWorker.AddNewRating(rating);
                return RedirectToRoute("GetRatingById", new { id = rating.Id.Value });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }            
        }

        // PUT api/lesson/3/rating/13
        [Authorize]
        [Route("api/lesson/{lessonId}/rating/{id}")]
        [HttpPut]
        public IHttpActionResult PutLessonRating(int id, RatingViewModel rating)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                CommandWorker.EditRating(rating);
                return RedirectToRoute("GetRatingById", new { id = rating.Id.Value });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // PUT api/lesson/3/rating/13/delete
        [Authorize]
        [Route("api/lesson/{lessonId}/rating/{id}/delete")]
        [HttpPut]
        public IHttpActionResult DeleteLessonRating(int id, RatingViewModel rating)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                CommandWorker.DeleteRating(rating);
                return RedirectToRoute("GetRatingById", new { id = rating.Id.Value });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }            
        }

        #endregion

        #region Queries
        [ValidationModelActionFilter]
        [HttpGet]
        public async Task<PagedList<Lesson>> Search([FromUri] LessonViewModel model)
        {
            return await QueryWorker.Search(model);
        }

        [HttpGet]
        public async Task<List<string>> FindDiscipline(string disciplineQ)
        {
            return await QueryWorker.FindDiscipline(disciplineQ);
        }

        [HttpGet]
        public async Task<List<string>> FindSchool(string schoolQ)
        {
            return await QueryWorker.FindSchool(schoolQ);
        }

        [HttpGet]
        public async Task<List<string>> FindClassRoom(string classRoomQ)
        {
            return await QueryWorker.FindClassRoom(classRoomQ);
        }

        [HttpGet]
        public async Task<List<string>> FindTags(string tagQ)
        {
            return await QueryWorker.FindTags(tagQ);
        }

        [HttpGet]
        public List<KeyValuePair<int, string>> LastLessonsId(int lastNum)
        {
            return QueryWorker.LastLessonsId(lastNum);
        }

        [Route("api/lesson/{lessonId}", Name = "GetLessonById")]
        [HttpGet,HttpPut]
        public async Task<Lesson> GetLesson(int lessonId)
        {
            Lesson lesson = await QueryWorker.GetLesson(lessonId);
            if(lesson == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            return lesson;
        }

        [Route("api/lesson/{lessonId}/comments")]
        [HttpGet]
        public async Task<List<LessonComment>> GetCommentsByLessonId(int lessonId)
        {
            return await QueryWorker.GetCommentsByLessonId(lessonId);
        }

        [Route("api/comment/{id}", Name = "GetCommentById")]
        [HttpGet,HttpPut]
        public async Task<LessonComment> GetCommentById(int id)
        {
            return await QueryWorker.GetCommentById(id);
        }

        [Route("api/lesson/{lessonId}/ratings")]
        [HttpGet]
        public async Task<List<LessonRating>> GetRatingsByLessonId(int lessonId)
        {
            return await QueryWorker.GetRatingsByLessonId(lessonId);
        }

        [Route("api/rating/{id}", Name = "GetRatingById")]
        [HttpGet, HttpPut]
        public async Task<LessonRating> GetRatingById(int id)
        {
            return await QueryWorker.GetRatingById(id);
        }

        #endregion

        //[NonAction]
        //private Func<Lesson, Object> getLessonField(string fieldName)
        //{
        //    Func<Lesson, Object> orderByFunc = null;
        //    switch (fieldName)
        //    {
        //        case "Title":
        //            orderByFunc = sl => sl.Title;
        //            break;
        //        case "PublishDate":
        //            orderByFunc = sl => sl.PublishDate;
        //            break;
        //        case "Rate":
        //            orderByFunc = sl => sl.Rate;
        //            break;
        //        // so on
        //        default:
        //            orderByFunc = sl => sl.PublishDate;
        //            break;
        //    }
        //    return orderByFunc;
        //}

        //[HttpGet]
        //public async Task<List<string>> FindDiscipline(string disciplineQ)
        //{
        //    IQueryable<string> disciplines = db.Lessons
        //                                        .Where(l => l.Discipline.Contains(disciplineQ))
        //                                        .Select(l => l.Discipline).Distinct();

        //    return await disciplines.ToListAsync();
        //}

        //[HttpGet]
        //public async Task<List<string>> FindSchool(string schoolQ)
        //{
        //    IQueryable<string> schools = db.Lessons
        //                                        .Where(l => l.School.Contains(schoolQ))
        //                                        .Select(l => l.School).Distinct();

        //    return await schools.ToListAsync();
        //}

        //[HttpGet]
        //public async Task<List<string>> FindClassRoom(string classRoomQ)
        //{
        //    IQueryable<string> classRooms = db.Lessons
        //                                        .Where(l => l.Classroom.Contains(classRoomQ))
        //                                        .Select(l => l.Classroom).Distinct();

        //    return await classRooms.ToListAsync();
        //}

        //[HttpGet]
        //public async Task<List<string>> FindTags(string tagQ)
        //{
        //    IQueryable<string> tags = db.LessonTags
        //                                        .Where(l => l.LessonTagName.Contains(tagQ))
        //                                        .Select(l => l.LessonTagName).Distinct();

        //    return await tags.ToListAsync();
        //}

        //[NonAction]
        //private List<string> GetQueryArrayParameters(string arrayValuesQueryString)
        //{
        //    var retval = new List<string>();
        //    foreach (var item in arrayValuesQueryString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //    {
        //        retval.Add(item);
        //    }
        //    return retval;
        //}

        //[NonAction]
        //private IQueryable<Lesson> SearchedLesson(
        //    string keyword=null, 
        //    bool inContent=false, 
        //    string discipline=null, 
        //    string school=null, 
        //    string classroom=null, 
        //    int rate=-1, 
        //    string tags=null,
        //    string publishedOn=null, 
        //    string publishedBy=null
        //    )
        //{
        //    IQueryable<Lesson> lessons = db.Lessons;

        //    if (!String.IsNullOrEmpty(keyword))
        //    {
        //        if (inContent)
        //            lessons = lessons.Where(l => l.Content.Contains(keyword) || l.Conclusion.Contains(keyword));
        //        else
        //            lessons = lessons.Where(l => l.Title.Contains(keyword));

        //    }
        //    if (!String.IsNullOrEmpty(discipline))
        //        lessons = lessons.Where(l => l.Discipline.Equals(discipline));
        //    if (!String.IsNullOrEmpty(school))
        //        lessons = lessons.Where(l => l.School.Equals(school));
        //    if (!String.IsNullOrEmpty(classroom))
        //        lessons = lessons.Where(l => l.Classroom.Equals(classroom));
        //    if (rate > -1)
        //        lessons = lessons.Where(l => l.Rate.Equals(rate));
        //    if (!String.IsNullOrEmpty(tags))
        //    {
        //        foreach (string tag in GetQueryArrayParameters(tags))
        //        {
        //            lessons = lessons.Where(l => l.Tags.Any(t => t.LessonTagName.Equals(tag)));
        //        }
        //    }
        //    if (!String.IsNullOrEmpty(publishedOn))
        //        lessons = lessons.Where(l => l.PublishDate.Equals(DateTime.Parse(publishedOn)));
        //    if (!String.IsNullOrEmpty(publishedBy))
        //        lessons = lessons.Where(l => l.Author.UserName.Equals(publishedBy));

        //    // Only published lessons are returned or private lessons (not published for user)
        //    lessons = lessons.Where(l =>
        //        l.Published.Equals(Constants.LESSON_PUBLISHED) ||
        //        (l.Published.Equals(Constants.LESSON_NOT_PUBLISHED) && l.Author.UserName.Equals(publishedBy))
        //        );
        //    //lessons = lessons.Where(l => l.Published.Equals(Constants.LESSON_PUBLISHED));
        //    // Only active lessons are returned
        //    lessons = lessons.Where(l => l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));

        //    return lessons;
        //}

        //[HttpGet]
        //public List<KeyValuePair<int, string>> LastLessonsId(int lastNum)
        //{
        //    List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();
        //    db.Database.Log = s => Debug.WriteLine(s);
        //    IQueryable<Lesson> lessons = db.Lessons;
        //    lessons = lessons.Where(
        //        l => l.Published.Equals(Constants.LESSON_PUBLISHED) &&
        //        l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
        //    lessons = lessons.OrderBy(Constants.LESSON_SEARCH_ORDER_FIELD + " " + Constants.LESSON_SEARCH_ORDER_DIR).Take(lastNum);
        //    var results = lessons
        //        .ToList()
        //        .Select(l => new KeyValuePair<int, string>(l.LessonId,l.Title));

        //    return results.ToList<KeyValuePair<int, string>>();
        //}

        //[HttpGet]
        //public async Task<PagedList<Lesson>> Search(
        //    string keyword=null, 
        //    bool inContent=false, 
        //    string discipline=null, 
        //    string school=null, 
        //    string classroom=null, 
        //    int rate=-1, 
        //    string tags=null,
        //    string publishedOn=null, 
        //    string publishedBy=null,
        //    int startRow=0,
        //    int pageSize=99999,
        //    string orderBy = Constants.LESSON_SEARCH_ORDER_FIELD,
        //    string orderDir = Constants.LESSON_SEARCH_ORDER_DIR)
        //{
        //    PagedList<Lesson> page = new PagedList<Lesson>();

        //    IQueryable<Lesson> lessons = SearchedLesson(
        //        keyword,
        //        inContent,
        //        discipline,
        //        school,
        //        classroom,
        //        rate,
        //        tags,
        //        publishedOn,
        //        publishedBy
        //    );

        //    page.Count = lessons.Count();
        //    page.StartRow = startRow;
        //    page.PageSize = pageSize;

        //    lessons = lessons.OrderBy(orderBy + " " +orderDir).Skip(startRow).Take(pageSize);
        //    page.Records = await lessons.ToListAsync<Lesson>();

        //    return page;
        //}

        // GET api/Lesson/5
        
        //[ResponseType(typeof(Lesson))]
        //public async Task<IHttpActionResult> GetLesson(int id)
        //{
        //    // Only lesson not (logically) deleted are returned
        //    Lesson lesson = await db.Lessons.FirstAsync(l => l.LessonId.Equals(id) && l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
        //    if (lesson == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(lesson);
        //}

        // PUT api/Lesson/5
        //[Route("api/lesson/{lessonId}/comments")]
        //[HttpGet]
        //public async Task<List<LessonComment>> GetCommentsByLessonId(int lessonId)
        //{
        //    IQueryable<LessonComment> comments = db.LessonComments
        //                                .Where(c => c.LessonId.Equals(lessonId) && c.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
        //    return await comments.ToListAsync();
        //}

        // POST api/lesson/13/comment
        //[Route("api/lesson/{lessonId}/ratings")]
        //[HttpGet]
        //public async Task<List<LessonRating>> GetRatingsByLessonId(int lessonId)
        //{
        //    IQueryable<LessonRating> ratings = db.LessonRatings
        //                                .Where(r => r.LessonId.Equals(lessonId) && r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
        //    return await ratings.ToListAsync();
        //}

        // POST api/lesson/5/rating


        //[Authorize]
        //[Route("api/lesson/{id}")]
        //[HttpPut]
        //[ResponseType(typeof(Lesson))]
        //public async Task<IHttpActionResult> PutLesson(int id, Lesson lesson)
        //{
        //    // Search for lesson by same version
        //    IQueryable<Lesson> _lessons = db.Lessons
        //                                .Where(l => l.LessonId.Equals(lesson.LessonId) &&
        //                                    l.Vers.Equals(lesson.Vers) &&
        //                                    l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
        //    Lesson _lesson = await _lessons.FirstAsync();
        //    if (_lesson == null)
        //    {
        //        return StatusCode(HttpStatusCode.Conflict);
        //    }

            
        //    if (_lesson.Title != lesson.Title)
        //        _lesson.Title = lesson.Title;
        //    if (_lesson.Discipline != lesson.Discipline)
        //        _lesson.Discipline = lesson.Discipline;

        //    _lesson.School = lesson.School;
        //    _lesson.Classroom = lesson.Classroom;
        //    _lesson.Discipline = lesson.Discipline;
        //    _lesson.Content = lesson.Content;
        //    _lesson.Conclusion = lesson.Conclusion;
        //    if (_lesson.PublishDate == null)
        //    {
        //        _lesson.PublishDate = DateTime.Now;
        //    }

        //    if (_lesson.Published != lesson.Published)
        //        _lesson.Published = lesson.Published;
        //    if (_lesson.Published.Equals(Constants.LESSON_NOT_PUBLISHED) && lesson.Published.Equals(Constants.LESSON_PUBLISHED))
        //    {
        //        _lesson.Published = _lesson.Published;
        //        _lesson.PublishDate = DateTime.Now;
        //    }

        //    foreach (LessonFeedback fb in lesson.FeedBacks)
        //    {
        //        LessonFeedback mfb;
        //        if (fb.LessonFeedbackId<=0)
        //        {
        //            mfb = new LessonFeedback();
        //            mfb.LessonId = _lesson.LessonId;
        //            mfb.Feedback = fb.Feedback;
        //            mfb.Nature = fb.Nature;
        //            db.LessonFeedbacks.Add(mfb);
        //        }
        //        else
        //        {
        //            mfb = _lesson.FeedBacks.First(f => f.LessonFeedbackId.Equals(fb.LessonFeedbackId));
        //            if (mfb != null && mfb.Feedback != fb.Feedback)
        //            {
        //                mfb.Feedback = fb.Feedback;
        //                db.Entry(mfb).State = EntityState.Modified;
        //            }
        //        }
        //    }

        //    foreach (LessonTag tag in lesson.Tags)
        //    {
        //        LessonTag t;
        //        if (tag.LessonId > 0 && tag.LessonId.Equals(_lesson.LessonId))
        //        {
        //            if (tag.status.Equals("C"))
        //            {
        //                t = _lesson.Tags.First(_t => _t.LessonId.Equals(_lesson.LessonId) && _t.LessonTagName.Equals(tag.LessonTagName));
        //                _lesson.Tags.Remove(t);
        //            }
        //            else if (tag.status.Equals("A"))
        //            {
        //                t = new LessonTag();
        //                t.LessonId = _lesson.LessonId;
        //                t.LessonTagName = tag.LessonTagName;
        //                db.LessonTags.Add(t);
        //            }
        //        }
        //        else
        //            return BadRequest();//TODO: sistemare eccezioni
        //    }

        //    _lesson.LastModifUser = lesson.LastModifUser;
        //    _lesson.LastModifDate = DateTime.Now;
        //    _lesson.Vers += 1;

        //    db.Entry(_lesson).State = EntityState.Modified;
        //    try
        //    {
        //        db.Database.Log = s => Debug.WriteLine(s);
        //        await db.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }

        //    return Ok(_lesson);
        //}


        //// POST api/Lesson
        //[Authorize]
        //[ResponseType(typeof(Lesson))]
        //public async Task<IHttpActionResult> PostLesson(Lesson lesson)
        //{

        //    if (lesson.PublishDate == null)
        //    {
        //        lesson.PublishDate = DateTime.Now;
        //    }
        //    User _user = await db.Users.FindAsync(lesson.Author.UserId);
        //    lesson.Author = _user;
        //    lesson.UserId = _user.UserId;
        //    lesson.Vers = 1;
        //    lesson.RecordState = Constants.RECORD_STATE_ACTIVE;
        //    lesson.CreationDate = DateTime.Now;
        //    lesson.LastModifDate = DateTime.Now;

        //    db.Lessons.Add(lesson);

        //    try
        //    {
        //        db.Database.Log = s => Debug.WriteLine(s);
        //        await db.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }

        //    return Ok(lesson);
        //}

        // DELETE api/Lesson/5

        #region Not yet used API
        //[ResponseType(typeof(Lesson))]
        //public async Task<IHttpActionResult> DeleteLesson(int id)
        //{
        //    Lesson lesson = await db.Lessons.FindAsync(id);
        //    if (lesson == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Lessons.Remove(lesson);
        //    await db.SaveChangesAsync();

        //    return Ok(lesson);
        //}
        #endregion

        /**** Lesson Comments ****/

        //[Authorize]
        //[Route("api/lesson/{lessonId}/comment")]
        //[HttpPost]
        //[ResponseType(typeof(LessonComment))]
        //public async Task<IHttpActionResult> PostLessonComment(LessonComment comment)
        //{
        //    LessonComment _comment = new LessonComment();
        //    _comment.Content = comment.Content;
        //    _comment.CreationDate = DateTime.Now;
        //    _comment.Date = DateTime.Now;
        //    _comment.LastModifDate = DateTime.Now;
        //    _comment.LessonId = comment.LessonId;
        //    _comment.Level = comment.Level;
        //    _comment.ParentId = comment.ParentId;
        //    _comment.Vers = 1;
        //    _comment.RecordState = Constants.RECORD_STATE_ACTIVE;
        //    _comment.UserId = comment.Author.UserId;
        //    try
        //    {
        //        _comment.Author = await db.Users.FindAsync(comment.Author.UserId);
        //        _comment.LastModifUser = _comment.Author.UserName;
        //    }
        //    catch(Exception e){
        //        return BadRequest(e.Message);
        //    }

        //    db.LessonComments.Add(_comment);
        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }

        //    return Ok(_comment);
        //}

        // PUT api/lesson/3/comment/13

        // PUT api/lesson/3/comment/13/delete
        //[Authorize]
        //[Route("api/lesson/{lessonId}/comment/{id}/delete")]
        //[HttpPut]
        //[ResponseType(typeof(LessonComment))]
        //public async Task<IHttpActionResult> DeleteLessonComment(int id, LessonComment comment)
        //{
        //    // Search for child comments, if exists the comment can't be deleted
        //    IQueryable<LessonComment> comments = db.LessonComments
        //                                .Where(c => (c.ParentId.HasValue && c.ParentId.Value.Equals(id)) && c.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
        //    int count = await comments.CountAsync();
        //    if (count == 0)
        //    {
        //        LessonComment _comment = await db.LessonComments.FindAsync(id);
        //        if (_comment == null)
        //        {
        //            return NotFound();
        //        }
        //        _comment.LastModifDate = DateTime.Now;
        //        _comment.LastModifUser = comment.Author.UserName;
        //        _comment.RecordState = Constants.RECORD_STATE_DELETED;

        //        db.Entry(_comment).State = EntityState.Modified;
        //        try
        //        {
        //            await db.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            return BadRequest(e.Message);
        //        }

        //        return Ok(_comment);

        //    }
        //    else
        //    {
        //        return BadRequest("comment linked by other user's comments");
        //    }
        //}

        /**** Lesson Ratings ****/
        //[Authorize]
        //[Route("api/lesson/{lessonId}/rating")]
        //[HttpPost]
        //[ResponseType(typeof(LessonRating))]
        //public async Task<IHttpActionResult> PostLessonRating(LessonRating rating)
        //{
        //    // Search for Rating submitted by the same Author on the same Lesson
        //    IQueryable<LessonRating> ratings = db.LessonRatings
        //                                .Where(r => r.UserId.Equals(rating.Author.UserId) && 
        //                                    r.LessonId.Equals(rating.LessonId) &&
        //                                    r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));
        //    int count = await ratings.CountAsync();
        //    if (count == 0)
        //    {
        //        LessonRating _rating = new LessonRating();
        //        _rating.LessonId = rating.LessonId;
        //        _rating.UserId = rating.Author.UserId;
        //        _rating.Rating = rating.Rating;
        //        _rating.Content = rating.Content ?? string.Empty;
        //        _rating.CreationDate = DateTime.Now;
        //        _rating.LastModifDate = DateTime.Now;
        //        _rating.Vers = 1;
        //        _rating.RecordState = Constants.RECORD_STATE_ACTIVE;
        //        try
        //        {
        //            _rating.Author = await db.Users.FindAsync(rating.Author.UserId);
        //            _rating.LastModifUser = _rating.Author.UserName;
        //        }
        //        catch (Exception e)
        //        {
        //            return BadRequest(e.Message);
        //        }
        //        // Add new Lesson's Rating
        //        db.LessonRatings.Add(_rating);

        //        // Update Lesson Rating with Average of Ratings on the same Lesson
        //        IQueryable<int> _prevRatings = db.LessonRatings
        //                                    .Where(r => r.LessonId.Equals(rating.LessonId) &&
        //                                        r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
        //                                    .Select(r => r.Rating);
        //        List<int> _RatingsList = await _prevRatings.ToListAsync();
        //        _RatingsList.Add(_rating.Rating);

        //        Lesson _lesson = await db.Lessons.FindAsync(rating.LessonId);
        //        _lesson.Rate = Math.Max((int)Math.Round(_RatingsList.Average()), 1);

        //        //TODO: insert table fields for history and versioning
        //        db.Entry(_lesson).State = EntityState.Modified;
                
        //        try
        //        {
        //            await db.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            return BadRequest(e.Message);
        //        }

        //        return Ok(_rating);
        //    }
        //    else
        //    {
        //        return BadRequest("User (id:" + rating.Author.UserId + ") already has submitted a rating for lesson id:" + rating.LessonId);
        //    }

        //}

        //// PUT api/lesson/3/rating/13
        //[Authorize]
        //[Route("api/lesson/{lessonId}/rating/{id}")]
        //[HttpPut]
        //[ResponseType(typeof(LessonRating))]
        //public async Task<IHttpActionResult> PutLessonRating(int id, LessonRating rating)
        //{
        //    LessonRating _rating = await db.LessonRatings.FindAsync(id);
        //    _rating.Rating = rating.Rating;
        //    _rating.Content = rating.Content ?? string.Empty;
        //    _rating.LastModifDate = DateTime.Now;
        //    _rating.Vers += 1;
        //    db.Entry(_rating).State = EntityState.Modified;

        //    // Update Lesson Rating with Average of Ratings on the same Lesson
        //    IQueryable<int> _prevRatings = db.LessonRatings
        //                                .Where(r => r.LessonId.Equals(rating.LessonId) &&
        //                                    !r.Id.Equals(id) &&
        //                                    r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
        //                                .Select(r => r.Rating);
        //    List<int> _RatingsList = await _prevRatings.ToListAsync();
        //    _RatingsList.Add(_rating.Rating);

        //    Lesson _lesson = await db.Lessons.FindAsync(rating.LessonId);
        //    _lesson.Rate = Math.Max((int)Math.Round(_RatingsList.Average()), 1);

        //    //TODO: insert table fields for history and versioning
        //    db.Entry(_lesson).State = EntityState.Modified;
                
        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }

        //    return Ok(_rating);
        //}

        //// PUT api/lesson/3/rating/13/delete
        //[Authorize]
        //[Route("api/lesson/{lessonId}/rating/{id}/delete")]
        //[HttpPut]
        //[ResponseType(typeof(LessonRating))]
        //public async Task<IHttpActionResult> DeleteLessonRating(int id, LessonRating rating)
        //{
        //    // Get the Rating on database
        //    LessonRating _rating = await db.LessonRatings.FindAsync(id);
        //    if (_rating == null)
        //    {
        //        return NotFound();
        //    }
        //    // Get the Lesson on database
        //    Lesson _lesson = await db.Lessons.FindAsync(rating.LessonId);
        //    if (_lesson == null)
        //    {
        //        return BadRequest("LessonId not valid:" + rating.LessonId);
        //    }
        //    if (_lesson.Author.UserId.Equals(rating.Author.UserId))
        //    {
        //        return BadRequest("Author's Lesson CANNOT delete his rating");
        //    }
        //    // enrich rating data
        //    _rating.LastModifDate = DateTime.Now;
        //    _rating.LastModifUser = rating.Author.UserName;
        //    _rating.RecordState = Constants.RECORD_STATE_DELETED;
        //    db.Entry(_rating).State = EntityState.Modified;

        //    // Update Lesson Rating with Average of Ratings on the same Lesson, but le rating is going to be deleted
        //    IQueryable<int> _prevRatings = db.LessonRatings
        //                                .Where(r => r.LessonId.Equals(rating.LessonId) &&
        //                                    !r.Id.Equals(id) &&
        //                                    r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
        //                                .Select(r => r.Rating);
        //    List<int> _RatingsList = await _prevRatings.ToListAsync();

        //    _lesson.Rate = Math.Max((int)Math.Round(_RatingsList.Average()),1);
        //    //TODO: insert table fields for history and versioning
        //    db.Entry(_lesson).State = EntityState.Modified;
                
        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }

        //    return Ok(_rating);


        //}



        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        //private bool LessonExists(int id)
        //{
        //    return db.Lessons.Count(e => e.LessonId == id) > 0;
        //}
    }
}