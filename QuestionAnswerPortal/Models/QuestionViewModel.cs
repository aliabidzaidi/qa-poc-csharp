using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuestionAnswerPortal.Models
{
    public class QuestionViewModel
    {
        public QuestionViewModel()
        {
            Answer = new AnswerViewModel();
        }
        public int Id { get; set; }
        [Required]
        public string QuestionText { get; set; }
        public DateTime DatePublished { get; set; }
        public string UserId { get; set; }

        public bool IsPublic { get; set; }

        public string UserName { get; set; }
        public string ImagePath { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public Nullable<int> AnswerId { get; set; }

        public AnswerViewModel Answer { get; set; }
    }
}