using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace Broker
{
    public class Broker
    {
        bool stupid = false;
        const string INQUEUE = "in";
        const string OUTEXCHANGE = "outExchange";
        const string CONFIG = "config";
        const string SELFURI = "amqp://guest:guest@localhost:5672";
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

        public Broker(bool stupid) : this()
        {
            Console.Out.WriteLine("Starting in stupid mode");
            this.stupid = stupid;
            
        }

        /// <summary>
        /// Registers a new component and sends the command to add a new queue to the camel instance
        /// </summary>
        /// <param name="c">Component to add</param>
        /// <returns>True on sucess</returns>
        public bool register(string instruction, string uri)
        {
            string type;
            try { type = instruction.Split(' ')[1]; }
            catch
            {
                type = instruction.Split(':')[1];
            }
            Component.Type t = Component.Type.GUI;
            switch (type)
            {
                case "broker":
                    t = Component.Type.Broker;
                    break;
                case "gui":
                    t = Component.Type.GUI;
                    break;
                case "solver":
                    t = Component.Type.Solver;
                    break;
                case "generator":
                    t = Component.Type.Generator;
                    break;
            }
            Component c = new Component(t, uri);
            configureRoute(c);
            registered.Add(c);
            Console.Out.WriteLine("Broker:\t New Component (" + c.type + ") registerd");
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
            foreach (Message m in messages)
            {
                queue.Remove(m);
                switch (m.instruction)
                {
                    case Message.Instruction.SOLVE:
                        processGenerated(m);
                        break;
                    case Message.Instruction.SOLVED:
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
            try { registered.Find(x => x.uri == m.sender).busy = false; }
            catch (NullReferenceException e) { }
            m.sender = SELFURI;
            Component c = sendMessage(m, Component.Type.Solver);
            //Mark solver as busy
            //if no fitting component is currently registered, store it in queue to send it later
            if (c != null) c.busy = true;
            else queue.Add(m);

        }

        public void processSolved(Message m)
        {
            registered.Find(x => x.uri == m.sender).busy = false;
            m.sender = SELFURI;
            Component c = sendMessage(m, Component.Type.Generator);
            //Mark generator as busy
            //if no fitting component is currently registered, store it in queue to send it later
            if (c != null) c.busy = true;
            else queue.Add(m);
        }

        public Component sendMessage(Message m, Component.Type recipientType)
        {
            String json = JsonConvert.SerializeObject(m);
            byte[] body = System.Text.Encoding.Default.GetBytes(json);
            if (recipientType == Component.Type.ALL)
            {
                foreach(Component c in registered)
                {
                    rqModel.BasicPublish(OUTEXCHANGE, c.id.ToString(), false, rqPPersistent, body);
                }
                Console.Out.WriteLine("Send:\t To all:\t" + json);
                return null;
            }
            else if(recipientType == Component.Type.ALLEXCEPTBROKER)
            {
                foreach (Component c in registered)
                {
                    if (c.type != Component.Type.Broker)
                    {
                        rqModel.BasicPublish(OUTEXCHANGE, c.id.ToString(), false, rqPPersistent, body);
                    }
                }
                Console.Out.WriteLine("Send:\t To all except broker:\t" + json);
                return null;
            }
            //try to find idling component
            else
            {
                Component c = registered.Find(x => (x.type == recipientType && !x.busy));
                //try to find a fitting (but busy) component
                if (c == null) c = registered.Find(x => x.type == recipientType);
                if (c != null)
                {
                    json = JsonConvert.SerializeObject(m);
                    body = System.Text.Encoding.Default.GetBytes(json);
                    rqModel.BasicPublish(OUTEXCHANGE, c.id.ToString(), false, rqPPersistent, body);
                    Console.Out.WriteLine("Send:\t To " + recipientType.ToString() + ":\t" + json);
                }
                return c;
            }
       
        }

        public void onRecieve(object sender, BasicDeliverEventArgs e)
        {
            String message = System.Text.Encoding.Default.GetString(e.Body);
            Message m = JsonConvert.DeserializeObject<Message>(message);
            Console.Out.WriteLine("Received:\t " + m.instruction);
            string instruction = m.instruction.Split(':')[0];
            Console.WriteLine(m.printSudoku());
            switch (instruction)
            {
                case Message.Instruction.REGISTER:
                    register(m.instruction, m.sender);
                    break;
                case Message.Instruction.UNREGISTER:
                    unregister(m.sender);
                    break;
                case Message.Instruction.SOLVE:
                    processGenerated(m);
                    break;
                case Message.Instruction.SOLVED:
                    processSolved(m);
                    break;
            }
            if (stupid)
            {
                try
                {
                    //Doesnt work this way, broker is never the sender
                    //Todo: change to identify already sent messages (looping)
                    if (registered.Find(x => x.uri == m.sender).type == Component.Type.Broker)
                    {
                        sendMessage(m, Component.Type.ALLEXCEPTBROKER);
                    }
                    else
                    {
                        sendMessage(m, Component.Type.ALL);
                    }
                }
                catch (NullReferenceException err)
                {

                }
            }
            rqModel.BasicAck(e.DeliveryTag, false);
        }

    }
}
