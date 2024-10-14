using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hoistmt.Models
{
    
    public class JobStatus
    {
        [Key]
        public int id {get; set;}

        public string title {get; set;}

        public string color {get; set;}


    }
}