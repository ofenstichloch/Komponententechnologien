using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace Broker
{
	public class Broker
	{

		const string INQUEUE = "q1";
		const string OUTEXCHANGE = "e1";
		const string SELFURI = "BrokerURI";
		ConnectionFactory rqFactory;
		IModel rqModel;
		IBasicProperties rqPPersistent;

		public Broker()
		{
			registered = new List<Component>();
			rqFactory = new ConnectionFactory();
			rqFactory.Uri = "amqp://test:test@localhost:5672/broker";
			IConnection conn = rqFactory.CreateConnection();
			rqModel = conn.CreateModel();
			rqPPersistent = rqModel.CreateBasicProperties();
			rqPPersistent.DeliveryMode = 2;
			EventingBasicConsumer consumer = new EventingBasicConsumer(rqModel);
			consumer.Received += onRecieve;
			rqModel.BasicConsume(INQUEUE, false, consumer);
			Console.Out.WriteLine("Broker up and running");
			/* byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("Hello, world!");
 			* rqModel.BasicPublish("e1", "", null, messageBodyBytes);
			*/
		}


		List<Component> registered;

		public bool register(Component c)
		{
			registered.Add(c);
			Console.Out.WriteLine("New Component (" +(Component.Type)c.type + ") registerd");
			return true;
		}

		public bool unregister(String uri)
		{
			return registered.Remove(registered.Find(x => x.uri.Equals(uri)));
		}

		public void processGenerated(Message m)
		{
			m.origin = SELFURI;
			Component c = sendMessage(m, Component.Type.Solver);
			//Mark solver as busy
			c.busy = true;

		}

		public void processSolved(Message m)
		{
			//Mark solver as idle
		}

		public Component sendMessage(Message m, Component.Type recipientType)
		{
			Component c = registered.Find(x => (x.type == recipientType && !x.busy));
			if (c == null) c = registered.Find(x => x.type == recipientType);
			String message = JsonConvert.SerializeObject(m);
			byte[] body = System.Text.Encoding.Default.GetBytes(message);
			rqModel.BasicPublish(OUTEXCHANGE, recipientType.ToString(), false,rqPPersistent, body);
			Console.Out.WriteLine(message + "\t" + recipientType.ToString());
			return c;
		}

		public void onRecieve(object sender, BasicDeliverEventArgs e)
		{
			String message = System.Text.Encoding.Default.GetString(e.Body);
			Message m = JsonConvert.DeserializeObject<Message>(message);
			Console.Out.WriteLine("Received: " + (Message.Type)m.instruction);
			switch ((Message.Type) m.instruction)
			{
				case Message.Type.Register:
					register(new Component((Component.Type) m.info, m.origin));
					break;
				case Message.Type.Unregister:
					unregister(m.origin);
					break;
				case Message.Type.Generated:
					processGenerated(m);
					break;
				case Message.Type.Solved:
					processSolved(m);
					break;
			}
			rqModel.BasicAck(e.DeliveryTag, false);
		}

	}
}
