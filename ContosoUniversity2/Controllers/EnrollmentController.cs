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

namespace ContosoUniversity2.Controllers
{
    public class EnrollmentController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Enrollment
        public ActionResult Index()
        {
            var enrollments = db.Enrollments.Include(e => e.Course).Include(e => e.Student);
            return View(enrollments.ToList());
        }

        // GET: Enrollment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // GET: Enrollment/Create
        public ActionResult Create()
        {
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName");
            return View();
        }

        // POST: Enrollment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EnrollmentID,CourseID,StudentID,Grade")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                // Check for overlap with other courses the student is taking 
                Boolean seminarOverlap = false; 

                foreach (Seminar s in db.Students.Single(st => st.ID == enrollment.StudentID).Seminars.Where(sem => sem.CourseID != enrollment.CourseID))
                {
                    foreach (Seminar seminar in db.Seminars.Where(sem => sem.CourseID == enrollment.CourseID))
                    {
                        DateTime semStart = seminar.SeminarTime;
                        DateTime semEnd = seminar.SeminarTime.AddHours(seminar.SeminarLength);
                        DateTime sStart = s.SeminarTime;
                        DateTime sEnd = s.SeminarTime.AddHours(s.SeminarLength);

                        if (DateOverlap(semStart, semEnd, sStart, sEnd)) { seminarOverlap = true; }
                    }
                }
                if (seminarOverlap)
                {
                    ModelState.AddModelError("SeminarOverlap", "There will be an overlap with other seminars this student is taking. They cannot enroll without seminars changing.");
                }
                else
                {
                    foreach (Seminar s in db.Seminars.Where(s => s.CourseID == enrollment.CourseID))
                    {
                        s.Students.Add(db.Students.Single(student => student.ID == enrollment.StudentID));
                    }
                    db.Enrollments.Add(enrollment);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
                ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName");
                return View();
                    
            }

            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);
            return View(enrollment);
        }

        private bool DateOverlap(DateTime d1_start, DateTime d1_end, DateTime d2_start, DateTime d2_end)
        {
            if (d1_start <= d2_start && d2_start <= d1_end) { return true; }
            else if (d1_start <= d2_end && d2_end <= d1_end) { return true; }
            else if (d2_start <= d1_start && d1_start <= d2_end) { return true; }
            else if (d2_end <= d1_end && d1_end <= d2_start) { return true; }
            return false;
        }

        // GET: Enrollment/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);
            return View(enrollment);
        }

        // POST: Enrollment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EnrollmentID, Grade")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(enrollment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollment/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // POST: Enrollment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Enrollment enrollment = db.Enrollments.Find(id);
            db.Enrollments.Remove(enrollment);
            foreach (Seminar s in db.Seminars.Where(s => s.CourseID == enrollment.CourseID))
            {
                s.Students.Remove(db.Students.Find(enrollment.StudentID));
            }
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
