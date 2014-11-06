using Discitur.Api.Common;
using Discitur.CommandStack.Worker;
using Discitur.Infrastructure;
using Discitur.Legacy.Migration.Worker;
using Discitur.QueryStack.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Discitur.Api.Controllers
{
    [Authorize(Roles = Constants.DISCITUR_ADMIN_ROLE)]
    public class AdminController : ApiController
    {
        private readonly LegacyMigrationWorker MigrationWorker;
        private readonly IAdminCommandWorker AdminCommandWorker;
        private readonly IAdminQueryWorker AdminQueryWorker;

        public AdminController(LegacyMigrationWorker migrationWorker, IAdminCommandWorker adminCommandWorker, IAdminQueryWorker adminQueryWorker)
        {
            Contract.Requires<System.ArgumentNullException>(migrationWorker != null, "migrationWorker");
            Contract.Requires<System.ArgumentNullException>(adminCommandWorker != null, "adminCommandWorker");
            Contract.Requires<System.ArgumentNullException>(adminQueryWorker != null, "adminQueryWorker");
            MigrationWorker = migrationWorker;
            AdminCommandWorker = adminCommandWorker;
            AdminQueryWorker = adminQueryWorker;
        }

        [Route("api/Migrate")]
        [HttpPost]
        public IHttpActionResult Migrate()
        {
            try
            {
                IList<string> log = MigrationWorker.ExecuteMigration();
                return Ok(log);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("api/ReplayAllEvents")]
        [HttpPost]
        public IHttpActionResult ReplayAllEvents()
        {
            try
            {
                AdminQueryWorker.ClearReadModel();
                AdminCommandWorker.ReplayAllEvents();
                return Ok(new List<string>(){"Read-Model regenerated successfully."});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[Route("api/ClearReadModel")]
        //[HttpPost]
        //public IHttpActionResult ClearReadModel()
        //{
        //    try
        //    {
        //        AdminQueryWorker.ClearReadModel();
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}
