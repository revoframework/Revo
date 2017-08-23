using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.Globalization;
using GTRevo.Platform.Web;
using Xunit;

namespace GTRevo.Platform.Tests.Web
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

    public class TestActionFilterAttributeTests
    {
        private TranslateAttribute instance;
        public TestActionFilterAttributeTests()
        {
            instance = new TranslateAttribute();
        }

        [Fact]
        public void TestPathsGetter()
        {
            //var paths = instance.GetPathsToTranslatables(typeof(TestClassWithTranslatable));
            var paths = instance.GetPathsToTranslatables(typeof(TestClassWithNestedTranslatable));
            //var paths = instance.GetPathsToTranslatables(typeof(TestClassWithCollectionOfTranslatables));
            //var paths = instance.GetPathsToTranslatables(typeof(TestClassWithCollectionOfNestedTranslatables));
        }
    }
}
