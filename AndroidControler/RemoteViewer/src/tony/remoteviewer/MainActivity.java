package tony.remoteviewer;
import tony.remoteviewer.R;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;

public class MainActivity extends Activity {
	boolean destroied = false;
	EditText txt_IPAddress;
	EditText txt_port;
	Button btn_connect;
	static final int msg_ConnectStart = 1;
	static final int msg_ConnectSuccess = 2;
	static final int msg_ConnectFailed = 3;

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
	@Override
	public synchronized void onDestroy() {
		destroied = true;
		handler.removeCallbacksAndMessages(null); 
		super.onDestroy();

	}

	void initialize() {
		this.txt_IPAddress = (EditText) findViewById(R.id.txt_IPAddress);
		this.txt_port = (EditText) findViewById(R.id.txt_port);
		this.btn_connect = (Button) findViewById(R.id.btn_connect);
		btn_connect.setOnClickListener(new Listener());
		this.txt_IPAddress.setText(Tool.getProperty(this,getString(R.string.lbl_IPAddress)));
		this.txt_port.setText(Tool.getProperty(this,getString(R.string.lbl_port)));
	}

	@SuppressLint("HandlerLeak")
	final class Listener implements OnClickListener {

		@Override
		public void onClick(View v) {
			// TODO Auto-generated method stub
			new connectThread().start();
		}

		final class connectThread extends Thread {
			@Override
			public synchronized void run() {
				Message msg = Message.obtain();
				msg.what = msg_ConnectStart;
				handler.sendMessage(msg);
				String IPAddress = txt_IPAddress.getText().toString();
				int port = Integer.valueOf(txt_port.getText().toString());
				msg = Message.obtain();
				if (GlobleAppSetting.SetServer(IPAddress, port)) {
					msg.what = msg_ConnectSuccess;
				} else {
					msg.what = msg_ConnectFailed;
					msg.obj = "Connect failed";
				}
				handler.sendMessage(msg);
			}
		};
	}
	Handler handler = new Handler() {
		@Override
		public synchronized void handleMessage(Message msg) {
			if(destroied)
				return;
			switch (msg.what) {
			case msg_ConnectStart:
				turn_On_Off_input(false);
				break;
			case msg_ConnectFailed:
				turn_On_Off_input(true);
				new AlertDialog.Builder(MainActivity.this)
						.setTitle("Failed").setMessage((String) msg.obj)
						.setNegativeButton("OK", null).show();
				break;
			case msg_ConnectSuccess:
				Tool.setProperty(MainActivity.this,getString(R.string.lbl_IPAddress), txt_IPAddress.getText().toString());
				Tool.setProperty(MainActivity.this,getString(R.string.lbl_port), txt_port.getText().toString());
				turn_On_Off_input(true);
				Intent moniter = new Intent(MainActivity.this,
						MonitorView.class);
				MainActivity.this.startActivity(moniter);
				break;
			default:
				break;
			}
		}
	};

	private void turn_On_Off_input(boolean on_off) {
		txt_IPAddress.setEnabled(on_off);
		txt_port.setEnabled(on_off);
		btn_connect.setEnabled(on_off);
	}

}
