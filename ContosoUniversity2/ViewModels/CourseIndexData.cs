using System.Collections.Generic;
using ContosoUniversity2.Models;

namespace ContosoUniversity2.ViewModels
{
    public class CourseIndexData
    {
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<Enrollment> Enrollments { get; set; }
    }
}