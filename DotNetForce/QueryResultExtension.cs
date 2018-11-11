//using DotNetForce.Common;
//using DotNetForce.Common.Models.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reactive.Linq;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace DotNetForce
//{
//    public static class QueryResultExtension
//    {
//        public static IEnumerable<T> GetEnumerable<T>(this QueryResult<T> queryResult, DNFClient client)
//        {
//            return queryResult == null ? Enumerable.Empty<T>()
//                : client?.GetEnumerable(queryResult) ?? Enumerable.Empty<T>();
//        }

//        public static IEnumerable<T> GetLazyEnumerable<T>(this QueryResult<T> queryResult, DNFClient client)
//        {
//            return queryResult == null ? Enumerable.Empty<T>()
//                : client?.GetLazyEnumerable(queryResult) ?? Enumerable.Empty<T>();
//        }
//    }
//}