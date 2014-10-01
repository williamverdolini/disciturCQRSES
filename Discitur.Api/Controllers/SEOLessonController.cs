using Discitur.Infrastructure;
using Discitur.QueryStack.Model;
using Discitur.QueryStack.Worker;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Discitur.Api.Controllers
{
    public class SEOLessonController : Controller
    {
        private readonly ILessonQueryWorker QueryWorker;

        public SEOLessonController(ILessonQueryWorker queryWorker)
        {
            Contract.Requires<System.ArgumentNullException>(queryWorker != null, "queryWorker");
            QueryWorker = queryWorker;
        }

        public string Index()
        {
            return "This is my <b>default</b> action...";
        }

        public ActionResult List()
        {
            return View(QueryWorker.GetLessons());
        }

        public async Task<ActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Only lesson not (logically) deleted are returned
            Lesson lesson = await QueryWorker.GetLesson(id.Value);
            if (lesson == null)
            {
                return HttpNotFound();
            }
            return View(lesson);
        }

	}
}