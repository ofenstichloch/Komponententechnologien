using System;
namespace Broker
{
	public class Component
	{
		public enum Type { Generator, Solver, Broker, GUI };
	 	public Type type { get; private set;}
		public string uri { get; private set; }
		public bool busy { get; set; }

		public Component(Type newType, string newURI)
		{
			type = newType;
			uri = newURI;
			busy = false;
		}
	}
}
