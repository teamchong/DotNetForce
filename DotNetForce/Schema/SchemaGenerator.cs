using DotNetForce;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class SchemaGenerator
{
    public string ProjectDir { get; set; }
    public string InstanceName { get; set; }
    public StringBuilder GenerationEnvironment { get; set; }
    public Action<string> Logger { get; set; }
    public Action<string> ErrorLogger { get; set; }

    public SchemaGenerator(string projectDir, string instanceName, StringBuilder generationEnvironment, Action<string> logger, Action<string> errorLogger)
    {
        ProjectDir = projectDir;
        InstanceName = instanceName;
        GenerationEnvironment = generationEnvironment;
        Logger = logger;
        ErrorLogger = errorLogger;
    }

    public JObject ReadProfile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        using (var jsonReader = new JsonTextReader(reader))
                        {
                            return new JsonSerializer().Deserialize<JObject>(jsonReader);
                        }
                    }
                }
            }
        }
        catch { }
        return null;
    }

    public async Task<JObject> GenerateAsync(
        Uri loginUri,
        string clientId,
        string clientSecret,
        string userName,
        string password)
    {
        var folder = Path.Combine(ProjectDir, InstanceName);
        var solutionDir = Path.GetDirectoryName(ProjectDir);

        Logger?.Invoke($"instance: {InstanceName}");
        Logger?.Invoke($"projectDir: {ProjectDir}");
        Logger?.Invoke($"solutionDir: {solutionDir}");
        Logger?.Invoke($"folder: {folder}");

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        foreach (var dir in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
        {
            Directory.Delete(dir, true);
        }
        foreach (var file in Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly))
        {
            File.Delete(file);
        }

        var client = await DNFClient.LoginAsync(loginUri, clientId, clientSecret, userName, password, Logger).ConfigureAwait(false);
        var describeGlobalResult = await client.GetObjectsAsync<JObject>().ConfigureAwait(false);

        var request = new CompositeRequest();
        foreach (var sobject in describeGlobalResult.SObjects)
        {
            var objectName = sobject["name"]?.ToString();
            if ((bool?)sobject["deprecatedAndHidden"] == true)
            {
                continue;
            }
            Logger?.Invoke(objectName);
            request.Describe(objectName, objectName);
        }

        var describeResult = await client.Composite.PostAsync(request).ConfigureAwait(false);

        foreach (var error in describeResult.Errors())
        {
            Logger?.Invoke($"{error.Key}\n{error.Value}");
        }

        var objects = JObject.FromObject(describeResult.Results());
        return objects;
    }

    public async Task GenerateFileAsync(string filePath)
    {
        var path = Path.Combine(ProjectDir, InstanceName, filePath);
        Directory.CreateDirectory(Directory.GetParent(path).FullName);
        Logger?.Invoke($"Writing to {path}.");
        using (var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(GenerationEnvironment.ToString()).ConfigureAwait(false);
            }
        }
        GenerationEnvironment.Clear();
    }

    public async Task WriteJsonAsync(string objName, JToken objDescribe)
    {
        GenerationEnvironment.Append(objDescribe);
        await GenerateFileAsync("Json\\" + objName + ".json").ConfigureAwait(false);
    }

    public async Task WriteObjectAsync(string objNamespace, string objName, JToken objDescribe)
    {
        GenerationEnvironment.Append(@"using DotNetForce;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;

namespace ").Append(objNamespace).Append(@"
{
	public class Sf").Append(objName).Append(@" : SfObjectBase
	{
		public Sf").Append(objName).Append(@"() : base("""") { }
		public Sf").Append(objName).Append(@"(string path) : base(path) { }
");
        var references = new JArray();


        if (objDescribe["fields"]?.Type == JTokenType.Array)
        {
            foreach (var field in ((JArray)objDescribe["fields"]))
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
		public SfAddressField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfAddressField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "anyType":
                        GenerationEnvironment.Append(@"
		public SfAnyTypeField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfAnyTypeField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "base64":
                        GenerationEnvironment.Append(@"
		public SfBase64Field<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfBase64Field<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "boolean":
                        GenerationEnvironment.Append(@"
		public SfBooleanField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfBooleanField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "combobox":
                        GenerationEnvironment.Append(@"
		public SfComboBoxField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfComboBoxField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@", ");
                        OutputPicklistDefaultValue(field);
                        GenerationEnvironment.Append(@", ");
                        OutputPicklists(field);
                        GenerationEnvironment.Append(@"); }
");
                        break;
                    case "complexvalue":
                        GenerationEnvironment.Append(@"
		public SfComplexValueField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfComplexValueField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "currency":
                        GenerationEnvironment.Append(@"
		public SfCurrencyField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfCurrencyField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "date":
                        GenerationEnvironment.Append(@"
		public SfDateField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfDateField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "datetime":
                        GenerationEnvironment.Append(@"
		public SfDateTimeField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfDateTimeField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "double":
                        GenerationEnvironment.Append(@"
		public SfDoubleField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfDoubleField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "email":
                        GenerationEnvironment.Append(@"
		public SfEmailField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfEmailField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "id":
                        GenerationEnvironment.Append(@"
		public SfIdField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfIdField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "int":
                        GenerationEnvironment.Append(@"
		public SfIntField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfIntField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "location":
                        GenerationEnvironment.Append(@"
		public SfLocationField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfLocationField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "multipicklist":
                        GenerationEnvironment.Append(@"
		public SfMultiPicklistField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfMultiPicklistField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@", ");
                        OutputPicklistDefaultValue(field);
                        GenerationEnvironment.Append(@", ");
                        OutputPicklists(field);
                        GenerationEnvironment.Append(@"); }
");
                        break;
                    case "percent":
                        GenerationEnvironment.Append(@"
		public SfPercentField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfPercentField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "phone":
                        GenerationEnvironment.Append(@"
		public SfPhoneField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfPhoneField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "picklist":
                        GenerationEnvironment.Append(@"
		public SfPicklistField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfPicklistField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@", ");
                        OutputPicklistDefaultValue(field);
                        GenerationEnvironment.Append(@", ");
                        OutputPicklists(field);
                        GenerationEnvironment.Append(@"); }
");
                        break;
                    case "reference":
                        if (((JArray)field["referenceTo"])?.Count != 1)
                        {
                            GenerationEnvironment.Append(@"
	/* referenceTo.Count != 1 ").Append(field["referenceTo"]).Append(@" */
");
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
		public SfIdField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfIdField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "string":
                        GenerationEnvironment.Append(@"
		public SfStringField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfStringField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "textarea":
                        GenerationEnvironment.Append(@"
		public SfTextAreaField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfTextAreaField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "time":
                        GenerationEnvironment.Append(@"
		public SfTimeField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfTimeField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    case "url":
                        GenerationEnvironment.Append(@"
		public SfUrlField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfUrlField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
");
                        break;
                    default:
                        GenerationEnvironment.Append(@"
		/* unknown type: ").Append(field["type"]?.ToString() ?? "").Append(@" */
		public SfTextField<Sf").Append(objName).Append(@"> ").Append(fieldName).Append(@" { get => new SfTextField<Sf").Append(objName).Append(@">(").Append(FormatPath(fieldName)).Append(@"); }
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
		public Sf").Append(referenceTo).Append(@" ").Append(relationshipName).Append(@" { get => new Sf").Append(referenceTo).Append(@"(").Append(FormatPath(relationshipName)).Append(@"); }
");
            }
            GenerationEnvironment.Append(@"

#endregion References

");
        }

        var childRelationships = new JArray();

        if (objDescribe["childRelationships"]?.Type == JTokenType.Array && ((JArray)objDescribe["childRelationships"]).Count > 0)
        {
            foreach (var childRelationship in ((JArray)objDescribe["childRelationships"]))
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
			get => new SfChildRelationship<Sf").Append(objName).Append(@", Sf").Append(childSObject).Append(@">(").Append(FormatPath(relationshipName)).Append(@");
		}
");
            }
            GenerationEnvironment.Append(@"

#endregion ChildRelationships

");
        }
        GenerationEnvironment.Append(@"

		public override string ToString() => string.IsNullOrEmpty(_Path) ? ").Append(Json(objName)).Append(@" : _Path;
	}
}
");
        await GenerateFileAsync("Sf" + objName + ".cs").ConfigureAwait(false);
    }

    string FormatPath(string fieldName)
    {
        return "string.IsNullOrEmpty(_Path) ? " + Json(fieldName) + " : _Path + " + Json("." + fieldName);
    }

    protected string Json(Object obj) => JsonConvert.SerializeObject(obj);

    protected void OutputPicklistDefaultValue(JToken field)
    {
        var picklistValues = (JArray)field["picklistValues"];
        if (picklistValues?.Count > 0)
        {
            foreach (var picklist in picklistValues)
            {
                if ((bool?)picklist["active"] == true && (bool?)picklist["defaultValue"] == true)
                {
                    GenerationEnvironment.Append(Json(picklist["value"].ToString()));
                    return;
                }
            }
        }
        GenerationEnvironment.Append(@"null");
    }

    protected void OutputPicklists(JToken field)
    {
        GenerationEnvironment.Append(@"new SfPicklistValue[] {");
        var picklistValues = (JArray)field["picklistValues"];
        if (picklistValues?.Count > 0)
        {
            foreach (var picklist in picklistValues)
            {
                if ((bool?)picklist["active"] == true)
                {
                    GenerationEnvironment.Append(@" new SfPicklistValue(").Append(Json(picklist["value"].ToString())).Append(@", ").Append(Json(picklist["label"]?.ToString() ?? picklist["value"].ToString())).Append(@"),");
                }
            }
        }
        GenerationEnvironment.Append(@"}");
    }
}