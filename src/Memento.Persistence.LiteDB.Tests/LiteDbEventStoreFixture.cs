using System;
using NUnit.Framework;
using Moq;
using Memento.Messaging;
using LiteDB;
using SharpTestsEx;
using System.IO;
using Memento.Persistence.LiteDB.Tests.Events;
using System.Linq;

namespace Memento.Persistence.LiteDB.Tests
{
    [TestFixture]
    public class LiteDbEventStoreFixture
    {
        LiteDbEventStore EventStore = null;
        //IEventStore EventStore = null;
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "local.db");

        [SetUp]
        public void SetUp()
        {
            var bus = new Mock<IEventDispatcher>().Object;
            var liteDatabase = new LiteDatabase(path);

            EventStore = new LiteDbEventStore(liteDatabase, bus);
        }

        [Test]
        public void LiteDbEventStore_Throws_When_EventDispatcher_Is_Null()
        {
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

        [Test]
        public void Save_Throws_When_Event_Is_Null()
        {
            Executing.This(() => EventStore.Save(null))
                .Should()
                .Throw<ArgumentNullException>()
                .And
                .ValueOf
                .ParamName
                .Should()
                .Be
                .EqualTo("event");
        }

        [Test]
        public void Save_Should_Allow_Retrieval()
        {
            var @event = new PlainEvent(Guid.NewGuid(), "Hello Memento", DateTime.Now, double.MaxValue);
            EventStore.Save(@event);

            var events = EventStore.Find<PlainEvent>(pe => pe.AggregateId == @event.AggregateId);
            Assert.AreEqual(events.Count(), 1);
            Assert.AreEqual(events.First().AggregateId, @event.AggregateId);
            Assert.AreEqual(events.First().Title, @event.Title);
            Assert.AreEqual(events.First().DataDiProva.ToString("u"), @event.DataDiProva.ToString("u"));
            Assert.AreEqual(events.First().Number, @event.Number);
        }
    }
}
