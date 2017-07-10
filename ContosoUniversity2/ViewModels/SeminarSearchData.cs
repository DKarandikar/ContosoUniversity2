using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity2.ViewModels
{
    public class SeminarSearchData
    {

        [Column("Date and Time")]
        public string DateAndTime { get; set; }
        public string Title{ get; set; }
        [Column("Full Names")]
        [DataType(DataType.MultilineText)]
        public string FullNames { get; set; }
        public string Location { get; set; }
        public string Instructor { get; set; }
        public int SeminarID { get; set; }
    }
}