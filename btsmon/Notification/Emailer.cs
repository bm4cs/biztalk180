using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using btsmon.Model;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Notification
{
    public class Emailer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static DateTime _lastMessageSentTime;

        public static void Send(Environment affectedEnvironment, List<Remediation> remediationList)
        {
            try
            {
                var emailThrottlePeriod = new TimeSpan(0, AppSettings.MailThrottleMinutes, 0);

                if (DateTime.Now - _lastMessageSentTime < emailThrottlePeriod)
                {
                    Logger.Warn($"Emails are being throttled to one email per {AppSettings.MailThrottleMinutes} minutes. Last email sent at {_lastMessageSentTime.ToLongTimeString()}");
                    return;
                }

                var plainTextBody = GetEmailBody(affectedEnvironment, remediationList);

                var mailMessage = new MailMessage(
                    AppSettings.MailFrom,
                    AppSettings.MailTo,
                    "btsmon notification",
                    plainTextBody)
                {
                    BodyEncoding = Encoding.ASCII,
                    BodyTransferEncoding = TransferEncoding.SevenBit,
                    IsBodyHtml = false
                };

                var smtpClient = new SmtpClient(AppSettings.MailServer);
                smtpClient.Send(mailMessage);

                _lastMessageSentTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send email notification");
                Logger.Error(ex);
            }
        }

        private static string GetEmailBody(Environment affectedEnvironment, IEnumerable<Remediation> remediationList)
        {
            var plainTextMessage = new StringBuilder();

            plainTextMessage.Append($@"
______ _____ ________  ________ _   _ 
| ___ \_   _/  ___|  \/  |  _  | \ | |
| |_/ / | | \ `--.| .  . | | | |  \| |
| ___ \ | |  `--. \ |\/| | | | | . ` |
| |_/ / | | /\__/ / |  | \ \_/ / |\  |
\____/  \_/ \____/\_|  |_/\___/\_| \_/

btsmon recently detected some problems with {affectedEnvironment.GroupServer}

===================================

");

            foreach (var remediation in remediationList)
                plainTextMessage.Append($@"{remediation.Name} ({remediation.Type})

Expected state was '{remediation.ExpectedState}' but actual was '{remediation.ActualState}'

Remediation result: {remediation.Success} {remediation.ErrorMessage}

===================================
");

            return plainTextMessage.ToString();
        }
    }
}