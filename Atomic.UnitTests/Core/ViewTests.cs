using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Atomic.Core;

namespace Atomic.UnitTests.Core
{
    [TestClass]
    public class IndexViewTests
    {
        private IValue _sourceValue = new AtomicValue() { Name = "TestValue" };
        private IndexView _view = null;

        [TestInitialize]
        public void InitializeTest()
        {
            _view = new IndexView() { Name = "ViewResult", SourceValue = _sourceValue };
        }

        [TestMethod]
        public void ViewIndexNullValue()
        {
            _sourceValue.Value = null;
            Assert.AreEqual(_view.Value, 0);
        }

        [TestMethod]
        public void ViewIndexIntegerValue()
        {
            _sourceValue.Value = -6;
            Assert.AreEqual(_view.Value, -6);
        }

        [TestMethod]
        public void ViewIndexNumberValue()
        {
            _sourceValue.Value = 3.15926;
            Assert.AreEqual(_view.Value, 3);
        }
    }
}
