using AutoMapper;
using Microsoft.AspNet.Identity;
using QuestionAnswerPortal.DataStore.Repositories;
using QuestionAnswerPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuestionAnswerPortal.Controllers
{
    //TODO: Code Review
    //Brute Force, Gets Question from database too many times on Add Answer, etc

    [Authorize]
    public class QuestionController : Controller
    {
        private QA_Repository repo = new QA_Repository();


        [AllowAnonymous]
        public ActionResult Index()
        {

            List<QuestionViewModel> questionList = new List<QuestionViewModel>();

            try
            {
                List<DataStore.Question> questionListEntity = repo.GetAllPublicQuestions();

                questionList = questionListEntity.Select(item => GetQuestionModel(item)).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "Unable to fetch questions... Please try again later");
            }

            return View(questionList);
        }


        public ActionResult AskQuestion()
        {
            return View();
        }


        [HttpPost]
        public ActionResult AskQuestion(QuestionViewModel question)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    question.DatePublished = DateTime.Now;
                    var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    //Map View Model -> Entity
                    var questionEntity = new DataStore.Question();
                    questionEntity.DatePublished = question.DatePublished;
                    questionEntity.QuestionText = question.QuestionText;
                    questionEntity.UserId = user;
                    //Add in DataBase
                    bool isInserted = repo.InsertQuestion(questionEntity);

                    if (isInserted)
                    {
                        ViewBag.InsertMessage = "Your Question has been successfully submitted, please wait for your response...";
                        return RedirectToAction("ViewMyQuestions");
                    }
                    else
                    {
                        ModelState.AddModelError("", "The Question could not be submitted... Please try again later");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "There is something wrong at server side, please be patient till we fix some things...");
            }
            return View();
        }


        public ActionResult ViewMyQuestions()
        {
            ViewBag.Message = "http://localhost:53029/?UserName=Jon\x3cscript\x3e%20alert(\x27pwnd\x27)%20\x3c/script\x3e";
            List<QuestionViewModel> questions = new List<QuestionViewModel>();
            try
            {
                //Get Questions from Database
                var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
                List<DataStore.Question> questionEntities = repo.GetQuestionsByUserId(user);

                //Convert Entity -> ViewModel
                questions = questionEntities.Select(item => GetQuestionModel(item)).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                ViewBag.ErrorMessage = "There is something wrong at server side, please be patient till we fix some things...";
            }
            return View(questions);
        }


        [AuthorizeUser(Roles = "AdminUser")]
        public ActionResult Queue()
        {
            List<QuestionViewModel> questions = new List<QuestionViewModel>();
            ViewBag.QueueCount = 0;
            try
            {
                questions = repo.GetUnAnsweredQuestions().OrderBy(item => item.DatePublished).Select(item => new QuestionViewModel { Id = item.Id, UserId = item.UserId, UserName = item.AspNetUsers.UserName, QuestionText = item.QuestionText, DatePublished = item.DatePublished }).ToList();
                ViewBag.QueueCount = questions.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "Unable to fetch questions... Please try again later");
            }
            return View(questions);
        }


        [AuthorizeUser(Roles = "AdminUser")]
        public ActionResult AddAnswer(int? id)
        {
            AnswerViewModel answer = new AnswerViewModel();
            try
            {
                if (id == null)
                {
                    ModelState.AddModelError("", "Invalid Question...");
                    return RedirectToAction("AccessDenied", "Error");
                }
                answer.QuestionId = (int)id;
                ViewBag.Question = new QuestionViewModel();

                var questionEntity = repo.GetQuestionById((int)id);
                ViewBag.Question = GetQuestionModel(questionEntity);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "Unable to fetch questions... Please try again later");
            }

            //return to AnswerQuestion Form
            return View(answer);
        }


        [AuthorizeUser(Roles = "AdminUser")]
        [HttpPost]
        public ActionResult AddAnswer([Bind(Include = "AnswerText, Link, QuestionId")] AnswerViewModel answer)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    //Create Question and Answer Entities
                    answer.AnsweredDate = DateTime.Now;
                    var answerEntity = GetAnswerEntity(answer);

                    //Make and call AnswerAgainstQuestionId
                    bool isUpdatedSuccess = repo.AddAnswer(answerEntity, answer.QuestionId);


                    if (isUpdatedSuccess)
                    {
                        //Redirect to View Answer with ViewBag Message Successfully Added
                        ViewBag.Message = "Answer Successfully Added in Database";
                        return RedirectToAction("ViewAnswer", new { questionId = answer.QuestionId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to Add Answer in Database, Please try again later!");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "Unable to submit Answer... Please try again later");
            }
            return View(answer);
        }


        [AllowAnonymous]
        [Route("Question/ViewAnswer/{questionId}")]
        public ActionResult ViewAnswer(int? questionId)
        {
            QuestionViewModel question = new QuestionViewModel();
            try
            {
                if (questionId == null)
                {
                    ModelState.AddModelError("", "Invalid Question...");
                    return RedirectToAction("AccessDenied", "Error");
                }

                //Fetch Question against Id
                DataStore.Question questionEntity = repo.GetQuestionById((int)questionId);

                question = GetQuestionModel(questionEntity);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "An Error Occured Retrieving the Answer against question, please try again later...");
            }
            return View(question);
        }


        #region private methods
        private QuestionViewModel GetQuestionModel(DataStore.Question questionEntity)
        {
            QuestionViewModel questionModel = new QuestionViewModel();
            try
            {
                questionModel.Id = questionEntity.Id;
                questionModel.QuestionText = questionEntity.QuestionText;
                questionModel.UserId = questionEntity.UserId;
                questionModel.DatePublished = questionEntity.DatePublished;
                questionModel.AnswerId = questionEntity.AnswerId;
                questionModel.IsPublic = questionEntity.IsPublic;

                if (questionEntity.Answer != null)
                {
                    questionModel.Answer = GetAnswerModel(questionEntity.Answer);
                }

                if (questionEntity.AspNetUsers != null)
                {
                    questionModel.UserName = questionEntity.AspNetUsers.UserName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return questionModel;
        }

        private DataStore.Question GetQuestionEntity(QuestionViewModel questionModel)
        {
            DataStore.Question questionEntity = new DataStore.Question();
            try
            {
                questionEntity.Id = questionModel.Id;
                questionEntity.QuestionText = questionModel.QuestionText;
                questionEntity.UserId = questionModel.UserId;
                questionEntity.DatePublished = questionModel.DatePublished;
                questionEntity.AnswerId = questionModel.AnswerId;
                questionEntity.IsPublic = questionModel.IsPublic;

                if (questionEntity.Answer != null)
                {
                    questionEntity.Answer = GetAnswerEntity(questionModel.Answer);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return questionEntity;
        }

        private AnswerViewModel GetAnswerModel(DataStore.Answer answerEntity)
        {
            AnswerViewModel answerModel = new AnswerViewModel();
            answerModel.AnswerText = answerEntity.AnswerText;
            answerModel.AnsweredDate = answerEntity.AnsweredDate;
            answerModel.Link = answerEntity.Link;

            return answerModel;
        }

        private DataStore.Answer GetAnswerEntity(AnswerViewModel answerModel)
        {
            DataStore.Answer answerEntity = new DataStore.Answer();
            answerEntity.AnswerText = answerModel.AnswerText;
            answerEntity.AnsweredDate = answerModel.AnsweredDate;
            answerEntity.Link = answerModel.Link;
            return answerEntity;
        }
        #endregion private methods

    }
}