using Discitur.CommandStack.ViewModel;
using Discitur.CommandStack.Worker;
using Discitur.Infrastructure;
//using Discitur.QueryStack.Model;
using Discitur.QueryStack.Worker;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Discitur.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        //private DisciturContext db = new DisciturContext();
        // Worker Services
        private readonly IUserQueryWorker QueryWorker;
        private readonly IUserCommandWorker CommandWorker;

        #region Ctors
        public UserController(IUserQueryWorker queryWorker, IUserCommandWorker commandWorker)
        {
            Contract.Requires<System.ArgumentNullException>(queryWorker != null, "queryWorker");
            Contract.Requires<System.ArgumentNullException>(commandWorker != null, "commandWorker");
            QueryWorker = queryWorker;
            CommandWorker = commandWorker;
        }
        #endregion

        #region Command
        // TODO: This should be refactored in task-based way. No PutUser, but ChangeEmail
        // PUT api/User
        //[ResponseType(typeof(User))]
        //public async Task<IHttpActionResult> PutUser(User user)
        public IHttpActionResult PutUser(ChangeUserEmailViewModel changeEmail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string userName = User.Identity.GetUserName();
            if (userName.Equals(changeEmail.UserName))
            {
                CommandWorker.ChangeUserEmail(changeEmail);
            }
            else
            {
                return BadRequest(ModelState);
            }

            return Ok();

            //ChangeUserEmailViewModel changeEmail = new ChangeUserEmailViewModel()
            //{
            //    UserId = user.UserId,
            //    UserName = user.UserName,
            //    Email = user.Email
            //};            //User cUser = await db.Users.Where(u => u.UserName.Equals(userName)).FirstAsync<Mag14.discitur.Models.User>();

            //if (cUser.UserId != user.UserId)
            //{
            //    return BadRequest();
            //}

            //cUser.Email = user.Email;

            //db.Entry(cUser).State = EntityState.Modified;

            //try
            //{
            //    await db.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!UserExists(user.UserId))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}
            //return Ok(cUser);
        }

        // TODO: This should be refactored in task-based way. No PostUserImage, but ChangePicture
        // POST api/User/Image
        [Route("Image")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostUserImage()
        {
            // Check if the request contains multipart/form-data.
            if (Request.Content.IsMimeMultipartContent())
            {
                try
                {
                    #region Create ChangePicture ViewModel
                    int id = int.Parse(HttpContext.Current.Request.Form["UserId"]);
                    byte[] imageBytes = await GetImageBytesFromRequest();

                    ChangeUserPictureViewModel changeImage = new ChangeUserPictureViewModel()
                    {
                        UserId = id,
                        Picture = imageBytes
                    };
                    #endregion

                    CommandWorker.ChangeUserPicture(changeImage);

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                catch
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            //// Check if the request contains multipart/form-data.
            //if (Request.Content.IsMimeMultipartContent())
            //{
            //    User user;
            //    //var FormData = await Request.Content.ReadAsFormDataAsync();
            //    try
            //    {
            //        int id = int.Parse(HttpContext.Current.Request.Form["UserId"]);
            //        user = db.Users.Find(id);
            //    }
            //    catch {
            //        throw new HttpResponseException(HttpStatusCode.BadRequest);
            //    }
            //    var streamProvider = new MultipartMemoryStreamProvider();
            //    streamProvider = await Request.Content.ReadAsMultipartAsync(streamProvider);
            //    foreach (var item in streamProvider.Contents.Where(c => !string.IsNullOrEmpty(c.Headers.ContentDisposition.FileName)))
            //    {   
            //        Stream stPictureSource = new MemoryStream(await item.ReadAsByteArrayAsync());
            //        Stream stThumbSource = new MemoryStream(await item.ReadAsByteArrayAsync());
            //        // Resize for Picture
            //        MemoryStream stPictureDest = new MemoryStream();
            //        var pictureSettings = new ResizeSettings
            //        {
            //            MaxWidth = Constants.USER_PICTURE_MAXWIDTH,
            //            MaxHeight = Constants.USER_PICTURE_MAXHEIGHT,
            //            Format = Constants.USER_PICTURE_FORMAT,
            //            Mode = FitMode.Crop
            //        };
            //        ImageBuilder.Current.Build(stPictureSource, stPictureDest, pictureSettings);
            //        // Resize for ThumbNail
            //        MemoryStream stThumbDest = new MemoryStream();
            //        var thumbSettings = new ResizeSettings
            //        {
            //            MaxWidth = Constants.USER_THUMB_MAXWIDTH,
            //            MaxHeight = Constants.USER_THUMB_MAXHEIGHT,
            //            Format = Constants.USER_THUMB_FORMAT,
            //            Mode = FitMode.Crop
            //        };
            //        ImageBuilder.Current.Build(stThumbSource, stThumbDest, thumbSettings);

            //        user.Picture = "data:image/gif;base64," + Convert.ToBase64String(stPictureDest.ToArray());
            //        user.Thumb = "data:image/gif;base64," + Convert.ToBase64String(stThumbDest.ToArray());
            //    }
            //    await db.SaveChangesAsync();
            //    return Request.CreateResponse(HttpStatusCode.OK);
            //}
            //else{
            //    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            //}
        }

        #endregion

        #region Query
        // GET api/User/anyEmail
        [AllowAnonymous]
        [ResponseType(typeof(bool))]
        [Route("anyEmail")]
        [HttpGet]
        public async Task<IHttpActionResult> isAnyEmail(string email)
        {
            try
            {
                return Ok(await QueryWorker.IsAnyUserByEmail(email));
            }
            catch
            {
                return Ok(false);
            }
        }

        #endregion

        #region Helpers
        private async Task<byte[]> GetImageBytesFromRequest()
        {
            var streamProvider = new MultipartMemoryStreamProvider();
            streamProvider = await Request.Content.ReadAsMultipartAsync(streamProvider);

            var image = streamProvider.Contents.Where(c => !string.IsNullOrEmpty(c.Headers.ContentDisposition.FileName)).First();
            return await image.ReadAsByteArrayAsync();
        }
        #endregion

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}



        //// GET api/User
        //public IQueryable<User> GetUsers()
        //{
        //    return db.Users;
        //}

        //// GET api/User/5
        //[ResponseType(typeof(User))]
        //public async Task<IHttpActionResult> GetUser(int id)
        //{
        //    User user = await db.Users.FindAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(user);
        //}

        //// POST api/User
        //[ResponseType(typeof(User))]
        //public async Task<IHttpActionResult> PostUser(User user)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Users.Add(user);
        //    await db.SaveChangesAsync();

        //    return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        //}

        //// DELETE api/User/5
        //[ResponseType(typeof(User))]
        //public async Task<IHttpActionResult> DeleteUser(int id)
        //{
        //    User user = await db.Users.FindAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Users.Remove(user);
        //    await db.SaveChangesAsync();

        //    return Ok(user);
        //}

        //private IHttpActionResult BadDisciturRequest(string errorCode)
        //{
        //    AddModelError(errorCode);
        //    return BadRequest(ModelState);
        //}

        //private void AddModelError(string errorCode)
        //{
        //    ModelState.AddModelError(Constants.DISCITUR_ERRORS, errorCode);
        //}

        //private bool UserExists(int id)
        //{
        //    return db.Users.Count(e => e.UserId == id) > 0;
        //}
    }
}