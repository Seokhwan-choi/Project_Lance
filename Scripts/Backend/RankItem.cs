using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;


namespace Lance
{
    public class RankItem
    {
        public string gamerInDate { get; private set; }
        public string nickname { get; private set; }
        public string score { get; private set; }
        public string index { get; private set; }
        public string rank { get; private set; }
        public string extraData { get; private set; }
        public string tableName { get; private set; }

        public RankItem(JsonData gameDataJson, string table, string extraDataColumnName)
        {
            tableName = table;

            gamerInDate = gameDataJson["gamerInDate"].ToString();
            nickname = gameDataJson.ContainsKey("nickname") ? gameDataJson["nickname"].ToString() : "-";
            score = gameDataJson["score"].ToString();
            index = gameDataJson["index"].ToString();
            rank = gameDataJson["rank"].ToString();

            if (!string.IsNullOrEmpty(extraDataColumnName) && gameDataJson.ContainsKey(extraDataColumnName))
            {
                extraData = gameDataJson[extraDataColumnName].ToString();
            }
        }
    }
}
