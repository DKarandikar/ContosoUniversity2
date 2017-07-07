using System;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity2.ViewModels
{
    public class CourseStatisticsData
    {

        public string CourseTitle { get; set; }
        public int CourseID { get; set; }

        public int StudentCount { get; set; }
    }
}