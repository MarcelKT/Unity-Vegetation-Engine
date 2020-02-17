using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets {
    // Just some maths functions I find useful
    static class Maths {
        // Linear interpolation (mix two values by "amount")
        public static float mix(float v1, float v2, float amount) {
            return ((1.0f - amount) * v1) + (amount * v2);
        }

        // Same as above, but with Vector3s
        public static Vector3 mix(Vector3 v1, Vector3 v2, float amount) {
            Vector3 output = new Vector3();

            output += ((1.0f - amount) * v1);
            output += amount * v2;

            return output;
        }
        
        // Same as above, but with Vector4s
        public static Vector4 mix(Vector4 v1, Vector4 v2, float amount) {
            Vector4 output = new Vector4();

            output += ((1.0f - amount) * v1);
            output += amount * v2;

            return output;
        }

        // Transform a 3D array position to 1D
        // (this is useful when working with a 1D array that's meant to represent 3D space,
        // as in 3D[width][height][depth], 1D[width * height * depth]
        public static int To1D(int x, int y, int z, int height, int depth) {
            return z + (y * depth) + (x * depth * height);
        }

        // A quick function to grab a sub-array from a bigger array
        public static T[] SubArray<T>(this T[] data, int index, int length) {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
