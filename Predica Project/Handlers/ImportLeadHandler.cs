using Predica.Xrm.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;
using System.IO;
using Newtonsoft.Json;
using Predica.ExternalModels;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Predica.QueueService;
using Predica.Xrm.Core.DataAccess;
using Xrm.Core;
using Microsoft.Crm.Sdk.Messages;
using System.Net.Mail;
using System.Net;
using CrmSynchronizationApp.Crm.DataAccess;

namespace CrmSynchronizationApp.Handlers
{
    public class ImportLeadHandler : ProcessHandlerHandler
    {
        private ILeadRepository leadRepository;
        private IContactRepository contactRepository;
        private IConferenceAttendanceRepository conferenceAttendanceRepository;
        private LeadQueueSender leadQueueSender;
        private SmtpClient smtpClient;
        private List<Lead> validLeads;
        private List<EntityReference> newAddedContacts;
        public ImportLeadHandler(ILogger log) : base(log)
        {
            this.leadRepository = new LeadRepository(this.Service);
            this.contactRepository = new ContactRepository(this.Service);
            this.conferenceAttendanceRepository = new ConferenceAttendanceRepository(this.Service);
            this.leadQueueSender = new LeadQueueSender(log);
            this.smtpClient = GetSmtpClient();
            this.validLeads = new List<Lead>();
            this.newAddedContacts = new List<EntityReference>();
        }

        public void ImportLeads(Stream blobStream)
        {
            var conferenceLeadsList = this.GetConferenceLeadsListFromBlob(blobStream);

            if (conferenceLeadsList != null && conferenceLeadsList.Leads != null)
            {
                this.ValidateLeadAndIntegrateWithCrm(conferenceLeadsList.Leads);
                var allNewContacts = this.GetAllNewlyCreatedContactsData();
                this.leadQueueSender.SendUsaLeadsToQueue(allNewContacts);
                this.SendEmailsToFranceLeads();
            }
            else
            {
                Log.LogError("There is no valid data in the file");
            }
        }

        private ConferenceLeadsList GetConferenceLeadsListFromBlob(Stream blobStream)
        {
            var conferenceLeadsList = new ConferenceLeadsList();
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(blobStream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                conferenceLeadsList = (ConferenceLeadsList)serializer.Deserialize(jsonTextReader, typeof(ConferenceLeadsList));
            }

            if (conferenceLeadsList != null && conferenceLeadsList?.Leads != null)
            {
                this.Log.LogInformation($"There were {conferenceLeadsList.Leads.Count} found in the file.");
            }
            else
            {
                this.Log.LogError($"File is invalid and has no leads!");
            }

            return conferenceLeadsList;

        }

        private void ValidateLeadAndIntegrateWithCrm(List<ConferenceLead> leads)
        {
            foreach (var conferenceLead in leads)
            {
                Lead crmLead = this.Mapper.Map<ConferenceLead, Lead>(conferenceLead);
                List<Lead> matchingExistingCrmLeads = this.leadRepository.GetLeadByEmail(crmLead.EMailAddress1);
                bool areRecordsCorrect;

                Guid? relatedContactId = this.FindRelatedLeadContactAndValidate(crmLead, matchingExistingCrmLeads, out areRecordsCorrect);

                if (!areRecordsCorrect)
                {
                    continue;
                }

                bool particaptedInOtherConferenceAtThisTime = this.conferenceAttendanceRepository.HadContactAnyConferenceAtThatTime(
                        relatedContactId,
                        conferenceLead.ConferenceBeginDate,
                        conferenceLead.ConferenceEndDate);

                if (!particaptedInOtherConferenceAtThisTime)
                {
                    SendLeadToCrmAndConvertToContact(crmLead, conferenceLead, relatedContactId, matchingExistingCrmLeads);
                }
                else
                {
                    this.Log.LogError($"Lead {crmLead?.FirstName} {crmLead?.LastName} won't be added to CRM " +
                        $"beacuse this person has been already added as participating in conference at this time");
                }

            }
        }

        private void SendLeadToCrmAndConvertToContact(Lead crmLead, ConferenceLead conferenceLead, Guid? relatedContactId, List<Lead> matchingExistingCrmLeads)
        {
            if (relatedContactId.HasValue)
            {
                crmLead.ParentContactId = new EntityReference(Contact.EntityLogicalName, relatedContactId.Value);
            }
            crmLead.new_issigningcontractsave = CheckIfContractSigningIsSave(conferenceLead.Age);

            Guid addedLeadId;
            QualifyLeadResponse qualifyResponse = null;

            if (matchingExistingCrmLeads.Count > 1)
            {
                this.Log.LogError($"More than one lead with the same email exists in CRM! Lead won't be transfered.Lead {crmLead?.FirstName} {crmLead?.LastName}, email {crmLead?.EMailAddress1}");
                return;
            }
            else if (matchingExistingCrmLeads.Count == 1)
            {
                this.Log.LogInformation($"Lead {crmLead?.FirstName} {crmLead?.LastName} already exists in CRM.");
                addedLeadId = matchingExistingCrmLeads[0].Id;
            }
            else
            {
                this.Log.LogInformation($"Lead {crmLead?.FirstName} {crmLead?.LastName} will be added to CRM");
                crmLead.Subject = "Conference";
                addedLeadId = this.leadRepository.Create(crmLead);
            }

            bool hasAnyLeadBeenFound = matchingExistingCrmLeads.Count > 0;
            if (!hasAnyLeadBeenFound || (hasAnyLeadBeenFound && matchingExistingCrmLeads[0].StatusCode != lead_statuscode.Qualified))
            {
                this.Log.LogInformation($"Lead {crmLead?.FirstName} {crmLead?.LastName} will be converted to contact");
                qualifyResponse = this.leadRepository.QualifyLeadToContact(addedLeadId, relatedContactId == null);
            }

            this.validLeads.Add(crmLead);

            EntityReference newContactReference = GetContactReference(qualifyResponse, relatedContactId);

            this.Log.LogInformation($"Contact {crmLead?.FirstName} {crmLead?.LastName} will have saved attandance in conference.");
            this.conferenceAttendanceRepository.Create(new new_conferenceattendance()
            {
                new_contactid = newContactReference,
                new_name = conferenceLead.ConferenceBeginDate?.ToString() + conferenceLead.ConferenceEndDate?.ToString(),
                new_conferencebegindate = conferenceLead.ConferenceBeginDate,
                new_conferenceleavedate = conferenceLead.ConferenceEndDate
            });
        }

        private Guid? FindRelatedLeadContactAndValidate(Lead crmLead, List<Lead> matchingExistingCrmLeads, out bool areRecordsCorrect)
        {
            List<Guid> relatedContactsId = this.contactRepository.GetContactsWithSameEmailOrFullNameOrPhone(crmLead.EMailAddress1, crmLead.FirstName, crmLead.LastName, crmLead.Telephone1);
            areRecordsCorrect = true;

            if (relatedContactsId.Count > 1 || matchingExistingCrmLeads.Count > 1)
            {
                this.Log.LogError($"Lead {crmLead?.FirstName} {crmLead?.LastName} with email {crmLead?.EMailAddress1} has contact or lead duplicate in CRM system. Record will be skipped.");
                areRecordsCorrect = false;
            }
            else if (relatedContactsId.Count == 1 && matchingExistingCrmLeads.Count == 1 && matchingExistingCrmLeads[0].ContactId != null &&
                relatedContactsId[0] != matchingExistingCrmLeads[0].ContactId.Id)
            {
                this.Log.LogError($"For lead {crmLead?.FirstName} {crmLead?.LastName} with email {crmLead?.EMailAddress1} CRM lead has other related contact that is could be assumed from other CRM contact email");
                areRecordsCorrect = false;
            }
            else if (matchingExistingCrmLeads.Count == 1 && matchingExistingCrmLeads[0].ContactId != null)
            {
                return matchingExistingCrmLeads[0].ContactId.Id;
            }
            else if (relatedContactsId.Count == 1)
            {
                return relatedContactsId[0];
            }
            return null;
        }
        private EntityReference GetContactReference(QualifyLeadResponse qualifyResponse, Guid? relatedContactId)
        {
            EntityReference contactReference = null;

            if (!relatedContactId.HasValue)
            {
                contactReference = qualifyResponse.CreatedEntities?[0];
                this.newAddedContacts.Add(contactReference);
            }
            else
            {
                contactReference = new EntityReference(Contact.EntityLogicalName, relatedContactId.Value);
            }

            return contactReference;
        }

        private bool? CheckIfContractSigningIsSave(int? age)
        {
            if (age.HasValue)
            {
                if (age.Value < 25 || age.Value > 50)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return null;
        }

        private void SendEmailsToFranceLeads()
        {
            foreach (var lead in this.validLeads)
            {
                if (lead?.Address1_Country?.ToLower() == "france" && lead?.EMailAddress1 != null)
                {
                    this.Log.LogInformation($"Thank you email will be send to lead {lead?.FirstName} {lead?.LastName}");
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("conferenceleadsemailssender@gmail.com"),
                        Subject = "Participation à la conférence",
                        Body = "<h1>Merci beaucoup d'avoir participé à notre conférence.</h1>",
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(lead.EMailAddress1);
                    this.smtpClient.Send(mailMessage);
                }
            }
        }

        private List<Contact> GetAllNewlyCreatedContactsData()
        {
            this.Log.LogInformation("Fetching contacts data for queue sending...");
            List<Contact> contactsToSend = new List<Contact>();
            foreach (var contactReference in this.newAddedContacts)
            {
                var contact = this.contactRepository.Retrieve(contactReference.Id);
                contactsToSend.Add(contact);
            }
            return contactsToSend;
        }

        private SmtpClient GetSmtpClient()
        {
            return new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("conferenceleadsemailssender@gmail.com", "predicatask"),
                EnableSsl = true,
            };
        }

    }
}
