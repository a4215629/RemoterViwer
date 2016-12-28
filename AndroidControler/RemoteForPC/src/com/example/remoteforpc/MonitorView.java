package com.example.remoteforpc;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.widget.ImageView;
import android.widget.TextView;

public class MonitorView extends Activity {

	public static final int EIGHT_ID = Menu.FIRST + 1;
	public static final int SIXTEEN_ID = Menu.FIRST + 2;
	public static final int TWENTY_FOUR_ID = Menu.FIRST + 3;
	public static final int TWO_ID = Menu.FIRST + 4;
	public static final int THIRTY_TWO_ID = Menu.FIRST + 5;
	public static final int FORTY_ID = Menu.FIRST + 6;
	public static final int ONE_ID = Menu.FIRST + 7;

	public static final int msg_receiverData = 1;
	public static final int msg_exit = 2;
	private Socket serverSocket = null;
	ImageView image_video;
	TextView txt_host;
	Bundle data;
	Thread update;
	boolean stopUpdateThread = false;

	@Override
	protected synchronized void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setTheme(GlobleAppSetting.theme);
		setContentView(R.layout.activity_monitor_view);
		data = getIntent().getExtras();
		image_video = (ImageView) findViewById(R.id.image_video);
		update = new UpdateImageThread();
		update.start();
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.monitor_view, menu);
		// this.populateMenu(menu);
		return super.onCreateOptionsMenu(menu);
	}

	final class CommandListener implements DialogInterface.OnClickListener {
		String command = "";

		public CommandListener(String command) {
			this.command = command;
		}

		@Override
		public void onClick(DialogInterface arg0, int arg1) {
			// TODO Auto-generated method stub
			new WriteThread("Shutdown", 0, 0).start();
		}
	}

	// 单击事件
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		int itemId = item.getItemId();
		switch (itemId) {
		case R.id.menu_shutdown:
			new AlertDialog.Builder(MonitorView.this)
					.setTitle("Shutdown!")
					.setMessage("Are you sure shutdown this PC?")
					.setPositiveButton("Shutdown",
							new CommandListener("Shutdown"))
					.setNegativeButton("Cancel", null).show();
			break;
		case R.id.menu_fit_screen:
			full_Screen();
		default:
			break;
		}
		return super.onOptionsItemSelected(item);
	}
	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event) {
		 
        if (keyCode == KeyEvent.KEYCODE_BACK && event.getRepeatCount() == 0) {
             if(GlobleAppSetting.theme == R.style.FullScreentTheme)
             {
            	 full_Screen();
            	 return false;
             }
          }
          return super.onKeyDown(keyCode, event);
      }
	
	@Override
	public synchronized void onDestroy() {
		stopUpdateThread = true;
		try {
			serverSocket.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
		}
		super.onDestroy();

	}

	@Override
	public boolean onTouchEvent(MotionEvent event) {
		int point[] = new int[4];
		this.image_video.getLocationInWindow(point);
		float picX = point[0];
		float picY = point[1];
		int picW = this.image_video.getWidth();
		int picH = this.image_video.getHeight();

		switch (event.getAction()) {
		// 触摸屏幕时刻
		case MotionEvent.ACTION_DOWN:
			break;
		// 触摸并移动时刻
		case MotionEvent.ACTION_MOVE:
			break;
		// 终止触摸时刻
		case MotionEvent.ACTION_UP:
			float x = event.getX();
			float y = event.getY();
			if (x < picX || y < picY)
				break;
			if (x > picX + picW || y > picY + picH)
				break;
			new WriteThread("LeftClick", (x - picX) / picW, (y - picY) / picH)
					.start();

			break;
		}

		return super.onTouchEvent(event);
	}

	private void exit() {
		stopUpdateThread = true;
		Message msg = Message.obtain();
		msg.what = msg_exit;
		handler.sendMessage(msg);
	}

	public void full_Screen() {
		stopUpdateThread = true;
		GlobleAppSetting.theme = GlobleAppSetting.theme == R.style.AppTheme ? R.style.FullScreentTheme
				: R.style.AppTheme;
		Intent intent = getIntent();
		overridePendingTransition(0, 0);// 不设置进入退出动画
		intent.addFlags(Intent.FLAG_ACTIVITY_NO_ANIMATION);
		finish();
		overridePendingTransition(0, 0);
		startActivity(intent);
	}

	final class WriteThread extends Thread {
		String flags;
		float x;
		float y;

		public WriteThread(String flags, float x, float y) {
			this.flags = flags;
			this.x = x;
			this.y = y;
		}

		@Override
		public synchronized void run() {
			if (!stopUpdateThread) {
				try {
					OutputStream out = serverSocket.getOutputStream();
					byte[] buffer = new byte[256];
					int index = 0;
					byte f[] = flags.getBytes();
					byte xl[] = String.valueOf(x).getBytes();
					byte yl[] = String.valueOf(y).getBytes();

					for (int i = index; i < f.length; i++) {
						buffer[index + i] = f[i];
					}
					index += 24;
					for (int i = 0; i < xl.length; i++) {
						buffer[index + i] = xl[i];
					}
					index += 8;
					for (int i = 0; i < yl.length; i++) {
						buffer[index + i] = yl[i];
					}
					out.write(buffer, 0, buffer.length);
					out.flush();

				} catch (Exception e) {
					return;
				}
			}
		}
	}

	final class UpdateImageThread extends Thread {
		@Override
		public synchronized void run() {
			serverSocket = GlobleAppSetting.GetNewServer();
			while (!stopUpdateThread) {
				try {
					InputStream in = serverSocket.getInputStream();
					final int arrayLength = Tool.byte2int(Tool.readData(in, 4));
					final byte[] buffer = Tool.readData(in, arrayLength);
					Message msg = Message.obtain();
					msg.what = msg_receiverData;
					msg.obj = buffer;
					handler.sendMessage(msg);

				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
					exit();

				}
			}

		}
	};

	Handler handler = new Handler() {
		@Override
		public void handleMessage(Message msg) {
			switch (msg.what) {
			case msg_receiverData:
				try {
					if (stopUpdateThread)
						return;
					ScreenShotPackage sPackage = new ScreenShotPackage((byte[]) msg.obj);
					image_video.setImageBitmap(sPackage.GetImage());
				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
				break;
			case msg_exit:
				finish();
				break;
			default:
				break;
			}
		}
	};

}
