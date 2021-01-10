using DeveloperTest.Model;
using Limilabs.Client.IMAP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeveloperTest.ViewModel
{
    public class MainWindowViewModel 
    {
        #region Variable
        private MainWindowModel mainWindowModel;
        private ICommand start;
        #endregion

        #region Setter getter
        public MainWindowModel MainWindowModel
        {
            get { return mainWindowModel; }
            set { mainWindowModel = value; }
        }

        public ICommand StartCommand
        {
            get
            {
                if (start == null)
                    start = new Starter();
                return start;
            }
            set
            {
                start = value;
            }
        }
        #endregion
        public MainWindowViewModel()
        {
            mainWindowModel = new MainWindowModel
            {
                ServerType = new List<string> { "IMAP", "POP3" },
                Encryption = new List<string> { "Unencrypted", "SSL/TLS", "STARTTLS" },
                Server = "imap.gmail.com",
                Port = 993,
                SelectedServerType = "IMAP",
                SelectedEncryption = "SSL/TLS"
            };
        }

        #region Start button
        private class Starter : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                MainWindowModel mainWindowModel = (MainWindowModel)parameter;
                EmailHeaderModel emailHeaderModel = new EmailHeaderModel();
                EmailBodyModel emailBodyModel = new EmailBodyModel();

                Task.Run(() => emailHeaderModel.DownloadEmailHeaders(mainWindowModel)).ConfigureAwait(false);
                Task.Run(() => emailBodyModel.DownloadEmailBodies(mainWindowModel)).ConfigureAwait(false);

            }
        }
        #endregion

    }
}
