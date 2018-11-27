using System.Collections.Generic;
using WebService.Models;

namespace WebService
{
    public class JobsRegistrationOut : ResultServiceVM
    {
        public JobsRegistrationOut()
        {
            this.result = new List<JobsRegistrationVM>();
        }

        public List<JobsRegistrationVM> result { get; set; }
    }
}