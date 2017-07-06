using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity2.Models
{
    public class Seminar
    {
        public int SeminarID { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Seminar Time")]
        public DateTime SeminarTime { get; set; }

        public int SeminarLength { get; set; }

        public int CourseID { get; set; }

        public virtual Course Course { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}