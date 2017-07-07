using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using ContosoUniversity2.Models;

namespace ContosoUniversity2.ViewModels
{
    public class CourseStatisticsTwoQueryData
    {
        public IEnumerable<CourseStatisticsData> MainQuery { get; set; }
        public IEnumerable<CourseStatisticsData> SecondQuery { get; set; }

    }
}