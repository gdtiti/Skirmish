﻿using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Helper functions
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Transform NDC space [-1,+1]^2 to texture space [0,1]^2
        /// </summary>
        private static readonly Matrix ndcTransform = new Matrix(
            0.5f, 0.0f, 0.0f, 0.0f,
            0.0f, -0.5f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.0f, 1.0f);

        public const uint PRIMEX = 0x8da6b343;
        public const uint PRIMEY = 0xd8163841;
        /// <summary>
        /// One radian
        /// </summary>
        public const float RADIAN = 0.0174532924f;

        /// <summary>
        /// Random number generator
        /// </summary>
        public static Random RandomGenerator = new Random();

        /// <summary>
        /// Serializes the specified object to XML file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        public static void SerializeToFile<T>(T obj, string fileName, string nameSpace = null)
        {
            using (StreamWriter wr = new StreamWriter(fileName, false, Encoding.Default))
            {
                XmlSerializer sr = new XmlSerializer(typeof(T), nameSpace);

                sr.Serialize(wr, obj);
            }
        }
        /// <summary>
        /// Deserializes an object from a XML file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeFromFile<T>(string fileName, string nameSpace = null)
        {
            using (StreamReader rd = new StreamReader(fileName, Encoding.Default))
            {
                XmlSerializer sr = new XmlSerializer(typeof(T), nameSpace);

                return (T)sr.Deserialize(rd);
            }
        }
        /// <summary>
        /// Generate an array initialized to defaultValue
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(int length, T defaultValue) where T : struct
        {
            T[] array = new T[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = defaultValue;
            }

            return array;
        }
        /// <summary>
        /// Generate an array initialized to function result
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="func">Function</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(int length, Func<T> func)
        {
            T[] array = new T[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = func.Invoke();
            }

            return array;
        }
        /// <summary>
        /// Merge two arrays
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="array1">First array</param>
        /// <param name="array2">Second array</param>
        /// <returns>Returns an array with both array values merged</returns>
        public static T[] Merge<T>(this T[] array1, T[] array2)
        {
            T[] newArray = new T[array1.Length + array2.Length];

            array1.CopyTo(newArray, 0);
            array2.CopyTo(newArray, array1.Length);

            return newArray;
        }
        /// <summary>
        /// Concatenates the members of a collection of type T, using the specified separator between each member.
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="list">Collection</param>
        /// <param name="separator">The string to use as a separator</param>
        /// <returns>A string that consists of the members of values delimited by the separator string</returns>
        public static string Join<T>(this ICollection<T> list, string separator = "")
        {
            List<string> res = new List<string>();

            list.ToList().ForEach(a => res.Add(a.ToString()));

            return string.Join(separator, res);
        }
        /// <summary>
        /// Performs distinc selection over the result of the provided function
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TKey">Function result type</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="getKey">Selection function</param>
        /// <returns>Returns a collection of distinct function results</returns>
        public static IEnumerable<TKey> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey)
        {
            Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>();

            foreach (TSource item in source)
            {
                TKey key = getKey(item);

                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, item);
                }
            }

            return dictionary.Select(item => item.Key);
        }
        /// <summary>
        /// Performs the specified action on each element of the System.Collections.Generic.IEnumerable<T>.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="enumeration">The enumeration instance</param>
        /// <param name="action">The System.Action`1 delegate to perform on each element of the System.Collections.Generic.IEnumerable<T>.</param>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
        /// <summary>
        /// Retrieves all the elements that match the conditions defined by the specified predicate
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="enumeration"></param>
        /// <param name="match">The System.Predicate delegate that defines the conditions of the elements to search for</param>
        /// <returns>A list containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty list</returns>
        public static List<T> FindAll<T>(this IEnumerable<T> enumeration, Predicate<T> match)
        {
            List<T> res = new List<T>();

            foreach (T item in enumeration)
            {
                if (match.Invoke(item))
                {
                    res.Add(item);
                }
            }

            return res;
        }
        /// <summary>
        /// Gets the internal items of the KeyCollection into a list
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="keys">Key collection</param>
        /// <returns>Returns the internal items of the KeyCollection into a list</returns>
        public static IList<TKey> ToList<TKey, TValue>(this Dictionary<TKey, TValue>.KeyCollection keys)
        {
            List<TKey> res = new List<TKey>(keys.Count);

            foreach (var key in keys)
            {
                res.Add(key);
            }

            return res;
        }
        /// <summary>
        /// Compares two enumerable lists, element by element
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enum1">First list</param>
        /// <param name="enum2">Second list</param>
        /// <returns>Returns true if both list are equal</returns>
        public static bool ListIsEqual<T>(this IEnumerable<T> enum1, IEnumerable<T> enum2)
        {
            if (enum1 == null && enum2 == null)
            {
                return true;
            }
            else if (enum1 != null && enum2 != null)
            {
                var list1 = enum1.ToList();
                var list2 = enum2.ToList();

                if (list1.Count == list2.Count)
                {
                    if (list1.Count > 0)
                    {
                        bool equatable = list1[0] is IEquatable<T>;

                        for (int i = 0; i < list1.Count; i++)
                        {
                            bool equal = false;

                            if (equatable)
                            {
                                equal = ((IEquatable<T>)list1[i]).Equals(list2[i]);
                            }
                            else
                            {
                                equal = list1[i].Equals(list2[i]);
                            }

                            if (!equal) return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets the internal items of the KeyCollection into an array
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="keys">Key collection</param>
        /// <returns>Returns the internal items of the KeyCollection into an array</returns>
        public static TKey[] ToArray<TKey, TValue>(this Dictionary<TKey, TValue>.KeyCollection keys)
        {
            TKey[] res = new TKey[keys.Count];

            int index = 0;
            foreach (var key in keys)
            {
                res[index++] = key;
            }

            return res;
        }
        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="obj">Object</param>
        public static void Dispose(Object obj)
        {
            Dispose((IDisposable)obj);
        }
        /// <summary>
        /// Dispose disposable object
        /// </summary>
        /// <param name="obj">Disposable object</param>
        public static void Dispose(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
            }
        }
        /// <summary>
        /// Dispose objects array
        /// </summary>
        /// <param name="array">Objects array</param>
        public static void Dispose(IEnumerable<Object> array)
        {
            if (array != null && array.Count() > 0)
            {
                foreach (var item in array)
                {
                    if (item is IDisposable)
                    {
                        Helper.Dispose((IDisposable)item);
                    }
                }
            }
        }
        /// <summary>
        /// Dispose disposable objects array
        /// </summary>
        /// <param name="array">Disposable objects array</param>
        public static void Dispose(IEnumerable<IDisposable> array)
        {
            if (array != null && array.Count() > 0)
            {
                foreach (var item in array)
                {
                    Helper.Dispose(item);
                }
            }
        }
        /// <summary>
        /// Dispose disposable objects dictionary
        /// </summary>
        /// <param name="dictionary">Disposable objects dictionary</param>
        public static void Dispose(IDictionary dictionary)
        {
            if (dictionary != null && dictionary.Count > 0)
            {
                foreach (var item in dictionary.Values)
                {
                    if (item is IDisposable)
                    {
                        Helper.Dispose((IDisposable)item);
                    }
                    else if (item is IEnumerable<IDisposable>)
                    {
                        Helper.Dispose((IEnumerable<IDisposable>)item);
                    }
                    else if (item is IDictionary)
                    {
                        Helper.Dispose((IDictionary)item);
                    }
                }

                dictionary.Clear();
            }
        }
        /// <summary>
        /// Writes stream to memory
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns a memory stream</returns>
        public static MemoryStream WriteToMemory(this Stream stream)
        {
            MemoryStream ms = new MemoryStream();

            stream.CopyTo(ms);

            ms.Position = 0;

            return ms;
        }
        /// <summary>
        /// Writes file to memory
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns a memory stream</returns>
        public static MemoryStream WriteToMemory(this string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return stream.WriteToMemory();
            }
        }
        /// <summary>
        /// Create the md5 sum string of the specified buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Returns the md5 sum string of the specified buffer</returns>
        public static string GetMd5Sum(this byte[] buffer)
        {
            byte[] result = null;
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                result = md5.ComputeHash(buffer);
            }

            StringBuilder sb = new StringBuilder();
            Array.ForEach(result, r => sb.Append(r.ToString("X2")));
            return sb.ToString();
        }
        /// <summary>
        /// Create the md5 sum string of the specified string
        /// </summary>
        /// <param name="content">String</param>
        /// <returns>Returns the md5 sum string of the specified string</returns>
        public static string GetMd5Sum(this string content)
        {
            byte[] tmp = new byte[content.Length * 2];
            Encoding.Unicode.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, tmp, 0, true);

            return tmp.GetMd5Sum();
        }
        /// <summary>
        /// Create the md5 sum string of the specified string list
        /// </summary>
        /// <param name="content">String list</param>
        /// <returns>Returns the md5 sum string of the specified string</returns>
        public static string GetMd5Sum(this string[] content)
        {
            string md5 = null;
            Array.ForEach(content, p => md5 += p.GetMd5Sum());

            return md5;
        }
        /// <summary>
        /// Create the md5 sum string of the specified stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the md5 sum string of the specified stream</returns>
        public static string GetMd5Sum(this MemoryStream stream)
        {
            return stream.ToArray().GetMd5Sum();
        }
        /// <summary>
        /// Create the md5 sum string of the specified stream list
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <returns>Returns the md5 sum string of the specified stream list</returns>
        public static string GetMd5Sum(this MemoryStream[] streams)
        {
            string md5 = null;
            Array.ForEach(streams, p => md5 += p.GetMd5Sum());

            return md5;
        }
        /// <summary>
        /// Hash function for Vector2
        /// </summary>
        /// <param name="x">The x-coordinate</param>
        /// <param name="y">The y-coordinate</param>
        /// <param name="n">Total size of hash table</param>
        /// <returns>A hash value</returns>
        public static int HashVector2(int x, int y, int n)
        {
            return ((x * 73856093) ^ (y * 19349663)) & (n - 1);
        }
        /// <summary>
        /// Gets the maximum value of the collection 
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="array">Array of parameters</param>
        /// <returns>Return the maximum value of the collection</returns>
        public static T Max<T>(params T[] array) where T : IComparable<T>
        {
            T res = default(T);

            for (int i = 0; i < array.Length; i++)
            {
                if (i == 0)
                {
                    res = array[i];
                }
                else if (res.CompareTo(array[i]) < 0)
                {
                    res = array[i];
                }
            }

            return res;
        }
        /// <summary>
        /// Gets the minimum value of the collection 
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="array">Array of parameters</param>
        /// <returns>Return the minimum value of the collection</returns>
        public static T Min<T>(params T[] array) where T : IComparable<T>
        {
            T res = default(T);

            for (int i = 0; i < array.Length; i++)
            {
                if (i == 0)
                {
                    res = array[i];
                }
                else if (res.CompareTo(array[i]) > 0)
                {
                    res = array[i];
                }
            }

            return res;
        }
        /// <summary>
        /// Checks if a and b are almost equals, taking into account the magnitude of floating point numbers (unlike SharpDX.MathUtil.WithinEpsilon(System.Single,System.Single,System.Single) method). See Remarks.
        /// See remarks
        /// </summary>
        /// <param name="a">The left value to compare</param>
        /// <param name="b">The right value to compare</param>
        /// <returns>true if a almost equal to b, false otherwise</returns>
        /// <remarks>The code is using the technique described by Bruce Dawson in Comparing Floating point numbers 2012 edition</remarks>
        public static bool NearEqual(Vector3 a, Vector3 b)
        {
            return
                MathUtil.NearEqual(a.X, b.X) &&
                MathUtil.NearEqual(a.Y, b.Y) &&
                MathUtil.NearEqual(a.Z, b.Z);
        }
        /// <summary>
        /// Checks if a - b are almost equals within a float epsilon.
        /// </summary>
        /// <param name="a">The left value to compare</param>
        /// <param name="b">The right value to compare</param>
        /// <param name="epsilon">Epsilon value</param>
        /// <returns>true if a almost equal to b within a float epsilon, false otherwise</returns>
        public static bool WithinEpsilon(Vector3 a, Vector3 b, float epsilon)
        {
            return
                MathUtil.WithinEpsilon(a.X, b.X, epsilon) &&
                MathUtil.WithinEpsilon(a.Y, b.Y, epsilon) &&
                MathUtil.WithinEpsilon(a.Z, b.Z, epsilon);
        }
        /// <summary>
        /// Gets the screen coordinates from the specified 3D point
        /// </summary>
        /// <param name="point">3D point</param>
        /// <param name="viewPort">View port</param>
        /// <param name="wvp">World * View * Projection</param>
        /// <param name="isInsideScreen">Returns true if the resulting point is inside the screen</param>
        /// <returns>Returns the resulting screen coordinates</returns>
        public static Vector2 UnprojectToScreen(Vector3 point, ViewportF viewPort, Matrix wvp, out bool isInsideScreen)
        {
            isInsideScreen = true;

            // Go to projection space
            Vector4 projected;
            Vector3.Transform(ref point, ref wvp, out projected);

            // Clip
            // 
            //  -Wp < Xp <= Wp 
            //  -Wp < Yp <= Wp 
            //  0 < Zp <= Wp 
            // 
            if (projected.X < -projected.W)
            {
                projected.X = -projected.W;
                isInsideScreen = false;
            }
            if (projected.X > projected.W)
            {
                projected.X = projected.W;
                isInsideScreen = false;
            }
            if (projected.Y < -projected.W)
            {
                projected.Y = -projected.W;
                isInsideScreen = false;
            }
            if (projected.Y > projected.W)
            {
                projected.Y = projected.W;
                isInsideScreen = false;
            }
            if (projected.Z < 0)
            {
                projected.Z = 0;
                isInsideScreen = false;
            }
            if (projected.Z > projected.W)
            {
                projected.Z = projected.W;
                isInsideScreen = false;
            }

            // Divide by w, to move from homogeneous coordinates to 3D coordinates again 
            projected.X = projected.X / projected.W;
            projected.Y = projected.Y / projected.W;
            projected.Z = projected.Z / projected.W;

            // Perform the viewport scaling, to get the appropiate coordinates inside the viewport 
            projected.X = ((float)(((projected.X + 1.0) * 0.5) * viewPort.Width)) + viewPort.X;
            projected.Y = ((float)(((1.0 - projected.Y) * 0.5) * viewPort.Height)) + viewPort.Y;
            projected.Z = (projected.Z * (viewPort.MaxDepth - viewPort.MinDepth)) + viewPort.MinDepth;

            return projected.XY();
        }
        /// <summary>
        /// Project polygon using axis
        /// </summary>
        /// <param name="axis">Plane axis</param>
        /// <param name="poly">Poligon vertices</param>
        /// <param name="npoly">Vertex count</param>
        /// <param name="rmin">Minimum range</param>
        /// <param name="rmax">Maximum range</param>
        public static void ProjectPolygon(Vector3 axis, Vector3[] poly, int npoly, out float rmin, out float rmax)
        {
            Helper.Dot2D(ref axis, ref poly[0], out rmin);
            Helper.Dot2D(ref axis, ref poly[0], out rmax);
            for (int i = 1; i < npoly; i++)
            {
                float d;
                Helper.Dot2D(ref axis, ref poly[i], out d);
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }
        }
        /// <summary>
        /// Limits the vector length to specified magnitude
        /// </summary>
        /// <param name="vector">Vector to limit</param>
        /// <param name="magnitude">Magnitude</param>
        /// <returns></returns>
        public static Vector3 Limit(this Vector3 vector, float magnitude)
        {
            if (vector.Length() > magnitude)
            {
                return Vector3.Normalize(vector) * magnitude;
            }

            return vector;
        }
        /// <summary>
        /// Returns xyz components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xyz components from Vector4</returns>
        public static Vector3 XYZ(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        /// <summary>
        /// Returns xy components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xy components from Vector4</returns>
        public static Vector2 XY(this Vector4 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        /// <summary>
        /// Returns rgb components from Color4
        /// </summary>
        /// <param name="color">Color4</param>
        /// <returns>Returns rgb components from Color4</returns>
        public static Color3 RGB(this Color4 color)
        {
            return new Color3(color.Red, color.Green, color.Blue);
        }
        /// <summary>
        /// Returns rgb components from Color
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns rgb components from Color</returns>
        public static Color3 RGB(this Color color)
        {
            return color.ToColor4().RGB();
        }
        /// <summary>
        /// Gets next pair of even number, if even
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Returns next pair</returns>
        public static int NextPair(this int num)
        {
            return num = num * 0.5f != (int)(num * 0.5f) ? num + 1 : num;
        }
        /// <summary>
        /// Gets next odd of even number, if even
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Returns next odd</returns>
        public static int NextOdd(this int num)
        {
            return num = num * 0.5f != (int)(num * 0.5f) ? num : num + 1;
        }
        /// <summary>
        /// Calculates the next highest power of two.
        /// </summary>
        /// <remarks>
        /// This is a minimal method meant to be fast. There is a known edge case where an input of 0 will output 0
        /// instead of the mathematically correct value of 1. It will not be fixed.
        /// </remarks>
        /// <param name="v">A value.</param>
        /// <returns>The next power of two after the value.</returns>
        public static int NextPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return v;
        }
        /// <summary>
        /// Calculates the next highest power of two.
        /// </summary>
        /// <remarks>
        /// This is a minimal method meant to be fast. There is a known edge case where an input of 0 will output 0
        /// instead of the mathematically correct value of 1. It will not be fixed.
        /// </remarks>
        /// <param name="v">A value.</param>
        /// <returns>The next power of two after the value.</returns>
        public static uint NextPowerOfTwo(uint v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return v;
        }
        /// <summary>
        /// Swaps values
        /// </summary>
        /// <typeparam name="T">Type of values</typeparam>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        public static void SwapValues<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }
        /// <summary>
        /// Calculates the binary logarithm of the input.
        /// </summary>
        /// <remarks>
        /// Inputs 0 and below have undefined output.
        /// </remarks>
        /// <param name="v">A value.</param>
        /// <returns>The binary logarithm of v.</returns>
        public static int Log2(int v)
        {
            int r;
            int shift;

            r = (v > 0xffff) ? 1 << 4 : 0 << 4;
            v >>= r;

            shift = (v > 0xff) ? 1 << 3 : 0 << 3;
            v >>= shift;
            r |= shift;

            shift = (v > 0xf) ? 1 << 2 : 0 << 2;
            v >>= shift;
            r |= shift;

            shift = (v > 0x3) ? 1 << 1 : 0 << 1;
            v >>= shift;
            r |= shift;

            r |= v >> 1;

            return r;
        }
        /// <summary>
        /// Calculates the binary logarithm of the input.
        /// </summary>
        /// <remarks>
        /// An input of 0 has an undefined output.
        /// </remarks>
        /// <param name="v">A value.</param>
        /// <returns>The binary logarithm of v.</returns>
        public static uint Log2(uint v)
        {
            uint r;
            int shift;

            r = (uint)((v > 0xffff) ? 1 << 4 : 0 << 4);
            v >>= (int)r;

            shift = (v > 0xff) ? 1 << 3 : 0 << 3;
            v >>= shift;
            r |= (uint)shift;

            shift = (v > 0xf) ? 1 << 2 : 0 << 2;
            v >>= shift;
            r |= (uint)shift;

            shift = (v > 0x3) ? 1 << 1 : 0 << 1;
            v >>= shift;
            r |= (uint)shift;

            r |= v >> 1;

            return r;
        }
        /// <summary>
        /// Converts specified number relative to a total size and min/max magnitudes
        /// </summary>
        /// <param name="n">Number</param>
        /// <param name="size">Total size</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Returns the relative value</returns>
        public static float GetRelative(this float n, float size, float min, float max)
        {
            float f = size / (max - min);

            return (n + (max - min) - max) * f;
        }
        /// <summary>
        /// Converts specified vector relative to a total size and min/max magnitudes
        /// </summary>
        /// <param name="v">Vector</param>
        /// <param name="size">Total size</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Returns the relative value</returns>
        public static Vector2 GetRelative(this Vector2 v, Vector2 size, Vector2 min, Vector2 max)
        {
            return new Vector2(
                v.X.GetRelative(size.X, min.X, max.X),
                v.Y.GetRelative(size.Y, min.Y, max.Y));
        }
        /// <summary>
        /// Converts specified vector relative to a total size and min/max magnitudes
        /// </summary>
        /// <param name="v">Vector</param>
        /// <param name="size">Total size</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Returns the relative value</returns>
        public static Vector3 GetRelative(this Vector3 v, Vector3 size, Vector3 min, Vector3 max)
        {
            return new Vector3(
                v.X.GetRelative(size.X, min.X, max.X),
                v.Y.GetRelative(size.Y, min.Y, max.Y),
                v.Z.GetRelative(size.Z, min.Z, max.Z));
        }
        /// <summary>
        /// Maps n into start and stop pairs
        /// </summary>
        /// <param name="n">Value to map</param>
        /// <param name="start1">Start reference 1</param>
        /// <param name="stop1">Stop reference 1</param>
        /// <param name="start2">Start reference 2</param>
        /// <param name="stop2">Stop reference 2</param>
        /// <returns>Returns mapped value of n</returns>
        public static float Map(this float n, float start1, float stop1, float start2, float stop2)
        {
            return ((n - start1) / (stop1 - start1)) * (stop2 - start2) + start2;
        }
        /// <summary>
		/// Normalizes a value in a specified range to be between 0 and 1.
		/// </summary>
		/// <param name="t">The value</param>
		/// <param name="t0">The lower bound of the range.</param>
		/// <param name="t1">The upper bound of the range.</param>
		/// <returns>A normalized value.</returns>
		public static float Normalize(float t, float t0, float t1)
        {
            return MathUtil.Clamp((t - t0) / (t1 - t0), 0.0f, 1.0f);
        }
        /// <summary>
        /// Gets if specified ranges overlaps within epsilon
        /// </summary>
        /// <param name="amin">Maximum A range</param>
        /// <param name="amax">Minimum A range</param>
        /// <param name="bmin">Maximum B range</param>
        /// <param name="bmax">Minimum B range</param>
        /// <param name="eps">Epsilon</param>
        /// <returns>Returns true if specified ranges overlaps within epsilon</returns>
        public static bool OverlapRange(float amin, float amax, float bmin, float bmax, float eps)
        {
            return ((amin + eps) > bmax || (amax - eps) < bmin) ? false : true;
        }
        /// <summary>
        /// Gets angle between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns angle value in radians</returns>
        public static float Angle(Vector2 one, Vector2 two)
        {
            //Get the dot product
            float dot = Vector2.Dot(one, two);

            // Divide the dot by the product of the magnitudes of the vectors
            dot = dot / (one.Length() * two.Length());

            //Get the arc cosin of the angle, you now have your angle in radians 
            return (float)Math.Acos(dot);
        }
        /// <summary>
        /// Gets angle between two quaternions
        /// </summary>
        /// <param name="one">First quaternions</param>
        /// <param name="two">Second quaternions</param>
        /// <returns>Returns angle value in radians</returns>
        public static float Angle(Quaternion one, Quaternion two)
        {
            float dot = Quaternion.Dot(one, two);

            return (float)Math.Acos(Math.Min(Math.Abs(dot), 1f)) * 2f;
        }
        /// <summary>
        /// Finds the quaternion between from and to quaternions traveling maxDelta radians
        /// </summary>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        /// <param name="maxDelta">Maximum radians</param>
        /// <returns>Gets the quaternion between from and to quaternions traveling maxDelta radians</returns>
        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDelta)
        {
            float angle = Helper.Angle(from, to);
            if (angle == 0f)
            {
                return to;
            }

            float delta = Math.Min(1f, maxDelta / angle);

            return Quaternion.Slerp(from, to, delta);
        }
        /// <summary>
        /// Gets angle between two vectors with sign
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns angle value</returns>
        public static float AngleSigned(Vector2 one, Vector2 two)
        {
            return (float)Math.Atan2(Cross(one, two), Vector2.Dot(one, two)) * MathUtil.Pi;
        }
        /// <summary>
        /// Performs cross product between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns the cross product</returns>
        public static float Cross(Vector2 one, Vector2 two)
        {
            return one.X * two.Y - one.Y * two.X;
        }
        /// <summary>
        /// Gets angle between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns angle value</returns>
        public static float Angle(Vector3 one, Vector3 two)
        {
            //Get the dot product
            float dot = Vector3.Dot(one, two);

            // Divide the dot by the product of the magnitudes of the vectors
            dot = dot / (one.Length() * two.Length());

            //Get the arc cosin of the angle, you now have your angle in radians 
            return (float)Math.Acos(dot);
        }
        /// <summary>
        /// Gets angle between two vectors in the same plane
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <param name="planeNormal">Plane normal</param>
        /// <returns>Returns angle value</returns>
        /// <remarks>Result signed</remarks>
        public static float Angle(Vector3 one, Vector3 two, Vector3 planeNormal)
        {
            Plane p = new Plane(planeNormal, 0);

            float dot = MathUtil.Clamp(Vector3.Dot(Vector3.Normalize(one), Vector3.Normalize(two)), 0, 1);

            float angle = (float)Math.Acos(dot);

            Vector3 cross = Vector3.Cross(one, two);

            if (Vector3.Dot(p.Normal, cross) > 0)
            {
                angle = -angle;
            }

            return angle;
        }
        /// <summary>
        /// Gets yaw and pitch values from vector
        /// </summary>
        /// <param name="vec">Vector</param>
        /// <param name="yaw">Yaw</param>
        /// <param name="pitch">Pitch</param>
        public static void GetAnglesFromVector(Vector3 vec, out float yaw, out float pitch)
        {
            yaw = (float)Math.Atan2(vec.X, vec.Y);

            if (yaw < 0.0f)
            {
                yaw += MathUtil.TwoPi;
            }

            if (Math.Abs(vec.X) > Math.Abs(vec.Y))
            {
                pitch = (float)Math.Atan2(Math.Abs(vec.Z), Math.Abs(vec.X));
            }
            else
            {
                pitch = (float)Math.Atan2(Math.Abs(vec.Z), Math.Abs(vec.Y));
            }

            if (vec.Z < 0.0f)
            {
                pitch = -pitch;
            }
        }
        /// <summary>
        /// Get vector from yaw and pitch angles
        /// </summary>
        /// <param name="yaw">Yaw angle</param>
        /// <param name="pitch">Pitch angle</param>
        /// <param name="vec">Vector</param>
        public static void GetVectorFromAngles(float yaw, float pitch, out Vector3 vec)
        {
            Quaternion rot = Quaternion.RotationYawPitchRoll(-yaw, pitch, 0.0f);
            Matrix mat = Matrix.RotationQuaternion(rot);

            vec = Vector3.TransformCoordinate(Vector3.Up, mat);
        }
        /// <summary>
        /// Gets the area of the triangle projected onto the XZ-plane.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <param name="area">The calculated area.</param>
        public static void Area2D(ref Vector3 a, ref Vector3 b, ref Vector3 c, out float area)
        {
            float abx = b.X - a.X;
            float abz = b.Z - a.Z;
            float acx = c.X - a.X;
            float acz = c.Z - a.Z;
            area = acx * abz - abx * acz;
        }
        /// <summary>
        /// Gets the area of the triangle projected onto the XZ-plane.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <returns>The calculated area.</returns>
        public static float Area2D(Vector3 a, Vector3 b, Vector3 c)
        {
            float area;
            Area2D(ref a, ref b, ref c, out area);
            return area;
        }
        /// <summary>
        /// Gets total distance between point list
        /// </summary>
        /// <param name="points">Point list</param>
        /// <returns>Returns total distance between point list</returns>
        public static float Distance(params Vector3[] points)
        {
            float length = 0;

            Vector3 p0 = points[0];

            for (int i = 1; i < points.Length; i++)
            {
                Vector3 p1 = points[i];

                length += Vector3.Distance(p0, p1);

                p0 = p1;
            }

            return length;
        }
        /// <summary>
        /// Calculates the distance between two points on the XZ plane.
        /// </summary>
        /// <param name="a">A point.</param>
        /// <param name="b">Another point.</param>
        /// <returns>The distance between the two points.</returns>
        public static float Distance2D(Vector3 a, Vector3 b)
        {
            float result;
            Distance2D(ref a, ref b, out result);
            return result;
        }
        /// <summary>
        /// Calculates the distance between two points on the XZ plane.
        /// </summary>
        /// <param name="a">A point.</param>
        /// <param name="b">Another point.</param>
        /// <param name="dist">The distance between the two points.</param>
        public static void Distance2D(ref Vector3 a, ref Vector3 b, out float dist)
        {
            float dx = b.X - a.X;
            float dz = b.Z - a.Z;
            dist = (float)Math.Sqrt(dx * dx + dz * dz);
        }
        /// <summary>
        /// Calculates the dot product of two vectors projected onto the XZ plane.
        /// </summary>
        /// <param name="left">A vector.</param>
        /// <param name="right">Another vector</param>
        /// <param name="result">The dot product of the two vectors.</param>
        public static void Dot2D(ref Vector3 left, ref Vector3 right, out float result)
        {
            result = left.X * right.X + left.Z * right.Z;
        }
        /// <summary>
        /// Calculates the dot product of two vectors projected onto the XZ plane.
        /// </summary>
        /// <param name="left">A vector.</param>
        /// <param name="right">Another vector</param>
        /// <returns>The dot product</returns>
        public static float Dot2D(ref Vector3 left, ref Vector3 right)
        {
            return left.X * right.X + left.Z * right.Z;
        }
        /// <summary>
		/// Calculates the perpendicular dot product of two vectors projected onto the XZ plane.
		/// </summary>
		/// <param name="left">A vector.</param>
		/// <param name="right">Another vector.</param>
		/// <returns>The perpendicular dot product on the XZ plane.</returns>
		public static float PerpendicularDotXZ(ref Vector3 left, ref Vector3 right)
        {
            return left.X * right.Z - left.Z * right.X;
        }
        /// <summary>
        /// Calculates the cross product of two vectors (formed from three points)
        /// </summary>
        /// <param name="p1">The first point</param>
        /// <param name="p2">The second point</param>
        /// <param name="p3">The third point</param>
        /// <returns>The 2d cross product</returns>
        public static float Cross2D(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float result;
            Cross2D(ref p1, ref p2, ref p3, out result);
            return result;
        }
        /// <summary>
        /// Calculates the cross product of two vectors (formed from three points)
        /// </summary>
        /// <param name="p1">The first point</param>
        /// <param name="p2">The second point</param>
        /// <param name="p3">The third point</param>
        /// <param name="result">The 2d cross product</param>
        public static void Cross2D(ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, out float result)
        {
            float u1 = p2.X - p1.X;
            float v1 = p2.Z - p1.Z;
            float u2 = p3.X - p1.X;
            float v2 = p3.Z - p1.Z;

            result = u1 * v2 - v1 * u2;
        }
        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="target">Target</param>
        /// <param name="yAxisOnly">Restricts the rotation axis to Y only</param>
        /// <returns>Returns rotation quaternion</returns>
        public static Quaternion LookAt(Vector3 eyePosition, Vector3 target, bool yAxisOnly = true)
        {
            Vector3 up = Vector3.Up;

            return LookAt(eyePosition, target, up, yAxisOnly);
        }
        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="target">Target</param>
        /// <param name="up">Up vector</param>
        /// <param name="yAxisOnly">Restricts the rotation axis to Y only</param>
        /// <returns>Returns rotation quaternion</returns>
        public static Quaternion LookAt(Vector3 eyePosition, Vector3 target, Vector3 up, bool yAxisOnly = true)
        {
            if (Vector3.Dot(Vector3.Up, Vector3.Normalize(eyePosition - target)) == 1f)
            {
                up = Vector3.Left;
            }

            Quaternion q = Quaternion.Invert(Quaternion.LookAtLH(target, eyePosition, up));

            if (yAxisOnly)
            {
                q.X = 0;
                q.Z = 0;

                q.Normalize();
            }

            return q;
        }
        /// <summary>
        /// Set transform to normal device coordinates
        /// </summary>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Projection matrix</param>
        /// <returns>Returns NDC matrix</returns>
        public static Matrix NormalDeviceCoordinatesTransform(Matrix view, Matrix projection)
        {
            return view * projection * ndcTransform;
        }
        /// <summary>
        /// Creates a new world Matrix
        /// </summary>
        /// <param name="position">The position vector.</param>
        /// <param name="forward">The forward direction vector.</param>
        /// <param name="up">The upward direction vector. Usually <see cref="Vector3.Up"/>.</param>
        /// <returns>The world Matrix</returns>
        public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
        {
            Matrix result = new Matrix();

            Vector3 x, y, z;
            Vector3.Normalize(ref forward, out z);
            Vector3.Cross(ref forward, ref up, out x);
            Vector3.Cross(ref x, ref forward, out y);
            x.Normalize();
            y.Normalize();

            result.Right = x;
            result.Up = y;
            result.Forward = z;
            result.TranslationVector = position;
            result.M44 = 1f;

            return result;
        }
        /// <summary>
        /// Gets the bounding box center
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the center of the current bounding box</returns>
        public static Vector3 GetCenter(this BoundingBox bbox)
        {
            return (bbox.Minimum + bbox.Maximum) * 0.5f;
        }
        /// <summary>
        /// Gets the x maganitude of the current bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the x maganitude of the current bounding box</returns>
        public static float GetX(this BoundingBox bbox)
        {
            return bbox.Maximum.X - bbox.Minimum.X;
        }
        /// <summary>
        /// Gets the y maganitude of the current bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the y maganitude of the current bounding box</returns>
        public static float GetY(this BoundingBox bbox)
        {
            return bbox.Maximum.Y - bbox.Minimum.Y;
        }
        /// <summary>
        /// Gets the z maganitude of the current bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns the z maganitude of the current bounding box</returns>
        public static float GetZ(this BoundingBox bbox)
        {
            return bbox.Maximum.Z - bbox.Minimum.Z;
        }
        /// <summary>
        /// Offset for the x-coordinate
        /// </summary>
        /// <param name="i">Starting number</param>
        /// <returns>A new offset</returns>
        public static float GetJitterX(int i)
        {
            return (((i * PRIMEX) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }
        /// <summary>
        /// Offset for the y-coordinate
        /// </summary>
        /// <param name="i">Starting number</param>
        /// <returns>A new offset</returns>
        public static float GetJitterY(int i)
        {
            return (((i * PRIMEY) & 0xffff) / 65535.0f * 2.0f) - 1.0f;
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Vector3 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Vector4 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z) || float.IsNaN(vector.W);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Color4 color)
        {
            return float.IsNaN(color.Red) || float.IsNaN(color.Green) || float.IsNaN(color.Blue) || float.IsNaN(color.Alpha);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector3 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y) || float.IsInfinity(vector.Z);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector4 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y) || float.IsInfinity(vector.Z) || float.IsInfinity(vector.W);
        }
        /// <summary>
        /// Gets wheter almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Color4 color)
        {
            return float.IsInfinity(color.Red) || float.IsInfinity(color.Green) || float.IsInfinity(color.Blue) || float.IsInfinity(color.Alpha);
        }
        /// <summary>
        /// Gets first normal texture size for the specified pixel count
        /// </summary>
        /// <param name="pixelCount">Pixel count</param>
        /// <returns>Returns the texture size</returns>
        public static int GetTextureSize(int pixelCount)
        {
            int texWidth = (int)Math.Sqrt((float)pixelCount) + 1;
            int texHeight = 1;
            while (texHeight < texWidth)
            {
                texHeight = texHeight << 1;
            }

            return texHeight;
        }

        /// <summary>
        /// Gets matrix description
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <returns>Return matrix description</returns>
        public static string GetDescription(this Matrix matrix)
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            if (matrix.Decompose(out scale, out rotation, out translation))
            {
                return string.Format("S:{0,-30} T:{1,-30} R:{2,-30}",
                    scale.GetDescription(Vector3.One, "None"),
                    translation.GetDescription(Vector3.Zero, "Zero"),
                    rotation.GetDescription());
            }
            else
            {
                return "Bad transform matrix";
            }
        }
        /// <summary>
        /// Gets quaternion description
        /// </summary>
        /// <param name="quaternion">Quaternion</param>
        /// <returns>Return quaternion description</returns>
        public static string GetDescription(this Quaternion quaternion)
        {
            Vector3 axis = quaternion.Axis;
            float angle = quaternion.Angle;

            return string.Format("Angle: {0:0.00} in axis {1}", angle, axis.GetDescription(Vector3.One, "None"));
        }
        /// <summary>
        /// Gets vector description
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <param name="none">Sets the vector who means wath</param>
        /// <param name="wath">Sets the string to write in description when the specified vector is near equal to none</param>
        /// <returns>Return vector description</returns>
        public static string GetDescription(this Vector3 vector, Vector3 none, string wath)
        {
            vector.X = (float)Math.Round(vector.X, 3);
            vector.Y = (float)Math.Round(vector.Y, 3);
            vector.Z = (float)Math.Round(vector.Z, 3);

            return string.Format("X:{0:0.000}; Y:{1:0.000}; Z:{2:0.000}", vector.X, vector.Y, vector.Z);
        }
        /// <summary>
        /// Gets matrix list description
        /// </summary>
        /// <param name="list">Matrix list</param>
        /// <returns>Return matrix list description</returns>
        public static string GetDescription(this Matrix[] list)
        {
            string desc = "";

            for (int i = 0; i < list.Length; i++)
            {
                desc += list[i].GetDescription() + Environment.NewLine;
            }

            return desc;
        }
        /// <summary>
        /// Gets matrix debug text
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <returns>Return matrix debug text</returns>
        public static string Debug(this Matrix matrix)
        {
            return string.Format(
                "11: {0:0.000} 12: {1:0.000} 13: {2:0.000} 14: {3:0.000}" + Environment.NewLine +
                "21: {4:0.000} 22: {5:0.000} 23: {6:0.000} 24: {7:0.000}" + Environment.NewLine +
                "31: {8:0.000} 32: {9:0.000} 33: {10:0.000} 34: {11:0.000}" + Environment.NewLine +
                "41: {12:0.000} 42: {13:0.000} 43: {14:0.000} 44: {15:0.000}" + Environment.NewLine,
                (float)Math.Round(matrix.M11, 3),
                (float)Math.Round(matrix.M12, 3),
                (float)Math.Round(matrix.M13, 3),
                (float)Math.Round(matrix.M14, 3),
                (float)Math.Round(matrix.M21, 3),
                (float)Math.Round(matrix.M22, 3),
                (float)Math.Round(matrix.M23, 3),
                (float)Math.Round(matrix.M24, 3),
                (float)Math.Round(matrix.M31, 3),
                (float)Math.Round(matrix.M32, 3),
                (float)Math.Round(matrix.M33, 3),
                (float)Math.Round(matrix.M34, 3),
                (float)Math.Round(matrix.M41, 3),
                (float)Math.Round(matrix.M42, 3),
                (float)Math.Round(matrix.M43, 3),
                (float)Math.Round(matrix.M44, 3));
        }
        /// <summary>
        /// Gets matrix list debug text
        /// </summary>
        /// <param name="list">Matrix list</param>
        /// <returns>Return matrix list debug text</returns>
        public static string Debug(this Matrix[] list)
        {
            string desc = "";

            for (int i = 0; i < list.Length; i++)
            {
                desc += list[i].Debug() + Environment.NewLine;
            }

            return desc;
        }
    }
}
