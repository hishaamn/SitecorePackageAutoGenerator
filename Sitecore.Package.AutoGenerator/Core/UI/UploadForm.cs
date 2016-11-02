
namespace Sitecore.Package.AutoGenerator.Core.UI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.UI;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.IO;
    using Sitecore.Shell.Applications.Install;
    using Sitecore.Text;
    using Sitecore.Web;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Pages;
    using Sitecore.Web.UI.Sheer;
    using Sitecore.Web.UI.XmlControls;

    public class UploadForm : WizardForm
    {
        protected Scrollbox FileList;
        protected Checkbox OverwriteCheck;
        protected XmlControl Location;

        protected string Directory
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["Directory"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                Context.ClientPage.ServerProperties["Directory"] = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");

            if (!Context.ClientPage.IsEvent && !Context.ClientPage.IsPostBack)
            {
                var packageGeneratorFolder = Settings.GetSetting("PackageGeneratorFolder");

                this.Directory = MainUtil.MapPath(string.Format("{0}/{1}", Settings.PackagePath, packageGeneratorFolder));

                this.FileList.Controls.Add(new LiteralControl(this.BuildFileInput()));
            }
            base.OnLoad(e);
        }

        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(newpage, "newpage");
            if (page == "Files" && newpage == "Settings")
            {
                if (!this.GetFileList().Any())
                {
                    Context.ClientPage.ClientResponse.Alert("Please specify at least one file to upload.");
                    return false;
                }
                string fileName = this.GetInvalidFileNames().FirstOrDefault();
                if (!string.IsNullOrEmpty(fileName))
                {
                    this.ShowInvalidFileMessage(fileName);
                    this.NextButton.Disabled = true;
                    return false;
                }
            }
            if (page == "Retry")
                newpage = "Settings";
            return base.ActivePageChanging(page, ref newpage);
        }

        protected override void ActivePageChanged(string page, string oldPage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(oldPage, "oldPage");
            base.ActivePageChanged(page, oldPage);
            if (page == "Uploading")
            {
                this.NextButton.Disabled = true;
                this.BackButton.Disabled = true;
                this.CancelButton.Disabled = true;

                Context.ClientPage.ClientResponse.SetAttribute("Path", "value", FileHandle.GetFileHandle(this.Directory));
                Context.ClientPage.ClientResponse.SetAttribute("Overwrite", "value", this.OverwriteCheck.Checked ? "1" : "0");
                Context.ClientPage.ClientResponse.Timer("StartUploading", 10);
            }
            if (page != "LastPage")
                return;
            this.NextButton.Disabled = true;
            this.BackButton.Disabled = true;
            this.CancelButton.Disabled = true;
            this.CancelButton.Disabled = false;
        }

        protected void StartUploading()
        {
            Context.ClientPage.ClientResponse.Eval("document.forms[0].submit()");
        }

        protected void EndUploading(string fileName)
        {
            Assert.ArgumentNotNull(fileName, "fileName");
            Context.ClientPage.ClientResponse.SetDialogValue("ok:" + fileName);
            this.Next();
        }

        protected override void EndWizard()
        {
            Context.ClientPage.ClientResponse.Eval("window.top.dialogClose()");
        }

        protected void FileChange()
        {
            string fileName = this.GetInvalidFileNames().FirstOrDefault();
            if (!string.IsNullOrEmpty(fileName))
            {
                this.ShowInvalidFileMessage(fileName);
                this.NextButton.Disabled = true;
                Context.ClientPage.ClientResponse.SetReturnValue(false);
            }
            else
            {
                this.NextButton.Disabled = false;

                Context.ClientPage.ClientResponse.SetReturnValue(true);
            }
        }

        private IEnumerable<string> GetInvalidFileNames()
        {
            return this.GetFileList().Where((s => !this.ValidateZipFile(s)));
        }

        private IEnumerable<string> GetFileList()
        {
            foreach (string index in Context.ClientPage.ClientRequest.Form.Keys)
            {
                if (index != null && index.StartsWith("File", StringComparison.InvariantCulture))
                {
                    var file = Context.ClientPage.ClientRequest.Form[index];

                    if (!string.IsNullOrEmpty(file))
                        yield return file;
                }
            }
        }

        protected void ShowInvalidFileMessage(string fileName)
        {
            Context.ClientPage.ClientResponse.Alert(Translate.Text("\"{0}\" is not a CSV file.", (object)fileName));
        }

        protected bool ValidateZipFile(string fileName)
        {
            try
            {
                string extension = FileUtil.GetExtension(fileName);
                return !string.IsNullOrEmpty(extension) && string.Compare(extension, "csv", StringComparison.InvariantCultureIgnoreCase) == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string BuildFileInput()
        {
            string uniqueId = Web.UI.HtmlControls.Control.GetUniqueID("File");
            Context.ClientPage.ServerProperties["LastFileID"] = uniqueId;
            string clientEvent = Context.ClientPage.GetClientEvent("FileChange");
            return "<input id=\"" + uniqueId + "\" name=\"" + uniqueId + "\" type=\"file\" value=\"browse\" style=\"width:100%\" onchange=\"" + clientEvent + "\"/>";
        }

        private static string GetUrl(string directory)
        {
            Assert.ArgumentNotNull(directory, "directory");
            var urlString = new UrlString(Constants.UploadFileApp);

            urlString.Append("di", ApplicationContext.StoreObject(directory));
            return urlString.ToString();
        }

        public static void Show(string directory, bool postback)
        {
            Assert.ArgumentNotNull(directory, "directory");
            Context.ClientPage.ClientResponse.ShowModalDialog(GetUrl(directory), postback);
        }

        public static void Show(string directory, string message)
        {
            Assert.ArgumentNotNull(directory, "directory");
            Assert.ArgumentNotNull(message, "message");
            Context.ClientPage.ClientResponse.ShowModalDialog(GetUrl(directory), message);
        }

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
            if (message.Name != "packager:endupload")
                return;
            string str = message.Arguments["filename"];
            string filename = FileHandle.GetFilename(str);
            if (!string.IsNullOrEmpty(filename))
                str = filename;
            this.EndUploading(Path.GetFileName(str));
        }
    }
}
