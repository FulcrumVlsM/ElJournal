using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElJournal.Controllers;
using ElJournal.Models;

namespace ElJournal.Tests.Controllers
{
    [TestClass]
    public class GroupsControllerTest
    {
        [TestMethod]
        public void Get()
        {
            //expected
            GroupsController controller = new GroupsController();
            Group group = new Group();

            //actual
            Response result1 = controller.Get().Result as Response;
            Response result2 = controller.Get(Guid.NewGuid().ToString()).Result as Response;

            //assert
            Assert.IsNotNull(result1, "метод Get вернул Null");
            Assert.AreEqual(true, result1?.Succesful, "метод Get вернул неверный ответ");

            Assert.IsNotNull(result2, "метод Get /id вернул null");
            Assert.AreEqual(true, result2?.Succesful,"метод Get /id вернул неверный ответ");
        }

        [TestMethod]
        public void Post()
        {
            //expected
            GroupsController controller = new GroupsController();
            Group group = new Group();

            //actual
            Response result = controller.Post(group).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Post вернул null");
            Assert.AreEqual(false, result?.Succesful, "метод Post вернул неверный ответ");
        }

        [TestMethod]
        public void Put()
        {
            //expected
            GroupsController controller = new GroupsController();
            Group group = new Group();

            //actual
            Response result = controller.Put(Guid.NewGuid().ToString(), group).Result as Response;

            //assert
            Assert.IsNotNull(result,"метод Put вернул null");
            Assert.AreEqual(false, result?.Succesful, "метод Put вернул неверный ответ");
        }

        [TestMethod]
        public void Delete()
        {
            //expected
            GroupsController controller = new GroupsController();
            Group group = new Group();

            //actual
            Response result = controller.Delete(Guid.NewGuid().ToString()).Result as Response;

            //assert
            Assert.IsNotNull(result, "метод Delete вернул null");
            Assert.AreEqual(false, result?.Succesful, "метод Delete вернул неверный ответ");
        }
    }
}
