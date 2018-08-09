using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/**<summary> File and other I/O related functions </summary>*/
public class FileIO {

    /**<summary> Get byte[] of a file </summary>*/
    public static byte[] GetFile(string path)
    {
        FileInfo fileInfo = new FileInfo(path);
        byte[] data = new byte[fileInfo.Length];
        using (FileStream fs = fileInfo.OpenRead())
        {
            fs.Read(data, 0, data.Length);
        }
        return data;
    }

    /**<summary> Convert an object to a byte array </summary>*/
    public static byte[] ObjectToByteArray(object obj)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, obj);
            return stream.ToArray();
        }
    }

    /**<summary> Convert a byte array to an object</summary>*/
    public static object ByteArrayToObject(byte[] bytes)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(stream);
        }
    }
}
