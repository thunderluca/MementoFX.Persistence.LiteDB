using System;

namespace Memento.Persistence.LiteDb.Tests.Events
{
    public class PlainEvent : DomainEvent
    {
        public PlainEvent(Guid aggregateId, string title, DateTime dataDiProva, double number)
        {
            this.AggregateId = aggregateId;
            this.Title = title;
            this.DataDiProva = dataDiProva;
            this.Number = number;
        }
        
        public Guid AggregateId { get; set; }

        public string Title { get; set; }

        public DateTime DataDiProva { get; set; }

        public double Number { get; set; }
    }
}
