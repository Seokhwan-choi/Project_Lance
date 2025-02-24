using System;
using System.Collections.Generic;
using System.Linq;
using ExcelToObject;
using System.IO;
using UnityEngine;

namespace Lance
{
	public class GameDataReader
	{
		public static IEnumerable<byte[]> GetGameDataBytes(string baseFolder)
		{
			if (baseFolder[baseFolder.Length - 1] != '\\')
				baseFolder = baseFolder + '\\';

			string[] xlsxList = SearchFiles(baseFolder, "*.xlsx");

			return xlsxList.Select(x => ReadBytes(baseFolder, x));
		}

		public static GameData ReadGameData(string baseFolder)
		{
			bool isSuccess = true;
			string errorInfo = string.Empty;

			GameData gameData = new GameData();

            try
            {
#if UNITY_EDITOR
				IEnumerable<byte[]> dataBytes = GetGameDataBytes(baseFolder);

				var tableList = new TableList(dataBytes);

				tableList.MapInto(gameData);
#else
			TextAsset text = Resources.Load("DataSheet/SavedDataSheet") as TextAsset;
			if (text != null)
			{
				Debug.Log("정상적으로 파싱 성공");

				gameData = JsonUtil.Base64FromJsonNewton<GameData>(text.text);
			}
#endif
			}
			catch (Exception e)
            {
				isSuccess = false;

				errorInfo = e.ToString();
			}
            finally
            {
				if(errorInfo.IsValid())
					Debug.LogError(errorInfo);
			}

			return gameData;
		}

		static byte[] ReadBytes(string baseFolder, string relativePath)
		{
			string path = Path.Combine(baseFolder, relativePath);

			using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				var bytes = new byte[(int)fs.Length];
				int read = fs.Read(bytes, 0, bytes.Length);

				if (read == bytes.Length)
				{
					return bytes;
				}
			}

			throw new Exception("Failed to read data file : " + path);
		}

		static string[] SearchFiles(string baseFolder, string pattern)
		{
			return Directory.GetFiles(baseFolder, pattern, SearchOption.AllDirectories)
							.Select(x => x.StartsWith(baseFolder) ? x.Substring(baseFolder.Length) : x)
							.Where(x => Path.GetFileName(x)[0] != '~')
							.ToArray();
		}
	}
}