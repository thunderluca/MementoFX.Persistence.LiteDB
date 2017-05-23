using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Persistence.LiteDB.Tests.Events
{
    public class ComplexEvent : DomainEvent
    {
        public ComplexEvent(Guid secondId, Component[] components)
        {
            this.SecondId = secondId;
            this.Components = components;
        }

        //public ComplexEvent() { }

        public Guid SecondId { get; set; }

        public Component[] Components { get; set; }

        public class Component
        {
            //public Component() { }

            public Component(string title, int number)
            {
                this.Title = title;
                this.Number = number;
            }

            public string Title { get; set; }

            public int Number { get; set; }
        }
    }
}
