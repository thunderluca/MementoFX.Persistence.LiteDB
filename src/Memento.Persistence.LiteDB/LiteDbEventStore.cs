using Memento.Messaging;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Memento.Persistence.LiteDb
{
    public class LiteDbEventStore : EventStore
    {
        public static LiteDatabase LiteDatabase { get; private set; }

        public LiteDbEventStore(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
            if (LiteDatabase == null)
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["EventStore"].ConnectionString;
                LiteDatabase = new LiteDatabase(connectionString, new BsonMapper());
            }
        }

        public LiteDbEventStore(LiteDatabase liteDatabase, IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
            LiteDatabase = liteDatabase ?? throw new ArgumentNullException(nameof(liteDatabase));
            BsonMapper.Global.IncludeNonPublic = true;
        }

        public override IEnumerable<T> Find<T>(Func<T, bool> filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            
            return LiteDatabase.GetCollection<T>(typeof(T).Name).Find(query);
        }

        public IEnumerable<T> Find<T>(Expression<Func<T, bool>> exp) where T : DomainEvent
        {
            if (exp == null)
            {
                throw new ArgumentNullException(nameof(exp));
            }

            var collectionName = typeof(T).Name;

            var events = LiteDatabase.GetCollection<T>(collectionName).Find(exp);

            return events;
        }

        public override IEnumerable<DomainEvent> RetrieveEvents(Guid aggregateId, DateTime pointInTime, IEnumerable<EventMapping> eventDescriptors, Guid? timelineId)
        {
            var events = new List<DomainEvent>();

            var descriptorsGrouped = eventDescriptors.GroupBy(k => k.EventType);

            foreach (var descriptorsGroup in descriptorsGrouped)
            {
                var eventType = descriptorsGroup.Key;
                var collectionName = eventType.Name;
                
                var filters = new List<Query>();

                for (int i = 0; i < descriptorsGroup.Count(); i++)
                {
                    var eventDescriptor = descriptorsGroup.ElementAt(i);
                    var filterBuilt = Query.EQ(eventDescriptor.AggregateIdPropertyName, new BsonValue(aggregateId));
                    filters.Add(filterBuilt);
                }

                var resultFilter = filters[0];
                for (var i = 1; i < filters.Count; i++)
                {
                    resultFilter = Query.Or(resultFilter, filters[i]);
                }

                resultFilter = Query.And(resultFilter, Query.LTE(nameof(DomainEvent.TimeStamp), new BsonValue(pointInTime)));

                if (!timelineId.HasValue)
                {
                    resultFilter = Query.And(resultFilter, Query.EQ(nameof(DomainEvent.TimelineId), BsonValue.Null));
                }
                else
                {
                    resultFilter = Query.And(
                        resultFilter, 
                        Query.Or(
                            Query.EQ(nameof(DomainEvent.TimelineId), BsonValue.Null), 
                            Query.EQ(nameof(DomainEvent.TimelineId), new BsonValue(timelineId.Value))));
                }

                var collection = LiteDatabase.GetCollection(collectionName).Find(resultFilter);
                foreach (var document in collection)
                {
                    var evt = LiteDatabase.Mapper.ToObject(eventType, document);
                    events.Add((DomainEvent)evt);
                }
            }

            return events.OrderBy(e => e.TimeStamp);
        }

        protected override void _Save(DomainEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var eventType = @event.GetType();

            var getCollectionMethod = typeof(LiteDatabase).GetMethods().First(m => m.Name == nameof(LiteDatabase.GetCollection));
            var getCollectionGeneric = getCollectionMethod.MakeGenericMethod(eventType);

            var collection = getCollectionGeneric.Invoke(LiteDatabase, new object[] { eventType.Name });

            var collectionType = collection.GetType();
            var method = collectionType.GetMethods().First(m => m.Name == nameof(LiteCollection<object>.Insert));

            method.Invoke(collection, new object[] { @event });
        }
    }
}
