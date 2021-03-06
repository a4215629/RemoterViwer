package tony.remoteviewer;

import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;

import android.content.Context;

public class Tool {
	public static byte[] readData(InputStream in, int length) throws IOException {
		byte[] data = new byte[length];
		int everCount = 100000;
		int redIndex = 0;
		while (length > 0) {
			int redCount;
			int sholdRead = length > everCount ? everCount : length;
			redCount = in.read(data, redIndex, sholdRead);
			redIndex += redCount;
			length -= redCount;
		}
		return data;
	}
	public static int byte2int(byte[] res) {   
		// 一个byte数据左移24位变成0x??000000，再右移8位变成0x00??0000   
		  
		int targets = (res[0] & 0xff) | ((res[1] << 8) & 0xff00) // | 表示安位或   
		| ((res[2] << 24) >>> 8) | (res[3] << 24);   
		return targets;   
		}
	 public static String getProperty(Context c, String name) {
		  String value = null;
		  Properties properties = new Properties();
		  try {
		   properties.load(c.openFileInput(c.getString(R.string.setting_file)));
		   value = properties.getProperty(name);
		  } catch (Exception e) {
		   e.printStackTrace();
		  }
		  return value;
		 }
	 
	public static void setProperty(Context c, String name,String value) {
		  Properties properties = new Properties();
		  try {
		   c.openFileOutput(c.getString(R.string.setting_file),Context.MODE_APPEND ).close();
		   properties.load(c.openFileInput(c.getString(R.string.setting_file)));
		   properties.setProperty(name, value);
		   properties.save(c.openFileOutput(c.getString(R.string.setting_file),Context.MODE_PRIVATE ),"None");
		  } catch (Exception e) {
		   e.printStackTrace();
		  }
		 }
}
