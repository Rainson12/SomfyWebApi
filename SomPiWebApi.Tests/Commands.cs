using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SomPiWebApi.Tests
{
    [TestClass]
    public class Commands
    {
        [TestMethod]
        public void Send()
        {
            var test = Convert.ToInt32("0x267045", 16); 
            //new SomPiWebApi.Helper.Commands().Send("livingRoom", Helper.SendAction.Down);
        }
    }
}
