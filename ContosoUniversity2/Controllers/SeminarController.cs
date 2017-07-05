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

namespace ContosoUniversity2.Controllers
{
    public class SeminarController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Seminar
        public ActionResult Index()
        {
            return View(db.Seminars.ToList());
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
            PopulateDepartmentsDropDownList();
            return View();
        }



        // POST: Seminar/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SeminarTime, SeminarLength, CourseID")] Seminar seminar)
        {
            if (ModelState.IsValid)
            {
                // Create a new list to hold the students
                seminar.Students = new List<Student>();

                // Find all students that are taking the course
                var studentsForSeminar = new List<Student>();
                foreach (Student student in db.Students.ToList())
                {
                    foreach (Enrollment enrollment in student.Enrollments)
                    {
                        if (enrollment.CourseID == seminar.CourseID)
                        {
                            studentsForSeminar.Add(student);
                        }
                    }
                }
                // Add all students on the course to all seminars for that course
                foreach (Student student in studentsForSeminar)
                {
                    seminar.Students.Add(student);
                }

                db.Seminars.Add(seminar);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(seminar);
        }

        // GET: Seminar/Edit/5
        public ActionResult Edit(int? id)
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


        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var seminarToUpdate = db.Seminars.Find(id);
            if (TryUpdateModel(seminarToUpdate, "",
               new string[] { "SeminarTime", "SeminarLength" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(seminarToUpdate);
        }

        private void PopulateDepartmentsDropDownList(object selectedCourse = null)
        {
            var coursesQuery = from d in db.Courses orderby d.Title select d;

            ViewBag.CourseID = new SelectList(coursesQuery, "CourseID", "Title", selectedCourse);
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
