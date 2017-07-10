using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ContosoUniversity2.DAL;
using ContosoUniversity2.Models;
using System.Data.Entity.Infrastructure;
using PagedList;
using ContosoUniversity2.ViewModels;

namespace ContosoUniversity2.Controllers
{
    public class SeminarController : Controller
    {
        const string COURSE_ERROR = "There will be an overlap for seminars on this course. Try another time.";
        const string STUDENT_ERROR = "There will be an overlap for some student on this course with another seminar they have. Try another time.";
        const string LOCATION_ERROR = "There is already a seminar scheduled at this location during this time. Try another time or location.";
        const string INSTRUCTOR_ERROR = "This instructor is already leading another seminar at this time. Try another time or instructor.";

        private SchoolContext db = new SchoolContext();

        // GET: Seminar
        public ActionResult Index(int? page, int pageSize = 5)
        {

            ViewBag.PageSize = new SelectList(new int[] { 1, 5, 10, 25, 50 }, pageSize);
            ViewBag.PageSizeCurrent = pageSize;

            int pageNumber = (page ?? 1);
            return View(db.Seminars.OrderBy(s => s.SeminarTime).ToPagedList(pageNumber, pageSize));
        }

        // GET: Seminar/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seminar seminar = db.Seminars.Find(id);
            if (seminar == null)
            {
                return HttpNotFound();
            }
            return View(seminar);
        }

        // GET: Seminar/Create
        public ActionResult Create()
        {
            PopulateCoursesDropDownList();
            return View();
        }



        // POST: Seminar/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SeminarTime, SeminarLength, CourseID, Location")] Seminar seminarToUpdate)
        {
            if (ModelState.IsValid)
            {
                // Check first whether the potential time and date overlaps with any other seminars
                Boolean overlap = false;

                // Set dateTime and end dateTime for the seminar in question
                DateTime semStart = seminarToUpdate.SeminarTime;
                DateTime semEnd = seminarToUpdate.SeminarTime.AddHours(seminarToUpdate.SeminarLength);

                foreach (Seminar s in db.Seminars.Where(s => s.CourseID == seminarToUpdate.CourseID))
                {
                    // Set dateTime and end dateTime for each seminar in the DB 
                    DateTime sStart = s.SeminarTime;
                    DateTime sEnd = s.SeminarTime.AddHours(s.SeminarLength);

                    // Check for overlap
                    if (DateOverlap(semStart, semEnd, sStart, sEnd)) { overlap = true; }
                }

                // Check for overlap with other seminars of students on this course
                Boolean overlapStudent = false;

                // Every student; if they are on this course; then for all enrollments that aren't this course; check for overlap

                foreach (Student student in db.Students.ToList())
                {
                    if (student.Enrollments.Any(enrol => enrol.CourseID == seminarToUpdate.CourseID))
                    {
                        foreach (Enrollment enrollment in student.Enrollments)
                        {
                            if (enrollment.CourseID != seminarToUpdate.CourseID)
                            {
                                foreach (Seminar s in student.Seminars.Where(sem => sem.CourseID != seminarToUpdate.CourseID))
                                {
                                    DateTime sStart = s.SeminarTime;
                                    DateTime sEnd = s.SeminarTime.AddHours(s.SeminarLength);

                                    // Check for overlap
                                    if (DateOverlap(semStart, semEnd, sStart, sEnd)) { overlapStudent = true; }
                                }
                            }
                        }
                    }
                    
                }
                // Check for overlap with other seminars in this location
                Boolean overlapLocation = false;

                foreach (Seminar s in db.Seminars)
                {
                    if ((s.Location != "") && (s.Location == seminarToUpdate.Location))
                    {
                        DateTime sStart = s.SeminarTime;
                        DateTime sEnd = s.SeminarTime.AddHours(s.SeminarLength);

                        // Check for overlap
                        if (DateOverlap(semStart, semEnd, sStart, sEnd)) { overlapLocation = true; }
                    }
                }



                // If there is an overlap, don't accept and display the appropriate error message
                if (overlap)
                {
                    ModelState.AddModelError("CourseOverlap", COURSE_ERROR);
                } 
                else if (overlapStudent)
                {
                    ModelState.AddModelError("StudentOverlap", STUDENT_ERROR);
                }
                else if (overlapLocation)
                {
                    ModelState.AddModelError("LocationOverlap", LOCATION_ERROR);                  
                }
                // Otherwise, add the new seminar
                else
                {
                    // Create a new list to hold the students that will be added
                    seminarToUpdate.Students = new List<Student>();

                    // Find all students that are taking the course, because they will all have to go to the seminar
                    var studentsForSeminar = new List<Student>();
                    foreach (Student student in db.Students.ToList())
                    {
                        foreach (Enrollment enrollment in student.Enrollments)
                        {
                            if (enrollment.CourseID == seminarToUpdate.CourseID)
                            {
                                studentsForSeminar.Add(student);
                            }
                        }
                    }
                    // Add all students on the course to all seminars for that course
                    foreach (Student student in studentsForSeminar)
                    {
                        seminarToUpdate.Students.Add(student);
                    }

                    // Pick the course out of the DB according to ID
                    seminarToUpdate.Course = db.Courses.Single(i => i.CourseID == seminarToUpdate.CourseID);

                    db.Seminars.Add(seminarToUpdate);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                
            }
            PopulateCoursesDropDownList(seminarToUpdate.Course);
            return View(seminarToUpdate);
        }

        private void PopulateCoursesDropDownList(object selectedCourse = null)
        {
            var coursesQuery = from d in db.Courses orderby d.Title select d;

            ViewBag.CourseID = new SelectList(coursesQuery, "CourseID", "Title", selectedCourse);
        }

        // GET: Seminar/Edit/5
        public ActionResult Edit(int? id, Boolean searched = false, string searchString = "")
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seminar seminar = db.Seminars.Find(id);
            if (seminar == null)
            {
                return HttpNotFound();
            }
            ViewBag.Searched = searched;
            ViewBag.SearchString = searchString;
            PopulateInstructorsDropDownList(seminar, seminar.InstructorID);
            return View(seminar);
        }


        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, Boolean? searched = false, string searchString = "")
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var seminarToUpdate = db.Seminars.Find(id);
            if (TryUpdateModel(seminarToUpdate, "",
               new string[] { "SeminarTime", "SeminarLength" , "Location", "InstructorID"}))
            {
                try
                {
                    // Check first whether the potential time and date overlaps with any other seminars
                    Boolean overlap = false;

                    // Set dateTime and end dateTime for the seminar in question
                    DateTime semStart = seminarToUpdate.SeminarTime;
                    DateTime semEnd = seminarToUpdate.SeminarTime.AddHours(seminarToUpdate.SeminarLength);

                    // Check all seminars on this course; except the one itself, because this can overlap
                    foreach (Seminar s in db.Seminars.Where(s => (s.CourseID == seminarToUpdate.CourseID) && 
                                                                 (s.SeminarID != seminarToUpdate.SeminarID)))
                    {
                        // Set dateTime and end dateTime for each seminar in the DB 
                        DateTime sStart = s.SeminarTime;
                        DateTime sEnd = s.SeminarTime.AddHours(s.SeminarLength);

                        // Check for overlap
                        if (DateOverlap(semStart, semEnd, sStart, sEnd)) { overlap = true; }
                    }

                    // Check for overlap with other seminars of students on this course
                    Boolean overlapStudent = false;

                    // Every student; if they are on this course; then for all enrollments that aren't this course; check for overlap

                    foreach (Student student in db.Students.ToList())
                    {
                        if (student.Enrollments.Any(enrol => enrol.CourseID == seminarToUpdate.CourseID))
                        {
                            foreach (Enrollment enrollment in student.Enrollments)
                            {
                                // No need to check for seminars on this course; this would be caught by the above error
                                if (enrollment.CourseID != seminarToUpdate.CourseID)
                                {
                                    foreach (Seminar s in student.Seminars.Where(sem => sem.CourseID != seminarToUpdate.CourseID))
                                    {
                                        DateTime sStart = s.SeminarTime;
                                        DateTime sEnd = s.SeminarTime.AddHours(s.SeminarLength);

                                        // Check for overlap
                                        if (DateOverlap(semStart, semEnd, sStart, sEnd)) { overlapStudent = true; }
                                    }
                                }
                            }
                        }
                    }
                    // Check for overlap with other seminars in this location
                    Boolean overlapLocation = false;

                    foreach (Seminar s in db.Seminars)
                    {
                        if ((s.Location != "") && (s.Location == seminarToUpdate.Location) &&
                                                                 (s.SeminarID != seminarToUpdate.SeminarID))
                        {
                            DateTime sStart = s.SeminarTime;
                            DateTime sEnd = s.SeminarTime.AddHours(s.SeminarLength);

                            // Check for overlap
                            if (DateOverlap(semStart, semEnd, sStart, sEnd)) { overlapLocation = true; }
                        }
                    }

                    // Check for overlap with other seminars run by instructor
                    Boolean overlapInstructor = false;

                    foreach (Seminar s in db.Seminars.Where(s => (s.InstructorID == seminarToUpdate.InstructorID) &&
                                                                 (s.SeminarID != seminarToUpdate.SeminarID)))
                    {
                        DateTime sStart = s.SeminarTime;
                        DateTime sEnd = s.SeminarTime.AddHours(s.SeminarLength);

                        // Check for overlap
                        if (DateOverlap(semStart, semEnd, sStart, sEnd)) { overlapInstructor = true; }
                    }

                    // If there is an overlap, don't accept and display the error message
                    if (overlap)
                    {
                        ModelState.AddModelError("CourseOverlap", COURSE_ERROR);
                    }
                    else if (overlapStudent)
                    {
                        ModelState.AddModelError("StudentOverlap", STUDENT_ERROR);
                    }
                    else if (overlapLocation)
                    {
                        ModelState.AddModelError("LocationOverlap", LOCATION_ERROR);
                    }
                    else if (overlapInstructor)
                    {
                        ModelState.AddModelError("InstructorOverlap", INSTRUCTOR_ERROR);
                    }

                    // Otherwise, add the new seminar
                    else
                    {
                        // Pick the instructor out of the DB according to ID, if there is one
                        if (seminarToUpdate.InstructorID != null)
                        {
                            seminarToUpdate.Instructor = db.Instructors.Single(i => i.ID == seminarToUpdate.InstructorID);
                        }

                        db.SaveChanges();

                        if (searched == true)
                        {
                            return RedirectToAction("Search", new { searchString = searchString });
                        }

                       
                        return RedirectToAction("Index");
                    }

                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateInstructorsDropDownList(seminarToUpdate, seminarToUpdate.InstructorID);
            return View(seminarToUpdate);
        }

        private void PopulateInstructorsDropDownList(Seminar seminar, object selectedInstructor = null)
        {
            var instructorsQuery = from i in db.Instructors orderby i.LastName select i;
            var instructorsOnCourse = instructorsQuery.Where(i => i.Courses.Any(c => c.CourseID == seminar.CourseID)).ToList();
            instructorsOnCourse.Add(null);
            ViewBag.InstructorID = new SelectList(instructorsOnCourse, "ID", "FullName", selectedInstructor);
        }

        // GET: Seminar/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seminar seminar = db.Seminars.Find(id);
            if (seminar == null)
            {
                return HttpNotFound();
            }
            return View(seminar);
        }

        // POST: Seminar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Seminar seminar = db.Seminars.Find(id);
            db.Seminars.Remove(seminar);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool DateOverlap(DateTime d1_start, DateTime d1_end, DateTime d2_start, DateTime d2_end)
        {
            if (d1_start <= d2_start && d2_start <= d1_end) { return true; }
            else if (d1_start <= d2_end && d2_end <= d1_end) { return true; }
            else if (d2_start <= d1_start && d1_start <= d2_end) { return true; }
            else if (d2_end <= d1_end && d1_end <= d2_start) { return true; }
            return false;
        }

        public ActionResult Search(string SearchString)
        {
            //var result = db.Database.SqlQuery<SeminarSearchData>("SeminarsByName").ToList();
            ViewBag.CurrentSearch = SearchString;

            if (SearchString != null)
            {
                var result = db.Database.SqlQuery<SeminarSearchData>("SeminarsByName @Search",
                    new System.Data.SqlClient.SqlParameter("Search", SearchString)).ToList();

                return View(result);
            }

            return View();
            
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
