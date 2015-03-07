using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Atomic.Core;

namespace Atomic.UnitTests.Core
{
    [TestClass]
    public class ElementTests
    {
        private IElement _element = null;

        [TestInitialize]
        public void InitializeTest()
        {
            _element = new AtomicElement();
        }

        [TestCleanup]
        public void CleanupTest()
        {
            _element = null;
        }

        [TestMethod]
        public void ElementInitialStateTest()
        {
            Assert.AreEqual(_element.Name, "element" + _element.GetHashCode());
            Assert.AreEqual(_element.ID, AtomicElement.GenerateID(_element.Name));
            Assert.AreEqual(_element.Locked, false);
        }

        [TestMethod]
        public void ElementNameTest()
        {
            string defaultName = "element" + _element.GetHashCode();

            _element.Name = null;
            Assert.AreEqual(_element.Name, defaultName);

            _element.Name = "";
            Assert.AreEqual(_element.Name, defaultName);

            _element.Name = "   ";
            Assert.AreEqual(_element.Name, defaultName);

            _element.Name = "Greet Person";
            Assert.AreEqual(_element.Name, "Greet Person");

            _element.Name = "  A Space Odyssey ";
            Assert.AreEqual(_element.Name, "A Space Odyssey");

            _element.Name = "\u0212\u0214";
            Assert.AreEqual(_element.Name, "\u0212\u0214");
        }

        [TestMethod]
        public void ElementGenerateIDTest()
        {
            Assert.AreEqual(AtomicElement.GenerateID(null), "");
            Assert.AreEqual(AtomicElement.GenerateID(""), "");
            Assert.AreEqual(AtomicElement.GenerateID("   "), "");
            Assert.AreEqual(AtomicElement.GenerateID("hey"), "hey");
            Assert.AreEqual(AtomicElement.GenerateID("Hi There"), "hi_there");
            Assert.AreEqual(AtomicElement.GenerateID("   A Space Odyssey   "), "a_space_odyssey");
            Assert.AreEqual(AtomicElement.GenerateID("\u0212\u0214"), "\u0212\u0214".ToLower());
        }

        [TestMethod]
        public void ElementLockedTest()
        {
            _element.Name = "Billy Jean";
            Assert.AreEqual(_element.Name, "Billy Jean");
            Assert.AreEqual(_element.ID, "billy_jean");

            _element.Locked = true;
            Assert.AreEqual(_element.Locked, true);

            _element.Name = "Joe";
            Assert.AreEqual(_element.Name, "Billy Jean");
            Assert.AreEqual(_element.ID, "billy_jean");
        }

        [TestMethod]
        public void ElementUpdateTest()
        {
            _element.Name = "Billy Jean";
            Assert.AreEqual(_element.Name, "Billy Jean");
            Assert.AreEqual(_element.ID, "billy_jean");

            _element.Update();
            Assert.AreEqual(_element.Name, "Billy Jean");
            Assert.AreEqual(_element.ID, "billy_jean");
        }
    }
}
