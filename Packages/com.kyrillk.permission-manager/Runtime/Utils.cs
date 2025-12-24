using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace PermissionSystem
{
    public static class Utils
    {
        /// <summary>
        /// Adds a string to a string array
        /// </summary>
        /// <param name="array">The source array</param>
        /// <param name="value">The value to add</param>
        /// <returns>A new array with the value added</returns>
        public static string[] AddToStringArray(string[] array, string value)
        {
            string[] newArray;
            if (array == null)
            {
                newArray = new string[1];
                newArray[0] = value;
            }
            else
            {
                newArray = new string[array.Length + 1];
                for (int i = 0; i < array.Length; i++)
                {
                    newArray[i] = array[i];
                }
                newArray[array.Length] = value;
            }
            return newArray;
        }

        /// <summary>
        /// Removes a specific string from a string array
        /// </summary>
        /// <param name="array">The source array</param>
        /// <param name="value">The value to remove</param>
        /// <returns>A new array without the specified value</returns>
        public static string[] RemoveFromStringArray(string[] array, string value)
        {
            if (array == null || array.Length == 0) return array;

            int removeIndex = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    removeIndex = i;
                    break;
                }
            }

            if (removeIndex == -1) return array;

            string[] newArray = new string[array.Length - 1];
            int newIndex = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (i != removeIndex)
                {
                    newArray[newIndex] = array[i];
                    newIndex++;
                }
            }

            return newArray;
        }

        /// <summary>
        /// Checks if a string array contains a specific value
        /// </summary>
        public static bool ContainsString(string[] array, string value)
        {
            if (array == null || array.Length == 0) return false;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        public static PermissionContainerBase[] AddToPermissionContainerBaseArray(PermissionContainerBase[] array, PermissionContainerBase value)
        {
            PermissionContainerBase[] newArray;
            if (array == null)
            {
                newArray = new PermissionContainerBase[1];
                newArray[0] = value;
            }
            else
            {
                newArray = new PermissionContainerBase[array.Length + 1];
                for (int i = 0; i < array.Length; i++)
                {
                    newArray[i] = array[i];
                }
                newArray[array.Length] = value;
            }
            return newArray;
        }

        public static PermissionContainerBase[] RemoveFromPermissionContainerBaseArray(PermissionContainerBase[] array, PermissionContainerBase value)
        {
            if (array == null || array.Length == 0) return array;

            int removeIndex = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    removeIndex = i;
                    break;
                }
            }

            if (removeIndex == -1) return array;

            PermissionContainerBase[] newArray = new PermissionContainerBase[array.Length - 1];
            int newIndex = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (i != removeIndex)
                {
                    newArray[newIndex] = array[i];
                    newIndex++;
                }
            }

            return newArray;
        }

        public static bool ContainsPermissionContainerBase(PermissionContainerBase[] array, PermissionContainerBase value)
        {
            if (array == null || array.Length == 0) return false;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        public static PermissionContainer[] mergePermissionContainerBaseArrays(PermissionContainer[] array1, PermissionContainer[] array2)
        {
            if (array1 == null || array1.Length == 0) return array2;
            if (array2 == null || array2.Length == 0) return array1;

            PermissionContainer[] mergedArray = new PermissionContainer[array1.Length + array2.Length];
            int index = 0;

            for (int i = 0; i < array1.Length; i++)
            {
                mergedArray[index] = array1[i];
                index++;
            }

            for (int j = 0; j < array2.Length; j++)
            {
                mergedArray[index] = array2[j];
                index++;
            }

            return mergedArray;
        }
    }
}
