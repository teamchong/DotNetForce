using DotNetForce.Common;
using DotNetForce.Common.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
//using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetForce
{
    public static class DNF
    {
        //public const string CSV_NULL = "#N/A";
        public const string STANDARD_PRICE_BOOK = "Standard Price Book";

        public const int COMPOSITE_LIMIT = 25;
        public const int COMPOSITE_QUERY_LIMIT = 5;

        public const int BATCH_LIMIT = 25;

        public const int DEFAULT_CONCURRENT_LIMIT = 50;
        
        public const int QUERY_CURSOR_LIMIT = 25;

        // for Full DotNet Framework, please set ServicePointManager.DefaultConnectionLimit (Default is 2)
        // for safy reason, max no of concurrent api call with transactions longer than 20 seconds.
        

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
                if (token?.Type == JTokenType.Array)
                {
                    return token.ToObject<ErrorResponses>();
                }
                else
                {
                    return new ErrorResponses { token.ToObject<ErrorResponse>() };
                }
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
                var request = result.Requests().Where(req => req.ReferenceId == errors.Key).FirstOrDefault();
                exList.Add(new ForceException(
                    errors.Value.Select(error => error.ErrorCode).FirstOrDefault() ?? $"{Error.Unknown}",
                    (request == null ? $"{errors.Key}:" : $"{request}") + Environment.NewLine +
                    string.Join(Environment.NewLine, errors.Value.Select(error => error.Message))
                ));
            }
            if (exList.Count > 0)
            {
                throw new AggregateException(exList);
            }
            return result;
        }

        public static SaveResponse ThrowIfError(SaveResponse response)
        {
            if (response?.HasErrors == true)
            {
                if (response.Results?.Count > 0)
                {
                    var messages = response.Results.Select(err => JsonConvert.SerializeObject(err) ?? "Unknown Error.");
                    throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages));
                }
                else
                {
                    var messages = response.Results.SelectMany(r => r.Errors.Select(err => err.Message));
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
            if (exList.Count > 0)
            {
                throw new AggregateException(exList);
            }
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

        public static string SOQLString(object inputObj)
        {
            if (inputObj == null)
            {
                return "null";
            }
            var input = inputObj.ToString();
            return "'" + string.Join("\\\\", input.Split('\\').Select(i1 =>
            {
                return string.Join("\\'", i1.Split('\'').Select(i2 =>
                {
                    return string.Join("\\\"", i2.Split('"').Select(i3 =>
                    {
                        return string.Join("\\f", i3.Split('\f').Select(i4 =>
                        {
                            return string.Join("\\b", i4.Split('\b').Select(i5 =>
                            {
                                return string.Join("\\t", i5.Split('\t').Select(i6 =>
                                {
                                    return string.Join("\\r", i6.Split('\r').Select(i7 =>
                                    {
                                        return string.Join("\\n", i7.Split('\n').Select(i8 =>
                                        {
                                            return i8;
                                        }));
                                    }));
                                }));
                            }));
                        }));
                    }));
                }));
            })) + "'";
        }

        public static string SOQLLike(string input)
        {
            if (input == null)
            {
                return "null";
            }
            return "'" + string.Join("\\\\", input.Split('\\').Select(i1 =>
            {
                return string.Join("\\'", i1.Split('\'').Select(i2 =>
                {
                    return string.Join("\\\"", i2.Split('"').Select(i3 =>
                    {
                        return string.Join("\\f", i3.Split('\f').Select(i4 =>
                        {
                            return string.Join("\\b", i4.Split('\b').Select(i5 =>
                            {
                                return string.Join("\\t", i5.Split('\t').Select(i6 =>
                                {
                                    return string.Join("\\r", i6.Split('\r').Select(i7 =>
                                    {
                                        return string.Join("\\n", i7.Split('\n').Select(i8 =>
                                        {
                                            return string.Join("\\_", i8.Split('_').Select(i9 =>
                                            {
                                                return string.Join("\\%", i9.Split('%').Select(i10 =>
                                                {
                                                    return i10;
                                                }));
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

        public static string SOQLDate(DateTime? date)
        {
            if (date == null) return null;
            return $"{date.Value:yyyy-MM-dd}";
        }

        public static DateTime? FromSOQLDate(string date)
        {
            if (date == null) return null;
            return DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dateOut)
                ? (DateTime?)dateOut : null;
        }

        public static string SOQLDateTime(DateTime? date)
        {
            if (date == null) return null;
            var uDate = date.Value.ToUniversalTime();
            return $@"{uDate:yyyy-MM-dd}T{uDate:HH}:{uDate:mm}:{uDate:ss}Z";
        }

        public static DateTime? FromSOQLDateTime(string date)
        {
            if (date == null) return null;
            return DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ssZ", null, System.Globalization.DateTimeStyles.None, out DateTime dateOut)
                ? (DateTime?)dateOut : null;
        }

        public static string SOQLTime(DateTime? date)
        {
            if (date == null) return null;
            var uDate = date.Value.ToUniversalTime();
            return $"{uDate:HH}:{uDate:mm}:{uDate:ss}Z";
        }

        public static DateTime? FromSOQLTime(string date)
        {
            if (date == null) return null;
            return DateTime.TryParseExact(date, "HH:mm:ssZ", null, System.Globalization.DateTimeStyles.None, out DateTime dateOut)
                ? (DateTime?)dateOut : null;
        }

        public static string SOQLDateTimeUtc(DateTime? date)
        {
            if (date == null) return null;
            return $"{date.Value:yyyy-MM-dd}T{date.Value:HH}:{date.Value:mm}:{date.Value:ss}Z";
        }

        public static string SOQLDateTime(DateTime? date, int timezone)
        {
            if (date == null) return null;
            var tDate = date.Value.AddHours(-timezone);
            return $@"{tDate:yyyy-MM-dd}T{tDate:HH}:{tDate:mm}:{tDate:ss}Z";
        }

        public static DateTime? ToDateTime<T>(T date)
        {
            DateTimeOffset dateVal;
            if (DateTimeOffset.TryParse(Convert.ToString(date), out dateVal))
                return dateVal.DateTime;
            if (DateTimeOffset.TryParse(Convert.ToString(date), out dateVal))
                return dateVal.DateTime;
            return null;
        }

        public static string ToID18<T>(T idObj)
        {
            var id = Convert.ToString(idObj);
            if (id?.Length != 15)
                return id;

            var triplet = new List<string> { id.Substring(0, 5),
                                    id.Substring(5, 5),
                                    id.Substring(10, 5) };
            var str = new StringBuilder(5);
            var suffix = string.Empty;
            foreach (var value in triplet)
            {
                str.Clear();
                var reverse = value.Reverse().ToList();
                reverse.ForEach(c => str.Append(Char.IsUpper(c) ? "1" : "0"));
                var parsedBinary = Convert.ToInt32(str.ToString(), 2);
                suffix += (char)(parsedBinary + (parsedBinary < 26 ? 65 : 22));
            }
            return id + suffix;
        }

        public static string ToID15<T>(T idObj)
        {
            var id = Convert.ToString(idObj);
            if (id?.Length != 18)
                return id;

            return id.Remove(15);
        }

        public static async Task<T> TryDeserializeObject<T>(Task<T> obj)
        {
            try
            {
                return await obj;
            }
            catch (ForceException ex)
            {
                if (ex.Error == Error.NonJsonErrorResponse)
                {
                    return JsonConvert.DeserializeObject<T>(ex.Message);
                }
                throw ex;
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
//                aes.Padding = PaddingMode.PKCS7;

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
//                aes.Padding = PaddingMode.PKCS7;

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
        
        public static JObject UnFlatten(JObject source) => new JObjectHelper(source).UnFlatten();

        public static JObject UnFlatten(JObject source, string name) => new JObjectHelper(source).UnFlatten(name);

        public static JObject Assign(JObject source, params JObject[] others) => new JObjectHelper(source).Assign( others);

        public static JObject Pick(JObject source, params string[] colNames) => new JObjectHelper(source).Pick(colNames);

        public static JObject Omit(JObject source, params string[] colNames) => new JObjectHelper(source).Omit(colNames);
        
#endregion JObjectHelper

        public static string EscapeDataString(string uri)
        {
            if (uri == null) return null;
            return string.Join("", DNF.Chunk(uri, 65519).Select(c => Uri.EscapeDataString(new string(c.ToArray()))));
        }
        
        public static IEnumerable<List<T>> Chunk<T>(IEnumerable<T> source, int size) => new EnumerableChunk<T>(source, size).GetEnumerable();
        
        public static IEnumerable<string> ChunkIds(IEnumerable<string> source, string soql, string template)
        {
            soql = soql.Trim().Replace("\r\n", "\n");
            var soqlMaxLen = 20000;
            var nonTemplateLength = soql.Replace(template, "").Length;
            var idsTextLen = soqlMaxLen - nonTemplateLength;
            var numOfTemplate = (int)Math.Ceiling((soql.Length - nonTemplateLength) / (double)template.Length);
            var numOfId = (int)Math.Max(1, Math.Floor(idsTextLen / (18.0 * numOfTemplate)));
            return new EnumerableChunk<string>(source, numOfId).GetEnumerable()
                .Select(l => soql.Replace(template, string.Join(",", l.Select(id => DNF.SOQLString(DNF.ToID15(id))))));
        }

    }
}