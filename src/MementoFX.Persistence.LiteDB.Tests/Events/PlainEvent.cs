using System;

namespace MementoFX.Persistence.LiteDb.Tests.Events
{
    public class PlainEvent : DomainEvent
    {
        public PlainEvent() { } //Required

        public PlainEvent(Guid aggregateId, string title, DateTime dataDiProva, double number)
        {
            this.AggregateId = aggregateId;
            this.Title = title;
            this.DataDiProva = dataDiProva;
            this.Number = number;
        }
        
        public Guid AggregateId { get; set; }

        public string Title { get; set; }

        [Domain.Timestamp]
        public DateTime DataDiProva { get; set; }

        public double Number { get; set; }
    }
}
