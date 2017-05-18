using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Persistence.LiteDB.Tests.Events
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

        public PlainEvent() { }

        public Guid AggregateId { get; set; }

        public string Title { get; set; }

        public DateTime DataDiProva { get; set; }

        public double Number { get; set; }
    }
}
