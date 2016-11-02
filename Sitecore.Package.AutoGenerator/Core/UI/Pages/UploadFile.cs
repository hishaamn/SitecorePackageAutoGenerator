
namespace Sitecore.Package.AutoGenerator.Core.UI.Pages
{
    using System;
    using System.Web.UI.HtmlControls;
    using Sitecore.Shell.Web.UI;
    using Sitecore.Web;

    public class UploadFile : SecurePage
    {
        protected HtmlGenericControl Upload;

        private void Page_Load(object sender, EventArgs e)
        {
            this.Upload.Attributes["src"] = Constants.UploadFile2App + WebUtil.GetQueryString();
        }

        protected override void OnInit(EventArgs e)
        {
            this.InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.Load += this.Page_Load;
        }
    }
}

