
namespace Sitecore.Package.AutoGenerator.Core.UI.Pages
{
    using System;
    using System.Reflection;
    using System.Web.UI;
    using Sitecore.Diagnostics;
    using Sitecore.Exceptions;
    using Sitecore.Globalization;
    using Sitecore.Pipelines;
    using Sitecore.Pipelines.Upload;
    using Sitecore.Shell.Web.UI;
    using Sitecore.Web;
    using Sitecore.Web.UI.XmlControls;

    public class UploadFile2 : SecurePage
    {
        protected override void OnInit(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            Control control = ControlFactory.GetControl("UploadFile");
            if (control != null)
                this.Controls.Add(control);
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (this.IsEvent || this.Request.Files.Count <= 0)
                return;
            string filename = FileHandle.GetFilename(this.Request.Form["Path"]);
            if (string.IsNullOrEmpty(filename))
            {
                SecurityException securityException = new SecurityException("Upload path invalid");
                Log.Error("File upload handle not found. Handle: " + this.Request.Form["Path"], securityException, this);
                throw securityException;
            }
            var string1 = StringUtil.GetString(new []
              {
                filename
              });
            string string2 = StringUtil.GetString(new []
              {
                this.Request.Form["Item"]
              });
            string string3 = StringUtil.GetString(new []
              {
                this.Request.Form["Language"]
              });
            bool flag1 = StringUtil.GetString(new []
              {
                this.Request.Form["Overwrite"]
              }) == "1";
            bool flag2 = StringUtil.GetString(new []
              {
                this.Request.Form["Unzip"]
              }) == "1";
            bool flag3 = StringUtil.GetString(new []
              {
                this.Request.Form["Versioned"]
              }) == "1";

            UploadArgs args = new UploadArgs
            {
                Files = this.Request.Files
            };
            if (!string.IsNullOrEmpty(string2))
            {
                args.Folder = string2;
                args.Destination = UploadDestination.Database;
            }
            else
            {
                args.Folder = string1;
                args.Destination = UploadDestination.File;
                this.SetFileOnlyIfExists(args);
            }
            args.Overwrite = flag1;
            args.Unpack = flag2;
            args.Versioned = flag3;
            args.Language = Language.Parse(string3);
            args.Parameters["message"] = "packager:endupload";
            PipelineFactory.GetPipeline("uiUpload").Start(args);
        }

        private void SetFileOnlyIfExists(UploadArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            PropertyInfo property = args.GetType().GetProperty("FileOnly");
            if (!(property != null))
                return;
            property.SetValue(args, true, null);
        }
    }
}
