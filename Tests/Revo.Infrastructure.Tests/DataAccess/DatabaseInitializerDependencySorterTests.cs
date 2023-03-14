using FluentAssertions;
using Revo.Infrastructure.DataAccess;
using Revo.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Revo.Infrastructure.Tests.DataAccess;

public class DatabaseInitializerDependencySorterTests
{
    private DatabaseInitializerDependencySorter sut = new();

    [Fact]
    public void Sort()
    {
        var initializers = new IDatabaseInitializer[]
        {
            new TestDiA1(),
            new TestDiA(),
        };

        var result = sut.GetSorted(initializers);
        result.Should().BeEquivalentTo(new IDatabaseInitializer[]
        {
            initializers.OfType<TestDiA>().First(),
            initializers.OfType<TestDiA1>().First(),
        });
    }

    [Fact]
    public void Sort_Transitive()
    {
        var initializers = new IDatabaseInitializer[]
        {
            new TestDiA1A(),
            new TestDiA1(),
            new TestDiA(),
        };

        var result = sut.GetSorted(initializers);
        result.Should().BeEquivalentTo(new IDatabaseInitializer[]
        {
            initializers.OfType<TestDiA>().First(),
            initializers.OfType<TestDiA1>().First(),
            initializers.OfType<TestDiA1A>().First()
        });
    }

    [Fact]
    public void Sort_MultipleDependencies()
    {
        var initializers = new IDatabaseInitializer[]
        {
            new TestDiC(),
            new TestDiA1A(),
            new TestDiA1(),
            new TestDiA(),
            new TestDiB(),
            new TestDiB1()
        };

        var result = sut.GetSorted(initializers);
        result.Should().Contain(initializers);

        result.IndexOf(initializers.OfType<TestDiA>().First()).Should()
            .BeLessThan(result.IndexOf(initializers.OfType<TestDiA1>().First()));
        result.IndexOf(initializers.OfType<TestDiA1>().First()).Should()
            .BeLessThan(result.IndexOf(initializers.OfType<TestDiA1A>().First()));
        result.IndexOf(initializers.OfType<TestDiB>().First()).Should()
            .BeLessThan(result.IndexOf(initializers.OfType<TestDiB1>().First()));
        result.IndexOf(initializers.OfType<TestDiA1A>().First()).Should()
            .BeLessThan(result.IndexOf(initializers.OfType<TestDiC>().First()));
        result.IndexOf(initializers.OfType<TestDiB1>().First()).Should()
            .BeLessThan(result.IndexOf(initializers.OfType<TestDiC>().First()));
    }

    [Fact]
    public void Sort_ThrowsOnCyclicDependency()
    {
        var initializers = new IDatabaseInitializer[]
        {
            new TestDiD(),
            new TestDiE()
        };

        sut.Invoking(x => x.GetSorted(initializers))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Sort_ThrowsOnMissingDependency()
    {
        var initializers = new IDatabaseInitializer[]
        {
            new TestDiD()
        };

        sut.Invoking(x => x.GetSorted(initializers))
            .Should().Throw<InvalidOperationException>();
    }
    
    public class TestDiA : IDatabaseInitializer
    {
        public Task InitializeAsync(IRepository repository)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestDiB : IDatabaseInitializer
    {
        public Task InitializeAsync(IRepository repository)
        {
            throw new System.NotImplementedException();
        }
    }

    [InitializeAfter(typeof(TestDiA))]
    public class TestDiA1 : IDatabaseInitializer
    {
        public Task InitializeAsync(IRepository repository)
        {
            throw new System.NotImplementedException();
        }
    }

    [InitializeAfter(typeof(TestDiB))]
    public class TestDiB1 : IDatabaseInitializer
    {
        public Task InitializeAsync(IRepository repository)
        {
            throw new System.NotImplementedException();
        }
    }

    [InitializeAfter(typeof(TestDiA1))]
    public class TestDiA1A : IDatabaseInitializer
    {
        public Task InitializeAsync(IRepository repository)
        {
            throw new System.NotImplementedException();
        }
    }

    [InitializeAfter(typeof(TestDiA1A))]
    [InitializeAfter(typeof(TestDiB1))]
    public class TestDiC : IDatabaseInitializer
    {
        public Task InitializeAsync(IRepository repository)
        {
            throw new System.NotImplementedException();
        }
    }

    [InitializeAfter(typeof(TestDiE))]
    public class TestDiD : IDatabaseInitializer
    {
        public Task InitializeAsync(IRepository repository)
        {
            throw new System.NotImplementedException();
        }
    }

    [InitializeAfter(typeof(TestDiD))]
    public class TestDiE : IDatabaseInitializer
    {
        public Task InitializeAsync(IRepository repository)
        {
            throw new System.NotImplementedException();
        }
    }
}