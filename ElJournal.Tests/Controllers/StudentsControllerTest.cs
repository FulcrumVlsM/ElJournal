using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElJournal.Controllers;
using ElJournal.Models;

namespace ElJournal.Tests.Controllers
{
    [TestClass]
    public class StudentsControllerTest
    {
        [TestMethod]
        public void Get()
        {
            //expcted
            StudentsController controller = new StudentsController();
            Student student = new Student();

            //actual
            Response result1 = controller.Get().Result as Response;
            Response result2 = controller.Get(Guid.NewGuid().ToString()).Result as Response;

            //assert
            Assert.IsNotNull(result1,"метод Get вернул Null");
            Assert.AreEqual(true, result1?.Succesful);

            Assert.IsNotNull(result2, "метод Get /id вернул null");
            Assert.AreEqual(true, result2?.Succesful, "метод Get /id вернул неверный ответ");
        }

        [TestMethod]
        public void Post()
        {
            //expected
            StudentsController controller = new StudentsController();
            Student student = new Student();

            //actual
            Response result = controller.Post(student).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Post вернул null");
            Assert.IsFalse((bool)result?.Succesful, "метод Post вернул неверный ответ");
        }

        [TestMethod]
        public void Put()
        {
            //expected
            StudentsController controller = new StudentsController();
            Student student = new Student();

            //actual
            Response result = controller.Put(Guid.NewGuid().ToString(), student).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Put вернул null");
            Assert.IsFalse((bool)result?.Succesful,"метод Put вернул неверный ответ");
        }

        [TestMethod]
        public void Delete()
        {
            //expected
            StudentsController controller = new StudentsController();

            //actual
            Response result = controller.Delete(Guid.NewGuid().ToString()).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Delete вернул null");
            Assert.IsFalse((bool)result?.Succesful,"метод Delete вернул неверный ответ");
        }
    }
}
