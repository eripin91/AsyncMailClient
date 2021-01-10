using Limilabs.Client.IMAP;
using Limilabs.Client.POP3;
using Limilabs.Mail;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeveloperTest.Model
{
    public class EmailHeaderModel
    {
        #region Variable
        private long? imapEmailId;
        private string pop3EmailId;
        private string from;
        private string subject;
        private DateTime? emailDate;
        private UtilModel utilModel = new UtilModel();
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
        public string From
        {
            get
            {
                return from;
            }
            set
            {
                from = value;
            }
        }
        public string Subject
        {
            get
            {
                return subject;
            }
            set
            {
                subject = value;
            }
        }
        public DateTime? EmailDate
        {
            get
            {
                return emailDate;
            }
            set
            {
                emailDate = value;
            }
        }
        #endregion


        #region Method
        public void DownloadEmailHeaders(MainWindowModel mainWindowModel)
        {
            try
            {
                // Create directory
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "LocalData"));

                // File path for email header list
                string emailHeaderListFilePath = Path.Combine(Environment.CurrentDirectory, "LocalData/" + mainWindowModel.Username + "_" + mainWindowModel.SelectedServerType + "_headers.json");

                // Read from local file 
                string existingEmailHeaderList = File.Exists(emailHeaderListFilePath) ? File.ReadAllText(emailHeaderListFilePath) : string.Empty;
                
                // Deserialize json 
                List<EmailHeaderModel> emailHeaderList = JsonConvert.DeserializeObject<List<EmailHeaderModel>>(existingEmailHeaderList)
                    ?? new List<EmailHeaderModel>();

                // Validate server type
                if (mainWindowModel.SelectedServerType.Equals("IMAP", StringComparison.InvariantCultureIgnoreCase))
                {
                    DownloadImapEmailHeaders(mainWindowModel,emailHeaderList,emailHeaderListFilePath);
                }
                else if (mainWindowModel.SelectedServerType.Equals("POP3", StringComparison.InvariantCultureIgnoreCase))
                {
                    DownloadPop3EmailHeaders(mainWindowModel, emailHeaderList, emailHeaderListFilePath);
                }
            }
            catch (Exception ex)
            {
                // Write to log
            }

        }

        private void DownloadImapEmailHeaders(MainWindowModel mainWindowModel, List<EmailHeaderModel> emailHeaderList, string emailHeaderListFilePath)
        {
            try
            {
                // Connect to imap
                Imap imap = utilModel.ConnectImap(mainWindowModel);
                List<long> allEmailHeaderIdList = imap.Search(Flag.All);

                // Get new data 
                List<long?> existingEmailHeaderIdList = emailHeaderList.Select(s => s.ImapEmailId).ToList();
                List<long> newEmailHeaderIdList = allEmailHeaderIdList.Where(s => !existingEmailHeaderIdList.Contains(s)).ToList();

                List<MessageInfo> Infos = imap.GetMessageInfoByUID(newEmailHeaderIdList);

                foreach (MessageInfo Info in Infos)
                {
                    EmailHeaderModel EmailHeader = new EmailHeaderModel
                    {
                        ImapEmailId = Info?.Envelope?.UID,
                        From = string.Join(",", Info?.Envelope?.From?.Select(s => s.Address)),
                        Subject = Info?.Envelope?.Subject,
                        EmailDate = Info?.Envelope?.Date
                    };
                    emailHeaderList.Add(EmailHeader);
                }

                imap.Close();

                // Insert existing data to view model
                mainWindowModel.EmailHeaderList = emailHeaderList;

                // Insert object to file if there is new data
                if (newEmailHeaderIdList.Count > 0)
                    utilModel.FileWriteAsync(emailHeaderListFilePath, JsonConvert.SerializeObject(emailHeaderList, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            }
            catch(Exception ex)
            {
                // Write to log
            }
        }

        private void DownloadPop3EmailHeaders(MainWindowModel mainWindowModel, List<EmailHeaderModel> emailHeaderList, string emailHeaderListFilePath)
        {
            try
            {
                // Connect to pop3
                Pop3 pop3 = utilModel.ConnectPop3(mainWindowModel);
                List<string> allEmailHeaderIdList = pop3.GetAll();

                MailBuilder mailBuilder = new MailBuilder();

                // Get new data 
                List<string> existingEmailHeaderIdList = emailHeaderList.Select(s => s.Pop3EmailId).ToList();
                List<string> newEmailHeaderIdList = allEmailHeaderIdList.Where(s => !existingEmailHeaderIdList.Contains(s)).ToList();

                foreach (string newEmailHeaderId in newEmailHeaderIdList)
                {
                    var headers = pop3.GetHeadersByUID(newEmailHeaderId);
                    IMail email = mailBuilder.CreateFromEml(headers);

                    EmailHeaderModel EmailHeader = new EmailHeaderModel
                    {
                        Pop3EmailId = newEmailHeaderId,
                        From = string.Join(",", email.From?.Select(s => s.Address)),
                        Subject = email.Subject,
                        EmailDate = email.Date
                    };
                    emailHeaderList.Add(EmailHeader);
                }
                pop3.Close();

                // Insert existing data to view model
                mainWindowModel.EmailHeaderList = emailHeaderList;

                // Insert object to file if there is new data
                if (newEmailHeaderIdList.Count > 0)
                    utilModel.FileWriteAsync(emailHeaderListFilePath, JsonConvert.SerializeObject(emailHeaderList, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            }
            catch(Exception ex)
            {
                // Write to log
            }
        }
        #endregion
    }
}
