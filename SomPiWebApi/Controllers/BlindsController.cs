using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SomPiWebApi.Helper;

namespace SomPiWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Blinds")]
    public class BlindsController : Controller
    {
        [HttpGet]
        public IEnumerable<RemoteController> GetBlindNames()
        {
            using (var db = new LiteDatabase(@"remotes.db"))
            {
                var remotes = db.GetCollection<RemoteController>("remotes");
                return remotes.FindAll().ToList();
            }
        }

        //https://www.thomaslevesque.com/2018/04/17/hosting-an-asp-net-core-2-application-on-a-raspberry-pi/
        //http://192.168.178.47/api/blinds/livingRoomDoor?action=down
        [HttpPost, Route("{blindName}")]
        public string Post(string blindName, [FromQuery]string action)
        {
            SendAction _action = SendAction.Down;
            if (action == "up")
                _action = SendAction.Up;
            else if (action == "stop")
                _action = SendAction.Stop;
            else if (action == "prog")
                _action = SendAction.Prog;

            RemoteController remote = null;
            using (var db = new LiteDatabase(@"remotes.db"))
            {
                var remotes = db.GetCollection<RemoteController>("remotes");
                remote = remotes.FindOne(x => x.Name == blindName);
                if (remote == null)
                {
                    remote = new RemoteController() { CurrentCounter = 0, Id = remotes.FindAll().ToArray().Count() > 0 ? remotes.FindAll().ToArray().Max(x => x.Id) + 1 : 0, Name = blindName };
                    remotes.Insert(remote);
                }
                try
                {
                    new Commands().Send(remote.Id, remote.CurrentCounter + 1, _action);
                    remote.CurrentCounter++;
                    remotes.Update(remote);
                    return "done";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
        [HttpPost, Route("{blindName}/SetCounter")]
        public ActionResult SetCounter(string blindName, int counter)
        {
           

            RemoteController remote = null;
            using (var db = new LiteDatabase(@"remotes.db"))
            {
                var remotes = db.GetCollection<RemoteController>("remotes");
                remote = remotes.FindOne(x => x.Name == blindName);
                if (remote == null)
                {
                    return NotFound();
                }
                try
                {
                    remote.CurrentCounter = counter;
                    remotes.Update(remote);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return StatusCode(500);
                }
            }
        }

    }
}