using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        #region Variable
        private IList<string> serverType;
        private IList<string> encryption;
        private string server;
        private int port;
        private string username;
        private string password;
        private string selectedServerType;
        private string selectedEncryption;
        private string selectedBody;
        private EmailHeaderModel selectedEmailHeader;
        private IList<EmailHeaderModel> emailHeaderList;
        #endregion

        #region Setter getter
        public IList<string> ServerType
        {
            get
            {
                return serverType;
            }
            set
            {
                serverType = value;
            }
        }
        public IList<string> Encryption
        {
            get
            {
                return encryption;
            }
            set
            {
                encryption = value;
            }
        }
        public string Server
        {
            get
            {
                return server;
            }
            set
            {
                server = value;
            }
        }
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }
        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
            }
        }
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }
        public string SelectedServerType
        {
            get
            {
                return selectedServerType;
            }
            set
            {
                selectedServerType = value;
            }
        }
        public string SelectedEncryption
        {
            get
            {
                return selectedEncryption;
            }
            set
            {
                selectedEncryption = value;
            }
        }
        public string SelectedBody
        {
            get
            {
                return selectedBody;
            }
            set
            {
                selectedBody = value;
                OnPropertyChanged("SelectedBody");
            }
        }
        public EmailHeaderModel SelectedEmailHeader
        {
            get
            {

                return selectedEmailHeader;
            }
            set
            {
                selectedEmailHeader = value;

                EmailBodyModel emailBodyModel = new EmailBodyModel();                
                Task.Run(() => emailBodyModel.DisplaySelectedBody(this)).ConfigureAwait(false);

                OnPropertyChanged("SelectedEmailHeader");
            }
        }
        public IList<EmailHeaderModel> EmailHeaderList
        {
            get
            {
                return emailHeaderList;
            }
            set
            {
                emailHeaderList = value;
                OnPropertyChanged("EmailHeaderList");
            }
        }
        #endregion

        #region INotifyPropertyChanged Members  

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
