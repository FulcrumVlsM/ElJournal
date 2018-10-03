using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElJournal.Controllers;
using ElJournal.Models;

namespace ElJournal.Tests.Controllers
{
    [TestClass]
    public class FacultiesControllerTest
    {
        [TestMethod]
        public void Get()
        {
            //expected

            //actual
            FacultiesController controller = new FacultiesController();
            Response result1 = controller.Get().Result as Response;
            Response result2 = controller.Get(Guid.NewGuid().ToString()).Result as Response;

            //assert
            Assert.IsNotNull(result1, "метод Get() вернул null");
            Assert.AreEqual(true, result1?.Succesful,"Метод Get() вернул Succesful=false");

            Assert.IsNotNull(result2, "метод Get /id вернул null");
            Assert.AreEqual(true, result2?.Succesful, "Метод Get(id) вернул Succesful=false");
        }

        [TestMethod]
        public void Post()
        {
            //expected
            FacultiesController controller = new FacultiesController();
            Faculty faculty = new Faculty();

            //actual
            Response result = controller.Post(faculty).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Post() вернул null");
            Assert.AreEqual(false, result?.Succesful);
        }

        [TestMethod]
        public void Put()
        {
            //expected
            FacultiesController controller = new FacultiesController();
            Faculty faculty = new Faculty();

            //actual
            Response result = controller.Put(String.Empty,faculty).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Put вернул null");
            Assert.AreEqual(false, result?.Succesful);
        }

        [TestMethod]
        public void Delete()
        {
            //expected
            FacultiesController controller = new FacultiesController();
            Faculty faculty = new Faculty();

            //actual
            Response result = controller.Put(string.Empty, faculty).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Delete вернул null");
            Assert.AreEqual(false, result?.Succesful);
        }
    }
}
