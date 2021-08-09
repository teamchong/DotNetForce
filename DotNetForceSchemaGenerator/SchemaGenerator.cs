using DotNetForce;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global

namespace DotNetForceSchemaGenerator
{
    public class SchemaGenerator
    {
        public SchemaGenerator(StringBuilder generationEnvironment, string schemaNamespace, string schemaName)
        {
            GenerationEnvironment = generationEnvironment;
            SchemaNamespace = schemaNamespace;
            SchemaName = schemaName;
        }

        public StringBuilder GenerationEnvironment { get; }
        public string SchemaNamespace { get; }
        public string SchemaName { get; }

        public static async Task<JObject> RetrieveSObjectsAsync(DnfClient client)
        {
            var describeGlobalResult = await client.GetObjectsAsync()
                .ConfigureAwait(false);

            var request = new CompositeRequest();
            if (describeGlobalResult?.SObjects != null)
                foreach (var sObject in describeGlobalResult.SObjects)
                {
                    var objectName = sObject["name"]?.ToString() ?? string.Empty;
                    if ((bool?)sObject["deprecatedAndHidden"] == true) continue;
                    request.Describe(objectName, objectName);
                }

            var describeResult = await client.Composite.PostAsync(request)
                .ConfigureAwait(false);

            var objects = JObject.FromObject(describeResult.Results());
            return objects;
        }

        public Task<string> GenerateAsync(DnfClient client)
        {
            return GenerateAsync(client, _ => true);
        }

        public async Task<string> GenerateAsync(DnfClient client, Func<JProperty, bool> filter)
        {
            var objects = await RetrieveSObjectsAsync(client)
                .ConfigureAwait(false);

            GenerationEnvironment.Append(@"using DotNetForce;
using DotNetForce.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ").Append(SchemaNamespace).Append(@"
{");
            GenerateSchema(objects);

            foreach (var prop in objects.Properties().Where(p => filter?.Invoke(p) != false))
                // await WriteJsonAsync(prop.Name, prop.Value).ConfigureAwait(false);
                GenerateObject(prop.Name, prop.Value);
            GenerationEnvironment.Append(@"
}");
            return GenerationEnvironment.ToString();
        }

        public void GenerateSchema(JObject objects)
        {
            GenerationEnvironment.Append(@"
	public class " + SchemaName + @"
	{
		private static " + SchemaName + @" _Instance { get; set; }
		public static " + SchemaName + @" Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new " + SchemaName + @"();
                }
                return _Instance;
            }
        }
        public static T Of<T>(Func<" + SchemaName + @", T> func) { return func(Instance); }
        public static void Of<T>(Func<" + SchemaName + @", T> func, Action<T> action) { action(func(Instance)); }
        public static TOut Of<T, TOut>(Func<" + SchemaName + @", T> func, Func<T, TOut> getter) { return getter(func(Instance)); }

");
            foreach (var objName in objects.Properties().Select(p => p.Name))
                GenerationEnvironment.Append(@"
		public Sf").Append(objName).Append(@" ").Append(objName).Append(@" { get { return new Sf").Append(objName).Append(@"(); } }
");
            GenerationEnvironment.Append(@"
	}
");
            //await GenerateFileAsync("" + SchemaName + @".cs").ConfigureAwait(false);
        }

        public void GenerateObject(string objName, JToken objDescribe)
        {
            GenerationEnvironment.Append(@"
	public class Sf").Append(objName).Append(@" : SfObjectBase
	{
		public Sf").Append(objName).Append(@"() : base("""") { }
		public Sf").Append(objName).Append(@"(string path) : base(path) { }
");
            var references = new List<JToken>();


            if (objDescribe["fields"]?.Type == JTokenType.Array)
                foreach (var field in objDescribe["fields"])
                {
                    var fieldName = field["name"]?.ToString();

                    if (string.IsNullOrEmpty(fieldName)) continue;

                    switch (field["type"]?.ToString() ?? "")
                    {
                        case "address":
                            GenerationEnvironment.Append(@"
		public SfAddressField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfAddressField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "anyType":
                            GenerationEnvironment.Append(@"
		public SfAnyTypeField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfAnyTypeField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "base64":
                            GenerationEnvironment.Append(@"
		public SfBase64Field<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfBase64Field<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "boolean":
                            GenerationEnvironment.Append(@"
		public SfBooleanField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfBooleanField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "combobox":
                            GenerationEnvironment.Append(@"
		public SfComboBoxField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfComboBoxField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@", ");
                            OutputPickListDefaultValue(field);
                            GenerationEnvironment.Append(@", ");
                            OutputPicklists(field);
                            GenerationEnvironment.Append(@"); } }
");
                            break;
                        case "complexvalue":
                            GenerationEnvironment.Append(@"
		public SfComplexValueField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfComplexValueField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "currency":
                            GenerationEnvironment.Append(@"
		public SfCurrencyField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfCurrencyField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "date":
                            GenerationEnvironment.Append(@"
		public SfDateField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfDateField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName))
                                .Append(@"); } }
");
                            break;
                        case "datetime":
                            GenerationEnvironment.Append(@"
		public SfDateTimeField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfDateTimeField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "double":
                            GenerationEnvironment.Append(@"
		public SfDoubleField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfDoubleField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "email":
                            GenerationEnvironment.Append(@"
		public SfEmailField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfEmailField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "id":
                            GenerationEnvironment.Append(@"
		public SfIdField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfIdField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName))
                                .Append(@"); } }
");
                            break;
                        case "int":
                            GenerationEnvironment.Append(@"
		public SfIntField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfIntField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName))
                                .Append(@"); } }
");
                            break;
                        case "location":
                            GenerationEnvironment.Append(@"
		public SfLocationField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfLocationField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "multipicklist":
                            GenerationEnvironment.Append(@"
		public SfMultiPicklistField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfMultiPicklistField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@", ");
                            OutputPickListDefaultValue(field);
                            GenerationEnvironment.Append(@", ");
                            OutputPicklists(field);
                            GenerationEnvironment.Append(@"); } }
");
                            break;
                        case "percent":
                            GenerationEnvironment.Append(@"
		public SfPercentField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfPercentField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "phone":
                            GenerationEnvironment.Append(@"
		public SfPhoneField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfPhoneField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "picklist":
                            GenerationEnvironment.Append(@"
		public SfPicklistField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfPicklistField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@", ");
                            OutputPickListDefaultValue(field);
                            GenerationEnvironment.Append(@", ");
                            OutputPicklists(field);
                            GenerationEnvironment.Append(@"); } }
");
                            break;
                        case "reference":
                            if (field["referenceTo"]?.Count() != 1)
                            {
#if DEBUG
                                var fieldReferenceTo = field["referenceTo"]?.ToString() ?? "";
                                Debug.WriteLine("referenceTo.Count != 1 " + (objName.Length > 50 ? objName.Remove(50) : objName) + "." +
                                                (fieldName.Length > 50 ? fieldName.Remove(50) : fieldName) + " " +
                                                (fieldReferenceTo.Length > 50 ? fieldReferenceTo.Remove(50) : fieldReferenceTo));
#endif
                                continue;
                            }
                            var relationshipName = field["relationshipName"]?.ToString();
                            var referenceTo = field["referenceTo"]?[0]?.ToString();

                            if (string.IsNullOrEmpty(relationshipName) || string.IsNullOrEmpty(referenceTo)) continue;
                            references.Add(field);
                            GenerationEnvironment.Append(@"
		public SfIdField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfIdField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName))
                                .Append(@"); } }
");
                            break;
                        case "textarea":
                            GenerationEnvironment.Append(@"
		public SfTextAreaField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfTextAreaField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
                        case "time":
                            GenerationEnvironment.Append(@"
		public SfTimeField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfTimeField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName))
                                .Append(@"); } }
");
                            break;
                        case "url":
                            GenerationEnvironment.Append(@"
		public SfUrlField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfUrlField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName))
                                .Append(@"); } }
");
                            break;
                        case "string":
                        case "encryptedstring":
                            GenerationEnvironment.Append(@"
		public SfStringField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfStringField<Sf").Append(objName).Append(@">(")
                                .Append(FormatPath(fieldName)).Append(@"); } }
");
                            break;
#if DEBUG
                        default:
                            Debug.WriteLine("unknown type: " + (objName.Length > 50 ? objName.Remove(50) : objName) + "." + (fieldName.Length > 50 ? fieldName.Remove(50) : fieldName) +
                                            " " + (field["type"]?.ToString() ?? ""));
//                        GenerationEnvironment.Append(@"
//		public SfStringField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfStringField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
//");
                            break;
#endif
                    }
                }

            if (references.Count > 0)
            {
                GenerationEnvironment.Append(@"
#region References
");

                foreach (var field in references)
                {
                    var relationshipName = field["relationshipName"]?.ToString();
                    var referenceTo = field["referenceTo"]?[0]?.ToString();
                    GenerationEnvironment.Append(@"
		public Sf").Append(referenceTo).Append(@" ").Append(relationshipName).Append(@" { get { return new Sf").Append(referenceTo).Append(@"(").Append(FormatPath(relationshipName))
                        .Append(@"); } }
");
                }
                GenerationEnvironment.Append(@"
#endregion References
");
            }

            var childRelationships = new List<JToken>();

            if (objDescribe["childRelationships"]?.Type == JTokenType.Array && objDescribe["childRelationships"].Any()) childRelationships.AddRange(from childRelationship in objDescribe["childRelationships"] let relationshipName = childRelationship["relationshipName"]?.ToString() let childSObject = childRelationship["childSObject"]?.ToString() where !string.IsNullOrEmpty(relationshipName) && !string.IsNullOrEmpty(childSObject) select childRelationship);

            if (childRelationships.Count > 0)
            {
                GenerationEnvironment.Append(@"
#region ChildRelationships
");
                foreach (var childRelationship in childRelationships)
                {
                    var relationshipName = childRelationship["relationshipName"]?.ToString();
                    var childSObject = childRelationship["childSObject"]?.ToString();
                    GenerationEnvironment.Append(@"
		public SfChildRelationship<Sf").Append(objName).Append(@", Sf").Append(childSObject).Append(@"> ").Append(relationshipName).Append(@"
		{
			get { return new SfChildRelationship<Sf").Append(objName).Append(@", Sf").Append(childSObject).Append(@">(").Append(FormatPath(relationshipName)).Append(@"); }
		}
");
                }
                GenerationEnvironment.Append(@"
#endregion ChildRelationships
");
            }
            GenerationEnvironment.Append(@"

		public override string ToString() { return string.IsNullOrEmpty(_Path) ? ").Append(EncodeJson(objName)).Append(@" : _Path; }
	}
");
            //await GenerateFileAsync("Sf" + objName + ".cs").ConfigureAwait(false);
        }

        private static string FormatPath(string fieldName)
        {
            return "string.IsNullOrEmpty(_Path) ? " + EncodeJson(fieldName) + " : _Path + " + EncodeJson("." + fieldName);
        }

        protected static string EncodeJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        protected void OutputPickListDefaultValue(JToken field)
        {
            var picklistValues = field["picklistValues"];
            if (picklistValues?.Any() == true)
                foreach (var picklist in picklistValues)
                    if ((bool?)picklist["active"] == true && (bool?)picklist["defaultValue"] == true)
                    {
                        GenerationEnvironment.Append(EncodeJson(picklist["value"]?.ToString()));
                        return;
                    }
            GenerationEnvironment.Append(@"null");
        }

        protected void OutputPicklists(JToken field)
        {
            GenerationEnvironment.Append(@"new SfPicklistValue[] {");
            var picklistValues = field["picklistValues"];
            if (picklistValues?.Any() == true)
                foreach (var picklist in picklistValues.Where(p => (bool?)p["active"] == true))
                {
                    GenerationEnvironment.Append(@" new SfPicklistValue(").Append(EncodeJson(picklist["value"]?.ToString())).Append(@", ")
.Append(EncodeJson(picklist["label"]?.ToString() ?? picklist["value"]?.ToString())).Append(@"),");
                }

            GenerationEnvironment.Append(@"}");
        }

        public override string ToString()
        {
            return GenerationEnvironment.ToString();
        }
    }
}
