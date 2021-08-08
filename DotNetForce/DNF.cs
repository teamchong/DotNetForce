using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetForce
{
    public static class Dnf
    {
        //public const string CSV_NULL = "#N/A";
        public const string StandardPriceBook = "Standard Price Book";

        public const int CompositeLimit = 25;
        public const int CompositeQueryLimit = 5;

        public const int BatchLimit = 25;

        public const int DefaultConcurrentLimit = 50;

        public const int QueryCursorLimit = 25;

        public const int QueryLocatorLimit = 10;

        public const int SoqlMaxLength = 20000;

        public static string? EscapeDataString(string? uri)
        {
            return uri == null ? null : string.Join("", EnumerableChunk.Create(uri, 65519).Select(c => Uri.EscapeDataString(new string(c.ToArray()))));
        }

        public static IEnumerable<string> ChunkIds(IEnumerable<string> source, string soql, string template)
        {
            soql = soql.Trim().Replace("\r\n", "\n");
            // var soqlMaxLen = 20000;
            var nonTemplateLength = soql.Replace(template, "").Length;
            // var idsTextLen = soqlMaxLen - nonTemplateLength;
            var idsTextLen = SoqlMaxLength - nonTemplateLength;
            var numOfTemplate = (int)Math.Ceiling((soql.Length - nonTemplateLength) / (double)template.Length);
            var numOfId = (int)Math.Max(1, Math.Floor(idsTextLen / (18.0 * numOfTemplate)));
            return EnumerableChunk.Create(source, numOfId)
                .Select(l => soql.Replace(template, string.Join(",", l.Select(id => SoqlString(ToId15(id))))));
        }

        public static IEnumerable<string> ChunkSoqlByFieldValues(IEnumerable<string> source, string templateSoql, string template)
        {
            templateSoql = templateSoql.Trim().Replace("\r\n", "\n");

            var replacement = new List<string>();
            var soql = templateSoql.Replace(template, "null");

            foreach (var sourceItem in source)
            {
                var soqlTest = templateSoql.Replace(template, string.Join(",", replacement.Union(new[] { SoqlString(sourceItem) })));

                if (soqlTest.Length <= SoqlMaxLength)
                {
                    replacement.Add(SoqlString(sourceItem));
                    soql = soqlTest;
                }
                else
                {
                    yield return soql;
                    replacement = new List<string> { SoqlString(sourceItem) };
                    soql = templateSoql.Replace(template, string.Join(",", replacement));
                }
            }

            if (replacement.Count > 0) yield return soql;
        }

        // for Full DotNet Framework, please set ServicePointManager.DefaultConnectionLimit (Default is 2)
        // for safety reason, max no of concurrent api call with transactions longer than 20 seconds.

        #region DataConvertion

        public static string SoqlString(object? inputObj)
        {
            if (inputObj == null) return "null";
            var input = inputObj.ToString();
            return "'" + string.Join("\\\\",
                input.Split('\\').Select(i1 =>
                {
                    return string.Join("\\'",
                        i1.Split('\'').Select(i2 =>
                        {
                            return string.Join("\\\"",
                                i2.Split('"').Select(i3 =>
                                {
                                    return string.Join("\\f",
                                        i3.Split('\f').Select(i4 =>
                                        {
                                            return string.Join("\\b",
                                                i4.Split('\b').Select(i5 =>
                                                {
                                                    return string.Join("\\t",
                                                        i5.Split('\t').Select(i6 =>
                                                        {
                                                            return string.Join("\\r",
                                                                i6.Split('\r').Select(i7 => { return string.Join("\\n", i7.Split('\n').Select(i8 => i8)); }));
                                                        }));
                                                }));
                                        }));
                                }));
                        }));
                })) + "'";
        }

        public static string SoqlLike(string? input)
        {
            if (input == null) return "null";
            return "'" + string.Join("\\\\",
                input.Split('\\').Select(i1 =>
                {
                    return string.Join("\\'",
                        i1.Split('\'').Select(i2 =>
                        {
                            return string.Join("\\\"",
                                i2.Split('"').Select(i3 =>
                                {
                                    return string.Join("\\f",
                                        i3.Split('\f').Select(i4 =>
                                        {
                                            return string.Join("\\b",
                                                i4.Split('\b').Select(i5 =>
                                                {
                                                    return string.Join("\\t",
                                                        i5.Split('\t').Select(i6 =>
                                                        {
                                                            return string.Join("\\r",
                                                                i6.Split('\r').Select(i7 =>
                                                                {
                                                                    return string.Join("\\n",
                                                                        i7.Split('\n').Select(i8 =>
                                                                        {
                                                                            return string.Join("\\_",
                                                                                i8.Split('_').Select(i9 =>
                                                                                {
                                                                                    return string.Join("\\%", i9.Split('%').Select(i10 => i10));
                                                                                }));
                                                                        }));
                                                                }));
                                                        }));
                                                }));
                                        }));
                                }));
                        }));
                })) + "'";
        }

        public static string ApexLong(long number)
        {
            return $"{number}L";
        }

        public static string ApexDate(DateTime? date)
        {
            return date == null ? "null" : $"Date.newInstance({date.Value.Year},{date.Value.Month},{date.Value.Day})";
        }

        public static string ApexDateTime(DateTime? date)
        {
            return date == null ? "null" : $"DateTime.newInstance({(date.Value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds}L)";
        }

        public static string? SoqlDate(DateTime? date)
        {
            return date == null ? null : $"{date.Value:yyyy-MM-dd}";
        }

        public static DateTime? FromSoqlDate(string? date)
        {
            if (date == null) return null;
            return DateTime.TryParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.None, out var dateOut)
                ? dateOut
                : (DateTime?)null;
        }

        public static string? SoqlDateTime(DateTime? date)
        {
            if (date == null) return null;
            var uDate = date.Value.ToUniversalTime();
            return $@"{uDate:yyyy-MM-dd}T{uDate:HH}:{uDate:mm}:{uDate:ss}Z";
        }

        public static DateTime? FromSoqlDateTime(string? date)
        {
            if (date == null) return null;
            return DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ssZ", null, DateTimeStyles.None, out var dateOut)
                ? dateOut
                : (DateTime?)null;
        }

        public static string? SoqlTime(DateTime? date)
        {
            if (date == null) return null;
            var uDate = date.Value.ToUniversalTime();
            return $"{uDate:HH}:{uDate:mm}:{uDate:ss}Z";
        }

        public static DateTime? FromSoqlTime(string? date)
        {
            if (date == null) return null;
            return DateTime.TryParseExact(date, "HH:mm:ssZ", null, DateTimeStyles.None, out var dateOut)
                ? dateOut
                : (DateTime?)null;
        }

        public static string? SoqlDateTimeUtc(DateTime? date)
        {
            return date == null ? null : $"{date.Value:yyyy-MM-dd}T{date.Value:HH}:{date.Value:mm}:{date.Value:ss}Z";
        }

        public static string? SoqlDateTime(DateTime? date, int timezone)
        {
            if (date == null) return null;
            var tDate = date.Value.AddHours(-timezone);
            return $@"{tDate:yyyy-MM-dd}T{tDate:HH}:{tDate:mm}:{tDate:ss}Z";
        }

        public static DateTime? ToDateTime<T>(T date)
        {
            if (DateTimeOffset.TryParse(Convert.ToString(date), out var dateVal))
                return dateVal.DateTime;
            if (DateTimeOffset.TryParse(Convert.ToString(date), out dateVal))
                return dateVal.DateTime;
            return null;
        }

        public static string ToId18<T>(T idObj)
        {
            var id = Convert.ToString(idObj);
            if (id.Length != 15)
                return id;

            var triplet = new List<string>
            {
                id[..5],
                id.Substring(5, 5),
                id.Substring(10, 5)
            };
            var str = new StringBuilder(5);
            var suffix = string.Empty;
            foreach (var value in triplet)
            {
                str.Clear();
                var reverse = value.Reverse().ToList();
                reverse.ForEach(c => str.Append(char.IsUpper(c) ? "1" : "0"));
                var parsedBinary = Convert.ToInt32(str.ToString(), 2);
                suffix += (char)(parsedBinary + (parsedBinary < 26 ? 65 : 22));
            }
            return id + suffix;
        }

        public static string ToId15<T>(T idObj)
        {
            var id = Convert.ToString(idObj);
            if (id.Length != 18)
                return id;

            return id.Length > 15 ? id.Remove(15) : id;
        }

        public static async Task<T> TryDeserializeObjectAsync<T>(Task<T> obj)
        {
            try
            {
                return await obj
                    .ConfigureAwait(false);
            }
            catch (ForceException ex)
            {
                if (ex.Error == Error.NonJsonErrorResponse)
                    return JsonConvert.DeserializeObject<T>(ex.Message) ?? throw ex;
                throw;
            }
        }

        #endregion DataConvertion

        //#region Encryption

        //        public static string Encrypt(string plainText, string passPhrase, string IV)
        //        {
        //            byte[] encrypted;

        //            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        //            {
        //                aes.KeySize = 256;
        //                aes.Key = Encoding.UTF8.GetBytes(passPhrase);
        //                aes.IV = Encoding.UTF8.GetBytes(IV);
        //                aes.Mode = CipherMode.CBC;
        // ReSharper disable once CommentTypo
        //                aes.Padding = PaddingMode.PKCS7;

        // ReSharper disable once CommentTypo
        //                ICryptoTransform enc = aes.CreateEncryptor(aes.Key, aes.IV);

        //                using (MemoryStream ms = new MemoryStream())
        //                {
        //                    using (CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
        //                    {
        //                        using (StreamWriter sw = new StreamWriter(cs))
        //                        {
        //                            sw.Write(plainText);
        //                        }

        //                        encrypted = ms.ToArray();
        //                    }
        //                }
        //            }

        //            return Convert.ToBase64String(encrypted);
        //        }

        //        public static string Decrypt(string encryptedText, string passPhrase, string IV)
        //        {
        //            string decrypted = null;
        //            byte[] cipher = Convert.FromBase64String(encryptedText);

        //            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
        //            {
        //                aes.KeySize = 256;
        //                aes.Key = Encoding.UTF8.GetBytes(passPhrase);
        //                aes.IV = Encoding.UTF8.GetBytes(IV);
        //                aes.Mode = CipherMode.CBC;
        // ReSharper disable once CommentTypo
        //                aes.Padding = PaddingMode.PKCS7;

        // ReSharper disable once CommentTypo
        //                ICryptoTransform dec = aes.CreateDecryptor(aes.Key, aes.IV);

        //                using (MemoryStream ms = new MemoryStream(cipher))
        //                {
        //                    using (CryptoStream cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
        //                    {
        //                        using (StreamReader sr = new StreamReader(cs))
        //                        {
        //                            decrypted = sr.ReadToEnd();
        //                        }
        //                    }
        //                }
        //            }

        //            return decrypted;
        //        }

        //#endregion Encryption


        #region JObjectHelper

        public static JObject UnFlatten(JObject source)
        {
            return new JObjectHelper(source).UnFlatten();
        }

        public static JObject UnFlatten(JObject source, string name)
        {
            return new JObjectHelper(source).UnFlatten(name);
        }

        public static JObject Assign(JObject source, params JObject[] others)
        {
            return new JObjectHelper(source).Assign(others);
        }

        public static JObject Pick(JObject source, params string[] colNames)
        {
            return new JObjectHelper(source).Pick(colNames);
        }

        public static JObject Omit(JObject source, params string[] colNames)
        {
            return new JObjectHelper(source).Omit(colNames);
        }

        #endregion JObjectHelper
    }
}
