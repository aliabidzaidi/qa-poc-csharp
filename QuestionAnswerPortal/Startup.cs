using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QuestionAnswerPortal.Startup))]
namespace QuestionAnswerPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
