using System.Collections.Generic;

namespace WebService
{
    public class JobVM
    {
        public int jobId { get; set; }

        public string registration { get; set; }

        public string name { get; set; }

        public string course { get; set; }

        public string unity { get; set; }


        public List<JobCategoriesVM> jobCategories { get; set; }
    }
}