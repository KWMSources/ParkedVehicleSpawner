using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ParkedVehicleSpawner.model
{
    class CarGeneratorItem
    {
        public Vector3 Position { set; get; }

        public float OrientX { set; get; }

        public float OrientY { set; get; }

        public float PerpendicularLength { set; get; }

        public string CarModel { set; get; }

        public BigInteger Flags { set; get; }

        public string PopGroup { set; get; }
    }
}
