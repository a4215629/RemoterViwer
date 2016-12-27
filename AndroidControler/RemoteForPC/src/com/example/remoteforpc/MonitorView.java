package com.example.remoteforpc;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;


import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.R.string;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.BitmapFactory.Options;
import android.graphics.Canvas;
import android.graphics.Rect;
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
	ImageView image_video;
	TextView txt_host;
	Socket server;
	Bundle data;
	Thread update;
	boolean stopUpdateThread = false;

	@Override
	protected synchronized void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
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
	final class CommandListener implements DialogInterface.OnClickListener
	{
		String command = "";
		public CommandListener(String command)
		{
			this.command = command;
		}
		@Override
		public void onClick(DialogInterface arg0, int arg1) {
			// TODO Auto-generated method stub
			new WriteThread("Shutdown",0,0).start();
		}
	}

	
	// µ¥»÷ÊÂ¼þ
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		int itemId = item.getItemId();
		if(itemId == R.id.menu_shutdown)
			new AlertDialog.Builder(MonitorView.this).setTitle("Shutdown!").setMessage("Are you sure shutdown this PC?").setPositiveButton("Shutdown",new CommandListener("Shutdown")).setNegativeButton("Cancel",null).show();
		return super.onOptionsItemSelected(item);
	}

	@Override
	public synchronized void onDestroy() {

		stopUpdateThread = true;

		try {
			server.close();
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
		// ´¥ÃþÆÁÄ»Ê±¿Ì
		case MotionEvent.ACTION_DOWN:
			break;
		// ´¥Ãþ²¢ÒÆ¶¯Ê±¿Ì
		case MotionEvent.ACTION_MOVE:
			break;
		// ÖÕÖ¹´¥ÃþÊ±¿Ì
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
					OutputStream out = server.getOutputStream();
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
			while (!stopUpdateThread) {
				try {
					if (server == null) {
						server = new Socket(data.getString("IPAddress"),
								data.getInt("port"));
					}

					InputStream in = server.getInputStream();
					final int arrayLength = Tool.byte2int(Tool.ReadData(in, 4));
					
					final byte[] Buffer = Tool.ReadData(in, arrayLength) ;
					Message msg = Message.obtain();
					Bundle bun = new Bundle();
					bun.putByteArray("data", Buffer);
					msg.setData(bun);
					handler.sendMessage(msg);

				} catch (Exception e) {
					try {

						Thread.sleep(100);
						server.close();
						server = null;
					} catch (Exception e1) {
						// TODO Auto-generated catch block
						e1.printStackTrace();
					}
				}
			}

		}
	};


	Handler handler = new Handler() {
		@Override
		public void handleMessage(Message msg) {
			if (!stopUpdateThread) {
				byte[] buffer = msg.getData().getByteArray("data");
			
				try {
					ScreenShotPackage sPackage =new ScreenShotPackage(buffer);
					image_video.setImageBitmap(sPackage.GetImage());
				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}

			}

		}
	};

}
