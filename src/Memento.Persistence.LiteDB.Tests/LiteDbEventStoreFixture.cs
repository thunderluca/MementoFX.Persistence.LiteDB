using LiteDB;
using Memento.Messaging;
using Memento.Persistence.LiteDb.Tests.Events;
using Moq;
using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Memento.Persistence.LiteDb.Tests
{
    [TestFixture]
    public class LiteDbEventStoreFixture
    {
        IEventStore EventStore = null;
        readonly string path = Path.Combine(Path.GetTempPath(), "lite.db");

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
