using UnityEngine;
using System.Collections;
using LitJson;
using System.Text;   //使用StringBuilder
using System.IO;     //使用文件流

public class JsonHelper : MonoBehaviour
{
    public static string GetJson()
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter writer = new JsonWriter(sb);

        bool mode = GameManager.Instance.mode;
        int totalMoves = GameManager.Instance.totalMoves;
        int[,] chessBoard = GameManager.Instance.chessBoard;
        MapData map = new MapData();
        map.mode = mode;
        map.totalMoves = totalMoves;
        map.chessBoard = new int[3 * 3];
        string str = "";    // 打印用
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                map.chessBoard[i * 3 + j] = chessBoard[i, j];
                str = str + " " + chessBoard[i, j].ToString();
            }
        }
        writer.WriteObjectStart();//字典开始
        writer.WritePropertyName("Map");  // 记录宽度
        writer.Write(JsonMapper.ToJson(map));
        writer.WriteObjectEnd();

        return sb.ToString();  //返回Json格式的字符串
    }

    //保存Json格式字符串
    public static void SaveJsonString(string JsonString)
    {
        FileInfo file = new FileInfo(Application.persistentDataPath + "/JsonData.Json");
        StreamWriter writer = file.CreateText();
        writer.Write(JsonString);
        writer.Close();
        writer.Dispose();
    }

    //从文件里面读取json数据
    public static string GetJsonString()
    {
        try
        {
            StreamReader reader = new StreamReader(Application.persistentDataPath + "/JsonData.Json");
            string jsonData = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            return jsonData;
        }
        catch
        {
            return "";
        }
    }
}

public class MapData
{
    public bool mode;
    public int totalMoves;
    public int[] chessBoard;
}