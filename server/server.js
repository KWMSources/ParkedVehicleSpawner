import * as alt from "alt-server";
import vehicleGenerators from "../CarGenerators.json";
import vehiclePopGroups from "../PopGroup.json";
import colorlessVehicles from "../ColorlessCars.json";
import vehicleColors from "../CarColorsRGBs.json";

const ALLOWED_FLAGS = [1568, 1632, 3616, 3680, 3681];

/**
 * All created vehicles
 *
 * @type {Array<alt.Vehicle>}
 * @static
 * @memberof ParkedVehicleSpawner
 */
const _vehicles = [];
/**
 * All taken spots
 *
 * @type {Array<alt.Vector3>}
 * @static
 * @memberof ParkedVehicleSpawner
 */
const _takenSpots = [];

/**
 * Creates the specified amount of parked vehicles
 *
 * @author LeonMrBonnie
 * @static
 * @param {number} [amount=1000]
 * @memberof ParkedVehicleSpawner
 */
export function spawn(amount = 1000) {
    let vehicleGens = vehicleGenerators.filter((generator) => {
        if (generator.Flags > 5000 || !ALLOWED_FLAGS.includes(generator.Flags))
            return false;
        return true;
    });

    for (
        let i = 0, random, item, model, vehicle, notFound = 0;
        i < amount;
        i++
    ) {
        if (notFound >= vehicleGens.length) break; // * There are no available spots anymore, cancel the loop
        random = getRandomInt(0, vehicleGens.length - 1);
        item = vehicleGens[random]; // * Get a random parking spot from the JSON data

        let spawnTest = new alt.Vector3(
            Math.ceil(item.Position.X / 5),
            Math.ceil(item.Position.Y / 5),
            Math.ceil(item.Position.Z / 5)
        );
        if (
            !!_takenSpots.find(
                (spot) =>
                    spot.x === spawnTest.x &&
                    spot.y === spawnTest.y &&
                    spot.z === spawnTest.z
            )
        ) {
            // * The spot is already taken, try again
            notFound++;
            i--;
            continue;
        }
        _takenSpots.push(spawnTest);

        // * Determine the correct vehicle model
        if (item.CarModel !== "") model = item.CarModel;
        else if (item.PopGroup !== "")
            model =
                vehiclePopGroups[item.PopGroup][
                    getRandomInt(0, vehiclePopGroups[item.PopGroup].length - 1)
                ];
        else
            model =
                vehiclePopGroups["none"][
                    getRandomInt(0, vehiclePopGroups["none"].length - 1)
                ];

        // * Create the vehicle
        vehicle = new alt.Vehicle(
            model,
            item.Position.X,
            item.Position.Y,
            item.Position.Z + 0.2,
            0,
            0,
            0 - Math.atan2(parseInt(item.OrientX), parseInt(item.OrientY))
        );
        vehicle.setStreamSyncedMeta("OrientX", item.OrientX);
        vehicle.setStreamSyncedMeta("OrientY", item.OrientY);

        // * Set the vehicle color
        if (!colorlessVehicles.includes(model)) {
            if (vehicleColors.length === 0)
                vehicle.customPrimaryColor = new alt.RGBA(
                    getRandomInt(0, 255),
                    getRandomInt(0, 255),
                    getRandomInt(0, 255),
                    1
                );
            else {
                let color =
                    vehicleColors[getRandomInt(0, vehicleColors.length - 1)];
                vehicle.customPrimaryColor = new alt.RGBA(
                    color.r,
                    color.g,
                    color.b,
                    color.a
                );
            }
        }

        vehicleGens.splice(random, 1);
        _vehicles.push(vehicle);
        notFound = 0;
    }

    alt.log(`Created ${_vehicles.length} generated vehicles`);
}
/**
 * Removes all existing parked vehicles
 *
 * @author LeonMrBonnie
 * @static
 * @memberof ParkedVehicleSpawner
 */
export function clear() {
    let vehs = Array.from(_vehicles);
    _vehicles = [];
    vehs.forEach((veh) => veh.destroy());
}

function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min;
}
