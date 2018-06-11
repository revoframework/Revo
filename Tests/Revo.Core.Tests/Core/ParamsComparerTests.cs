using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Core;
using Xunit;

namespace Revo.Core.Tests.Core
{
    public class ParamsComparerTests
    {
        [Fact]
        public void Test_Same_Odd()
        {
            Assert.Throws<ArgumentException>(() => ParamsComparer.Same("1", null, new object()));
        }

        [Fact]
        public void Test_Same_1()
        {
            var o = new object();
            var result = ParamsComparer.Same("first",o,"first",o);
            Assert.True(result);
        }

        [Fact]
        public void Test_Same_2()
        {
            var o = new object();
            var result = ParamsComparer.Same("first", o, "second", o);
            Assert.False(result);
        }

        [Fact]
        public void Test_Same_3()
        {
            var o = new object();
            var result = ParamsComparer.Same("first", o, null, o);
            Assert.False(result);
        }

        [Fact]
        public void Test_Same_4()
        {
            var o = new object();
            var result = ParamsComparer.Same(null, o, null, o);
            Assert.True(result);
        }

        [Fact]
        public void Test_SamePairs_Odd()
        {
            Assert.Throws<ArgumentException>(() => ParamsComparer.SamePairs("1", null, new object()));
        }


        [Fact]
        public void Test_SamePairs_1()
        {
            var o = new object();
            var result = ParamsComparer.SamePairs("first", "first", o, o);
            Assert.True(result);
        }

        [Fact]
        public void Test_SamePairs_2()
        {
            var o = new object();
            var result = ParamsComparer.SamePairs("first", "second", o, o);
            Assert.False(result);
        }

        [Fact]
        public void Test_SamePairs_3()
        {
            var o = new object();
            var result = ParamsComparer.SamePairs("first", null, o , o);
            Assert.False(result);
        }

        [Fact]
        public void Test_SamePairs_4()
        {
            var o = new object();
            var result = ParamsComparer.SamePairs(null, null, o, o);
            Assert.True(result);
        }

        [Fact]
        public void Test_NotSame()
        {
            DateTimeOffset? validFrom1 = new DateTimeOffset(new DateTime(2018,6,1,0,0,0));
            DateTimeOffset? validFrom2 = new DateTimeOffset(new DateTime(2018, 6, 30, 0, 0, 0));
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var result = ParamsComparer.Same("Name", "Description", validFrom1, null, guid1, guid2,
                "Name", "Description", validFrom2, null, guid1, guid2);
            Assert.False(result);
        }

    }
}
