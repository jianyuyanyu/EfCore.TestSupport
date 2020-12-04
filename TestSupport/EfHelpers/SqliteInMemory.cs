﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestSupport.EfHelpers.Internal;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This static class contains extension methods to use with in-memory Sqlite databases
    /// </summary>
    public static class SqliteInMemory
    {
        /// <summary>
        /// Created a Sqlite Options for in-memory database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptionsDisposable<T> CreateOptions<T>(Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            return new DbContextOptionsDisposable<T>(SetupConnectionAndBuilderOptions<T>(builder)
                .Options);
        }

        /// <summary>
        /// Created a Sqlite Options for in-memory database while capturing EF Core's logging output. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="efLog">This is a method that receives a LogOutput whenever EF Core logs something</param>
        /// <param name="logLevel">Optional: Sets the logLevel you want to capture. Defaults to Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        [Obsolete("Suggest using CreateOptionsWithLogTo<T> which gives more logging options")]
        public static DbContextOptionsDisposable<T> CreateOptionsWithLogging<T>(Action<LogOutput> efLog,
            LogLevel logLevel = LogLevel.Information, Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            return new DbContextOptionsDisposable<T>(
                SetupConnectionAndBuilderOptions<T>(builder)
                    .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(efLog, logLevel) }))
                .Options);
        }

        /// <summary>
        /// Created a Sqlite Options for in-memory database while using LogTo to get the EF Core logging output. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logAction">This action is called with each log output</param>
        /// <param name="logToOptions">Optional: This allows you to define what logs you want and what format. Defaults to LogLevel.Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptionsDisposable<T> CreateOptionsWithLogTo<T>(Action<string> logAction,
            LogToOptions logToOptions = null , Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            if (logAction == null) throw new ArgumentNullException(nameof(logAction));

            return new DbContextOptionsDisposable<T>(
                SetupConnectionAndBuilderOptions(builder)
                    .AddLogTo(logAction, logToOptions)
                    .Options);
        }


        /// <summary>
        /// Created a Sqlite Options for in-memory database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static DbContextOptionsBuilder<T> SetupConnectionAndBuilderOptions<T>(Action<DbContextOptionsBuilder<T>> applyExtraOption) //#A
            where T : DbContext
        {
            //Thanks to https://www.scottbrady91.com/Entity-Framework/Entity-Framework-Core-In-Memory-Testing
            var connectionStringBuilder =         //#B
                new SqliteConnectionStringBuilder //#B
                    { DataSource = ":memory:" };  //#B
            var connectionString = connectionStringBuilder.ToString(); //#C
            var connection = new SqliteConnection(connectionString); //#D
            connection.Open();  //#E             //see https://github.com/aspnet/EntityFramework/issues/6968

            // create in-memory context
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseSqlite(connection); //#F
            builder.ApplyOtherOptionSettings(); //#G
            applyExtraOption?.Invoke(builder);

            return builder; //#H
        }

        /****************************************************************
        #A By default it will throw an exception if a QueryClientEvaluationWarning is logged (see section 15.8). You can turn this off by providing a value of false as a parameter
        #B Creates a SQLite connection string with the DataSource set to ":memory:"
        #C Turns the SQLiteConnectionStringBuilder into a string 
        #D Forms a SQLite connection using the connection string
        #E You must open the SQLite connection. If you don't, the in-memory database doesn't work.
        #F Builds a DbContextOptions<T> with the SQLite database provider and the open connection
        #G Calls a general method used on all your option builders. This enables sensitive logging so you get more information, and if throwOnClientServerWarning is true, it configures the warning to throw on a QueryClientEvaluationWarning being logged
        #H Returns the DbContextOptions<T> to use in the creation of your application's DbContext
         * **************************************************************/
    }
}