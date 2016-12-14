package Kompotech;
import java.io.BufferedReader;
import java.io.InputStreamReader;

import org.apache.camel.CamelContext;
import org.apache.camel.Exchange;
import org.apache.camel.Processor;
import org.apache.camel.builder.RouteBuilder;
import org.apache.camel.component.rest.RestApiComponent;
import org.apache.camel.component.rest.RestApiEndpoint;
import org.apache.camel.component.rmi.RmiEndpoint;
import org.apache.camel.impl.DefaultCamelContext;
import org.apache.camel.model.rest.RestBindingMode;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.module.SimpleModule;

public class Server{
	public static void main(String[] args) throws Exception {
		ObjectMapper m = new ObjectMapper();
		m.registerModule(new SimpleModule().addSerializer(new DataSerializer(Data.class)).addDeserializer(Data.class,new DataDeserializer(Data.class)));
		Data d = new Data();
		d.a=1;
		d.b=55;
		String s = m.writeValueAsString(d);
		Data b = m.readValue(s, Data.class);
		
		System.out.println(s);
		CamelContext context = new DefaultCamelContext();
//		RestApiComponent api = new RestApiComponent();
//		context.addComponent("calc", api);
		context.addRoutes(new RouteBuilder(){
			public void configure(){
				RmiEndpoint e = (RmiEndpoint) endpoint("rmi://localhost:1199/calc");
				restConfiguration().component("restlet").bindingMode(RestBindingMode.json).port(8888);
				from("restlet:http://localhost:8888/calc/add?restletMethod=post").process(new Processor(){

					@Override
					public void process(Exchange exchange) throws Exception {
						String json = exchange.getIn().getBody(String.class);
						exchange.getOut().setBody(m.readValue(json, Data.class));
					}
					
				}).to(e);
				//rest("/calc").post("/add").to("rmi://localhost:1199/calc");
				
			}
		});
		context.start();
		System.out.println("Started");
		new BufferedReader(new InputStreamReader(System.in)).readLine();
		System.out.println("ending");
	}
}
