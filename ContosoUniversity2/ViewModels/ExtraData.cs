using System.Collections.Generic;
using ContosoUniversity2.Models;

namespace ContosoUniversity2.ViewModels
{
    public class ExtraData
    {
        public int? A { get; set; }
        public int? B { get; set; }
        public int? Result { get; set; }

        public string FirstWord { get; set; }
        public string SecondWord { get; set; }
        public string ResultWord { get; set; }


        public string WordList { get; set; }
        public string ResultList { get; set; }
    }
}