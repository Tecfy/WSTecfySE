using System;
using System.Collections.Generic;

namespace WebService
{
    public class JobsRegistrationVM
    {
        public int jobId { get; set; }

        public string code { get; set; }

        public string registration { get; set; }


        public List<JobCategoriesRegistrationVM> jobCategories { get; set; }
    }
}