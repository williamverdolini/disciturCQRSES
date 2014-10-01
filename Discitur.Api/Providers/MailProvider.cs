using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Discitur.Api.Providers
{
    public class MailProvider
    {
        private static SmtpConfig smtpConfig;

        public static MailProvider GetMailprovider() {
            MailProvider mp = new MailProvider();
            smtpConfig = (SmtpConfig)SmtpConfig.GetConfiguration();
            return mp;
        }

        public async Task<bool> SendActivationEmail(string strTo, string username, string newPassword, string activationKey, string absoluteURL)
        {
            System.Collections.Specialized.ListDictionary replacements = new System.Collections.Specialized.ListDictionary();

            ActivationMailConfig config = MailConfigProvider.GetConfiguration<ActivationMailConfig>();
            string fromURL = string.IsNullOrEmpty(config.ActivationURL) ? absoluteURL : config.ActivationURL;

            replacements.Add("<%Username%>", username);
            replacements.Add("<%Password%>", newPassword);
            replacements.Add("<%ActivationKey%>", activationKey);
            replacements.Add("<%ActivationUrl%>", @fromURL);
            replacements.Add("<%ActivationPath%>", @config.ActivationPath);

            DisciturMailDef<ActivationMailConfig> md = new DisciturMailDef<ActivationMailConfig>(replacements);
            MailMessage mm = md.CreateMailMessage(strTo);

            return await this.SendEmail(mm, config.From, null);
        }

        public async Task<bool> SendForgottenPwdEmail(string strTo, string newPassword)
        {
            System.Collections.Specialized.ListDictionary replacements = new System.Collections.Specialized.ListDictionary();
            ForgottenPwdMailConfig config = MailConfigProvider.GetConfiguration<ForgottenPwdMailConfig>();
            replacements.Add("<%Password%>", newPassword);

            DisciturMailDef<ForgottenPwdMailConfig> md = new DisciturMailDef<ForgottenPwdMailConfig>(replacements);
            MailMessage mm = md.CreateMailMessage(strTo);

            return await this.SendEmail(mm, config.From, null);
        }

        private async Task<bool> SendEmail(MailMessage mm, String strFrom, string strAttachmentPath)
        {
            mm.From = new MailAddress(strFrom);

            if (strAttachmentPath != null && strAttachmentPath != "")
            {
                //Add Attachment
                Attachment attachFile = new Attachment(strAttachmentPath);
                mm.Attachments.Add(attachFile);
            }

            // For more info:
            // https://support.google.com/mail/answer/1173270?hl=en
            //
            SmtpClient smtp = new SmtpClient();
            try
            {
                smtp.Host = smtpConfig.Host;
                smtp.Port = smtpConfig.Port;//Specify your port No;
                smtp.EnableSsl = smtpConfig.EnableSsl; //Depending on server SSL Settings true/false
                smtp.UseDefaultCredentials = false;
                System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                NetworkCred.UserName = smtpConfig.UserName;
                NetworkCred.Password = smtpConfig.Password;
                smtp.Credentials = NetworkCred;
                await smtp.SendMailAsync(mm);
                return true;
            }
            catch
            {
                mm.Dispose();
                smtp = null;
                return false;
            }

        }

    }
}