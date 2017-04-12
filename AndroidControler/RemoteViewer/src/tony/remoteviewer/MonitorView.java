package tony.remoteviewer;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;
import java.util.Date;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.ImageView;
import android.widget.TextView;

public class MonitorView extends Activity {
	public static final int msg_receiverData = 1;
	public static final int msg_exit = 2;
	public static final int msg_showFPS = 3;
	private Socket serverSocket = null;
	ImageView image_video;
	ViewGroup main_layout;
	TextView txt_fps;
	MenuItem menu_show_hide_fps;
	Bundle data;
	Thread update;
	Thread fpsThread;
	boolean stopUpdateThread = false;
	int lastFrames = 0;
	int cureentFrames = 0;
	int lastReceivedFrames = 0;
	int cureentReceivedFrames = 0;
	int queueLength = 0;

	@Override
	protected synchronized void onCreate(Bundle savedInstanceState) {
		getWindow().setFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON,WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
		setTheme(GlobleAppSetting.theme);
		setContentView(R.layout.activity_monitor_view);
		super.onCreate(savedInstanceState);
		data = getIntent().getExtras();
		main_layout = (ViewGroup)findViewById(R.id.monitor_main_layout);
		image_video = (ImageView) findViewById(R.id.image_video);
		txt_fps = (TextView) findViewById(R.id.txt_fps);
		ShowOrHideFPSBySetting();
		update = new UpdateImageThread();
		fpsThread = new TFSCalculateThread();
		update.start();
		fpsThread.start();
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.monitor_view, menu);
		boolean result = super.onCreateOptionsMenu(menu);
		menu_show_hide_fps = menu.findItem(R.id.menu_show_or_hide_fps);
		ShowOrHideFPSBySetting();
		return result;
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
			break;
		case R.id.menu_show_or_hide_fps:
			GlobleAppSetting.showFPS = !GlobleAppSetting.showFPS;
			ShowOrHideFPSBySetting();
			break;
		default:
			break;
		}
		return super.onOptionsItemSelected(item);
	}
	
	private void ShowOrHideFPSBySetting()
	{
		txt_fps.setVisibility(GlobleAppSetting.showFPS?View.VISIBLE:View.GONE);
		if(menu_show_hide_fps!=null)
			menu_show_hide_fps.setTitle(getString(GlobleAppSetting.showFPS?R.string.action_hide_fps:R.string.action_show_fps));
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
			handler.removeCallbacksAndMessages(null); 
			serverSocket.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
		}
		super.onDestroy();

	}

	static float lastTouchPicX = -1;
	static float lastTouchPicY = -1;
	static float lastTouchDownX = -1;
	static float lastTouchDownY = -1;
	static boolean isMoving = false;
	static Date lastTouchDoneTime;
	static Date lastTouchUpTime;
	@Override
	public boolean onTouchEvent(MotionEvent event) {
		
		int point[] = new int[4];
		this.image_video.getLocationInWindow(point);
		float picX = point[0];
		float picY = point[1];
		int picW = this.image_video.getWidth();
		int picH = this.image_video.getHeight();
		float x = event.getX();
		float y = event.getY();
		float moveIndexX =0;
		float moveIndexY =0;
		switch (event.getAction()) {
		// 触摸屏幕时刻
		case MotionEvent.ACTION_DOWN:
			isMoving = false;
			lastTouchDownX = x;
			lastTouchDownY = y;
			lastTouchDoneTime = new Date();
			break;
		// 触摸并移动时刻
		case MotionEvent.ACTION_MOVE:
			moveIndexX = x - lastTouchDownX;
			moveIndexY = y - lastTouchDownY;
			if(moveIndexX <=1 && moveIndexY<=1)
				break;
			if(lastTouchDoneTime !=null && new Date().getTime()-lastTouchDoneTime.getTime() > 200)
			{
				new WriteThread("MouseWheel", moveIndexX , moveIndexY ).start();
				lastTouchDownX = x;
				lastTouchDownY = y;
				lastTouchDoneTime = new Date();
			}
			isMoving = true;
			break;
		// 终止触摸时刻
		case MotionEvent.ACTION_UP:
			if(isMoving)
			{	
				moveIndexX = x - lastTouchDownX;
				moveIndexY = y - lastTouchDownY;
				new WriteThread("MouseWheel", moveIndexX , moveIndexY ).start();
				isMoving = false;
				break;
			}
			Date thisTouchUpTime = new Date();
			if (x < picX || y < picY)
				break;
			if (x > picX + picW || y > picY + picH)
				break;
			float touchPicX = (x - picX)  / picW;
			float touchPicY = (y - picY) / picH;
			
			if(lastTouchDoneTime !=null && thisTouchUpTime.getTime()-lastTouchDoneTime.getTime() > 500)
				new WriteThread("RightClick", touchPicX , touchPicY ).start();  //Right click
			else if(lastTouchUpTime!=null && thisTouchUpTime.getTime()-lastTouchUpTime.getTime() < 300)
				new WriteThread("LeftClick", lastTouchPicX , lastTouchPicY ).start();  //Double click
			else
				new WriteThread("LeftClick", touchPicX , touchPicY ).start();
			lastTouchPicX = touchPicX;
			lastTouchPicY = touchPicY;
			lastTouchUpTime = thisTouchUpTime;
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

	private void full_Screen() {
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
					if(queueLength <= 2)
					{
						msg.what = msg_receiverData;
						msg.obj = buffer;
						handler.sendMessage(msg);
						queueLength++;
					}
					cureentReceivedFrames++;
				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
					exit();

				}
			}

		}
	};
	final class TFSCalculateThread extends Thread {
		@Override
		public synchronized void run() {
			while (!stopUpdateThread) {
				int fps = cureentFrames - lastFrames;
				int rfPs = cureentReceivedFrames - lastReceivedFrames;
				lastFrames = cureentFrames; 
				lastReceivedFrames = cureentReceivedFrames;
				Message msg = Message.obtain();
				msg.what = msg_showFPS;
				msg.obj = "RFPS:"+rfPs+" FPS:"+fps;
				handler.sendMessage(msg);
				try {
					Thread.sleep(1000);
				} catch (InterruptedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}
		}
	};
	
	 Handler handler = new Handler(){
		@Override
		public void handleMessage(Message msg) {
			switch (msg.what) {
			case msg_receiverData:
				try {
					if (stopUpdateThread)
						return;
					ScreenShotPackage sPackage = new ScreenShotPackage((byte[]) msg.obj);
					Bitmap picture = sPackage.GetImage();
					double displayRate = Math.min(main_layout.getWidth()/(float)picture.getWidth(), main_layout.getHeight()/(float)picture.getHeight());
					double displayWidth = picture.getWidth() * displayRate;
					double displayHeight = picture.getHeight() * displayRate;
					Bitmap displayPicture = Bitmap.createScaledBitmap(picture, (int)displayWidth,(int)displayHeight, true);
					image_video.setImageBitmap(displayPicture);
					cureentFrames++;
					queueLength--;
				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
				break;
			case msg_exit:
				finish();
				break;
			case msg_showFPS:
				txt_fps.setText(msg.obj.toString());
			default:
				break;
			}
		}

	};

}
