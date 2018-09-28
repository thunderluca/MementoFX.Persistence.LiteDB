using LiteDB;
using MementoFX.Messaging;
using MementoFX.Persistence.LiteDb.Tests.Events;
using Moq;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace MementoFX.Persistence.LiteDb.Tests
{
    public class LiteDbEventStoreFixture
    {
        IEventStore EventStore = null;
        readonly string path = Path.Combine(Path.GetTempPath(), "lite.db");
        
        public LiteDbEventStoreFixture()
        {
            var bus = new Mock<IEventDispatcher>().Object;
            var liteDatabase = new LiteDatabase(path);
            var collectionNames = liteDatabase.GetCollectionNames().ToArray();
            if (collectionNames.Length > 0)
            {
                foreach (var collectionName in collectionNames)
                {
                    liteDatabase.DropCollection(collectionName);
                }
            }

            EventStore = new LiteDbEventStore(liteDatabase, bus);
        }

        [Fact]
        public void LiteDbEventStore_Throws_When_EventDispatcher_Is_Null()
        {
            Executing.This(() => new LiteDbEventStore(path, null))
                .Should()
                .Throw<ArgumentNullException>()
                .And
                .ValueOf
                .ParamName
                .Should()
                .Be
                .EqualTo("eventDispatcher");
        }

        [Fact]
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

        [Fact]
        public void Save_Should_Allow_Retrieval()
        {
            var @event = new PlainEvent(Guid.NewGuid(), "Hello Memento", DateTime.Now, double.MaxValue);
            EventStore.Save(@event);

            var events = EventStore.Find<PlainEvent>(pe => pe.AggregateId == @event.AggregateId);
            Assert.Single(events);
            Assert.Equal(events.First().AggregateId, @event.AggregateId);
            Assert.Equal(events.First().Title, @event.Title);
            Assert.Equal(events.First().DataDiProva.ToString("u"), @event.DataDiProva.ToString("u"));
            Assert.Equal(events.First().Number, @event.Number);
        }

        [Fact]
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
            Assert.Single(events);
            Assert.Equal(typeof(PlainEvent), events.First().GetType());
        }
    }
}
