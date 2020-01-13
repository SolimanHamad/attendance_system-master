using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AttendanceSystem.Extensions
{
    public static class StringsExtensions
    {
        /// <summary>
        /// Evaluates weather a string is null or empty string
        /// </summary>
        /// <param name="s">string to be checked</param>
        /// <returns>true if the string is null or empty,false otherwise</returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// Converts a string from pascal or camel case to sentence or normal text case
        /// </summary>
        /// <param name="str">string to convert</param>
        /// <returns>sentence or normal text case</returns>
        public static string ToSentenceCase(this string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }
        
        /// <summary>
        /// converts a string to its MD5 hash
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>MD5 hash of a string</returns>
        public static string ToMD5Hash(this string input)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashedBytes = MD5.Create().ComputeHash(asciiBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Converts a string array to single string 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Join(this IEnumerable<string> input, char seperator)
        {
            return string.Join(seperator, input);
        }
    }
}
