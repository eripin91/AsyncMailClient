using Limilabs.Client.IMAP;
using Limilabs.Client.POP3;
using Limilabs.Mail;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public class EmailBodyModel
    {
        #region Variable
        private long? imapEmailId;
        private string pop3EmailId;

        private static List<EmailBodyModel> emailBodyList = new List<EmailBodyModel>();
        private static readonly object downloadEmailBodiesLock = new object();
        private UtilModel utilModel = new UtilModel();
        //reserved for attachment model
        #endregion

        #region Setter getter
        public long? ImapEmailId
        {
            get
            {
                return imapEmailId;
            }
            set
            {
                imapEmailId = value;
            }
        }
        public string Pop3EmailId
        {
            get
            {
                return pop3EmailId;
            }
            set
            {
                pop3EmailId = value;
            }
        }
        #endregion
        #region Method
        public void DownloadEmailBodies(MainWindowModel mainWindowModel)
        {
            try
            {
                string emailBodyListFilePath;

                lock (downloadEmailBodiesLock)
                {
                    // Create directory
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "EmailFile/" + mainWindowModel.SelectedServerType));
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "LocalData"));

                    // File path for email header list
                    emailBodyListFilePath = Path.Combine(Environment.CurrentDirectory, "LocalData/" + mainWindowModel.Username + "_" + mainWindowModel.SelectedServerType + @"_bodies.json");

                    // Read from local file 
                    string existingEmailBodyList = File.Exists(emailBodyListFilePath) ? File.ReadAllText(emailBodyListFilePath) : string.Empty;

                    // Deserialize json 
                    emailBodyList = JsonConvert.DeserializeObject<List<EmailBodyModel>>(existingEmailBodyList)
                        ?? new List<EmailBodyModel>();
                }

                // Validate server type
                if (mainWindowModel.SelectedServerType.Equals("IMAP", StringComparison.InvariantCultureIgnoreCase))
                {
                    DownloadImapEmailBodies(mainWindowModel, emailBodyListFilePath);
                }
                else if (mainWindowModel.SelectedServerType.Equals("POP3", StringComparison.InvariantCultureIgnoreCase))
                {
                    DownloadPop3EmailBodies(mainWindowModel, emailBodyListFilePath);
                }
            }
            catch (Exception ex)
            {
                // Write to log
            }
        }
        public string DisplaySelectedBody(MainWindowModel mainWindowModel)
        {
            try
            {
                if (mainWindowModel.SelectedEmailHeader != null)
                {
                    string textFilePath = string.Empty;

                    // Create directory
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "EmailFile/" + mainWindowModel.SelectedServerType));

                    if (mainWindowModel.SelectedServerType.Equals("IMAP", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Read imap text file
                        textFilePath = Path.Combine(Environment.CurrentDirectory, "EmailFile/" + mainWindowModel.SelectedServerType + "/" + mainWindowModel.Username + "_" + mainWindowModel.SelectedEmailHeader.ImapEmailId + ".txt");
                    }
                    else if (mainWindowModel.SelectedServerType.Equals("POP3", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Read pop3 text file
                        textFilePath = Path.Combine(Environment.CurrentDirectory, "EmailFile/" + mainWindowModel.SelectedServerType + "/" + mainWindowModel.Username + "_" + mainWindowModel.SelectedEmailHeader.Pop3EmailId + ".txt");
                    }

                    // Check selected email in downloaded file
                    if (File.Exists(textFilePath))
                    {
                        // Update view model to show selected email bodies
                        mainWindowModel.SelectedBody = File.ReadAllText(textFilePath);
                    }
                    else
                    {
                        // Download on demand
                        OnDemandDownloadEmailBodies(mainWindowModel);
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                // Write to log
                return ex.Message;
            }
        }
        public void OnDemandDownloadEmailBodies(MainWindowModel mainWindowModel)
        {
            try
            {
                if (mainWindowModel.SelectedServerType.Equals("IMAP", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (mainWindowModel.SelectedEmailHeader.ImapEmailId != null)
                    {
                        // Connect to imap
                        Imap imap = utilModel.ConnectImap(mainWindowModel);

                        mainWindowModel.SelectedBody = DownloadImapEmailBody(imap, mainWindowModel.SelectedEmailHeader.ImapEmailId.Value, mainWindowModel);

                        imap.Close();
                    }
                }
                else if (mainWindowModel.SelectedServerType.Equals("POP3", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (mainWindowModel.SelectedEmailHeader.Pop3EmailId != null)
                    {
                        // Connect to pop3
                        Pop3 pop3 = utilModel.ConnectPop3(mainWindowModel);
                        MailBuilder mailBuilder = new MailBuilder();

                        mainWindowModel.SelectedBody = DownloadPop3EmailBody(pop3, mailBuilder, mainWindowModel.SelectedEmailHeader.Pop3EmailId, mainWindowModel);

                        pop3.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // Write to log                
            }

        }

        #region Imap
        private void DownloadImapEmailBodies(MainWindowModel mainWindowModel, string emailBodyListFilePath)
        {
            try
            {
                // Connect to imap
                Imap imap = utilModel.ConnectImap(mainWindowModel);

                List<long> allEmailBodyIdList = imap.Search(Flag.All);

                // Get existing emailid list
                List<long?> existingEmailBodyIdList = emailBodyList.Select(s => s.ImapEmailId).ToList();

                // Get new emailid list
                List<long> newEmailBodyIdList = allEmailBodyIdList.Where(s => !existingEmailBodyIdList.Contains(s)).ToList();

                foreach (long newEmailBodyId in newEmailBodyIdList)
                {
                    DownloadImapEmailBody(imap, newEmailBodyId, mainWindowModel);
                }
                imap.Close();

                // Insert object to file if there is new data
                if (newEmailBodyIdList.Count > 0)
                    utilModel.FileWriteAsync(emailBodyListFilePath, JsonConvert.SerializeObject(emailBodyList, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            }
            catch (Exception ex)
            {
                // Write to log
            }
        }
        private string DownloadImapEmailBody(Imap imap, long newEmailBodyId, MainWindowModel mainWindowModel)
        {
            try
            {
                lock (downloadEmailBodiesLock)
                {
                    // Validate downloaded email bodies
                    if (emailBodyList.FirstOrDefault(s => s.ImapEmailId == newEmailBodyId) == null)
                    {
                        // Get the structure of the email
                        BodyStructure structure = imap.GetBodyStructureByUID(newEmailBodyId);

                        // Download only text and html parts
                        string text = string.Empty;
                        string html = string.Empty;

                        if (structure.Text != null)
                            text = imap.GetTextByUID(structure.Text);
                        if (structure.Html != null)
                            html = imap.GetTextByUID(structure.Html);

                        // Write text file
                        string textFilePath = Path.Combine(Environment.CurrentDirectory, "EmailFile/" + mainWindowModel.SelectedServerType + "/" + mainWindowModel.Username + "_" + newEmailBodyId + ".txt");
                        utilModel.FileWriteAsync(textFilePath, text);

                        // Write html file
                        string htmlFilePath = Path.Combine(Environment.CurrentDirectory, "EmailFile/" + mainWindowModel.SelectedServerType + "/" + mainWindowModel.Username + "_" + newEmailBodyId + ".html");
                        utilModel.FileWriteAsync(htmlFilePath, html);

                        EmailBodyModel EmailBody = new EmailBodyModel
                        {
                            ImapEmailId = newEmailBodyId
                            // Reserved for attachment
                        };
                        emailBodyList.Add(EmailBody);

                        return text;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                // Write to log
                return ex.Message;
            }
        }
        #endregion

        #region Pop3
        private void DownloadPop3EmailBodies(MainWindowModel mainWindowModel, string emailBodyListFilePath)
        {
            try
            {
                // Connect to pop3
                Pop3 pop3 = utilModel.ConnectPop3(mainWindowModel);
                List<string> allEmailBodyIdList = pop3.GetAll();

                MailBuilder mailBuilder = new MailBuilder();

                // Get new data 
                List<string> existingEmailBodyIdList = emailBodyList.Select(s => s.Pop3EmailId).ToList();
                List<string> newEmailBodyIdList = allEmailBodyIdList.Where(s => !existingEmailBodyIdList.Contains(s)).ToList();

                foreach (string newEmailBodyId in newEmailBodyIdList)
                {
                    DownloadPop3EmailBody(pop3, mailBuilder, newEmailBodyId, mainWindowModel);
                }
                pop3.Close();

                // Insert object to file if there is new data
                if (newEmailBodyIdList.Count > 0)
                    utilModel.FileWriteAsync(emailBodyListFilePath, JsonConvert.SerializeObject(emailBodyList, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            }
            catch (Exception ex)
            {
                // Write to log
            }
        }
        private string DownloadPop3EmailBody(Pop3 pop3, MailBuilder mailBuilder, string newEmailBodyId, MainWindowModel mainWindowModel)
        {
            try
            {
                lock (downloadEmailBodiesLock)
                {
                    // Validate downloaded email bodies
                    if (emailBodyList.FirstOrDefault(s => s.Pop3EmailId == newEmailBodyId) == null)
                    {
                        // Get the structure of the email
                        var bodies = pop3.GetMessageByUID(newEmailBodyId);
                        IMail email = mailBuilder.CreateFromEml(bodies);

                        // Write text file
                        string textFilePath = Path.Combine(Environment.CurrentDirectory, "EmailFile/" + mainWindowModel.SelectedServerType + "/" + mainWindowModel.Username + "_" + newEmailBodyId + ".txt");
                        utilModel.FileWriteAsync(textFilePath, email.Text);

                        // Write html file
                        string htmlFilePath = Path.Combine(Environment.CurrentDirectory, "EmailFile/" + mainWindowModel.SelectedServerType + "/" + mainWindowModel.Username + "_" + newEmailBodyId + ".html");
                        utilModel.FileWriteAsync(htmlFilePath, email.Html);

                        EmailBodyModel EmailBody = new EmailBodyModel
                        {
                            Pop3EmailId = newEmailBodyId
                            // Reserved for attachment
                        };
                        emailBodyList.Add(EmailBody);

                        return email.Text;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                // Write to log
                return ex.Message;
            }
        }
        #endregion

        

        #endregion
    }
}
