package tony.remoteviewer;

import java.io.IOException;
import java.net.InetSocketAddress;
import java.net.Socket;

import tony.remoteviewer.R;

public class GlobleAppSetting {

	private static String _iPAddress = null;
	private static int _port = 0;
	public static int theme = R.style.AppTheme;
	public static boolean showFPS = false;

	public static boolean SetServer(String iPAddress, int port) {
		// TODO Auto-generated method stub
		Socket server = new Socket();
		InetSocketAddress inet = new InetSocketAddress(iPAddress, port);
		try {
			server.connect(inet , 5000);
			server.close();
			_iPAddress = iPAddress;
			_port = port;
			return true;
		} catch (IOException e) {
			// TODO Auto-generated catch block
			return false;
		}

	}

	public static Socket GetNewServer() {
		Socket server = new Socket();
		InetSocketAddress inet = new InetSocketAddress(_iPAddress, _port);
		try {
			server.connect(inet , 5000);
			return server;
		} catch (IOException e) {
			// TODO Auto-generated catch block
			return null;
		}
	}

}
