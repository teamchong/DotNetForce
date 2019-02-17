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
                cmdApp.OnExecute(() => sql2csv(sqlArgs.Values));
            });
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });
            app.Execute(args);
        }

        static async Task<int> sql2csv(List<string> sqlFiles)
        {
            foreach (var sqlFile in sqlFiles)
            {
                var fileLines = await File.ReadAllLinesAsync(sqlFile);
                var connectionString = fileLines.FirstOrDefault()?.TrimStart('-').Trim();
                var cmdText = string.Join(Environment.NewLine, fileLines.Skip(1));
                var outputFile = Path.Combine(Environment.CurrentDirectory, $"{Path.GetFileNameWithoutExtension(sqlFile)}.csv");

                using (var conn = new SqlConnection(connectionString))
                {
                    Console.WriteLine($"Connecting to {connectionString}");
                    await conn.OpenAsync();

                    Console.WriteLine($"Connected executing sql\n{cmdText}");
                    var cmd = new SqlCommand(cmdText, conn);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Console.WriteLine($"Outputing result to\n{outputFile}");
                        using (var streamWriter = new StreamWriter(outputFile))
                        {
                            var csvConfig = new CsvHelper.Configuration.Configuration();
                            csvConfig.Delimiter = "\t";
                            using (var csvWriter = new CsvWriter(streamWriter, csvConfig))
                            {

                            }
                        }
                    }
                }
            }

            return 0;
        }
}
}
