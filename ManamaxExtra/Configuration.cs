using System;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace ManamaxExtra
{
    public class Configuration
    {
        // Lokasi file konfigurasi akan disimpan di folder TShock.
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "ManamaxExtra.json");

        // Nilai maksimum mana default (bawaan Terraria biasanya 200)
        public int DefaultManaMax = 200;

        // Nilai maksimum mana yang diinginkan (misalnya, 300 atau lebih)
        public int CustomManaMax = 300;

        /// <summary>
        /// Menulis konfigurasi ke file dengan format JSON.
        /// </summary>
        /// <param name="path">Lokasi file konfigurasi.</param>
        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var sw = new StreamWriter(fs))
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                sw.Write(json);
            }
        }

        /// <summary>
        /// Membaca konfigurasi dari file. Jika file tidak ada, mengembalikan konfigurasi default.
        /// </summary>
        /// <param name="path">Lokasi file konfigurasi.</param>
        /// <returns>Instance dari Configuration.</returns>
        public static Configuration Read(string path)
        {
            if (!File.Exists(path))
                return new Configuration();

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sr = new StreamReader(fs))
            {
                string json = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Configuration>(json);
            }
        }
    }
}
