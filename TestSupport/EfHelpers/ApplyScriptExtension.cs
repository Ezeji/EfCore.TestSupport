﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// Static class holding extension methods for applying SQL scripts to a database
    /// </summary>
    public static class ApplyScriptExtension
    {
        /// <summary>
        /// This reads in a SQL script file and executes each command to the database pointed at by the DbContext
        /// Each command should have an GO at the start of the line after the command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath"></param>
        public static void ExecuteScriptFileInTransaction(this DbContext context, string filePath)
        {
            var scriptContent = File.ReadAllText(filePath);
            var regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var commands = regex.Split(scriptContent).Select(x => x.Trim());

            using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                foreach (var command in commands)
                {
                    if (command.Length > 0)
                    {
                        try
                        {
                            context.Database.ExecuteSqlRaw(command);
                        }
                        catch (SqlException)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                transaction.Commit();
            }
        }
    }
}