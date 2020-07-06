using AltV.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ParkedVehicleSpawner.utils
{
    class FileLoader
    {
        public static TDumpType LoadDataFromJsonFile<TDumpType>(string dumpFileName)
            where TDumpType : new()
        {
            TDumpType dumpResult = default;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dumpFileName);
            Alt.Log("Search File " + filePath);
            if (!File.Exists(filePath))
            {
                Alt.Log($"Could not find dump file at {filePath}");
                return default;
            }

            try
            {
                dumpResult = JsonConvert.DeserializeObject<TDumpType>(File.ReadAllText(filePath));
                Console.WriteLine($"Successfully loaded dump file {dumpFileName}.");
            }
            catch (Exception e)
            {
                Alt.Log($"Failed loading dump: {e}");
            }

            return dumpResult;
        }
    }
}
