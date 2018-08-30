using System;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Revo.Extensions.History.ChangeTracking;
using Revo.Extensions.History.ChangeTracking.Model;
using Xunit;

namespace Revo.Extensions.History.Tests.ChangeTracking
{
    public class TrackedChangeRecordConverterTests
    {
        private readonly TrackedChangeRecordConverter sut;
        private readonly IChangeDataTypeCache changeDataTypeCache;

        public TrackedChangeRecordConverterTests()
        {
            changeDataTypeCache = Substitute.For<IChangeDataTypeCache>();
            changeDataTypeCache.GetChangeDataTypeName(typeof(TestChangeData)).Returns("TestChangeData");
            changeDataTypeCache.GetClrChangeDataType("TestChangeData").Returns(typeof(TestChangeData));

            sut = new TrackedChangeRecordConverter(changeDataTypeCache);
        }

        [Fact]
        public void FromRecord_Result()
        {
            TrackedChangeRecord record = new TrackedChangeRecord()
            {
                Id = Guid.NewGuid(),
                ActorName = "actor",
                AggregateId = Guid.NewGuid(),
                AggregateClassId = Guid.NewGuid(),
                EntityId = Guid.NewGuid(),
                EntityClassId = Guid.NewGuid(),
                ChangeDataClassName = "TestChangeData",
                ChangeDataJson = "{\"Foo\":\"bar\"}",
                ChangeTime = DateTime.Now
            };

            TrackedChange change = sut.FromRecord(record);

            Assert.Equal(record.Id, change.Id);
            Assert.Equal(record.ActorName, change.ActorName);
            Assert.Equal(record.AggregateId, change.AggregateId);
            Assert.Equal(record.AggregateClassId, change.AggregateClassId);
            Assert.Equal(record.EntityId, change.EntityId);
            Assert.Equal(record.EntityClassId, change.EntityClassId);
            Assert.Equal(typeof(TestChangeData), change.ChangeData.GetType());
            Assert.Equal("bar", ((TestChangeData)change.ChangeData).Foo);
            Assert.Equal(record.ChangeTime, change.ChangeTime);
        }

        [Fact]
        public void ToRecord_Result()
        {
            TrackedChange change = new TrackedChange(Guid.NewGuid(),
                new TestChangeData() { Foo = "bar" }, "actor", Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.Now);

            TrackedChangeRecord record = sut.ToRecord(change);

            Assert.Equal(change.Id, record.Id);
            Assert.Equal(change.ActorName, record.ActorName);
            Assert.Equal(change.UserId, record.UserId);
            Assert.Equal(change.AggregateId, record.AggregateId);
            Assert.Equal(change.AggregateClassId, record.AggregateClassId);
            Assert.Equal(change.EntityId, record.EntityId);
            Assert.Equal(change.EntityClassId, record.EntityClassId);
            Assert.Equal("TestChangeData", record.ChangeDataClassName);
            Assert.Equal("bar", JObject.Parse(record.ChangeDataJson)["Foo"]);
            Assert.Equal(change.ChangeTime, record.ChangeTime);
        }
        
        public class TestChangeData : ChangeData
        {
            public string Foo { get; set; }
        }
    }
}
