using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Globalization;

using ContosoUniversity2.DAL;
using ContosoUniversity2.ViewModels;

namespace ContosoUniversity2.Controllers
{
    public class HomeController : Controller
    {
        private SchoolContext db = new SchoolContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            IQueryable<EnrollmentDateGroup> data = from student in db.Students
                                                   group student by student.EnrollmentDate into dateGroup
                                                   select new EnrollmentDateGroup()
                                                   {
                                                       EnrollmentDate = dateGroup.Key,
                                                       StudentCount = dateGroup.Count()
                                                   };
            return View(data.ToList());
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Statistics(string searchString, Boolean clear = false)
        {
            var result = db.Database.SqlQuery<CourseStatisticsData>("StudentNoByCourseID").ToList();
            var mod = new CourseStatisticsTwoQueryData();

            mod.MainQuery = result;

            ViewBag.CurrentSearch = searchString;

            if (searchString != null)
            {
                var result2 = db.Database.SqlQuery<CourseStatisticsData>("StudentNoByCourseIDSearch @Search",
                    new System.Data.SqlClient.SqlParameter("Search",searchString)).ToList();
                mod.SecondQuery = result2;
            }

            if (clear) { mod.SecondQuery = null; }

            return View(mod);
        }

        //[HttpPost]
        public ActionResult Extra(ExtraData model)
        {
            ViewBag.WordError = false; //Tells whether an error has been made in inputting words in the alphabetic comparison
            // If an error has been made then the HTML changes to reflect this

            int? numberA = model.A;
            int? numberB = model.B;
            int remainder;

            if (model.B == 0)
            {
                ModelState.AddModelError("Divide0", "Division by 0 is not allowed");
                return View(model);
            }
            if (model.B < 0 || model.A < 0 )
            {
                ModelState.AddModelError("Less0", "Negative numbers aren't permitted");
                return View(model);
            }

            if (numberA == null) {remainder = 0;}
            else{ remainder = (int)numberA;}

            // Provided there are no null or 0; simply subtract B from A until the result is >= B
            // Could move this to a function; but it's so small
            if (numberA != null && numberB != null)
            {                
                while (remainder >= numberB)
                {
                    remainder = remainder - (int) numberB;
                }
            }
            model.Result = remainder;

            model.ResultWord = CompareWords(model.FirstWord, model.SecondWord);

            model.ResultList = CompareList(model.WordList);
            
            return View(model);
        }

        private string CompareWords(string A, string B)
        {
            Boolean found = false;
            String result = "These are the same";

            // Check first for any Null words; or any unwanted characters
            if (A == null || B == null)
            {
                ViewBag.WordError = true;
                return "";
            }
            if (!((A.All(Char.IsLetter)) && (B.All(Char.IsLetter)))){
                ViewBag.WordError = true;
                return "Only letters allowed";  
            }

            // Define a list of letters in alphabet order
            List<string> letters = new List<string>{ "a","b","c","d","e","f","g","h","i","j","k","l","m"
                ,"n","o","p","q","r","s","t","u","v","w","x","y","z" };

            // Use this dictionary only for the alternate method
            //Dictionary<string, int> values = new Dictionary<string, int>();
            //int i = 1;
            //foreach (string s in letters)
            //{
            //    values[s] = i;
            //    i += 1;
            //}

            int sRef = 0; //This will tick along the string

            while ((!found) && (sRef < A.Length) && (sRef < B.Length))
            {
                string nth_A = Char.ToLower(A[sRef]).ToString();
                string nth_B = Char.ToLower(B[sRef]).ToString();

                int tickAlongList = 0; // This will tick along the alphabet and be used to say which is first

                while ((!found) && (tickAlongList < 26))
                {
                    if ((letters[tickAlongList] == nth_A) && (letters[tickAlongList] != nth_B))
                    {
                        found = true;
                        result = A;
                    } 
                    else if ((letters[tickAlongList] == nth_B) && (letters[tickAlongList] != nth_A))
                    {
                        found = true;
                        result = B;
                    }
                    else
                    {
                        tickAlongList += 1;
                    }
                }
                // This is a better alternative using the above dictionary; but it does use > and < to compare letters

                //if (values[nth_A] < values[nth_B])
                //{
                //    found = true;
                //    result = A;
                //}
                //else if (values[nth_A] > values[nth_B])
                //{
                //    found = true;
                //    result = B;
                //} else
                //    sRef += 1;
                //{

                    sRef += 1;

            }

            if (result == "These are the same")
            {
                // If they have the same word for the length of one; but one is longer; then the shorter one goes first
                if (A.Length > B.Length)
                {
                    return B;
                }
                else if (B.Length > A.Length)
                {
                    return A;
                }
                else
                {
                    ViewBag.WordError = true;
                }
               
            }
            return result;
        }

        private string CompareList(string Words)
        {
            List<string> words = new List<string>();
            if (Words == null)
            {
                return "";
            }
            // Remove any spaces and new line characters; but only those will not flag as "Only Letters Allowed"
            string WordsWithoutSpaces = Words.Replace(" ", string.Empty).Replace(System.Environment.NewLine, string.Empty);

            words = WordsWithoutSpaces.Split(',').ToList();

            int finalWordRef = words.Count;
            int currentWordRef = 0;

            // Essentially do a pairwise sort along the list repeatedly; requires n*(n+1)/2 comparisons
            while (finalWordRef >0)
            {
                while (currentWordRef < finalWordRef-1 )
                {
                    string wordA = words[currentWordRef];
                    string wordB = words[currentWordRef+1];
                    string firstWord;

                    // If the words are the same; then don't use the CompareWords function because it will return 'these are the same'
                    if (wordA == wordB)
                    {
                        firstWord = wordA;
                    }
                    else
                    {
                        firstWord = CompareWords(wordA, wordB);
                    }

                    if (firstWord == "Only letters allowed") { return "Only letters allowed"; }

                    // Set the earlier word; and then the second as appropriate
                    words[currentWordRef] = firstWord;
                    if (wordA == firstWord)
                    {
                        words[currentWordRef + 1] = wordB;
                    } else
                    {
                        words[currentWordRef + 1] = wordA;
                    }
                    currentWordRef += 1;                   
                }
                finalWordRef -= 1;
                currentWordRef = 0;
            }

            string result = "";
            foreach (String s in words)
            {
                result = result + s + ", ";
            }
            return result.TrimEnd(new char[] { ',', ' ' });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}