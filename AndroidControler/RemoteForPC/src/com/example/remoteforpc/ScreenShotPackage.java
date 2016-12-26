package com.example.remoteforpc;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;

public class ScreenShotPackage {
    public int XCount;
    public int YCount;
    public boolean[][] ChunksChange;
    public int[][] ChunksLength;
    public byte[][][] ChunksJpgData;
    private static ScreenShotPackage ScreenShotPackage = null;
    
    public static ScreenShotPackage GeneraterScreenShotPackage(byte[] bytes) throws IOException{
    	ScreenShotPackage newPackage = new ScreenShotPackage(bytes);
    	if(ScreenShotPackage == null)
    		ScreenShotPackage = newPackage;
    	else {
    		for (int x = 0; x < ScreenShotPackage.XCount; x++) {
    			for (int y = 0; y < ScreenShotPackage.YCount; y++){
    				if(newPackage.ChunksLength[x][y]!=0)
    				ScreenShotPackage.ChunksJpgData[x][y] = newPackage.ChunksJpgData[x][y];
    			}
    		}
		}
    	return ScreenShotPackage;
    }
	private ScreenShotPackage(byte[] bytes) throws IOException{
		InputStream stream = new ByteArrayInputStream(bytes);
		XCount = Tool.byte2int(Tool.ReadData(stream, 4));
		YCount = Tool.byte2int(Tool.ReadData(stream, 4));
		ChunksJpgData = new byte[XCount][YCount][];
		ChunksLength = new int[XCount][YCount];
		for (int x = 0; x < XCount; x++) {
			for (int y = 0; y < YCount; y++){
				ChunksLength[x][y] = Tool.byte2int(Tool.ReadData(stream, 4));
			}
		}
		for (int x = 0; x < XCount; x++) {
			for (int y = 0; y < YCount; y++){
				ChunksJpgData[x][y] =Tool.ReadData(stream, ChunksLength[x][y]);
			}
		}
	}
	
	private boolean compare(byte[] b1,byte[] b2)
	{
		if(b1.length != b2.length)
			return false;
		for (int x = 0; x < b1.length; x++) {
			if(b1[x] != b2[x])
				return false;
		}
		return true;
		
	}
}
