using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleZip
{
    public class Example : MonoBehaviour
    {
        public Text Text;

        /// <summary>
        /// Usage example
        /// </summary>
        public void Start()
        {
            var sample = "El perro de San Roque no tiene rabo porque Ramón Rodríguez se lo ha robado.";
            var compressed = Zip.CompressToString(sample);
            var decompressed = Zip.Decompress(compressed);

            Text.text = $"Plain text: {sample}\n\nCompressed: {compressed}\n\nDecompressed: {decompressed}";

            Directory.CreateDirectory("Assets/Temp");
            Zip.CompressDirectory("Assets/SimpleZip", "Assets/Temp/SimpleZip.zip");
            Zip.CompressFile("Assets/SimpleZip/Readme.txt", "Assets/Temp/Readme.zip");
            Zip.DecompressArchive("Assets/Temp/Readme.zip", "Assets/Temp/");
        }
    }
}