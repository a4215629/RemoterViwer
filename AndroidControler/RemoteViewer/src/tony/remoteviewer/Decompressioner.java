package tony.remoteviewer;


public class Decompressioner {

	/**
	 * @param args
	 */
	public static byte[] Decompression (byte[] oldData,byte[] updateIndex,byte[] updateData,int newDataLength) {
		byte[] newData = oldData.clone();
		boolean isCame=true;
		int indexIndex = 0;
		int NoCameIndex= UnsignedLDT(updateIndex[indexIndex++], 16) + UnsignedLDT(updateIndex[indexIndex++], 8)+UnsignedLDT(updateIndex[indexIndex++], 0);
		int CameIndex = 0;
		
		try
		{
			int i=0,j=0;
		for (; i < newDataLength; i++) {
			if(isCame)
			{
				i=NoCameIndex-1;
				isCame=false;
				CameIndex =UnsignedLDT(updateIndex[indexIndex++], 16) + UnsignedLDT(updateIndex[indexIndex++], 8)+UnsignedLDT(updateIndex[indexIndex++], 0);
				
			}
			else
			{
				if(i<CameIndex)
					newData[i]=updateData[j++];
				else
				{
					isCame = true;
					i--;
					NoCameIndex=UnsignedLDT(updateIndex[indexIndex++], 16) + UnsignedLDT(updateIndex[indexIndex++], 8)+UnsignedLDT(updateIndex[indexIndex++], 0);
				}
			}
		}
		return newData;
		}
		catch(Exception e)
		{
			e.printStackTrace();
			return newData;
		}
		
		
	}
	

	private static int UnsignedLDT(byte num, int Displacement)
	{
		if(num>=0)
			return num<<Displacement;
		int temp = 256 + num;
		return temp<<Displacement;
	}
}
