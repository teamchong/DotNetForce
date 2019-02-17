using CsvHelper;
using DotNetForce;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Plugin.Clipboard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace DataConvertForInspector
{
    class Program
    {
        static void Main(string[] args)
        {
            //var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
            //var connectionString = config["ConnectionString"]?.Trim();
            //Debug.Assert(!string.IsNullOrEmpty(connectionString), "ConnectionString is empty");

            var app = new CommandLineApplication();
            app.HelpOption("-?|-h|--help");
            app.Command("sql2csv", cmdApp =>
            {
                var sqlArgs = cmdApp.Argument("-sql", "source SQL file", true);
                var dOpt = cmdApp.Option("-d", "Delimiter", CommandOptionType.SingleValue);
                var eOpt = cmdApp.Option("-e", "Encoding", CommandOptionType.SingleValue);
                cmdApp.OnExecute(() => sql2csv(sqlArgs.Values, dOpt.Value(), eOpt.Value()));
            });
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });
            app.Execute(args);
        }

        static async Task<int> sql2csv(List<string> sqlFiles, string delimiter, string encoding)
        {
            return await sqlFiles.ToObservable().Select(sqlFile =>
                Observable.FromAsync(() => File.ReadAllLinesAsync(sqlFile))
                .SelectMany(fileLines => Observable.Defer(() =>
                {
                    var connectionString = fileLines.FirstOrDefault()?.TrimStart('-').Trim();
                    var cmdText = string.Join(Environment.NewLine, fileLines.Skip(1));
                    var outputFile = Path.Combine(Environment.CurrentDirectory, $"{Path.GetFileNameWithoutExtension(sqlFile)}.csv");

                    return Observable.Using(() => new SqlConnection(connectionString), conn =>
                    Observable.Start(() => Console.WriteLine($"Connecting to {connectionString}"))
                    .SelectMany(_ => Observable.FromAsync(() => conn.OpenAsync()))
                    .SelectMany(_ => Observable.Start(() => Console.WriteLine($"Connected executing sql\n{cmdText}")))
                    .SelectMany(_ => Observable.FromAsync(async () =>
                    {
                        var cmd = new SqlCommand(cmdText, conn);
                        var readerRes = await cmd.ExecuteReaderAsync();
                        return Observable.Using(() => readerRes, reader =>
                            Observable.Start(() => Console.WriteLine($"Outputing result to\n{outputFile}"))
                            .SelectMany(_1 => Observable.Using(() => new StreamWriter(outputFile), streamWriter => Observable.Defer(() =>
                            {
                                var csvConfig = new CsvHelper.Configuration.Configuration();

                                if (!string.IsNullOrEmpty(delimiter))
                                {
                                    csvConfig.Delimiter = "\t";
                                }

                                if (!string.IsNullOrWhiteSpace(encoding))
                                {
                                    csvConfig.Encoding = System.Text.Encoding.GetEncoding(encoding);
                                }

                                return Observable.Using(() => new CsvWriter(streamWriter, csvConfig),
                                    csvWriter => Observable.FromAsync(() => reader.ReadAsync())
                                    .TakeWhile(hasValue => hasValue)
                                    .SelectMany(_2 => Observable.Start(() =>
                                    {
                                        var values = Observable.Range(0, reader.FieldCount).Select(i => 
                                            Observable.FromAsync(() => reader.IsDBNullAsync(i)).SelectMany(isDBNull =>
                                            {
                                                var fieldType = reader.GetFieldType(i);
                                                switch (fieldType)
                                                {
                                                    case var type when type == typeof(bool):
                                                        return isDBNull ? "" : reader.GetBoolean(i) ? "TRUE" : "FALSE";
                                                    case var type when type == typeof(byte):
                                                        return isDBNull ? "" : Convert.ToBase64String(new [] { reader.GetByte(i) });
                                                    case var type when type == typeof(byte[]):
                                                        return isDBNull ? "" : Convert.ToBase64String(new [] {  });
                                                    case var type when type == typeof(long):
                                                        return isDBNull ? "" : reader.GetInt64(i).ToString();
                                                    case var type when type == typeof(int):
                                                        return isDBNull ? "" : reader.GetInt32(i).ToString();
                                                    case var type when type == typeof(short):
                                                        return isDBNull ? "" : reader.GetInt16(i).ToString();
                                                    case var type when type == typeof(float):
                                                        return isDBNull ? "" : reader.GetFloat(i).ToString();
                                                    case var type when type == typeof(double):
                                                        return isDBNull ? "" : reader.GetDouble(i).ToString();
                                                    case var type when type == typeof(decimal):
                                                        return isDBNull ? "" : reader.GetDecimal(i).ToString();
                                                    default:
                                                        break;
                                                }
                                            })
                                        );;
                                    }))
                                );
                            })))
                        );
                    })));
                })
            ))
            .Count()
            .Select(_ => 0);
        }
    }
}
