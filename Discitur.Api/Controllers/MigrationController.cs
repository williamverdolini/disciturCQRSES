using Discitur.Api.Common;
using Discitur.Infrastructure;
using Discitur.Legacy.Migration.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Discitur.Api.Controllers
{
    [Authorize(Roles = Constants.DISCITUR_ADMIN_ROLE)]
    public class MigrationController : ApiController
    {
        private readonly LegacyMigrationWorker Worker;

        public MigrationController(LegacyMigrationWorker worker)
        {
            Contract.Requires<System.ArgumentNullException>(worker != null, "worker");
            Worker = worker;
        }

        [Route("api/Migrate")]
        [HttpPost]
        public IHttpActionResult Migrate()
        {
            try
            {
                Worker.ExecuteMigration();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
