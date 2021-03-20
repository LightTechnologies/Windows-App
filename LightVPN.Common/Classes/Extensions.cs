/* --------------------------------------------
 * 
 * Extension methods - Main class
 * Copyright (C) Light Technologies LLC
 * 
 * File: Extensions.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */
using Newtonsoft.Json.Linq;
using System;

namespace LightVPN.Common
{
    public static class Extensions
    {
        /// <summary>
        /// Checks if the input string is valid Json data using JToken
        /// </summary>
        /// <param name="strInput">The input string</param>
        /// <returns>True or false value whether the string was valid Json or not</returns>
        public static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            try
            {
                var obj = JToken.Parse(strInput);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
