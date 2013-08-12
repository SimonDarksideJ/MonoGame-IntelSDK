using Microsoft.Xna.Framework;

namespace SimpleLib.Helpers
{
    public class Vector3SmoothingSampler
    {
        private int index = 0;		//current reading
        private Vector3 total = Vector3.Zero;		//running total
        private Vector3 smoothedValue = Vector3.Zero;	//the average of sensor values
        private readonly Vector3[] readings;	//initialize array to hold sensor data
        private int numReadings = 0;    //total number of readings in the array

        public Vector3SmoothingSampler(int numSamples)
        {
            readings = new Vector3[numSamples];
        }

        // calculate and return the value of the smoothing algorithim
        public Vector3 GetValue(Vector3 newValue)
        {
            if (numReadings == readings.Length)
            {
                //subtract last reading
                total.X = total.X - readings[index].X;
                total.Y = total.Y - readings[index].Y;
                total.Z = total.Z - readings[index].Z;
            }

            //inc numReadings to length of readings array
            if (numReadings < readings.Length)
            {
                numReadings++;
            }

            //read from sensor
            readings[index] = newValue;

            //add reading to total
                total.X += readings[index].X;
                total.Y += readings[index].Y;
                total.Z += readings[index].Z;

            //advance to next index in array
            index++;

            //if we reach end of array, wrap around
            if (index >= readings.Length)
            {
                index = 0;
            }
            //If this is the first time it is run just return the input
            if (readings.Length == 1)
            {
                smoothedValue = newValue;
            }
            else
            {
                //calculate the average:
                for (int i = 0; i < numReadings; i++)
                {
                    smoothedValue.X = total.X / numReadings;
                    smoothedValue.Y = total.Y / numReadings;
                    smoothedValue.Z = total.Z / numReadings;
                }
            }

            //return the smoothed value
            return smoothedValue;
        }
    }

    public class PXCMPoint3DF32SmoothingSampler
    {
        private int index = 0;		//current reading
        private PXCMPoint3DF32 total = new PXCMPoint3DF32();		//running total
        private PXCMPoint3DF32 smoothedValue;	//the average of sensor values
        private readonly PXCMPoint3DF32[] readings;	//initialize array to hold sensor data
        private int numReadings = 0;    //total number of readings in the array

        public PXCMPoint3DF32SmoothingSampler(int numSamples)
        {
            readings = new PXCMPoint3DF32[numSamples];
        }

        // calculate and return the value of the smoothing algorithim
        public PXCMPoint3DF32 GetValue(PXCMPoint3DF32 newValue)
        {
            if (numReadings == readings.Length)
            {
                //subtract last reading
                total.x = total.x - readings[index].x;
                total.y = total.y - readings[index].y;
                total.z = total.z - readings[index].z;
            }

            //inc numReadings to length of readings array
            if (numReadings < readings.Length)
            {
                numReadings++;
            }

            //read from sensor
            readings[index] = newValue;

            //add reading to total
            total.x += readings[index].x;
            total.y += readings[index].y;
            total.z += readings[index].z;

            //advance to next index in array
            index++;

            //if we reach end of array, wrap around
            if (index >= readings.Length)
            {
                index = 0;
            }
            //If this is the first time it is run just return the input
            if (readings.Length == 1)
            {
                smoothedValue = newValue;
            }
            else
            {
                //calculate the average:
                for (int i = 0; i < numReadings; i++)
                {
                    smoothedValue.x = total.x / numReadings;
                    smoothedValue.y = total.y / numReadings;
                    smoothedValue.z = total.z / numReadings;
                }
            }

            //return the smoothed value
            return smoothedValue;
        }
    }

}
