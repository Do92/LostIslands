using System;

namespace Miscellaneous
{
    public class ArrayUtilities
    {
        // Flatten a 2D array to a 1D array, can be useful for sending arrays over a network
        public static T[] FlattenArray<T>(T[,] array)
        {
            T[] flatArray = new T[array.GetLength(0) * array.GetLength(1)];
            int width = array.GetLength(1);

            for (int y = 0; y < array.GetLength(0); y++)
                for (int x = 0; x < array.GetLength(1); x++)
                    flatArray[y * width + x] = array[y, x];

            return flatArray;
        }

        // Unflatten a 1D to a 2D array, will require a with of the 0 dimension
        public static T[,] UnflatArray<T>(T[] array, int width)
        {
            T[,] unflatArray = new T[array.Length / width, width];

            for (int y = 0; y < unflatArray.GetLength(0); y++)
                for (int x = 0; x < unflatArray.GetLength(1); x++)
                    unflatArray[y, x] = array[y * width + x];

            return unflatArray;
        }

        // Convert an enum array to an int array, is useful for sending data over a network
        public static int[] ToIntArray<T>(T[] array) where T : struct
        {
            int[] convertedArray = new int[array.Length];

            for (int i = 0; i < array.Length; i++)
                convertedArray[i] = (int)(ValueType)array[i];

            return convertedArray;
        }

        // Convert an int array to a enum array
        public static T[] ToEnumArray<T>(int[] array)
        {
            T[] convertedArray = new T[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                Type genericType = typeof(T);
                convertedArray[i] = ((T[])Enum.GetValues(genericType))[array[i]];
            }

            return convertedArray;
        }
    }
}