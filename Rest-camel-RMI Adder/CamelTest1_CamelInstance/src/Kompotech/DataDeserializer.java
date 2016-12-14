package Kompotech;
import java.io.IOException;

import com.fasterxml.jackson.core.JsonGenerator;
import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.JsonToken;
import com.fasterxml.jackson.databind.DeserializationContext;
import com.fasterxml.jackson.databind.SerializerProvider;
import com.fasterxml.jackson.databind.deser.std.StdDeserializer;

public class DataDeserializer extends StdDeserializer {

	public DataDeserializer(){
		super(Data.class);
	}
	
	public DataDeserializer(Class<Data> t) {
		super(t);
	}

	@Override
	public Object deserialize(JsonParser p, DeserializationContext ctxt) throws IOException, JsonProcessingException {
		Data d = new Data();
		JsonToken token = null;
		while ((token = p.nextValue()) != null){
			if(token.isNumeric()){
				if(p.getCurrentName().equals("a")){
					d.a = p.getIntValue();
				}else if (p.getCurrentName().equals("b")){
					d.b=p.getIntValue();
				}
			}
		}
		return d;
	}
	

}
