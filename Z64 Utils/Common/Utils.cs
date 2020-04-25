﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public static class Utils
    {
        public static T ToObject<T>(this JsonElement element)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json);
        }

        public static int BomSwap(int a) => (a << 24) | ((a & 0xFF00) << 8) | ((a & 0xFF0000) >> 8) | (a >> 24);
        public static uint BomSwap(uint a) => (a << 24) | ((a & 0xFF00) << 8) | ((a & 0xFF0000) >> 8) | (a >> 24);

        public static string BytesToHex(byte[] data, string separator = " ")
        {
            return BitConverter.ToString(data).Replace("-", separator);
        }
        public static byte[] HexToBytes(string hex)
        {
            hex = hex.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }
        public static bool IsValidHex(string hex)
        {
            hex = hex.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
            if (hex.Length % 2 != 0)
                return false;
            return Regex.IsMatch(hex, "^[0-9a-fA-F]*$");
        }

        public static List<int> FindData(byte[] data, byte[] pattern, int align = 1)
        {
            if (align < 1)
                align = 1;

            List<int> indices = new List<int>();
            for (int i = 0; i < data.Length; i+= align)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (pattern[j] != data[i+j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    indices.Add(i);
            }

            return indices;
        }

        public static bool IsValidFileName(string name)
        {
            foreach (var c in name)
            {
                if (Path.GetInvalidFileNameChars().Contains(c))
                    return false;
            }
            return true;
        }

    }


    public class BitFlag<T>
        where T : Enum
    {
        public T Value { get; set; }

        public override string ToString()
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            string ret = "";
            int count = 0;
            foreach (var v in values)
            {
                if (Value.HasFlag(v))
                {
                    var name = Enum.GetName(typeof(T), v);
                    if (count == 0) ret = name;
                    else ret += $" | {name}";
                    count++;
                }
            }
            if (count == 0)
                return Enum.GetName(typeof(T), 0);

            return ret;
        }

        public BitFlag(T v)
        {
            Value = v;
        }
    }
}
