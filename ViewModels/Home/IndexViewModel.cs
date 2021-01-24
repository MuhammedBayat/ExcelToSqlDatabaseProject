using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExceltoDatabaseProject.ViewModels.Home
{
    public class IndexViewModel
    {
        public IFormFile ExcelFile { get; set; }

        public List<string> ExcelColumnList { get; set; }
    }
}
