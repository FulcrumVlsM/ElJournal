using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElJournal.Controllers;
using ElJournal.Models;

namespace ElJournal.Tests.Controllers
{
    [TestClass]
    public class PersonsControllerTest
    {
        [TestMethod]
        public void Get()
        {
            //expected
            PeopleController controller = new PeopleController();

            //actual
            Response result1 = controller.Get().Result as Response;
            Response result2 = controller.Get(Guid.NewGuid().ToString()).Result as Response;
            Response result3 = controller.Get(string.Empty, string.Empty).Result as Response;

            //assert
            Assert.IsNotNull(result1, "метод Get вернул null");
            Assert.AreEqual(true, result1?.Succesful,"метод Get вернул неверный ответ");

            Assert.IsNotNull(result2, "метод Get /id вернул null");
            Assert.AreEqual(true, result2?.Succesful, "метод Get /id вернул неверный ответ");

            Assert.IsNotNull(result3, "метод Get ?id&name вернул null");
            Assert.AreEqual(true, result3?.Succesful, "метод Get ?id&name вернул неверный ответ");
        }

        [TestMethod]
        public void Post()
        {
            //expected
            PeopleController controller = new PeopleController();
            Person person = new Person();

            //actual
            Response result = controller.Post(person).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Post вернул null");
            Assert.AreEqual(false, result?.Succesful,"метод Post вернул неверный ответ");
        }

        [TestMethod]
        public void Put()
        {
            //expected
            PeopleController controller = new PeopleController();
            Person person = new Person();

            //actual
            Response result = controller.Put(Guid.NewGuid().ToString(), person).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Put вернул Null");
            Assert.AreEqual(false, result?.Succesful, "метод Put вернул неверный ответ");
        }

        [TestMethod]
        public void Delete()
        {
            //expected
            PeopleController controller = new PeopleController();
            Person person = new Person();

            //actual
            Response result = controller.Delete(Guid.NewGuid().ToString()).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Delete вернул Null");
            Assert.AreEqual(false, result?.Succesful, "метод Delete вернул неверный ответ");
        }
    }
}
