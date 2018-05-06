using System;
using System.Collections.Generic;
using Revo.Core.Globalization;
using Revo.Platforms.AspNet.Web;
using Xunit;

namespace Revo.Platforms.AspNet.Tests.Web
{
    class TestTranslatable : ITranslatable
    {
        public string Code { get; }
        public string Name { get; }
        public string Culture { get; set; }
    }

    class TestClassWithTranslatable
    {
        public TestTranslatable TranslatableProperty { get; set; }
    }

    class TestClassWithCollectionOfTranslatables
    {
        public List<TestTranslatable> CollectionOfTranslatables { get; set; }
    }

    class TestClassWithCollectionOfNestedTranslatables
    {
        public List<TestClassWithTranslatable> CollectionOfTestClassWithTranslatable { get; set; }
    }

    class TestClassWithNestedTranslatable
    {
        public TestClassWithTranslatable NestedProperty { get; set; }
        public TestTranslatable TranslatableProperty { get; set; }
        public TestClassWithTranslatable AnotherNestedProperty { get; set; }
        public List<TestTranslatable> CollectionOfTranslatables { get; set; }
        public List<TestClassWithTranslatable> CollectionOfTestClassWithTranslatable { get; set; }
    }

    class TestClassWithTypeField
    {
        public Type TypeField { get; set; }
    }

    public class TestActionFilterAttributeTests
    {
        private readonly TranslateAttribute instance;
        public TestActionFilterAttributeTests()
        {
            instance = new TranslateAttribute();
        }

        [Fact]
        public void TestPathsGetter()
        {
            //1
            var paths = instance.GetPathsToTranslatables(typeof(TestClassWithTranslatable), new Stack<Type>());
            Assert.Equal(1, paths.Count);
        }

        [Fact]
        public void TestPathsGetter2()
        {
            //5
            var paths = instance.GetPathsToTranslatables(typeof(TestClassWithNestedTranslatable), new Stack<Type>());
            Assert.Equal(5, paths.Count);
        }

        [Fact]
        public void TestPathsGetter3()
        {
            //1
            var paths = instance.GetPathsToTranslatables(typeof(TestClassWithCollectionOfTranslatables), new Stack<Type>());
            Assert.Equal(1, paths.Count);
        }

        [Fact]
        public void TestPathsGetter4()
        {
            //1
            var paths = instance.GetPathsToTranslatables(typeof(TestClassWithCollectionOfNestedTranslatables), new Stack<Type>());
            Assert.Equal(1, paths.Count);
        }

        [Fact]
        public void TestWithTypeField()
        {
            var paths = instance.GetPathsToTranslatables(typeof(TestClassWithTypeField), new Stack<Type>());
            Assert.Equal(0, paths.Count);
        }
    }
}
