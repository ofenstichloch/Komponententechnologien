using System;
namespace Broker
{
	public class Component
	{
        public enum Type { Broker, Generator, Solver, GUI };

        public static int currentID = 0;

        //id is used as a routing key for the camel instance to send a message to a specific component
        public int id;
        public Type type { get; private set;}
		public string uri { get; private set; }
		public bool busy { get; set; }

		public Component(Type newType, string newURI)
		{
            id = ++currentID;
			type = newType;
			uri = newURI;
			busy = false;
		}
	}
}
