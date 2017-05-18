using System;
using NUnit.Framework;
using Moq;
using Memento.Messaging;
using LiteDB;
using SharpTestsEx;

namespace Memento.Persistence.LiteDB.Tests
{
    [TestFixture]
    public class UnitTest1
    {
        IEventStore EventStore = null;

        [SetUp]
        public void SetUp()
        {
            var bus = new Mock<IEventDispatcher>().Object;
            var liteDatabase = new LiteDatabase(@"local.db");
            EventStore = new LiteDbEventStore(liteDatabase, bus);
        }

        [Test]
        public void LiteDbEventStore_Throws_When_EventDispatcher_Is_Null()
        {
            var bus = new Mock<IEventDispatcher>().Object;
            Executing.This(() => new LiteDbEventStore(null))
                .Should()
                .Throw<ArgumentNullException>()
                .And
                .ValueOf
                .ParamName
                .Should()
                .Be
                .EqualTo("eventDispatcher");
        }
    }
}
