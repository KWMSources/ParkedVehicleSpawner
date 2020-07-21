using System;
using System.Collections.Generic;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ParkedVehicleSpawner.model;
using ParkedVehicleSpawner.utils;

namespace ParkedVehicleSpawner
{
    public class ParkedVehicleSpawner: Resource
    {
        //All Parking Spots
        private List<CarGeneratorItem> carGeneratorItems = new List<CarGeneratorItem>();

        //All Cars on which a vehicle was spawned
        private List<Vector3> carGeneratorItemsSpawns = new List<Vector3>();

        //Vehicle Models in the PopGroups
        private Dictionary<String, String[]> popGroup = new Dictionary<string, string[]>();

        //Vehicles without a random color
        private List<String> colorlessCars = new List<string>();

        //Vehicle colors (if empty the color will be randomly generated)
        private List<JsonRGBA> colorsRGB = new List<JsonRGBA>();

        //Vehicle colors (if empty the color will be randomly generated)
        private List<int> colorsNum = new List<int>();

        //Spawn count of random cars
        private int ToSpawnCount = 3000;

        //Load all JSONs
        public void Init()
        {
            this.carGeneratorItems = FileLoader.LoadDataFromJsonFile<List<CarGeneratorItem>>("resources/ParkedVehicleSpawner/CarGenerators.json");
            this.popGroup = FileLoader.LoadDataFromJsonFile<Dictionary<String, String[]>>("resources/ParkedVehicleSpawner/PopGroup.json");
            this.colorlessCars = FileLoader.LoadDataFromJsonFile<List<String>>("resources/ParkedVehicleSpawner/ColorlessCars.json");
            this.colorsRGB = FileLoader.LoadDataFromJsonFile<List<JsonRGBA>>("resources/ParkedVehicleSpawner/CarColorsRGBs.json");
            this.colorsNum = FileLoader.LoadDataFromJsonFile<List<int>>("resources/ParkedVehicleSpawner/CarColorsNum.json");
        }

        private string[] NumberPlateChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public string GenerateNumberPlate()
        {
            Random randomizer = new Random();

            string numberplate = "";

            for (int i = 0; i < 8; i++) numberplate += NumberPlateChars[randomizer.Next(0, NumberPlateChars.Length - 1)];

            return numberplate;
        }

        public void SpawnParkedVehicles()
        {
            Random randomizer = new Random();

            int randomValueMax = Convert.ToInt32(this.carGeneratorItems.Count / this.ToSpawnCount), randomValue;
            int counter = 0;

            //Randomly spawn vehicles all over the map (and not just on one spot)
            while (randomValueMax * counter < carGeneratorItems.Count)
            {
                //Select a random navMesh in this section
                randomValue = randomizer.Next(0, randomValueMax);

                CarGeneratorItem carGeneratorItem = carGeneratorItems[counter * randomValue];

                //Flag over 5000 will spawn in the vehicle on the middle of the street and not on a parking spot
                if (carGeneratorItem.Flags > 5000) continue;

                //Let vehicles spawn not on the same spot, but on different spots
                Vector3 spawnTest = new Vector3((float)Math.Ceiling(carGeneratorItem.Position.X / 5), (float)Math.Ceiling(carGeneratorItem.Position.Y / 5), (float)Math.Ceiling(carGeneratorItem.Position.Z / 5));
                if (carGeneratorItemsSpawns.Contains(spawnTest)) continue;
                carGeneratorItemsSpawns.Add(spawnTest);

                //Load the vehicle model to spawn
                string model = "";
                if (carGeneratorItem.CarModel != "") model = carGeneratorItem.CarModel;
                else if (carGeneratorItem.PopGroup != "") model = popGroup[carGeneratorItem.PopGroup][randomizer.Next(0, popGroup[carGeneratorItem.PopGroup].Length-1)];
                else model = popGroup["none"][randomizer.Next(0, popGroup["none"].Length - 1)];

                //Create the vehicle
                IVehicle vehicle = Alt.CreateVehicle(
                    model,
                    new Position(carGeneratorItem.Position.X, carGeneratorItem.Position.Y, carGeneratorItem.Position.Z+2),
                    new Rotation(0, 0, 0 -
                        (float)Math.Atan2(
                         Convert.ToDouble(carGeneratorItem.OrientX),
                         Convert.ToDouble(carGeneratorItem.OrientY)
                        )
                    )
                );
                vehicle.SetStreamSyncedMetaData("OrientY", Convert.ToDouble(carGeneratorItem.OrientY));
                vehicle.SetStreamSyncedMetaData("OrientX", Convert.ToDouble(carGeneratorItem.OrientX));

                //Set the vehicle color
                if (!this.colorlessCars.Contains(model)) {
                    if (colorsNum.Count != 0)
                    {
                        byte color = (byte)colorsNum[randomizer.Next(0, colorsNum.Count - 1)];
                        vehicle.PrimaryColor = color;
                        vehicle.SecondaryColor = color;
                    }
                    else if (colorsRGB.Count == 0)
                    {
                        Rgba color = new Rgba((byte)randomizer.Next(0, 255), (byte)randomizer.Next(0, 255), (byte)randomizer.Next(0, 255), 1);
                        vehicle.PrimaryColorRgb = color;
                        vehicle.SecondaryColorRgb = color;
                    }
                    else
                    {
                        Rgba color = colorsRGB[randomizer.Next(0, colorsRGB.Count - 1)].parse();
                        vehicle.PrimaryColorRgb = color;
                        vehicle.SecondaryColorRgb = color;
                    }
                }

                vehicle.NumberplateText = GenerateNumberPlate();

                counter++;
            }

            Alt.Log("[INFO] ParkedVehicleSpawner spawned " + carGeneratorItemsSpawns.Count + " vehicles");
        }

        public void SetRandomParkedVehicleCount(int Count)
        {
            if (Count > carGeneratorItems.Count) this.ToSpawnCount = carGeneratorItems.Count;
            else this.ToSpawnCount = Count;
        }

        public override void OnStart()
        {
            this.Init();

            Alt.Export("SetRandomParkedVehicleCount", new Action<int>(this.SetRandomParkedVehicleCount));
            Alt.Export("SpawnParkedVehicles", new Action(this.SpawnParkedVehicles));
        }

        public override void OnStop()
        {
        }
    }
}
