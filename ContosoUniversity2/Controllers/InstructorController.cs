﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ContosoUniversity2.DAL;
using ContosoUniversity2.Models;
using ContosoUniversity2.ViewModels;
using System.Data.Entity.Infrastructure;

namespace ContosoUniversity2.Controllers
{
    public class InstructorController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Instructor
        public ActionResult Index(int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();
            viewModel.Instructors = db.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.Courses.Select(c => c.Department))
                .OrderBy(i => i.LastName);

            if (id != null)
            {
                ViewBag.InstructorID = id.Value;
                viewModel.Courses = viewModel.Instructors.Where(
                    i => i.ID == id.Value).Single().Courses;
            }

            if (courseID != null)
            {
                ViewBag.CourseID = courseID.Value;

                // Explicit loading
                var selectedCourse = viewModel.Courses.Where(x => x.CourseID == courseID).Single();
                db.Entry(selectedCourse).Collection(x => x.Enrollments).Load();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    db.Entry(enrollment).Reference(x => x.Student).Load();
                }

                viewModel.Enrollments = selectedCourse.Enrollments;
            }

            return View(viewModel);
        }

        // GET: Instructor/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        // GET: Instructor/Create
        public ActionResult Create()
        {
            var instructor = new Instructor();
            instructor.Courses = new List<Course>();
            PopulateAssignedCourseData(instructor);
            return View();
        }

        // POST: Instructor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LastName,FirstMidName,HireDate,OfficeAssignment")]Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.Courses = new List<Course>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = db.Courses.Find(int.Parse(course));
                    instructor.Courses.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                // Check if trying to assign the same office
                Boolean officeOverlap = false;

                foreach (Instructor i in db.Instructors)
                {
                    if ((i.OfficeAssignment != null) && (instructor.OfficeAssignment != null))
                    {
                        if ((i.OfficeAssignment.Location == instructor.OfficeAssignment.Location) &&
                                instructor.OfficeAssignment.Location != null)
                        {
                            officeOverlap = true;
                        }
                    }
                    
                }
                if (officeOverlap)
                {
                    ModelState.AddModelError("OfficeOverlap", "Someone already has this office, choose another location or leave unnasigned.");
                }
                else
                {
                    db.Instructors.Add(instructor);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructor/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.Courses)
                .Where(i => i.ID == id)
                .Single();
            PopulateAssignedCourseData(instructor);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }



        // POST: Instructor/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var instructorToUpdate = db.Instructors
               .Include(i => i.OfficeAssignment)
               .Include(i => i.Courses)
               .Where(i => i.ID == id)
               .Single();

            if (TryUpdateModel(instructorToUpdate, "",
               new string[] { "LastName", "FirstMidName", "HireDate", "OfficeAssignment" }))
            {
                try
                {
                    // Check if trying to assign the same office
                    Boolean officeOverlap = false;

                    foreach (Instructor i in db.Instructors)
                    {
                        if (i.ID != instructorToUpdate.ID) //Ensure this instructor doesn't block the assignment
                        {
                            if ((i.OfficeAssignment != null) && (instructorToUpdate.OfficeAssignment != null))
                            {
                                if ((i.OfficeAssignment.Location == instructorToUpdate.OfficeAssignment.Location) &&
                                    instructorToUpdate.OfficeAssignment.Location != null)
                                {
                                    officeOverlap = true;
                                }
                            }
                        }           
                    }

                    if (officeOverlap)
                    {
                        ModelState.AddModelError("OfficeOverlap", "Someone already has this office, choose another location or leave unnasigned.");
                    }
                    else
                    {

                        if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment.Location))
                        {
                            instructorToUpdate.OfficeAssignment = null;
                        }

                        UpdateInstructorCourses(selectedCourses, instructorToUpdate);

                        // If the instructor no longer teaches a course; remove them from the seminars

                        // Construct a list of courseIDs
                        List<int> courseIDs = new List<int>();
                        foreach (Course c in instructorToUpdate.Courses)
                        {
                            courseIDs.Add(c.CourseID);
                        }

                        // Check each seminar and if it has the same courseID as a course they aren't teaching, set instructor to null
                        foreach (Seminar s in db.Seminars.Where(sem => sem.InstructorID == instructorToUpdate.ID))
                        {
                            if (!(courseIDs.Any(i => i == s.CourseID)))
                            {
                                s.InstructorID = null;
                                s.Instructor = null;
                            }

                        }

                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedCourseData(instructorToUpdate);
            return View(instructorToUpdate);
        }

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = db.Courses;
            var instructorCourses = new HashSet<int>(instructor.Courses.Select(c => c.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewBag.Courses = viewModel;
        }

        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.Courses = new List<Course>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>
                (instructorToUpdate.Courses.Select(c => c.CourseID));
            foreach (var course in db.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.Courses.Add(course);
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.Courses.Remove(course);
                    }
                }
            }
        }

        // GET: Instructor/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        // POST: Instructor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Instructor instructor = db.Instructors
                .Include(i => i.OfficeAssignment)
                .Where(i => i.ID == id)
                .Single();

            var department = db.Departments
                .Where(d => d.InstructorID == id)
                .SingleOrDefault();

            if (department != null)
            {
                department.InstructorID = null;
            }

            // If the instructor no longer teaches a course; remove them from the seminars
            // Check each seminar and if it is taught by them, set instructor to null
            foreach (Seminar s in db.Seminars.Where(sem => sem.InstructorID == instructor.ID))
            {  
                s.InstructorID = null;
                s.Instructor = null;
            }

            db.Instructors.Remove(instructor);
            db.SaveChanges();
            return RedirectToAction("Index");
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
