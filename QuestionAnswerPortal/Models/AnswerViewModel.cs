using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuestionAnswerPortal.Models
{
    public class AnswerViewModel
    {
        public int Id { get; set; }
        [Required]
        public string AnswerText { get; set; }
        public string Link { get; set; }
        public DateTime AnsweredDate { get; set; }
        public int QuestionId { get; set; }
    }
}