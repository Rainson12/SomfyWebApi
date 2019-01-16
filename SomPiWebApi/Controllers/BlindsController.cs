using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public IEnumerable<string> GetBlindNames()
        {
            string[] remotes = Directory.GetFiles("remotes");
            var blindNames = new List<string>();
            foreach (var remote in remotes)
            {
                blindNames.Add(Path.GetFileNameWithoutExtension(remote));
            }
            return blindNames;
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
            var fileExists = System.IO.File.Exists("remotes/" + blindName + ".txt");
            if (fileExists)
            {
                try
                {
                    var lines = System.IO.File.ReadAllLines("remotes/" + blindName + ".txt");
                    var remote = Convert.ToInt32(lines[0], 16);
                    var rollCounter = int.Parse(lines[1]);
                    new Commands().Send(remote, rollCounter, _action);
                    System.IO.File.WriteAllLines("remotes/" + blindName + ".txt", new string[] { remote.ToString("X4"), (rollCounter + 1).ToString() });
                    return "done";
                }
                catch(Exception ex)
                {
                    return ex.Message;
                }
            }
            return null;
        }

    }
}