package Kompotech;
import java.rmi.Remote;
import java.rmi.RemoteException;

public interface ICalculator extends Remote{
	public int add(Data d) throws RemoteException;
}
