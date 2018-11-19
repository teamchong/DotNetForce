using DotNetForce;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class SchemaGenerator
{
    // public string ProjectDir { get; set; }
    // public string InstanceName { get; set; }
    public StringBuilder GenerationEnvironment { get; set; }
    // public Action<string> Logger { get; set; }
    // public Action<string> ErrorLogger { get; set; }

    public SchemaGenerator() : this(new StringBuilder()) { }

    // public SchemaGenerator(string projectDir, string instanceName, StringBuilder generationEnvironment, Action<string> logger, Action<string> errorLogger)
    public SchemaGenerator(StringBuilder generationEnvironment)
    {
        // ProjectDir = projectDir;
        // InstanceName = instanceName;
        GenerationEnvironment = generationEnvironment;
        // Logger = logger;
        // ErrorLogger = errorLogger;
    }

    //public JObject ReadProfile(string path)
    //{
    //    try
    //    {
    //        if (File.Exists(path))
    //        {
    //            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    //            {
    //                using (var reader = new StreamReader(stream))
    //                {
    //                    using (var jsonReader = new JsonTextReader(reader))
    //                    {
    //                        return new JsonSerializer().Deserialize<JObject>(jsonReader);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    catch { }
    //    return null;
    //}

    public async Task<JObject> RetreiveSObjectsAsync(DNFClient client)
    {
        //var folder = Path.Combine(ProjectDir, InstanceName);
        //var solutionDir = Path.GetDirectoryName(ProjectDir);

        //Logger?.Invoke($"instance: {InstanceName}");
        //Logger?.Invoke($"projectDir: {ProjectDir}");
        //Logger?.Invoke($"solutionDir: {solutionDir}");
        //Logger?.Invoke($"folder: {folder}");

        //if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        //foreach (var dir in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
        //{
        //    Directory.Delete(dir, true);
        //}
        //foreach (var file in Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly))
        //{
        //    File.Delete(file);
        //}

        // var client = await DNFClient.LoginAsync(loginUri, clientId, clientSecret, userName, password, Logger).ConfigureAwait(false);
        var describeGlobalResult = await client.GetObjectsAsync().ConfigureAwait(false);

        var request = new CompositeRequest();
        foreach (var sobject in describeGlobalResult.SObjects)
        {
            var objectName = sobject["name"]?.ToString();
            if ((bool?)sobject["deprecatedAndHidden"] == true)
            {
                continue;
            }
            //Logger?.Invoke(objectName);
            request.Describe(objectName, objectName);
        }

        var describeResult = await client.Composite.PostAsync(request).ConfigureAwait(false);

        //foreach (var error in describeResult.Errors())
        //{
        //    Logger?.Invoke($"{error.Key}\n{error.Value}");
        //}

        var objects = JObject.FromObject(describeResult.Results());
        return objects;
    }

    //public async Task GenerateFileAsync(string filePath)
    //{
    //    var path = Path.Combine(ProjectDir, InstanceName, filePath);
    //    Directory.CreateDirectory(Directory.GetParent(path).FullName);
    //    Logger?.Invoke($"Writing to {path}.");
    //    using (var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
    //    {
    //        using (var writer = new StreamWriter(stream))
    //        {
    //            await writer.WriteAsync(GenerationEnvironment.ToString()).ConfigureAwait(false);
    //        }
    //    }
    //    GenerationEnvironment.Clear();
    //}

    //public async Task WriteJsonAsync(string objName, JToken objDescribe)
    //{
    //    GenerationEnvironment.Append(objDescribe);
    //    await GenerateFileAsync("Json\\" + objName + ".json").ConfigureAwait(false);
    //}
    
    public async Task<string> GenerateAsync(DNFClient client, string objNamespace)
    {
        var objects = await RetreiveSObjectsAsync(client);
        
        GenerationEnvironment.Append(@"using DotNetForce;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ").Append(objNamespace).Append(@"
{");
		GenerateSchema(objNamespace, objects);

		foreach (var prop in objects.Properties())
		{
			// await WriteJsonAsync(prop.Name, prop.Value).ConfigureAwait(false);
			GenerateObject(objNamespace, prop.Name, prop.Value);
		}
        GenerationEnvironment.Append(@"
}");
        return GenerationEnvironment.ToString();
    }

    public void GenerateSchema(string objNamespace, JObject objects)
    {
        GenerationEnvironment.Append(@"
	public class Schema
	{
		private static Schema _Instance { get; set; }
		public static Schema Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Schema();
                }
                return _Instance;
            }
        }
        public static T Of<T>(Func<Schema, T> func) { return func(Instance); }
        public static void Of<T>(Func<Schema, T> func, Action<T> action) { action(func(Instance)); }
        public static TOut Of<T, TOut>(Func<Schema, T> func, Func<T, TOut> getter) { return getter(func(Instance)); }
        public static JObjectWrapper Wrap(JObject obj) { return new JObjectWrapper(obj); }
        public static async Task<IEnumerable<JObjectWrapper>> Wrap(Task<IEnumerable<JObject>> objs) { return Wrap(await objs); }
        public static IEnumerable<JObjectWrapper> Wrap(IEnumerable<JObject> objs) { return objs?.Select(o => new JObjectWrapper(o)); }

");
		foreach (var objName in objects.Properties())
		{
            GenerationEnvironment.Append(@"
		public Sf").Append(objName).Append(@" ").Append(objName).Append(@" { get { return new Sf").Append(objName).Append(@"(); } }
");
		}
        GenerationEnvironment.Append(@"
	}
");
		//await GenerateFileAsync("Schema.cs").ConfigureAwait(false);
    }

    public void GenerateObject(string objNamespace, string objName, JToken objDescribe)
    {
        GenerationEnvironment.Append(@"
	public class Sf").Append(objName).Append(@" : SfObjectBase
	{
		public Sf").Append(objName).Append(@"() : base("""") { }
		public Sf").Append(objName).Append(@"(string path) : base(path) { }
");
        var references = new List<JToken>();


        if (objDescribe["fields"]?.Type == JTokenType.Array)
        {
            foreach (var field in objDescribe["fields"])
            {
                var fieldName = field["name"]?.ToString();

                if (string.IsNullOrEmpty(fieldName))
                {
                    continue;
                }

                switch (field["type"]?.ToString() ?? "")
                {
                    case "address":
                        GenerationEnvironment.Append(@"
		public SfAddressField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfAddressField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "anyType":
                        GenerationEnvironment.Append(@"
		public SfAnyTypeField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfAnyTypeField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "base64":
                        GenerationEnvironment.Append(@"
		public SfBase64Field<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfBase64Field<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "boolean":
                        GenerationEnvironment.Append(@"
		public SfBooleanField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfBooleanField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "combobox":
                        GenerationEnvironment.Append(@"
		public SfComboBoxField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfComboBoxField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@", ");
                        OutputPicklistDefaultValue(field);
                        GenerationEnvironment.Append(@", ");
                        OutputPicklists(field);
                        GenerationEnvironment.Append(@"); } }
");
                        break;
                    case "complexvalue":
                        GenerationEnvironment.Append(@"
		public SfComplexValueField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfComplexValueField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "currency":
                        GenerationEnvironment.Append(@"
		public SfCurrencyField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfCurrencyField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "date":
                        GenerationEnvironment.Append(@"
		public SfDateField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfDateField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "datetime":
                        GenerationEnvironment.Append(@"
		public SfDateTimeField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfDateTimeField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "double":
                        GenerationEnvironment.Append(@"
		public SfDoubleField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfDoubleField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "email":
                        GenerationEnvironment.Append(@"
		public SfEmailField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfEmailField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "id":
                        GenerationEnvironment.Append(@"
		public SfIdField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfIdField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "int":
                        GenerationEnvironment.Append(@"
		public SfIntField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfIntField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "location":
                        GenerationEnvironment.Append(@"
		public SfLocationField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfLocationField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "multipicklist":
                        GenerationEnvironment.Append(@"
		public SfMultiPicklistField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfMultiPicklistField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@", ");
                        OutputPicklistDefaultValue(field);
                        GenerationEnvironment.Append(@", ");
                        OutputPicklists(field);
                        GenerationEnvironment.Append(@"); } }
");
                        break;
                    case "percent":
                        GenerationEnvironment.Append(@"
		public SfPercentField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfPercentField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "phone":
                        GenerationEnvironment.Append(@"
		public SfPhoneField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfPhoneField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "picklist":
                        GenerationEnvironment.Append(@"
		public SfPicklistField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfPicklistField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@", ");
                        OutputPicklistDefaultValue(field);
                        GenerationEnvironment.Append(@", ");
                        OutputPicklists(field);
                        GenerationEnvironment.Append(@"); } }
");
                        break;
                    case "reference":
                        if (field["referenceTo"].Count() != 1)
                        {
                            GenerationEnvironment.Append(@"
	/* referenceTo.Count != 1 ").Append(field["referenceTo"]).Append(@" */");
                            continue;
                        }
                        var relationshipName = field["relationshipName"]?.ToString();
                        var referenceTo = field["referenceTo"]?[0]?.ToString();

                        if (string.IsNullOrEmpty(relationshipName) || string.IsNullOrEmpty(referenceTo))
                        {
                            continue;
                        }
                        references.Add(field);
                        GenerationEnvironment.Append(@"
		public SfIdField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfIdField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "string":
                        GenerationEnvironment.Append(@"
		public SfStringField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfStringField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "textarea":
                        GenerationEnvironment.Append(@"
		public SfTextAreaField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfTextAreaField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "time":
                        GenerationEnvironment.Append(@"
		public SfTimeField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfTimeField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    case "url":
                        GenerationEnvironment.Append(@"
		public SfUrlField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfUrlField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                    default:
                        GenerationEnvironment.Append(@"
		/* unknown type: ").Append(field["type"]?.ToString() ?? "").Append(@" */
		public SfTextField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get { return new SfTextField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); } }
");
                        break;
                }
            }
        }

        if (references.Count > 0)
        {
            GenerationEnvironment.Append(@"
#region References
");

            foreach (var field in references)
            {
                var fieldName = field["name"]?.ToString();
                var relationshipName = field["relationshipName"]?.ToString();
                var referenceTo = field["referenceTo"]?[0]?.ToString();
                GenerationEnvironment.Append(@"
		public Sf").Append(referenceTo).Append(@" ").Append(relationshipName).Append(@" { get { return new Sf").Append(referenceTo).Append(@"(").Append(FormatPath(relationshipName)).Append(@"); } }
");
            }
            GenerationEnvironment.Append(@"
#endregion References
");
        }

        var childRelationships = new List<JToken>();

        if (objDescribe["childRelationships"]?.Type == JTokenType.Array && objDescribe["childRelationships"].Count() > 0)
        {
            foreach (var childRelationship in objDescribe["childRelationships"])
            {
                var relationshipName = childRelationship["relationshipName"]?.ToString();
                var childSObject = childRelationship["childSObject"]?.ToString();

                if (string.IsNullOrEmpty(relationshipName) || string.IsNullOrEmpty(childSObject))
                {
                    continue;
                }

                childRelationships.Add(childRelationship);
            }
        }

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

    string FormatPath(string fieldName)
    {
        return "string.IsNullOrEmpty(_Path) ? " + EncodeJson(fieldName) + " : _Path + " + EncodeJson("." + fieldName);
    }

    protected string EncodeJson(object obj) => JsonConvert.SerializeObject(obj);

    protected void OutputPicklistDefaultValue(JToken field)
    {
        var picklistValues = field["picklistValues"];
        if (picklistValues?.Any() == true)
        {
            foreach (var picklist in picklistValues)
            {
                if ((bool?)picklist["active"] == true && (bool?)picklist["defaultValue"] == true)
                {
                    GenerationEnvironment.Append(EncodeJson(picklist["value"].ToString()));
                    return;
                }
            }
        }
        GenerationEnvironment.Append(@"null");
    }

    protected void OutputPicklists(JToken field)
    {
        GenerationEnvironment.Append(@"new SfPicklistValue[] {");
        var picklistValues = field["picklistValues"];
        if (picklistValues?.Any() == true)
        {
            foreach (var picklist in picklistValues)
            {
                if ((bool?)picklist["active"] == true)
                {
                    GenerationEnvironment.Append(@" new SfPicklistValue(").Append(EncodeJson(picklist["value"].ToString())).Append(@", ").Append(EncodeJson(picklist["label"]?.ToString() ?? picklist["value"].ToString())).Append(@"),");
                }
            }
        }
        GenerationEnvironment.Append(@"}");
    }

    public override string ToString()
    {
        return GenerationEnvironment.ToString();
    }
}