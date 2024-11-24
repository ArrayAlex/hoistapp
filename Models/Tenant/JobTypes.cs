using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hoistmt.Models
{

    public class JobTypes
    {

        [Key]
        public int id {get; set;}
        public int hourly_rate {get; set;}

        public string title {get; set;}

        public string color {get; set;}


    }
}