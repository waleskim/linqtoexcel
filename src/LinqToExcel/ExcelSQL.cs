﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System.Data.OleDb;
using log4net;
using System.Reflection;
using LinqToExcel.Extensions.Reflection;
using System.Data;

namespace LinqToExcel
{
    public class ExcelSQL
    {
        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Executes the query based upon the Linq statement against the Excel worksheet
        /// </summary>
        /// <param name="expression">Expression created from the Linq statement</param>
        /// <param name="fileName">File path to the Excel workbook</param>
        /// <param name="columnMapping">
        /// Property to column mapping. 
        /// Properties are the dictionary keys and the dictionary values are the corresponding column names.
        /// </param>
        /// <returns>Returns the results from the query</returns>
        public object ExecuteQuery(Expression expression, string fileName, Dictionary<string, string> columnMapping)
        {
            Type dataType = expression.Type.GetGenericArguments()[0];
            PropertyInfo[] props = dataType.GetProperties();

            //Build the SQL string
            ExpressionToSQL sql = new ExpressionToSQL();
            sql.BuildSQLStatement(expression, columnMapping);

            string connString = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties= ""Excel 8.0;HDR=YES;""", fileName);
            if (_log.IsDebugEnabled) _log.Debug("Connection String: " + connString);

            object results = Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType));
            using (OleDbConnection conn = new OleDbConnection(connString))
            using (OleDbCommand command = conn.CreateCommand())
            {                
                conn.Open();
                command.CommandText = sql.SQLStatement;
                command.Parameters.Clear();
                command.Parameters.AddRange(sql.Parameters.ToArray());
                OleDbDataReader data = command.ExecuteReader();
                
                //Get the excel column names
                List<string> columns = new List<string>();
                DataTable sheetSchema = data.GetSchemaTable();
                foreach (DataRow row in sheetSchema.Rows)
                    columns.Add(row["ColumnName"].ToString());

                while (data.Read())
                {
                    object result = Activator.CreateInstance(dataType);
                    foreach (PropertyInfo prop in props)
                    {
                        //Set the column name to the property mapping if there is one, else use the property name for the column name
                        string columnName = (columnMapping.ContainsKey(prop.Name)) ? columnMapping[prop.Name] : prop.Name;
                        if (columns.Contains(columnName))
                            result.SetProperty(prop.Name, Convert.ChangeType(data[columnName], prop.PropertyType));
                        else if (columnMapping.ContainsKey(prop.Name))
                        {
                            //Logging a warning for a property with a column mapping that does not exist in the excel worksheet
                            _log.Warn(string.Format("'{0}' column that is mapped to the '{1}' property does not exist in the '{2}' worksheet",
                                                    columnName, prop.Name, "Sheet1"));
                        }
                    }
                    results.CallMethod("Add", result);
                }
            }
            return results;
        }
    }
}
