package org.komponententechnologien.broker;

import java.io.IOException;
import org.apache.camel.CamelContext;
import org.apache.camel.builder.RouteBuilder;
import org.apache.camel.impl.DefaultCamelContext;
import com.rabbitmq.client.*;

public class CamelInstance {

	static CamelContext context;
	static ConnectionFactory factory;
	//this is my queue to consume outgoing messages from
	static final String OUTQUEUE = "rabbitmq://10.4.0.6/out?vhost=broker&username=test&password=test&queue=out&skipQueueDeclare=true";
	
	public static void main(String[] args) throws Exception {
		context = new DefaultCamelContext();
		factory = new ConnectionFactory();
		factory.setUri("amqp://test:test@10.4.0.6:5672/broker");
		Connection conn = factory.newConnection();
		Channel channel = conn.createChannel();
		//setup config-consumer that consumes and processes config-messages from the broker
		DefaultConsumer cons = new DefaultConsumer(channel){
			@Override
	         public void handleDelivery(String consumerTag,
	                                    Envelope envelope,
	                                    AMQP.BasicProperties properties,
	                                    byte[] body)
	             throws IOException
	         {
	             String routingKey = envelope.getRoutingKey();
	             long deliveryTag = envelope.getDeliveryTag();
	             String uri = new String(body);
	             configureRoute(uri, routingKey);
	             
	             channel.basicAck(deliveryTag, false);
	         }
		};
		channel.basicConsume("config", false,"camelInstance",cons);
		context.start();
	}
	
	public static void configureRoute(String uri, String id){
		//If dont have a route with that routingKey (componentID) create it, otherwise delete it
		if(context.getRoute(id)==null){
		try {
			context.addRoutes(new RouteBuilder() {
				
				@Override
				public void configure() throws Exception {
					from(OUTQUEUE+"&routingKey="+id).to(uri).id(id);
					System.out.println("Added route from RoutingKey "+id+" to "+uri);
				}
			});
		} catch (Exception e) {
			System.out.println("Could not create new route to "+uri);
		}
		}
		else{
			try{
				context.stopRoute(id);
				context.removeRoute(id);
			}catch (Exception e) {
				System.out.println("Could not delete route to "+uri);
			}
		}
	}


}
