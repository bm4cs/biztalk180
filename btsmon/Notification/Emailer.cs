using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using btsmon.Model;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Notification
{
    public class Emailer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static DateTime LastMessageSentTime;

        public static void Send(Environment affectedEnvironment, List<Remediation> remediationList)
        {
            try
            {
                var emailThrottlePeriod = new TimeSpan(0, 0, AppSettings.MailThrottleSeconds);

                if (DateTime.Now - LastMessageSentTime < emailThrottlePeriod)
                {
                    Logger.Info("Email notifications are being throttled");
                    return;
                }

                var plainTextBody = GetEmailBody(affectedEnvironment, remediationList);

                var mailMessage = new MailMessage(
                    AppSettings.MailFrom,
                    AppSettings.MailTo,
                    "btsmon notification",
                    plainTextBody);

                mailMessage.BodyEncoding = Encoding.ASCII;
                // var textView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, MediaTypeNames.Text.Plain);
                // mailMessage.AlternateViews.Add(textView);

                var smtpClient = new SmtpClient(AppSettings.MailServer);
                // smtpClient.EnableSsl = true;
                // smtpClient.Credentials = new NetworkCredential(username, password);
                smtpClient.Send(mailMessage);

                LastMessageSentTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send email notification");
                Logger.Error(ex);
            }
        }

        private static string GetEmailBody(Environment affectedEnvironment, List<Remediation> remediationList)
        {
            var plainTextMessage = new StringBuilder();

            plainTextMessage.Append($@"▄▄▄▄· ▄▄▄▄▄.▄▄ · • ▌ ▄ ·.        ▐ ▄ 
▐█ ▀█▪•██  ▐█ ▀. ·██ ▐███▪▪     •█▌▐█
▐█▀▀█▄ ▐█.▪▄▀▀▀█▄▐█ ▌▐▌▐█· ▄█▀▄ ▐█▐▐▌
██▄▪▐█ ▐█▌·▐█▄▪▐███ ██▌▐█▌▐█▌.▐▌██▐█▌
·▀▀▀▀  ▀▀▀  ▀▀▀▀ ▀▀  █▪▀▀▀ ▀█▄▀▪▀▀ █▪

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