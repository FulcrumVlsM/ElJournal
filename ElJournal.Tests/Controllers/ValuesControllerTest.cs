using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElJournal;
using ElJournal.Controllers;

namespace ElJournal.Tests.Controllers
{
    [TestClass]
    public class ValuesControllerTest
    {
        [TestMethod]
        public void Get()
        {
            // Упорядочение
            ValuesController controller = new ValuesController();

            // Действие
            IEnumerable<string> result = controller.Get();

            // Утверждение
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("value1", result.ElementAt(0));
            Assert.AreEqual("value2", result.ElementAt(1));
        }

        [TestMethod]
        public void GetById()
        {
            // Упорядочение
            ValuesController controller = new ValuesController();

            // Действие
            string result = controller.Get(5);

            // Утверждение
            Assert.AreEqual("value", result);
        }

        [TestMethod]
        public void Post()
        {
            // Упорядочение
            ValuesController controller = new ValuesController();

            // Действие
            controller.Post("value");

            // Утверждение
        }

        [TestMethod]
        public void Put()
        {
            // Упорядочение
            ValuesController controller = new ValuesController();

            // Действие
            controller.Put(5, "value");

            // Утверждение
        }

        [TestMethod]
        public void Delete()
        {
            // Упорядочение
            ValuesController controller = new ValuesController();

            // Действие
            controller.Delete(5);

            // Утверждение
        }
    }
}
