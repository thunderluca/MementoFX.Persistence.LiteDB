using System;

namespace MementoFX.Persistence.LiteDb.Tests.Events
{
    public class ComplexEvent : DomainEvent
    {
        public ComplexEvent() { }

        public ComplexEvent(Guid secondId, Component[] components)
        {
            this.SecondId = secondId;
            this.Components = components;
        }
        
        public Guid SecondId { get; set; }

        public Component[] Components { get; set; }

        public class Component
        {
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
