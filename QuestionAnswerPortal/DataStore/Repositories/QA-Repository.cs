using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace QuestionAnswerPortal.DataStore.Repositories
{
    public class QA_Repository
    {
        private QAEntities qaEntity;

        public QA_Repository()
        {
            qaEntity = new QAEntities();
        }


        public bool InsertQuestion(Question question)
        {
            bool isInserted = false;
            try
            {
                qaEntity.Question.Add(question);
                qaEntity.SaveChanges();
                isInserted = true;
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return isInserted;
        }


        public List<Question> GetAllQuestions()
        {
            List<Question> questions = new List<Question>();
            try
            {
                //Include Answer Foreign Key
                questions = qaEntity.Question.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return questions;
        }


        public List<Question> GetAllPublicQuestions()
        {
            List<Question> questions = new List<Question>();
            try
            {
                questions = qaEntity.Question.Where(item => item.IsPublic == true).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return questions;
        }


        public List<Question> GetQuestionsByUserId(string userId)
        {
            List<Question> questions = new List<Question>();
            try
            {
                //Include Answer Foreign Key
                questions = qaEntity.Question.Include("Answer").Where(item => item.UserId == userId).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return questions;
        }


        public List<Question> GetUnAnsweredQuestions()
        {
            List<Question> questions = new List<Question>();
            try
            {
                questions = qaEntity.Question.Include("Answer").Where(ques => ques.AnswerId == null).ToList();

                Console.WriteLine(questions.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return questions;
        }


        public Question GetQuestionById(int questionId)
        {
            Question question = new Question();
            try
            {
                question = qaEntity.Question.Include("Answer").Include("AspNetUsers").Where(ques => ques.Id == questionId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return question;
        }


        public bool AddAnswer(Answer answer, int questionId)
        {
            bool isInserted = false;
            try
            {
                //Add Answer to Data base
                Answer ans = qaEntity.Answer.Add(answer);
                qaEntity.SaveChanges();

                //Update Answer against Question
                Question question = qaEntity.Question.Find(questionId);
                question.AnswerId = ans.Id;
                question.IsPublic = true;
                qaEntity.Entry(question).State = System.Data.Entity.EntityState.Modified;
                qaEntity.SaveChanges();

                isInserted = true;
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return isInserted;
        }

    }
}