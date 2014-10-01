using Discitur.Infrastructure.Api;
using Discitur.QueryStack.Model;
using Discitur.QueryStack.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discitur.QueryStack.Worker
{
    public interface ILessonQueryWorker : IQueryWorker
    {
        Task<List<string>> FindClassRoom(string classRoomQ);
        Task<List<string>> FindDiscipline(string disciplineQ);
        Task<List<string>> FindSchool(string schoolQ);
        Task<List<string>> FindTags(string tagQ);
        Task<List<LessonComment>> GetCommentsByLessonId(int lessonId);
        Task<Lesson> GetLesson(int id);
        Task<List<LessonRating>> GetRatingsByLessonId(int lessonId);
        List<KeyValuePair<int, string>> LastLessonsId(int lastNum);
        Task<LessonComment> GetCommentById(int id);
        Task<PagedList<Lesson>> Search(LessonViewModel model);
        Task<LessonRating> GetRatingById(int id);
        Task<IList<Lesson>> GetLessons();
    }
}
