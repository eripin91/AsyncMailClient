using Limilabs.Client.IMAP;
using Limilabs.Client.POP3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.Model
{
    public class UtilModel
    {
        #region Method
        public Imap ConnectImap(MainWindowModel mainWindowModel)
        {
            Imap imap = new Imap();

            if (mainWindowModel.SelectedEncryption.Equals("SSL/TLS", StringComparison.InvariantCultureIgnoreCase))
                imap.ConnectSSL(mainWindowModel.Server, mainWindowModel.Port);
            else
            {
                imap.Connect(mainWindowModel.Server, mainWindowModel.Port);
                if (mainWindowModel.SelectedEncryption.Equals("STARTTLS", StringComparison.InvariantCultureIgnoreCase))
                    imap.StartTLS();
            }

            imap.UseBestLogin(mainWindowModel.Username, mainWindowModel.Password);

            imap.SelectInbox();

            return imap;
        }
        public Pop3 ConnectPop3(MainWindowModel mainWindowModel)
        {
            Pop3 pop3 = new Pop3();

            if (mainWindowModel.SelectedEncryption.Equals("SSL/TLS", StringComparison.InvariantCultureIgnoreCase))
                pop3.ConnectSSL(mainWindowModel.Server, mainWindowModel.Port);
            else
            {
                pop3.Connect(mainWindowModel.Server, mainWindowModel.Port);
                if (mainWindowModel.SelectedEncryption.Equals("STARTTLS", StringComparison.InvariantCultureIgnoreCase))
                    pop3.StartTLS();
            }

            pop3.Login(mainWindowModel.Username, mainWindowModel.Password);

            return pop3;
        }
        public async Task FileWriteAsync(string path, string data)
        {
            using (var sw = new StreamWriter(path))
            {
                await sw.WriteAsync(data);
            }
        }
        #endregion 
    }
}
