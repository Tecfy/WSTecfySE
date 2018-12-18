using System.Collections.Generic;
using WebService.Models;

namespace WebService
{
    public class JobOut : ResultServiceVM
    {
        public JobOut()
        {
            this.result = new JobVM();
        }

        public JobVM result { get; set; }
    }
}