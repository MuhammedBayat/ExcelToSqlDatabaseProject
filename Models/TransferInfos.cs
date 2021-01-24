using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelToSqlDatabaseProject.Models
{
    public class TransferInfos
    {
        public IFormFile ExcelFile { get; set; }
    }
}
