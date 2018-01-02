using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElJournal.Controllers;
using ElJournal.Models;

namespace ElJournal.Tests.Controllers
{
    [TestClass]
    public class DepartmentsControllerTest
    {
        [TestMethod]
        public void Get()
        {
            //expected
            DepartmentsController controller = new DepartmentsController();
            Department department = new Department();

            //actual
            Response result1 = controller.Get().Result as Response;
            Response result2 = controller.Get(Guid.NewGuid().ToString()).Result as Response;

            //assert
            Assert.IsNotNull(result1, "метод Get вернул null");
            Assert.AreEqual(true, result1?.Succesful);

            Assert.IsNotNull(result2, "метод Get /id вернул null");
            Assert.AreEqual(true, result2?.Succesful);
        }

        [TestMethod]
        public void Post()
        {
            //expected
            DepartmentsController controller = new DepartmentsController();
            Department department = new Department();

            //actual
            Response result = controller.Post(department).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Post вернул null");
            Assert.AreEqual(false, result?.Succesful, "метод Post вернул неверный ответ");
        }

        [TestMethod]
        public void Put()
        {
            //expected
            DepartmentsController controller = new DepartmentsController();
            Department department = new Department();

            //actual
            Response result = controller.Put(Guid.NewGuid().ToString(), department).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Put вернул null");
            Assert.AreEqual(false, result?.Succesful, "метод Put вернул неверный ответ");
        }

        [TestMethod]
        public void Delete()
        {
            //expected
            DepartmentsController controller = new DepartmentsController();
            Department department = new Department();

            //actual
            Response response = controller.Delete(Guid.NewGuid().ToString(), department).Result as Response;

            //assert
            Assert.IsNotNull(response, "Метод Delete вернул null");
            Assert.AreEqual(false, response?.Succesful);
        }
    }
}
