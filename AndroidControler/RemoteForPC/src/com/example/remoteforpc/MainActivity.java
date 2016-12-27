package com.example.remoteforpc;

import java.net.InetSocketAddress;
import java.net.Socket;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;

public class MainActivity extends Activity {

	EditText txt_IPAddress;
	EditText txt_port;
	Button btn_connect;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		initialize();
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}
	
	void initialize()
	{
		this.txt_IPAddress = (EditText)findViewById(R.id.txt_IPAddress);
		this.txt_port =(EditText)findViewById(R.id.txt_port);
		this.btn_connect =(Button)findViewById(R.id.btn_connect);
		btn_connect.setOnClickListener(new Listener());
	}
	
	final class Listener implements OnClickListener
	{

		@Override
		public void onClick(View v) {
			// TODO Auto-generated method stub
			new connectThread().start();
		}
		
		final class connectThread extends Thread
		{
			@Override
			public synchronized void run()
			{
				try {
					Bundle data1 = new Bundle();
					data1.putBoolean("isStart", true);
					Message msg1 = new Message();
					msg1.setData(data1);
					handler.sendMessage(msg1);
					
					String IPAddress = txt_IPAddress.getText().toString();
					int port = Integer.valueOf(txt_port.getText().toString());
					Socket server = new Socket();
					InetSocketAddress inet = new InetSocketAddress(IPAddress, port);
					server.connect(inet , 5000);
					server.close();
					
					Message msg2 = new Message();
					Bundle data2 = new Bundle();
					data2.putBoolean("isStart", false);
					data2.putBoolean("isConnect",true);
					data2.putString("IPAddress", IPAddress);
					data2.putInt("port", port);
					msg2.setData(data2);
					handler.sendMessage(msg2);
					
				} catch (Exception e) {
					Message msg = new Message();
					Bundle data = new Bundle();
					data.putBoolean("isConnect", false);
					data.putString("exception", e.getMessage());
					msg.setData(data);
					handler.sendMessage(msg);
					e.printStackTrace();
				}
			}
		};
		Handler handler = new Handler()
		{
			@Override
			public synchronized void  handleMessage (Message msg) {

				Bundle data =msg.getData();
				if(data.getBoolean("isStart"))
				{
					txt_IPAddress.setEnabled(false);
					txt_port.setEnabled(false);
					btn_connect.setEnabled(false);
				}
				else
				{
					txt_IPAddress.setEnabled(true);
					txt_port.setEnabled(true);
					btn_connect.setEnabled(true);
					if(!data.getBoolean("isConnect"))
					{
						new AlertDialog.Builder(MainActivity.this).setTitle("Failed").setMessage(data.getString("exception")).setNegativeButton("OK", null).show();
					}
					else
					{
						Intent moniter = new Intent(MainActivity.this,MonitorView.class);
						moniter.putExtras(data);
						MainActivity.this.startActivity(moniter);
					}
				}
				
			}
		};
	}

	
}
