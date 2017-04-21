using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game;
using UnityEngine;
using Random = System.Random;

namespace Miscellaneous
{
    public enum DimensionType
    {
        YZ,
        ZX,
        XZ,
        ZY
    }

    public static class ExtensionMethods
    {
        private static Random random = new Random();

        // Add a range of enumerable items to a hashSet
        public static bool AddRange<T>(this HashSet<T> collection, IEnumerable<T> items)
        {
            bool allAdded = true;

            foreach (T item in items)
                allAdded &= collection.Add(item);

            return allAdded;
        }

        // Shuffle any sort of listed collection
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // Check if an array contains certain data
        public static bool Contains<T>(T[] array, params T[] values)
        {
            bool hasAllValues = true;

            foreach (T value in values)
                hasAllValues &= array.Contains(value);

            return hasAllValues;
        }

        // Get the relative of a vector based upon the given direction, will only expand by 1
        public static Vector2 GetRelative(this Vector2 vector, DirectionType direction)
        {
            return GetRelative(vector, direction, 1);
        }

        // Get the relative of a vector based upon the given direction, will expand by distance
        public static Vector2 GetRelative(this Vector2 vector, DirectionType direction, float distance)
        {
            switch (direction)
            {
                case DirectionType.Up:
                    vector.y += distance;
                    break;
                case DirectionType.Right:
                    vector.x += distance;
                    break;
                case DirectionType.Down:
                    vector.y -= distance;
                    break;
                case DirectionType.Left:
                    vector.x -= distance;
                    break;
            }

            return vector;
        }

        // Convert a Vector2 to a Vector3 with the Y axis of value
        public static Vector3 ToVector3(this Vector2 vector, float value)
        {
            return vector.ToVector3(DimensionType.XZ, value);
        }

        // Convert a Vector2 to a Vector3 with the values on the given dimensions
        public static Vector3 ToVector3(this Vector2 vector, DimensionType dimension, float value)
        {
            switch (dimension)
            {
                case DimensionType.YZ:
                    return new Vector3(value, vector.x, vector.y);
                case DimensionType.ZX:
                    return new Vector3(vector.y, value, vector.x);
                case DimensionType.XZ:
                    return new Vector3(vector.x, value, vector.y);
                case DimensionType.ZY:
                    return new Vector3(value, vector.y, vector.x);
                default:
                    return new Vector3(vector.x, value, vector.y);
            }
        }

        public static bool IsBetween<T>(this T actual, T lower, T upper) where T : IComparable<T>
        {
            return actual.CompareTo(lower) > 0 && actual.CompareTo(upper) < 0;
        }

        public static bool EqualsIgnoreCase(this string text, string compareTo)
        {
            return text.Equals(compareTo, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsEmpty(this string text)
        {
            return string.IsNullOrEmpty(text) || text.Trim().Length == 0;
        }

        public static bool ContainsBetweenAxes(this Vector2 vector, float value)
        {
            return vector.x <= value && vector.y >= value;
        }

        // Get the angle of a direction, default angle will be 0
        public static float GetAngle(this DirectionType direction)
        {
            switch (direction)
            {
                case DirectionType.Right:
                    return 90;
                case DirectionType.Down:
                    return 180;
                case DirectionType.Left:
                    return 270;
                default:
                    return 0;
            }
        }

        // Get the shortest rotate distance between the original and target player direction in either a linear or circular motion
        public static int GetShortestRotateDistance(this DirectionType originalDirection, DirectionType targetDirection)
        {
            List<DirectionType> allDirections = new List<DirectionType>(((DirectionType[])Enum.GetValues(typeof(DirectionType))));

            return allDirections.GetShortestIndexDistance(originalDirection, targetDirection);
        }

        // Get the shortest index distance between two objects in a collection, either in a linear or circular motion
        // Clarification: It continues counting the distance in a circular motion at default index 0 when it reached the last index
        public static int GetShortestIndexDistance<T>(this IList<T> listWithAllObjects, T firstObject, T secondObject)
        {
            int firstIndex = listWithAllObjects.IndexOf(firstObject);
            int secondIndex = listWithAllObjects.IndexOf(secondObject);

            // This counts the difference between the two given index arguments and makes it absolute since it could become a negative counted difference
            int absoluteDifference = Mathf.Abs(firstIndex - secondIndex);

            // This counts the difference from the highest given index argument to the default index 0 (e.g., 2->3->0 = 2)
            int initialFurtherCountedDifference = listWithAllObjects.Count - Mathf.Max(firstIndex, secondIndex);
            // This counts even further from default index 0 to the lowest given index argument (e.g., 2 + 0->1 = 3)
            int totalFurtherCountedDifference = initialFurtherCountedDifference + Mathf.Min(firstIndex, secondIndex);

            // This returns the lowest difference as the quickest distance to the target index
            return Mathf.Min(absoluteDifference, totalFurtherCountedDifference);
        }

        [Flags]
        public enum AllowedFlagsData : byte
        {
            MinOne = 1 << 0,
            MaxOne = 1 << 1,
            OnlyOne = MinOne | MaxOne
        }

        /// <summary>
        /// This validates the flags of the specified enum data by the specified flags validation data rules
        /// and can either throw an exception or return the result boolean
        /// </summary>
        /// <param name="enumData">The enum data to validate</param>
        /// <param name="flagsValidationData">The flags validation data which tells what is allowed</param>
        /// <param name="canThrowException">Throws an exception if the enum data is invalid</param>
        /// <returns>True if the enum data is invalid, false if otherwise</returns>
        public static bool ValidateFlags(this Enum enumData, AllowedFlagsData flagsValidationData, bool canThrowException)
        {
            int rawEnumData = Convert.ToInt32(enumData); // Contains the decimal value instead of the enum type data

            // The decimal sum of all valid flags
            int flagsSum = 0;
            foreach (byte flag in Enum.GetValues(enumData.GetType()))
            {
                // This allows single flags but not combinations
                if ((flag & (flag - 1)) == 0)
                    flagsSum += flag;
            }

            // Check if the enumData is defined at all
            if (rawEnumData > flagsSum)
            {
                if (canThrowException)
                    throw new ArgumentException(string.Format
                    (
                        "The {0} argument is not defined! Highest possible flag combination is {1}!",
                        enumData.GetType(), Enum.ToObject(enumData.GetType(), flagsSum)
                    ));

                return true;
            }

            // The default flag(0) does not count as a valid flag
            bool hasMinOneFlag = rawEnumData != 0;

            // E.g.  AnswerData.C               = 4 OR 0100 (in binary)
            // and   AnswerData.C - 1           = 3 OR 0011 OR (AnswerData.B = 0010 | AnswerData.A = 0001)
            // so    0100 & 0011 (bitwise AND)  = 0 OR 0000 because AnswerData.C is not part of (AnswerData.B | AnswerData.A) and thus has no multiple flags
            // while 0011 & (0011 - 1 = 0010)   = 2 OR 0010 because (AnswerData.B | AnswerData.A) contains AnswerData.B and thus has multiple flags (check individual bits)
            bool hasMaxOneFlag = (rawEnumData & (rawEnumData - 1)) == 0;

            switch (flagsValidationData)
            {
                case AllowedFlagsData.MinOne: // Multiplicity: 1..*
                    if (!hasMinOneFlag)
                    {
                        if (canThrowException)
                            throw new ArgumentException(string.Format("The {0} argument requires minimal one valid {0} enum flag!", enumData.GetType()));

                        return true;
                    }
                    break;
                case AllowedFlagsData.MaxOne: // Multiplicity: 0..1
                    if (!hasMaxOneFlag)
                    {
                        if (canThrowException)
                            throw new ArgumentException(string.Format("The {0} argument requires maximal one valid {0} enum flag!", enumData.GetType()));

                        return true;
                    }
                    break;
                case AllowedFlagsData.OnlyOne: // Multiplicity: 1..1
                    if (!hasMinOneFlag || !hasMaxOneFlag)
                    {
                        if (canThrowException)
                            throw new ArgumentException(string.Format("The {0} argument requires only one valid {0} enum flag!", enumData.GetType()));

                        return true;
                    }
                    break;
            }

            return false;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        // This will return a string of the specified Color object including purple, orange and the different yellow of RGBA: 1.0f, 1.0f, 0.0f, 1.0f
        public static string ToName(this Color color)
        {
            // Some missing colors as well as the correct yellow Color object (standard yellow is RGBA: 1.0f, 0.9215686f, 0.01568628f, 1.0f)
            Dictionary<string, Color> validColors = new Dictionary<string, Color>
            {
                { "yellow", new Color(1.0f, 1.0f, 0.0f, 1.0f) },
                { "purple", new Color(1.0f, 0.0f, 1.0f, 1.0f) },
                { "orange", new Color(1.0f, 128.0f / 255.0f, 0.0f, 1.0f) }
            };

            // We simply return the valid color name if it equals the specified color
            foreach (KeyValuePair<string, Color> keyValueValidColor in validColors)
                if (keyValueValidColor.Value.Equals(color))
                    return keyValueValidColor.Key;

            try
            {
                // Same as above but here we check against reflected color properties
                foreach (PropertyInfo colorProperty in typeof(Color).GetProperties())
                    if (colorProperty.GetValue(typeof(Color), null).Equals(color))
                        return colorProperty.Name;
            }
            catch (Exception)
            {
                // We don't need any reflection related exceptions, so keep this blank
            }

            // If nothing had been found
            return null;
        }
    }
}