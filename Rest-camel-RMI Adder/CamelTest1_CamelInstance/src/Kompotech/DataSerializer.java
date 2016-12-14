package Kompotech;
import java.io.IOException;

import com.fasterxml.jackson.core.JsonGenerator;
import com.fasterxml.jackson.databind.SerializerProvider;
import com.fasterxml.jackson.databind.ser.std.StdSerializer;

public class DataSerializer extends StdSerializer<Data>{

	public DataSerializer(Class<Data> t) {
		super(t);
	}

	@Override
	public void serialize(Data value, JsonGenerator gen, SerializerProvider provider) throws IOException {
		gen.writeStartObject();
		gen.writeNumberField("a", value.a);
		gen.writeNumberField("b", value.b);
		gen.writeEndObject();
		
	}

}
