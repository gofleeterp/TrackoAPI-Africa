using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.IO;
namespace Repository.Pattern.Ef6.DbInterceptors
{

    internal class SqlSyntaxLogger : IDbCommandInterceptor
    {
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogTSqlStatement("NonQueryExecuted",
                String.Format(" IsAsync: {0}, Command Text: {1}", interceptionContext.IsAsync, command.CommandText));
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogTSqlStatement("NonQueryExecuting",
                String.Format(" IsAsync: {0}, Command Text: {1}", interceptionContext.IsAsync, command.CommandText));
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogTSqlStatement("ReaderExecuted",
                String.Format(" IsAsync: {0}, Command Text: {1}", interceptionContext.IsAsync, command.CommandText));
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogTSqlStatement("ReaderExecuting",
                String.Format(" IsAsync: {0}, Command Text: {1}", interceptionContext.IsAsync, command.CommandText));
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogTSqlStatement("ScalarExecuted",
                String.Format(" IsAsync: {0}, Command Text: {1}", interceptionContext.IsAsync, command.CommandText));
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogTSqlStatement("ScalarExecuting",
                String.Format(" IsAsync: {0}, Command Text: {1}", interceptionContext.IsAsync, command.CommandText));
        }

        private void LogTSqlStatement(string command, string commandText)
        {
            // Creates the log file in your desktop.
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            using (var file = new StreamWriter(path + @"\tsql-log.txt", true))
            {
                file.WriteLine("");
                file.WriteLine("Intercepted on: {0} :- {1} ", command, commandText);
            }
        }
    }
}
