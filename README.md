# The Objective

The objective of this test is to create the back-end code for a small email application. Each requirement is there for a very specific reason and will test your skills as a developer on Mailbird. If you have any questions before you start the test please don't hesitate to ask.

# Application Requirements

- Connect, using Mail.dll, to a mail server of the type specified in the 'Server type' combobox. The options should be 'IMAP' and 'POP3'. The encryption options should be 'Unencrypted', 'SSL/TLS' and 'STARTTLS'.
- Once connected, the app should begin downloading just the message envelopes/headers for all mail in the inbox, and display the messages in the data grid on the left as they download. The columns should at least include 'From', 'Subject' and 'Date'.
- While downloading the envelopes, the app should also be downloading the message bodies (the actual HTML/Text) of the downloaded envelopes in separate threads, so they're ready to be viewed when a message is selected.
- 5 connections total to the server should be made, for fetching headers and bodies as fast as possible, and these should be closed properly when no longer used.
- Clicking on a message in the data grid should select it, and show the message body HTML/Text in the text box on the right side. The body should be downloaded from the server on demand, if not already downloaded, or if already downloaded just shown immediately. Downloading on demand should use a new, separate, connection that exists outside of the 5 other connections.
- The application should be completely thread safe, and no header or message body should ever (even in theory) be able to download more than once. To accomplish this, some locking will likely be required.

# Mail.dll Reference

The following are direct links to the appropriate samples from the Mail.dll samples page, to help you with the implementation.

## IMAP

- [Connect to server](http://www.limilabs.com/blog/use-ssl-with-imap)
- [Download headers](http://www.limilabs.com/blog/get-email-information-from-imap-fast)
- [Download bodies](http://www.limilabs.com/blog/download-parts-of-email-message)

## POP3

- [Connect to server](http://www.limilabs.com/blog/use-ssl-with-pop3)
- [Download headers](http://www.limilabs.com/blog/get-email-headers-using-pop3-top-command) (you can assume that the TOP command is supported) 
- [Download bodies](http://www.limilabs.com/blog/get-common-email-fields-subject-text-with-pop3)


# Particular specification

- Embrace the KISS design principle. Simple is better than complex.
- If for whatever reason you're unable to complete all of the requirements, make a note of what you didn't complete, how you would have done it, and make sure that what IS completed is working perfectly and the code is nice and clean.
- Code duplication is kept to a minimum by using inheritance and otherwise unifying code.
- Magic strings and other code smells are kept to a minimum / is non existent. The new C# language features will help here.
- The view (MainWindow.xaml) uses MVVM and data binding extensively/exclusively.
- The solution directories and files are nicely structured.
- The app is built with a focus on speed, using multiple connections (within reason) to download headers and bodies.
- The app is thread safe and uses proper synchronization.
- The UI is never blocking and all expensive operations are run on other threads.
- Variables have meaningful names, and naming in general, including properties and methods, is consistent with: https://msdn.microsoft.com/en-us/library/ms229002(v=vs.110).aspx
