using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace Broker
{
	public class Broker
	{

		const string INQUEUE = "in";
		const string OUTEXCHANGE = "outExchange";
        const string CONFIG = "config";
		const string SELFURI = "amqp://test:test@10.4.0.6:5672/broker";
		ConnectionFactory rqFactory;
		IModel rqModel;
		IBasicProperties rqPPersistent;
        List<Component> registered;
        List<Message> queue;

		public Broker()
		{
			registered = new List<Component>();
            queue = new List<Message>();
			rqFactory = new ConnectionFactory();
			rqFactory.Uri = SELFURI;
			IConnection conn = rqFactory.CreateConnection();
			rqModel = conn.CreateModel();
			rqPPersistent = rqModel.CreateBasicProperties();
			rqPPersistent.DeliveryMode = 2;
			EventingBasicConsumer consumer = new EventingBasicConsumer(rqModel);
			consumer.Received += onRecieve;
			rqModel.BasicConsume(INQUEUE, false, consumer);
			Console.Out.WriteLine("Broker up and running");
		}

		/// <summary>
        /// Registers a new component and sends the command to add a new queue to the camel instance
        /// </summary>
        /// <param name="c">Component to add</param>
        /// <returns>True on sucess</returns>
        public bool register(Component c)
		{
            configureRoute(c);
			registered.Add(c);
			Console.Out.WriteLine("Broker:\t New Component (" +c.type + ") registerd");
            //try to flush queue
            flushQueue(c);
			return true;
		}

        /// <summary>
        /// Sends a component to the camel instance. If the camel instance does not know the component, a new route is added, otherwise deleted
        /// Every component gets an id that is used as a routing key
        /// </summary>
        /// <param name="c">Component to add</param>
        public void configureRoute(Component c)
        {
            //Sends a new route configuration to camel instance
            byte[] body = System.Text.Encoding.Default.GetBytes(c.uri);
            rqModel.BasicPublish(CONFIG, c.id.ToString(), false, rqPPersistent, body);
            Console.Out.WriteLine("Config:\t Send route to " + c.uri);
        }
    
        private void flushQueue(Component c)
        {
            Message[] messages = new Message[queue.Count];
            queue.CopyTo(messages);
            foreach(Message m in messages){
                queue.Remove(m);
                switch (m.instruction)
                {
                    case Message.Instruction.Generated:
                        processGenerated(m);
                        break;
                    case Message.Instruction.Solved:
                        processSolved(m);
                        break;
                }
            }
        }

        public bool unregister(String uri)
		{
            Component c = registered.Find(x => x.uri.Equals(uri));
            configureRoute(c);
            return registered.Remove(c);
		}

		public void processGenerated(Message m)
		{
            registered.Find(x => x.uri == m.origin).busy = false;
            m.origin = SELFURI;
			Component c = sendMessage(m, Component.Type.Solver);
            //Mark solver as busy
            //if no fitting component is currently registered, store it in queue to send it later
            if (c != null) c.busy = true;
            else queue.Add(m);

		}

		public void processSolved(Message m)
		{
            registered.Find(x => x.uri == m.origin).busy = false;
            m.origin = SELFURI;
            Component c = sendMessage(m, Component.Type.Generator);
            //Mark generator as busy
            //if no fitting component is currently registered, store it in queue to send it later
            if (c != null) c.busy = true;
            else queue.Add(m);
        }

		public Component sendMessage(Message m, Component.Type recipientType)
		{
            //try to find idling component
			Component c = registered.Find(x => (x.type == recipientType && !x.busy));
            //try to find a fitting (but busy) component
			if (c == null) c = registered.Find(x => x.type == recipientType);
            if (c != null)
            {
                String json = JsonConvert.SerializeObject(m);
                byte[] body = System.Text.Encoding.Default.GetBytes(json);
                rqModel.BasicPublish(OUTEXCHANGE, c.id.ToString(), false, rqPPersistent, body);
                Console.Out.WriteLine("Send:\t To "+ recipientType.ToString()+":\t" +json);
            }
			return c;
		}

		public void onRecieve(object sender, BasicDeliverEventArgs e)
		{
			String message = System.Text.Encoding.Default.GetString(e.Body);
			Message m = JsonConvert.DeserializeObject<Message>(message);
			Console.Out.WriteLine("Received:\t " + m.instruction);
			switch (m.instruction)
			{
				case Message.Instruction.Register:
					register(new Component((Component.Type) m.info, m.origin));
					break;
				case Message.Instruction.Unregister:
					unregister(m.origin);
					break;
				case Message.Instruction.Generated:
					processGenerated(m);
					break;
				case Message.Instruction.Solved:
					processSolved(m);
					break;
			}
			rqModel.BasicAck(e.DeliveryTag, false);
		}

	}
}
