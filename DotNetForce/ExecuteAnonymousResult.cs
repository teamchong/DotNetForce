using Newtonsoft.Json;

namespace DotNetForce
{
    public class ExecuteAnonymousResult
    {
        [JsonProperty("compiled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Compiled { get; set; }

        [JsonProperty("compileProblem", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CompileProblem { get; set; }

        [JsonProperty("reason", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Reason { get; set; }

        [JsonProperty("success", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Success { get; set; }

        [JsonProperty("line", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Line { get; set; }

        [JsonProperty("column", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Column { get; set; }

        [JsonProperty("exceptionMessage", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ExceptionMessage { get; set; }

        [JsonProperty("exceptionStackTrace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ExceptionStackTrace { get; set; }
    }
}
