using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; //using System.Security.Cryptography;

namespace DotNetForce
{
    [JetBrains.Annotations.PublicAPI]
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

        public static string EscapeDataString(string uri)
        {
            if (uri == null) return null;
            return string.Join("", Chunk(uri, 65519).Select(c => Uri.EscapeDataString(new string(c.ToArray()))));
        }

        public static IEnumerable<IList<T>> Chunk<T>(IEnumerable<T> source, int size)
        {
            return new EnumerableChunk<T>(source, size).GetEnumerable();
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
            return new EnumerableChunk<string>(source, numOfId).GetEnumerable()
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


        #region ResponseHandling

        public static bool IsQueryResult(JToken token)
        {
            return token?.Type == JTokenType.Object
                   && token["totalSize"]?.Type == JTokenType.Integer
                   && token["done"]?.Type == JTokenType.Boolean
                   && token["records"]?.Type == JTokenType.Array;
        }

        public static bool IsSuccessResponse(JToken token)
        {
            return token?.Type == JTokenType.Object
                   && token["id"]?.Type == JTokenType.String
                   && token["success"]?.Type == JTokenType.Boolean
                   && token["errors"]?.Type == JTokenType.Array;
        }

        public static ErrorResponses GetErrorResponses(JToken token)
        {
            try
            {
                if (token?.Type == JTokenType.Array) return token.ToObject<ErrorResponses>();
                return new ErrorResponses { token?.ToObject<ErrorResponse>() };
            }
            catch
            {
                return new ErrorResponses
                {
                    new ErrorResponse
                    {
                        ErrorCode = "UNKNOWN",
                        Message = token?.ToString()
                    }
                };
            }
        }


        public static CompositeResult ThrowIfError(CompositeResult result)
        {
            var exList = new List<ForceException>();
            foreach (var errors in result.Errors())
            {
                var request = result.Requests().FirstOrDefault(req => req.ReferenceId == errors.Key);
                exList.Add(new ForceException(
                    errors.Value.Select(error => error.ErrorCode).FirstOrDefault() ?? $"{Error.Unknown}",
                    (request == null ? $"{errors.Key}:" : $"{request}") + Environment.NewLine +
                    string.Join(Environment.NewLine, errors.Value.Select(error => error.Message))
                ));
            }
            if (exList.Count > 0) throw new ForceException(exList);
            return result;
        }

        public static SaveResponse ThrowIfError(SaveResponse response)
        {
            if (response?.HasErrors == true)
            {
                if (response.Results?.Count > 0)
                {
                    var messages = response.Results.Select(JsonConvert.SerializeObject);
                    throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages));
                }
                else
                {
                    var messages = response.Results?.SelectMany(r => r.Errors.Select(err => err.Message)) ?? new string[] { };
                    throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages));
                }
            }
            return response;
        }


        public static BatchResult ThrowIfError(BatchResult result)
        {
            var exList = new List<ForceException>();
            foreach (var errors in result.Errors())
            {
                var request = result.Requests().Where((req, reqIdx) => $"{reqIdx}" == errors.Key).FirstOrDefault();
                exList.Add(new ForceException(
                    errors.Value.Select(error => error.ErrorCode).FirstOrDefault() ?? $"{Error.Unknown}",
                    (request == null ? $"{errors.Key}:" : $"{request}") + Environment.NewLine +
                    string.Join(Environment.NewLine, errors.Value.Select(error => error.Message))
                ));
            }
            if (exList.Count > 0) throw new AggregateException(exList);
            return result;
        }

        public static SuccessResponse ThrowIfError(SuccessResponse response)
        {
            if (response?.Errors != null)
            {
                var errors = JToken.FromObject(response.Errors);
                if (errors.Any())
                {
                    var messages = errors.Select(err => err?.ToString() ?? "Unknown Error.");
                    throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages));
                }
            }
            return response;
        }

        #endregion ResponseHandling

        #region DataConvertion

        public static string SoqlString(object inputObj)
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

        public static string SoqlLike(string input)
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
            if (date == null)
                return "null";

            return $"Date.newInstance({date.Value.Year},{date.Value.Month},{date.Value.Day})";
        }

        public static string ApexDateTime(DateTime? date)
        {
            if (date == null)
                return "null";

            return $"DateTime.newInstance({(date.Value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds}L)";
        }

        public static string SoqlDate(DateTime? date)
        {
            if (date == null) return null;
            return $"{date.Value:yyyy-MM-dd}";
        }

        public static DateTime? FromSoqlDate(string date)
        {
            if (date == null) return null;
            return DateTime.TryParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.None, out var dateOut)
                ? (DateTime?)dateOut
                : null;
        }

        public static string SoqlDateTime(DateTime? date)
        {
            if (date == null) return null;
            var uDate = date.Value.ToUniversalTime();
            return $@"{uDate:yyyy-MM-dd}T{uDate:HH}:{uDate:mm}:{uDate:ss}Z";
        }

        public static DateTime? FromSoqlDateTime(string date)
        {
            if (date == null) return null;
            return DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ssZ", null, DateTimeStyles.None, out var dateOut)
                ? (DateTime?)dateOut
                : null;
        }

        public static string SoqlTime(DateTime? date)
        {
            if (date == null) return null;
            var uDate = date.Value.ToUniversalTime();
            return $"{uDate:HH}:{uDate:mm}:{uDate:ss}Z";
        }

        public static DateTime? FromSoqlTime(string date)
        {
            if (date == null) return null;
            return DateTime.TryParseExact(date, "HH:mm:ssZ", null, DateTimeStyles.None, out var dateOut)
                ? (DateTime?)dateOut
                : null;
        }

        public static string SoqlDateTimeUtc(DateTime? date)
        {
            if (date == null) return null;
            return $"{date.Value:yyyy-MM-dd}T{date.Value:HH}:{date.Value:mm}:{date.Value:ss}Z";
        }

        public static string SoqlDateTime(DateTime? date, int timezone)
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
                id.Substring(0, 5),
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
                return await obj.ConfigureAwait(false);
            }
            catch (ForceException ex)
            {
                if (ex.Error == Error.NonJsonErrorResponse) return JsonConvert.DeserializeObject<T>(ex.Message);
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
