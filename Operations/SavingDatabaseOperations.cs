using ExceltoDatabaseProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace ExceltoDatabaseProject.Operations
{
    public class SavingDatabaseOperations
    {
        public bool ExcelToDatabaseTransfer(string excelFilePath, string Server, string Database, string User, string Password, string tableName, List<ExcelToSqlMapping> excelToSqlMappingList)
        {
            if (!string.IsNullOrEmpty(excelFilePath))
            {
                DataTable dataTable = new DataTable();

                ExcelFileOperations(excelFilePath, dataTable);

                string databaseConnectionString = ("server = " + Server + "; database = " + Database + "; User ID = " + User + "; Password = " + Password);

                using (SqlConnection connection = new SqlConnection(databaseConnectionString))
                {
                    using SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection)
                    {
                        DestinationTableName = tableName
                    };

                    foreach (var excelToSqlMap in excelToSqlMappingList)
                    {
                        sqlBulkCopy.ColumnMappings.Add(excelToSqlMap.ExcelColumnName, excelToSqlMap.SqlColumnName);
                    }

                    connection.Open();
                    sqlBulkCopy.WriteToServer(dataTable);
                    connection.Close();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void ExcelFileOperations(string path, DataTable dataTable)
        {
            string constr = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0 Xml;HDR=YES;""", path);
            using OleDbConnection excelConnection = new OleDbConnection(constr);
            using OleDbCommand excelCommand = new OleDbCommand();
            using OleDbDataAdapter oleDbDataAdapterExcel = new OleDbDataAdapter();
            excelCommand.Connection = excelConnection;

            excelConnection.Open();
            DataTable dtExcelSchema;
            dtExcelSchema = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
            excelConnection.Close();

            /*excelConnection.Open();*/     //SORGU DUZENLENECEK TABLO İSMİ LİST OLARAK DÖNDÜRÜLECEK
            excelCommand.CommandText = "Select * FROM[" + sheetName + "]";//excel deki tablo satır isimleri tanımlandığı yer
            oleDbDataAdapterExcel.SelectCommand = excelCommand;
            oleDbDataAdapterExcel.Fill(dataTable);
        }

    }
}