//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using DotNetForce.Common;
//using DotNetForce.Common.Models.Json;
//using System;
//using System.Linq;

//namespace DotNetForce
//{
//    public static class SaveResponseExtension
//    {
//        public static void ThrowIfError(this SaveResponse response)
//        {
//            if (response?.HasErrors == true)
//            {
//                if (response.Results?.Count > 0)
//                {
//                    var messages = response.Results.Select(err => JsonConvert.SerializeObject(err) ?? "Unknown Error.");
//                    throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages));
//                }
//                else
//                {
//                    var messages = response.Results.SelectMany(r => r.Errors.Select(err => err.Message));
//                    throw new ForceException(Error.Unknown, string.Join(Environment.NewLine, messages));
//                }
//            }
//        }
//    }
//}

