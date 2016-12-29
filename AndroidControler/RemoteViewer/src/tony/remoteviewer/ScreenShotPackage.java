package tony.remoteviewer;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Rect;

public class ScreenShotPackage {
	private static final int type_completeImage = 1;
	private static final int type_splittingImage = 2;
	private static Bitmap baseImage = null;

	private void GeneraterImageBySplittingData(InputStream stream) throws IOException {
		
		int SplitXCount = Tool.byte2int(Tool.readData(stream, 4));
		int SplitYCount = Tool.byte2int(Tool.readData(stream, 4));
		byte[][][] ChunksJpgData = new byte[SplitXCount][SplitYCount][];
		int[][] ChunksLength = new int[SplitXCount][SplitYCount];
		for (int x = 0; x < SplitXCount; x++) {
			for (int y = 0; y < SplitYCount; y++) {
				ChunksLength[x][y] = Tool.byte2int(Tool.readData(stream, 4));
			}
		}
		for (int x = 0; x < SplitXCount; x++) {
			for (int y = 0; y < SplitYCount; y++) {
				ChunksJpgData[x][y] = Tool.readData(stream, ChunksLength[x][y]);
			}
		}
		
		if (baseImage == null) {
			byte[] jpg0 = ChunksJpgData[0][0];
			Bitmap imgModel = BitmapFactory.decodeByteArray(jpg0, 0,
					jpg0.length);
			baseImage = Bitmap.createBitmap(imgModel.getWidth() * SplitXCount,
					imgModel.getHeight() * SplitYCount, Bitmap.Config.RGB_565);
		}

		Canvas canvas = new Canvas(baseImage);
		for (int x = 0; x < SplitXCount; x++) {
			for (int y = 0; y < SplitYCount; y++) {
				if (ChunksLength[x][y] == 0)
					continue;
				byte[] blockData = ChunksJpgData[x][y];
				Bitmap blockImg = BitmapFactory.decodeByteArray(blockData, 0,
						blockData.length);
				int sx = blockImg.getWidth() * x;
				int sy = blockImg.getHeight() * y;
				int w = blockImg.getWidth();
				int h = blockImg.getHeight();
				Rect destRect = new Rect(sx, sy, sx + w, sy + h);
				canvas.drawBitmap(blockImg, null, destRect, null);
			}
		}
	}
	private void GeneraterImageByComplitedData(InputStream stream){
		baseImage = BitmapFactory.decodeStream(stream).copy(Bitmap.Config.RGB_565, true);
	}

	public ScreenShotPackage(byte[] bytes) throws IOException {

		int DataType = Tool.byte2int(new byte[]{bytes[0],bytes[1],bytes[2],bytes[3]});
		InputStream stream = new ByteArrayInputStream(bytes,4,bytes.length-4);
		if(DataType == type_completeImage)
			GeneraterImageByComplitedData(stream);
		else
			GeneraterImageBySplittingData(stream);
	}

	public Bitmap GetImage() {
		return baseImage;
	}
}
