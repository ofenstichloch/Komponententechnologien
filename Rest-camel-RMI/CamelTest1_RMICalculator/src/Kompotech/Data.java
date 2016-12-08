package Kompotech;
import java.io.Serializable;
/*
import com.fasterxml.jackson.databind.annotation.JsonDeserialize;
import com.fasterxml.jackson.databind.annotation.JsonSerialize;
@JsonDeserialize(using=DataDeserializer.class)
@JsonSerialize
An Interface would solve this problem
*/
public class Data implements Serializable {
	int a;
	int b;
}
