package com.example.remoteforpc;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Rect;

public class ScreenShotPackage {
    public int XCount;
    public int YCount;
    public int[][] ChunksLength;
    public byte[][][] ChunksJpgData;
    private static Bitmap image = null;
    
    private void GeneraterImage()
    {
    	if(image == null)
    	{
			byte[] jpg0 =  ChunksJpgData[0][0];
    		Bitmap imgModel = BitmapFactory.decodeByteArray(jpg0, 0, jpg0.length);
    		image = Bitmap.createBitmap(imgModel.getWidth()*XCount, imgModel.getHeight()*YCount,Bitmap.Config.RGB_565);
    	}
    
		Canvas canvas = new Canvas(image);
		for (int x = 0; x < XCount; x++) {
			for (int y = 0; y < YCount; y++){
				if(ChunksLength[x][y]==0)
					continue;
				byte[] blockData = ChunksJpgData[x][y];
				Bitmap blockImg = BitmapFactory.decodeByteArray(blockData, 0, blockData.length);
				int sx = blockImg.getWidth()*x;
				int sy =  blockImg.getHeight()*y;
				int w = blockImg.getWidth();
				int h = blockImg.getHeight();
				Rect destRect = new Rect(sx,sy,sx+w,sy+h);  
				canvas.drawBitmap(blockImg,null,destRect,null);
			}
		}
    }
    
	public ScreenShotPackage(byte[] bytes) throws IOException{
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
		GeneraterImage();
	}
	public Bitmap GetImage()
	{
		return image;
	}
}
