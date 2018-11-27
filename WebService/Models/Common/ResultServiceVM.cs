using System.Collections.Generic;

namespace WebService.Models
{
    public class ResultServiceVM
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ResultServiceVM()
        {
            this.messages = new List<string>();
            this.successMessage = null;
        }

        /// <summary>
        /// Operation Result Validation
        /// </summary>
        public bool success
        {
            get
            {
                return this.messages.Count == 0;
            }
        }

        /// <summary>
        /// Error Messages
        /// </summary>
        public List<string> messages { get; set; }

        /// <summary>
        /// Success Message
        /// </summary>
        public string successMessage { get; set; }
    }
}