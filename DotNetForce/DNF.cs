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
using System.Security.Cryptography;
using System.Text;
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


        // for Full DotNet Framework, please set ServicePointManager.DefaultConnectionLimit (Default is 2)
        public static int ConcurrentRequestLimit = 2000;

        //public enum MetadataType
        //{
        //    AccountSettings,
        //    ActivitiesSettings,
        //    AddressSettings,
        //    AdjustmentsSettings,
        //    ApexClass,
        //    ApexClassMember,
        //    ApexCodeCoverage,
        //    ApexCodeCoverageAggregate,
        //    ApexComponent,
        //    ApexComponentMember,
        //    ApexDebuggerSession,
        //    ApexEmailNotification,
        //    ApexExecutionOverlayAction,
        //    ApexExecutionOverlayResult,
        //    ApexLog,
        //    ApexOrgWideCoverage,
        //    ApexPage,
        //    ApexPageInfo,
        //    ApexPageMember,
        //    ApexTestQueueItem,
        //    ApexTestResult,
        //    ApexTestResultLimits,
        //    ApexTestRunResult,
        //    ApexTestSuite,
        //    ApexTrigger,
        //    ApexTriggerMember,
        //    AssignmentRule,
        //    AsyncApexJob,
        //    AuraDefinition,
        //    AuraDefinitionBundle,
        //    AuraDefinitionChange,
        //    AutoResponseRule,
        //    BrandingSet,
        //    BusinessHoursEntry,
        //    BusinessHoursSettings,
        //    BusinessProcess,
        //    CaseSettings,
        //    Certificate,
        //    ChannelLayout,
        //    ChatterAnswersSettings,
        //    ChatterMobileSettings,
        //    CleanDataService,
        //    CleanRule,
        //    CompactLayout,
        //    CompactLayoutInfo,
        //    CompactLayoutItemInfo,
        //    CompanySettings,
        //    ContainerAsyncRequest,
        //    ContractSettings,
        //    ColorDefinition,
        //    CountriesAndStates,
        //    Country,
        //    CspTrustedSite,
        //    CustomApplication,
        //    CustomField,
        //    CustomFieldMember,
        //    CustomObject,
        //    CustomTab,
        //    DashboardMobileSettings,
        //    DataIntegrationRecordPurchasePermission,
        //    DataSourceSettings,
        //    DataType,
        //    DebugLevel,
        //    Document,
        //    DuplicateJobDefinition,
        //    DuplicateJobMatchingRuleDefinition,
        //    EmailTemplate,
        //    EmailToCaseRoutingAddress,
        //    EmailToCaseSettings,
        //    EmbeddedServiceBranding,
        //    EmbeddedServiceConfig,
        //    EntitlementSettings,
        //    EntityDefinition,
        //    EntityLimit,
        //    EntityParticle,
        //    EventDelivery,
        //    EventSubscription,
        //    ExternalServiceRegistration,
        //    ExternalString,
        //    FeedItemSettings,
        //    FieldDefinition,
        //    FieldMapping,
        //    FieldMappingField,
        //    FieldMappingRow,
        //    FieldSet,
        //    FileTypeDispositionAssignmentBean,
        //    FileUploadAndDownloadSecuritySettings,
        //    FindSimilarOppFilter,
        //    FlexiPage,
        //    Flow,
        //    FlowCategory,
        //    FlowDefinition,
        //    ForecastRangeSettings,
        //    ForecastingSettings,
        //    ForecastingTypeSettings,
        //    FormulaFunction,
        //    FormulaFunctionCategory,
        //    FormulaOperator,
        //    GlobalValueSet,
        //    Group,
        //    HomePageComponent,
        //    HomePageLayout,
        //    IconDefinition,
        //    IDEPerspective,
        //    IDEWorkspace,
        //    IdeasSettings,
        //    Index,
        //    IndexField,
        //    InstalledSubscriberPackage,
        //    InstalledSubscriberPackageVersion,
        //    IpRange,
        //    KnowledgeAnswerSettings,
        //    KnowledgeCaseField,
        //    KnowledgeCaseFieldsSettings,
        //    KnowledgeCaseSettings,
        //    KnowledgeLanguage,
        //    KnowledgeLanguageSettings,
        //    KnowledgeSettings,
        //    KnowledgeSitesSettings,
        //    KnowledgeSuggestedArticlesSettings,
        //    KnowledgeWorkOrderField,
        //    KnowledgeWorkOrderFieldsSettings,
        //    KnowledgeWorkOrderLineItemField,
        //    KnowledgeWorkOrderLineItemFieldsSettings,
        //    Layout,
        //    LeadConvertSettings,
        //    LiveAgentSettings,
        //    LookupFilter,
        //    MacroSettings,
        //    MenuItem,
        //    MetadataContainer,
        //    MetadataContainerMember,
        //    MobileSettings,
        //    Name,
        //    NameSettings,
        //    NetworkAccess,
        //    ObjectSearchSetting,
        //    OperationLog,
        //    OpportunityListFieldsLabelMapping,
        //    OpportunityListFieldsSelectedSettings,
        //    OpportunityListFieldsUnselectedSettings,
        //    OpportunitySettings,
        //    OrderSettings,
        //    OrgPreferenceSettings,
        //    OrganizationSettingsDetail,
        //    OwnerChangeOptionInfo,
        //    Package2,
        //    Package2Member,
        //    Package2Version,
        //    Package2VersionCreateRequest,
        //    Package2VersionCreateRequestError,
        //    PackageVersionInstallRequestError,
        //    PackageVersionUninstallRequestError,
        //    PasswordPolicies,
        //    PartitionLevelScheme,
        //    PathAssistant,
        //    PathAssistantSettings,
        //    PathAssistantStepInfo,
        //    PathAssistantStepItem,
        //    PermissionSet,
        //    PermissionSetTabSetting,
        //    PersonalJourneySettings,
        //    PlatformCachePartition,
        //    PlatformCachePartitionType,
        //    PostTemplate,
        //    ProductSettings,
        //    Profile,
        //    ProfileLayout,
        //    Publisher,
        //    QuickActionDefinition,
        //    QuickActionList,
        //    QuickActionListItem,
        //    QuotasSettings,
        //    QuoteSettings,
        //    RecentlyViewed,
        //    RecordType,
        //    RelationshipDomain,
        //    RelationshipInfo,
        //    ReleasedApexClassRel,
        //    ReleasedApexIdentifier,
        //    ReleasedApexIdentifierOption,
        //    ReleasedEntityState,
        //    RemoteProxy,
        //    SandboxInfo,
        //    SandboxProcess,
        //    SFDCMobileSettings,
        //    Scontrol,
        //    SearchLayout,
        //    SearchSettings,
        //    SearchSettingsByObject,
        //    SecurityHealthCheck,
        //    SecurityHealthCheckRisks,
        //    SecuritySettings,
        //    SessionSettings,
        //    SetupNode,
        //    SiteDetail,
        //    StandardAction,
        //    StandardValueSet,
        //    State,
        //    StaticResource,
        //    SubscriberPackage,
        //    SubscriberPackageVersion,
        //    SubscriberPackageVersionInstallRequest,
        //    SubscriberPackageVersionUninstallRequest,
        //    Territory2Settings,
        //    Territory2SettingsOpportunityFilter,
        //    TestSuiteMembership,
        //    TouchMobileSettings,
        //    TraceFlag,
        //    TransactionSecurityPolicy,
        //    User,
        //    UserEntityAccess,
        //    UserFieldAccess,
        //    UserPreference,
        //    UserRole,
        //    ValidationRule,
        //    ValidationRuleMember,
        //    WebLink,
        //    WebToCaseSettings,
        //    WorkflowAlert,
        //    WorkflowAlertMember,
        //    WorkflowFieldUpdate,
        //    WorkflowFieldUpdateMember,
        //    WorkflowOutboundMessage,
        //    WorkflowOutboundMessageMember,
        //    WorkflowRule,
        //    WorkflowRuleMember,
        //    WorkflowTask,
        //    WorkflowTaskMember,
        //    ApexDebuggerLicense,
        //    DataAssessmentConfigItem,
        //    FieldServiceSettings,
        //    ForecastingDisplayedFamily,
        //    ForecastingDisplayedFamilySettings,
        //    MetadataPackage,
        //    MetadataPackageVersion,
        //    PackageInstallRequest,
        //    PackageUploadRequest,
        //    VisualforceAccessMetrics
        //}
        //public static string ValueTypeColumnName = "column0";


        //public static JArray Records(JToken token)
        //{
        //    return token?["records"]?.ToObject<JArray>();
        //}

        //public static IEnumerable<T> Records<T>(JToken token) where T : JObject
        //{
        //    return Records(token)?.Cast<T>() ?? Enumerable.Empty<T>();
        //}

        //public static DNFClient.DataQueryResult AsQuery(DNFClient client, JObject token)
        //{
        //    return new DNFClient.DataQueryResult(client, token);
        //}

        //internal static IEnumerable<T> RecordAll<T>(JToken token, DNFClient client) where T : JObject
        //{
        //    var last = 0;
        //    var records = Records<T>(token).ToList();
        //    var result = new DNFClient.DataQueryResult(client, token as JObject);
        //    var next = result.Next();

        //    for (var i = last; i < records.Count; i++)
        //    {
        //        yield return records[i];
        //    }

        //    while (next != null)
        //    {
        //        last = records.Count;
        //        var nextResult = next.Result;

        //        for (var i = last; i < nextResult.Records.Count; i++)
        //        {
        //            yield return nextResult.Records[i] as T;
        //        }

        //        next = nextResult.Next();
        //    }
        //}

        //public static bool ValueAreEquals(JToken v1, JToken v2)
        //{
        //    if (v1 == null && v2 == null)
        //        return true;
        //    if (v1 == null || v2 == null)
        //        return false;
        //    if (JToken.DeepEquals(v2, v1) == true)
        //        return true;
        //    if (v1.Type == JTokenType.Null && string.IsNullOrEmpty(v2.ToString()))
        //        return true;
        //    if (v2.Type == JTokenType.Null && string.IsNullOrEmpty(v1.ToString()))
        //        return true;
        //    if (new[] { v1, v2 }.Count(_ => _.Type == JTokenType.Integer || _.Type == JTokenType.Float) == 2 &&
        //        (v1.ToObject<double?>() ?? 0) == (v1.ToObject<double?>() ?? 0))
        //        return true;
        //    return false;
        //}

        public static string SOQLString(string input)
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

        //public static void CopyTo(JObject source, JObject target)
        //{
        //    if (source != null && target != null)
        //    {
        //        foreach (var p in source.Properties())
        //        {
        //            target[p.Name] = p.Value;
        //        }
        //    }
        //}

        //public static JObject GetChangedFrom(JObject target, JObject changes)
        //{
        //    JObject difference = null;
        //    if (target == null)
        //    {
        //        difference = new JObject();
        //        foreach (var p in changes.Properties())
        //        {
        //            difference[p.Name] = p.Value;
        //        }
        //    }
        //    else if (changes != null)
        //    {
        //        foreach (var p in changes.Properties())
        //        {
        //            if (ValueAreEquals(target[p.Name], p.Value) != true)
        //            {
        //                if (difference == null)
        //                    difference = new JObject();
        //                difference[p.Name] = p.Value;
        //            }
        //        }
        //    }
        //    return difference;
        //}

        //public static void DataTableToCsv(DataTable tbl, string file, string delimiter = ",")
        //{
        //    if (tbl != null)
        //    {
        //        using (var writer = new StreamWriter(file))
        //        {
        //            var csv = new CsvWriter(writer);
        //            csv.Configuration.Delimiter = delimiter;
        //            foreach (DataColumn column in tbl.Columns)
        //            {
        //                csv.WriteField(column.ColumnName);
        //            }
        //            csv.NextRecord();

        //            foreach (DataRow row in tbl.Rows)
        //            {
        //                for (var i = 0; i < tbl.Columns.Count; i++)
        //                {
        //                    if (row.IsNull(i) || string.IsNullOrEmpty(row.ToString()))
        //                    {
        //                        csv.WriteField(CSV_NULL);
        //                    }
        //                    else if (tbl.Columns[i].DataType == typeof(DateTime))
        //                    {
        //                        csv.WriteField(SOQLDate(row.Field<DateTime>(i)));
        //                    }
        //                    else
        //                    {
        //                        csv.WriteField(row[i]);
        //                    }
        //                }
        //                csv.NextRecord();
        //            }
        //        }
        //    }
        //}

        //public static JToken GetByName(JToken token, string propNameWithDot)
        //{
        //    if (token == null)
        //    {
        //        return null;
        //    }
        //    else if (propNameWithDot.Contains('.'))
        //    {
        //        var names = propNameWithDot.Split('.');
        //        return GetByName(token[names[0]], string.Join(".", names.Skip(1)));
        //    }
        //    else
        //    {
        //        return token[propNameWithDot];
        //    }
        //}

        //private static List<string> FetchCsvColumnName(JObject obj, string name)
        //{
        //    var prefix = "";
        //    if (string.IsNullOrEmpty(name) != true)
        //    {
        //        prefix = name + ".";
        //    }
        //    var names = new List<string>();
        //    if (obj != null)
        //    {
        //        foreach (var prop in obj.Properties())
        //        {
        //            names.Add(prefix + prop.Name);
        //            if (prop.Type == JTokenType.Object)
        //            {
        //                names.AddRange(FetchCsvColumnName(prop.Value as JObject, prefix + prop.Name));
        //            }
        //        }
        //    }
        //    return names;
        //}

        //public static void JsonToCsv(JArray jarray, string file, string delimiter = ",")
        //{
        //    if (jarray != null)
        //    {
        //        var columns = new List<string>();
        //        foreach (var row in jarray)
        //        {
        //            if (row.Type == JTokenType.Object)
        //            {
        //                columns.AddRange(FetchCsvColumnName(row as JObject, string.Empty));
        //            }
        //        }
        //        var isValType = columns.Count <= 0;
        //        if (isValType)
        //        {
        //            using (var writer = new StreamWriter(file))
        //            {
        //                var csv = new CsvWriter(writer);
        //                csv.Configuration.Delimiter = delimiter;
        //                csv.WriteField(ValueTypeColumnName);
        //                csv.NextRecord();

        //                foreach (var row in jarray)
        //                {
        //                    if (row == null)
        //                    {
        //                        csv.WriteField(string.Empty);
        //                    }
        //                    else if (row.Type == JTokenType.Null || string.IsNullOrEmpty(row.ToString()))
        //                    {
        //                        csv.WriteField(CSV_NULL);
        //                    }
        //                    else
        //                    {
        //                        csv.WriteField(row);
        //                    }
        //                    csv.NextRecord();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            using (var writer = new StreamWriter(file))
        //            {
        //                var csv = new CsvWriter(writer);
        //                csv.Configuration.Delimiter = delimiter;
        //                foreach (var column in columns)
        //                {
        //                    csv.WriteField(column);
        //                }
        //                csv.NextRecord();
        //                foreach (var row in jarray)
        //                {
        //                    foreach (var column in columns)
        //                    {
        //                        var value = DNF.GetByName(row, column);
        //                        if (value == null)
        //                        {
        //                            csv.WriteField(string.Empty);
        //                        }
        //                        else if (value.Type == JTokenType.Null || string.IsNullOrEmpty(value.ToString()))
        //                        {
        //                            csv.WriteField(CSV_NULL);
        //                        }
        //                        else if (value.Type == JTokenType.Date)
        //                        {
        //                            csv.WriteField(SOQLDate(value.ToObject<DateTime>()));
        //                        }
        //                        else
        //                        {
        //                            csv.WriteField(value);
        //                        }
        //                    }
        //                    csv.NextRecord();
        //                }
        //            }
        //        }
        //    }
        //}

        //public static JArray JsonToJsonDataSet(JArray jarray)
        //{
        //    var result = new JArray();
        //    if (jarray != null)
        //    {
        //        var columns = new List<string>();
        //        foreach (var row in jarray)
        //        {
        //            if (row.Type == JTokenType.Object)
        //            {
        //                columns.AddRange(FetchCsvColumnName(row as JObject, string.Empty));
        //            }
        //        }
        //        var isValType = columns.Count <= 0;
        //        if (isValType)
        //        {
        //            result.Add(new JArray { ValueTypeColumnName });

        //            foreach (var row in jarray)
        //            {
        //                var resultRow = new JArray();
        //                resultRow.Add(row);
        //                result.Add(resultRow);
        //            }
        //        }
        //        else
        //        {
        //            result.Add(JArray.FromObject(columns));

        //            foreach (var row in jarray)
        //            {
        //                var resultRow = new JArray();
        //                foreach (var column in columns)
        //                {
        //                    var value = DNF.GetByName(row, column);
        //                    if (value == null || value.Type == JTokenType.Null)
        //                    {
        //                        resultRow.Add(null);
        //                    }
        //                    else
        //                    {
        //                        resultRow.Add(value);
        //                    }
        //                }
        //                result.Add(resultRow);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        result.Add(new JArray { ValueTypeColumnName });
        //    }
        //    return result;
        //}

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

        public static string SOQLDateTime(DateTime? date)
        {
            if (date == null) return null;
            var uDate = date.Value.ToUniversalTime();
            return $@"{uDate:yyyy-MM-dd}T{uDate:HH}:{uDate:mm}:{uDate:ss}Z";
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

        #region Encryption

        public static string Encrypt(string plainText, string passPhrase, string IV)
        {
            byte[] encrypted;

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 256;
                aes.Key = Encoding.UTF8.GetBytes(passPhrase);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform enc = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        encrypted = ms.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string encryptedText, string passPhrase, string IV)
        {
            string decrypted = null;
            byte[] cipher = Convert.FromBase64String(encryptedText);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 256;
                aes.Key = Encoding.UTF8.GetBytes(passPhrase);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform dec = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipher))
                {
                    using (CryptoStream cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            decrypted = sr.ReadToEnd();
                        }
                    }
                }
            }

            return decrypted;
        }

        #endregion

        public static string ToID18<T>(T idObj)
        {
            var id = Convert.ToString(idObj);
            if (id.Length != 15)
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
            if (id.Length != 18)
                return id;

            return id.Remove(15);
        }

        public static async Task<T> TryDeserializeObject<T>(Func<Task<T>> task)
        {
            try
            {
                return await task();
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
    }
}