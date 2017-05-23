using System;
using NUnit.Framework;
using Moq;
using Memento.Messaging;
using LiteDB;
using SharpTestsEx;
using System.IO;
using Memento.Persistence.LiteDB.Tests.Events;
using System.Linq;
using System.Collections.Generic;

namespace Memento.Persistence.LiteDB.Tests
{
    [TestFixture]
    public class LiteDbEventStoreFixture
    {
        LiteDbEventStore EventStore = null;
        //IEventStore EventStore = null;
        string path = Path.Combine(Path.GetTempPath(), "lite.db");

        [SetUp]
        public void SetUp()
        {
            var bus = new Mock<IEventDispatcher>().Object;
            var liteDatabase = new LiteDatabase(path, new BsonMapper());

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

        [Test]
        public void Event_Retrieval_Shoul_Work_Properly()
        {
            var aggregateId = Guid.NewGuid();

            var firstEvent = new PlainEvent(aggregateId, "Hello Memento", DateTime.Now, double.MaxValue);
            EventStore.Save(firstEvent);

            var secondEvent = new PlainEvent(aggregateId, "Hello Mastreeno", DateTime.Now.AddDays(2), 0.0D);
            EventStore.Save(secondEvent);

            var eventDescriptors = new List<EventMapping>()
            {
                new EventMapping
                {
                    AggregateIdPropertyName = nameof(PlainEvent.AggregateId),
                    EventType = typeof(PlainEvent)
                }
            };

            var events = EventStore.RetrieveEvents(aggregateId, DateTime.Now, eventDescriptors, timelineId: null);
            Assert.AreEqual(events.Count(), 1);
            //Assert.AreEqual(events.First().GetType(), typeof(PlainEvent));
        }
    }
}
