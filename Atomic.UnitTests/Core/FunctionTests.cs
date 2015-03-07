using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Atomic.Core;
using System.Reflection;

namespace Atomic.UnitTests.Core
{
    [TestClass]
    public class FunctionTests
    {
        private IFunction _function = null;
        public delegate void MethodDef(int num);

        [TestInitialize]
        public void InitializeTest()
        {
            MethodDef def = PublicStaticMethod;
            _function = new AtomicFunction(def.GetMethodInfo());
        }

        [TestCleanup]
        public void CleanupTest()
        {
            _function = null;
        }

        [TestMethod]
        public void FunctionInitialStateTest()
        {
            _function = new AtomicFunction();
            Assert.AreEqual(_function.AsmName, "");
            Assert.AreEqual(_function.Method, null);
            Assert.AreEqual(_function.MethodName, "");
            Assert.AreEqual(_function.ModuleName, "");
            Assert.AreEqual(_function.Name, "function" + _function.GetHashCode());
        }
        
        [TestMethod]
        public void FunctionWithMethodInfoTest()
        {
            MethodDef def = PublicStaticMethod;
            MethodInfo meth = def.GetMethodInfo();
            Assert.IsNotNull(meth);

            _function = new AtomicFunction(meth);
            Assert.AreEqual(_function.AsmName, meth.Module.Assembly.FullName);
            Assert.AreEqual(_function.Method, meth);
            Assert.AreEqual(_function.MethodName, meth.Name);
            Assert.AreEqual(_function.ModuleName, meth.DeclaringType.Name);

            // non public method test
            def = NonPublicStaticMethod;
            meth = def.GetMethodInfo();
            Assert.IsNotNull(meth);

            _function = new AtomicFunction(meth);
            Assert.AreEqual(_function.AsmName, "");
            Assert.AreEqual(_function.Method, null);
            Assert.AreEqual(_function.MethodName, "");
            Assert.AreEqual(_function.ModuleName, "");

            // non static method
            def = PublicNonStaticMethod;
            meth = def.GetMethodInfo();
            Assert.IsNotNull(meth);

            _function = new AtomicFunction(meth);
            Assert.AreEqual(_function.AsmName, "");
            Assert.AreEqual(_function.Method, null);
            Assert.AreEqual(_function.MethodName, "");
            Assert.AreEqual(_function.ModuleName, "");

            // internal non-static test
            def = NonPublicNonStaticMethod;
            meth = def.GetMethodInfo();
            Assert.IsNotNull(meth);

            _function = new AtomicFunction(meth);
            Assert.AreEqual(_function.AsmName, "");
            Assert.AreEqual(_function.Method, null);
            Assert.AreEqual(_function.MethodName, "");
            Assert.AreEqual(_function.ModuleName, "");

            // null test
            _function = new AtomicFunction(null);
            Assert.AreEqual(_function.AsmName, "");
            Assert.AreEqual(_function.Method, null);
            Assert.AreEqual(_function.MethodName, "");
            Assert.AreEqual(_function.ModuleName, "");
        }

        [TestMethod]
        public void FunctionSetPropertiesTest()
        {
            // get valid properties
            string asmName = _function.AsmName;
            string moduleName = _function.ModuleName;
            string methodName = _function.MethodName;

            // clear the existing definition
            _function = new AtomicFunction();
            Assert.AreEqual(_function.Method, null);

            // get a comparison method
            MethodDef def = PublicStaticMethod;
            MethodInfo meth = def.GetMethodInfo();

            _function.SetProperties(asmName, moduleName, methodName);
            Assert.AreEqual(_function.Method, meth);

            // access try: public non static
            _function.SetProperties(asmName, moduleName, "NonStaticPublicMethod");
            Assert.AreEqual(_function.Method, null);

            // access try: non public static
            _function.SetProperties(asmName, moduleName, "StaticNonPublicMethod");
            Assert.AreEqual(_function.Method, null);

            // access try: non public non static
            _function.SetProperties(asmName, moduleName, "NonStaticNonPublicMethod");
            Assert.AreEqual(_function.Method, null);

            // access try: bad method name
            _function.SetProperties(asmName, moduleName, "Huey");
            Assert.AreEqual(_function.Method, null);

            // access try: bad method name
            _function.SetProperties(asmName, moduleName, null);
            Assert.AreEqual(_function.Method, null);

            // access try: bad module name
            _function.SetProperties(asmName, "George", methodName);
            Assert.AreEqual(_function.Method, null);

            // access try: null module
            _function.SetProperties(asmName, null, methodName);
            Assert.AreEqual(_function.Method, null);

            // access try: by DLL name
            _function.SetProperties(meth.Module.FullyQualifiedName, moduleName, methodName);
            Assert.AreEqual(_function.Method, null);

            // access try: by assembly name only
            _function.SetProperties("Atomic.UnitTests.Core", moduleName, methodName);

            // access try: bad assembly name only
            _function.SetProperties("SuperSecret", moduleName, methodName);

            // access try: null assembly name only
            _function.SetProperties(null, moduleName, methodName);
        }

        [TestMethod]
        public void FunctionEqualsTest()
        {
            // get a comparison method
            MethodDef def = PublicStaticMethod;
            MethodInfo meth = def.GetMethodInfo();

            IFunction func = new AtomicFunction(meth);
            Assert.IsTrue(_function.Equals(func));

            def = PublicStaticMethod2;
            meth = def.GetMethodInfo();

            func = new AtomicFunction(meth);
            Assert.IsFalse(_function.Equals(func));

            Assert.IsFalse(_function.Equals(new AtomicFunction()));

            _function = new AtomicFunction();
            Assert.IsTrue(_function.Equals(new AtomicFunction()));
            Assert.IsTrue(_function.Equals(Undefined.Function));

            Assert.IsFalse(_function.Equals(12));
            Assert.IsFalse(_function.Equals(null));
        }

        static public void PublicStaticMethod(int num) { }

        static public void PublicStaticMethod2(int num) { }

        static void NonPublicStaticMethod(int num) { }

        public void PublicNonStaticMethod(int num) { }

        void NonPublicNonStaticMethod(int num) { }
    }
}
