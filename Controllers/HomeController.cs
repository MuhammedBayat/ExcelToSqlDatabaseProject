using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExcelToSqlDatabaseProject.Models;
using Microsoft.AspNetCore.Hosting;
using ExceltoDatabaseProject.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Data;
using ExceltoDatabaseProject.Operations;
using ExceltoDatabaseProject.ViewModels.Home;
using System.Data.SqlClient;

namespace ExcelToSqlDatabaseProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment hostingEnvironment;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            hostingEnvironment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new TransferInfos());
        }
        [HttpPost]
        public IActionResult Index(TransferInfos transferInfos)
        {
            string filePath = SaveExcelFile(transferInfos.ExcelFile);



            //DataTable excelDataTable = new DataTable();

            //new SavingDatabaseOperations().ExcelFileOperations(filePath, excelDataTable);
            //DataColumnCollection collection = excelDataTable.Columns;

            //ViewBag.ExcelColumnList = excelTableColumnList;

            return Redirect("match-columns?file=" + transferInfos.ExcelFile.FileName);
        }

        private string SaveExcelFile(IFormFile excelFile)
        {
            string excelFilesDirectory = Path.Combine(hostingEnvironment.ContentRootPath, "ExcelFiles");
            string excelFilePath = Path.Combine(excelFilesDirectory, excelFile.FileName);
            FileInfo fileInfo = new FileInfo(excelFilePath);
            if (!fileInfo.Exists)
            {
                using (FileStream stream = new FileStream(path: excelFilePath, FileMode.CreateNew))
                {
                    excelFile.CopyTo(stream);
                }
            }
            return excelFilePath;
        }
        public string GetExcelFileDirectory(string fileName)
        {
            string excelFilesDirectory = Path.Combine(hostingEnvironment.ContentRootPath, "ExcelFiles");
            string excelFilePath = Path.Combine(excelFilesDirectory, fileName);
            return excelFilePath;
        }

        public void GetExcelFileColumns()
        {

        }
        public IActionResult MatchColumns()
        {
            if (!string.IsNullOrEmpty(Request.Query["file"]))
            {
                DataTable excelDataTable = new DataTable();
                string filePath = GetExcelFileDirectory(Request.Query["file"]);
                new SavingDatabaseOperations().ExcelFileOperations(filePath, excelDataTable);
                List<string> excelTableColumnList = new List<string>();
                foreach (DataColumn item in excelDataTable.Columns)
                {
                    excelTableColumnList.Add(item.ColumnName);
                }

                var model = new MatchColumnViewModel
                {
                    ExcelColumnList = excelTableColumnList
                };
                return View(model);
            }
            else
            {
                return Redirect("/");
            }
        }

        public JsonResult CheckConnection(string serverName, string UserName, string password)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(@"Server=" + serverName + ";User ID=" + UserName + ";Password=" + password))
                {
                    con.Open();
                    return new JsonResult(new { result = true, message = "Bağlantı Sağlandı" });
                }

            }
            catch (Exception)
            {
                return new JsonResult(new { result = true, message = "Geçersiz Bağlantı" });
            }
        }
        public JsonResult GetDatabases(string serverName, string UserName, string password)
        {
            string connectionString = string.Format("Data Source=" + serverName + ";User ID={0};Password={1};", UserName, password);
            List<string> tableList = new List<string>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                tableList.Add(dr[0].ToString());
                            }
                        }
                    }
                    con.Close();
                    return new JsonResult(new { result = true, data = tableList });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { result = true, data = ex.Message });
            }
        }
        public JsonResult GetTables(string serverName, string UserName, string password, string database)
        {
            string connectionString = string.Format("Data Source=" + serverName + ";User ID={0};Password={1};", UserName, password);
            List<string> tableList = new List<string>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string getTablesCommand = "USE [" + database + @"]
                                                SELECT TABLE_NAME
                                                FROM INFORMATION_SCHEMA.TABLES
                                                WHERE TABLE_TYPE = 'BASE TABLE'
                                                order by TABLE_NAME";
                    using (SqlCommand cmd = new SqlCommand(getTablesCommand, con))
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                tableList.Add(dr[0].ToString());
                            }
                        }
                    }
                    con.Close();
                    return new JsonResult(new { result = true, data = tableList });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { result = true, data = ex.Message });
            }
        }
        public JsonResult GetTableColumns(string serverName, string UserName, string password, string database, string tableName)
        {
            string connectionString = string.Format("Data Source=" + serverName + ";User ID={0};Password={1};", UserName, password);
            List<string> tableColumnList = new List<string>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string getTablesCommand = @"USE [" + database + @"] SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('" + tableName + "')";
                    using (SqlCommand cmd = new SqlCommand(getTablesCommand, con))
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                tableColumnList.Add(dr[0].ToString());
                            }
                        }
                    }
                    con.Close();
                    return new JsonResult(new { result = true, data = tableColumnList });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { result = true, data = ex.Message });
            }
        }
        public JsonResult TransferDatabase(string serverName, string userName, string password, string database, string selectingTable, string fileName, List<string> excelColumns, List<string> sqlColumns)
        {
            List<ExcelToSqlMapping> excelToSqlMappingList = new List<ExcelToSqlMapping>();
            for (int i = 0; i < excelColumns.Count; i++)
            {
                excelToSqlMappingList.Add(new ExcelToSqlMapping { ExcelColumnName = excelColumns[i], SqlColumnName = sqlColumns[i] });
            }
            string message = "";
            bool transferResult = false;
            try
            {
                transferResult = new SavingDatabaseOperations().ExcelToDatabaseTransfer(
                excelFilePath: GetExcelFileDirectory(fileName),
                Server: serverName,
                Database: database,
                User: userName,
                Password: password,
                tableName: selectingTable,
                excelToSqlMappingList: excelToSqlMappingList);
                message = "Data Transferred Successfully";
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return new JsonResult(new { result = transferResult ? true : false, data = "" });
        }

















        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}