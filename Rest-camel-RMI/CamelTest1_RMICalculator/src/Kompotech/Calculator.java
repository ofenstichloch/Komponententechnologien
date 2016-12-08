package Kompotech;
import java.rmi.AlreadyBoundException;
import java.rmi.RemoteException;
import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;
import java.rmi.server.UnicastRemoteObject;

public class Calculator extends UnicastRemoteObject implements ICalculator{

	public Calculator() throws RemoteException {
		super();
	}
	
	public static void main(String[] args) throws RemoteException, AlreadyBoundException {
		Registry reg;
		reg = LocateRegistry.createRegistry(1199);
        Calculator server = new Calculator();
        reg.bind("calc", server);
        System.out.println("Up and running");
	}

	@Override
	public int add(Data d) {
		System.out.println("added "+d.a+"+"+d.b);
		return d.a+d.b;
	}

}
