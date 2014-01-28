using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class VHBinaryParser
{
    #region Variables
    BinaryReader binReader = null;
    #endregion

    #region Functions
    public VHBinaryParser()    { }

    public VHBinaryParser(string filename)
    {
        //FileStream inStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
        //long nBytesToRead = inStream.Length;

        //inStream.re
        Open(filename);
    }

    public void Open(string fileName)
    {
#if !UNITY_WEBPLAYER
        if (binReader != null)
        {
            Close();
        }

        if (File.Exists(fileName))
        {
            try
            {
                binReader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read));
            }
            catch (System.Exception e)
            {
                Debug.LogError("VHBinaryParser Error: " + e.Message);
            }
            finally
            {
            }
        }
        else
        {
            Debug.LogError("File " + fileName + " doesn't exist");
        }
#endif
    }

    public void Close()
    {
        if (binReader != null)
        {
            binReader.Close();
            binReader = null;
        }
    }

    public int ReadInt32()
    {
        return binReader.ReadInt32();
        //int retval = binReader.ReadInt32();
        //byte[] data = BitConverter.GetBytes(retval);
        //Array.Reverse(data);
        //return BitConverter.ToInt32(data, 0);
    }
    #endregion

}
