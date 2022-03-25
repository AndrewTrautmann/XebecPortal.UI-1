﻿using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage;
using Azure;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XebecPortal.UI.Pages.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace XebecPortal.UI.Pages.Applicant
{
    public partial class ApplicationProfile
    {
        private StringBuilder status = new StringBuilder("waiting");
        private ResumeResultModel resumeResultModel = new ResumeResultModel();

        private int increment = 1;
        private bool workHistUpdate;
        private bool eduUpdate;
        private bool editMode;
        private bool workEditMode;
        private bool eduEditMode;

        private List<WorkHistory> workHistoryList = new();
        private WorkHistory workHistory = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
        private List<Education> educationList = new();
        private Education education = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
        private ProfilePortfolioLink profilePortfolio = new() { AppUserId = 1 };
        private AdditionalInformation additionalInformation = new() { AppUserId = 1, Disability = "No" };
        private PersonalInformation personalInformation = new() { AppUserId = 1 }; // Not sure if it even stores the information correctly
        private List<PersonalInformation> personalInformationList = new();
        private List<References> referencesList = new();
        private References references = new() { AppUserId = 1 };


        private List<SkillsInformation> selectedSkillsList1 = new();

        private IList<SkillBank> apiSkills = new List<SkillBank>();
        private IList<SkillBank> skillListFilter = new List<SkillBank>();

        private IJSObjectReference _jsModule;
        string _dragEnterStyle;
        IBrowserFile fileNames;
        private int maxAllowedSize = 10 * 1024 * 1024;
        private string progressBar = 0.ToString("0");


        private bool educationProgressVal = false;
        private bool workProgressVal = false;
        private bool referenceProgressVal = false;

        private APIRoot apiroot = new APIRoot();

        protected override async Task OnInitializedAsync()
        {
            //apiSkills = await httpClient.GetFromJsonAsync<IList<SkillBank>>("https://xebecapi.azurewebsites.net/api/SkillsBank");
            _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./jsPages/Applicant/ApplicationProfile.js");
            populateList();
            skillListFilter = apiSkills;
            // var token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjNDNjZCRjIzMjBGNkY4RDQ2QzJERDhCMjI0MEVGMTFENTZEQkY3MUYiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJQR2FfSXlEMi1OUnNMZGl5SkE3eEhWYmI5eDgifQ.eyJuYmYiOjE2NDgxMDMxNzksImV4cCI6MTY0ODEwNjc3OSwiaXNzIjoiaHR0cHM6Ly9hdXRoLmVtc2ljbG91ZC5jb20iLCJhdWQiOlsiZW1zaV9vcGVuIiwiaHR0cHM6Ly9hdXRoLmVtc2ljbG91ZC5jb20vcmVzb3VyY2VzIl0sImNsaWVudF9pZCI6InF0dGF5Y2Y4cDdodWEwamIiLCJlbWFpbCI6ImFuZHJldy50cmF1dG1hbm5AMW5lYnVsYS5jb20iLCJjb21wYW55IjoiTmVidWxhIiwibmFtZSI6IkFuZHJldyBUcmF1dG1hbm4iLCJpYXQiOjE2NDgxMDMxNzksInNjb3BlIjpbImVtc2lfb3BlbiJdfQ.UaGiM8wC7TBB4sDeF8PjzGWYb8Scu4BFy8IrjXTuv4nOMDuhdsIpYMesLneYSDeA0vyuBcVEtmast-J7c5GO15SMF-KE4347pyEauKATaxYAAxSTyzYKcI_eouvh1ZLLoFPyLnO5OL72ivu2eRcTFhnmWWhPZpdt4Hg3NjIUzTM3A6NbTnA3WAKZ7u6O8_KVMiQFg4fPCqEMQXnLS_MwQFWLLA2fblrqRMb82k6XabDUg1WKU3u5sFls9DDi-FZtBqpOxKR4bOMslgCVWxsG6LCrzagv2L0-nz-pOjdfHYwQsl3i2u7Zzl6fVowhlgMHZImMvUglFmYzj_OGTgNVNA";
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // apiroot = await httpClient.GetFromJsonAsync<APIRoot>("https://emsiservices.com/skills/versions/latest/skills?limit=100");
            //var client = new RestClient("https://emsiservices.com/skills/versions/latest/skills"); // this will provide you with all of the available skills
            //var request = new RestRequest(Method.GET);
            //request.AddHeader("Authorization", "Bearer <ACCESS_TOKEN>");
            //IRestResponse response = client.Execute(request);

            
            Console.WriteLine("apiskills count: " + apiSkills.Count());
        }
       
        private string skillWarning = "";
        private bool warning;


        private void populateList()
        {
            apiSkills.Add(new()
            {
                Description = "Java",
            });
            apiSkills.Add(new()
            {
                Description = "CSS",
            });
            apiSkills.Add(new()
            {
                Description = "C#",
            });
            apiSkills.Add(new()
            {
                Description = "Azure",
            });
        }

        string searchedSkill;

        private async Task SearchSkillList(ChangeEventArgs e)
        {
            searchedSkill = e.Value.ToString();
            Console.WriteLine("searchedSkill: " + searchedSkill);
            skillListFilter = apiSkills; // joblist is the skills that you will get from the DB
            //FilterDataDisplayHelper();
            if (!string.IsNullOrEmpty(searchedSkill) && searchedSkill != " ")
            {
                skillListFilter = skillListFilter.Where(x => x.Description.Contains(searchedSkill, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            if (searchedSkill?.Any() == true)
            {
                skillListFilter = skillListFilter.Where(x => x.Description.Contains(searchedSkill, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }
        }

        private void addToSelectedInfo(SkillBank info)
        {
            warning = false;
            var validCheck = selectedSkillsList1.FindAll(r => r.Description.Equals(info.Description));
            if (validCheck.Count == 0)
            {
                selectedSkillsList1.Add(new()
                {
                    Description = info.Description,
                    AppUserId = 1,
                });
            }
            else
            {
                warning = true;
                skillWarning = "Skill has already been added!";
            }
        }
        private void removeFromSelectedInfo(SkillsInformation info)
        {
            selectedSkillsList1.RemoveAll(x => x.Description.Equals(info.Description)); ;
        }
        /* Use later
        private static string GetMultiSelectionTextSkills(List<string> selectedValues)
        {
            return $"Selected Skill{(selectedValues.Count > 1 ? "s" : " ")}: {string.Join(", ", selectedValues.Select(x => x))}";
        }
        */
        /* wait for the DB  then I can use this
        private object CardClassSelect(Skills developer)
        {
            if (selectedSkills.FindAll(d => d.id == developer.id).Count() > 0)
                return "card-class-for-skills-selected";
            return "card-class-for-skills";
        }
        */
        private void AddReferences()
        {
            var validCheck = referencesList.FindAll(r => r.Name.Equals(references.Name) && string.Equals(r.Surname, references.Surname, StringComparison.OrdinalIgnoreCase) && string.Equals(r.Email, references.Email, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ContactNum, references.ContactNum, StringComparison.OrdinalIgnoreCase));

            var emptyCheck = referencesList.FindAll(r => string.IsNullOrEmpty(r.Name) || string.IsNullOrEmpty(r.Surname) || string.IsNullOrEmpty(r.Email) || string.IsNullOrEmpty(r.ContactNum));

            referencesList.Add(new()
            {
                AppUserId = 1,
                Name = references.Name,
                Surname = references.Surname,
                Email = references.Email,
                ContactNum = references.ContactNum,
            });
            references = new();
        }

        private References tempRef;

        private void Save(References referenceValues)
        {
            editMode = false;
            Logger.LogInformation("Valid submit called");
            int index = referencesList.FindIndex(x => x.Equals(referenceValues));
            referencesList[index] = references;
            references = new();
        }

        private void Cancel(References referenceValues)
        {
            int index = referencesList.FindIndex(x => x.Equals(referenceValues));
            referencesList[index] = tempRef;
            references = new();
            editMode = false;

        }

        private void DeleteReference(int refID)
        {
            if (referencesList.Count == 1)
            {
                referenceProgressVal = true;
            }
            referencesList.RemoveAll(x => x.Id == refID);
        }


        private void SelectReference(References referenceValues)
        {
            editMode = true;
            int index = referencesList.FindIndex(x => x.Equals(referenceValues));
            references = referencesList[index];
            tempRef = (References)references.Clone();
        }

        private WorkHistory tempWorkHistory;

        private void addWorkHistoryTest()
        {
            workHistoryList.Add(new()
            {
                AppUserId = 1,
                CompanyName = workHistory.CompanyName,
                JobTitle = workHistory.JobTitle,
                StartDate = workHistory.StartDate,
                EndDate = workHistory.EndDate,
                Description = workHistory.Description
            });
            workHistory = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
        }

        private void DeleteWorkHistory(WorkHistory workHistoryValues)
        {
            if (workHistoryList.Count == 1)
            {
                workProgressVal = true;
            }
            workHistoryList.RemoveAll(x => x == (workHistoryValues));
            workHistory = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
            workHistUpdate = false;
        }

        private void SelectWorkHistory(WorkHistory workHistoryValues)
        {
            workEditMode = true;
            int index = workHistoryList.FindIndex(x => x == (workHistoryValues));
            workHistory = workHistoryList[index];
            tempWorkHistory = (WorkHistory)workHistory.Clone();
        }

        // This is to display the selectedHistory tab
        private object GetStyling(WorkHistory item)
        {
            if ((workHistory.CompanyName == item.CompanyName) && (workHistory.JobTitle == item.JobTitle) && (workHistory.Description == item.Description))
                return "box-shadow: inset 0px -50px 36px -28px #49E5EF, inset 0px -50px 36px -28px #2294E3, inset 0px -50px 36px -28px #d35bc9, inset 0px -50px 36px -28px #00bcae;background: rgba(255, 255, 255, 0);backdrop - filter: blur(5.6px);-webkit-backdrop-filter: blur(5.6px); border: 1px solid rgba(255, 255, 255, 0.04); min-height:15vh; overflow-y: auto; ";
            return "";
        }

        private object GetEduStyling(Education item)
        {
            if ((education.Insitution == item.Insitution) && (education.Qualification == item.Qualification))
                return "box-shadow: inset 0px -50px 36px -28px #49E5EF, inset 0px -50px 36px -28px #2294E3, inset 0px -50px 36px -28px #d35bc9, inset 0px -50px 36px -28px #00bcae;backdrop - filter: blur(5.6px);-webkit - backdrop - filter: blur(5.6px);border: 1px solid rgba(255, 255, 255, 0.04);max - height: 60vh;overflow - y: auto; ";
            return "";
        }
        private object GetRefStyling(References item)
        {
            if ((references.Email == item.Email) && (references.Name == item.Name) && (references.ContactNum == item.ContactNum) && (references.Email == item.Email))
                return "box-shadow: inset 0px -50px 36px -28px #49E5EF, inset 0px -50px 36px -28px #2294E3, inset 0px -50px 36px -28px #d35bc9, inset 0px -50px 36px -28px #00bcae;backdrop - filter: blur(5.6px);-webkit - backdrop - filter: blur(5.6px);border: 1px solid rgba(255, 255, 255, 0.04);max - height: 60vh;overflow - y: auto; ";
            return "";
        }
        private void SaveWorkHistory(WorkHistory workHistoryValues)
        {
            workEditMode = false;
            int index = workHistoryList.FindIndex(x => x.Equals(workHistoryValues));
            workHistoryList[index] = workHistory;
            workHistory = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
        }

        private void CancelWorkHistory(WorkHistory workHistoryValues)
        {
            int index = workHistoryList.FindIndex(x => x.Equals(workHistoryValues));
            workHistoryList[index] = tempWorkHistory;
            workHistory = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
            workEditMode = false;
        }

        private async Task UpdateWorkHistory(WorkHistory workHistoryValues)
        {
            if (await _jsModule.InvokeAsync<bool>("WorkHistory"))
            {
                int index = workHistoryList.FindIndex(x => x.Id == workHistoryValues.Id);
                workHistoryList[index] = workHistoryValues;
                workHistory = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
                workHistUpdate = false;
            }
        }

        private void PopulateWorkHistory(int id)
        {
            workHistory = workHistoryList.FirstOrDefault(x => x.Id == id);
            workHistUpdate = true;
        }

        private Education tempEducation;
        private void AddEducationTakeTwo()
        {
            educationList.Add(new()
            {
                AppUserId = 1,
                Insitution = education.Insitution,
                Qualification = education.Qualification,
                StartDate = education.StartDate,
                EndDate = education.EndDate,
            });
            education = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
        }

        private void DeleteEducation(Education educationValues)
        {
            if (educationList.Count == 1)
            {
                educationProgressVal = true;
            }

            educationList.RemoveAll(x => x.Equals(educationValues));
            education = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
            eduUpdate = false;
        }

        private void SelectEducation(Education educationValues)
        {
            eduEditMode = true;
            int index = educationList.FindIndex(x => x.Equals(educationValues));
            education = educationList[index];
            tempEducation = (Education)education.Clone();
        }

        private void SaveEducation(Education educationValues)
        {
            eduEditMode = false;
            int index = educationList.FindIndex(x => x.Equals(educationValues));
            educationList[index] = education;
            education = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
        }

        private void CancelEducation(Education educationValues)
        {
            int index = educationList.FindIndex(x => x.Equals(educationValues));
            educationList[index] = tempEducation;
            education = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
            eduEditMode = false;
        }

        private async Task UpdateEducation(Education educationValues)
        {
            if (await _jsModule.InvokeAsync<bool>("Education"))
            {
                int index = educationList.FindIndex(x => x.Id == educationValues.Id);
                educationList[index] = educationValues;
                education = new() { StartDate = DateTime.Today, EndDate = DateTime.Today };
                eduUpdate = false;
            }
        }

        private void PopulateEducation(int id)
        {
            education = educationList.FirstOrDefault(x => x.Id == id);
            eduUpdate = true;
        }

        private void StartDateCheck()
        {
            workHistory.StartDate = workHistory.StartDate > workHistory.EndDate ? workHistory.EndDate : workHistory.StartDate;
            education.StartDate = education.StartDate > education.EndDate ? education.EndDate : education.StartDate;
        }

        private void EndDateCheck()
        {
            workHistory.EndDate = workHistory.EndDate < workHistory.StartDate ? workHistory.StartDate : workHistory.EndDate;
            education.EndDate = education.EndDate < education.StartDate ? education.StartDate : education.EndDate;
        }

        private async Task Submit()
        {
            await httpClient.PostAsJsonAsync("https://xebecapi.azurewebsites.net/api/PersonalInformation", personalInformation);
            await httpClient.PostAsJsonAsync("https://xebecapi.azurewebsites.net/api/AdditionalInformation", additionalInformation);

            foreach (var item in workHistoryList)
            {
                await httpClient.PostAsJsonAsync("https://xebecapi.azurewebsites.net/api/WorkHistory", item);
            }
            foreach (var item in educationList)
            {
                await httpClient.PostAsJsonAsync("https://xebecapi.azurewebsites.net/api/Education", item);
            }
            foreach (var item in referencesList)
            {
                await httpClient.PostAsJsonAsync("https://xebecapi.azurewebsites.net/api/Reference", item);
            }

            foreach (var item in selectedSkillsList1)
            {
                await httpClient.PostAsJsonAsync("https://xebecapi.azurewebsites.net/api/Skill", item);
            }


            await httpClient.PostAsJsonAsync("https://xebecapi.azurewebsites.net/api/ProfilePortfolioLink", profilePortfolio);

            //if (await _jsModule.InvokeAsync<bool>("PersonalInformation"))
            //{

            //}
        }
        // This is just used to indicate to the user that their info has been successfully added to the DB
        /* using 
         (var msg = await httpClient.PostAsJsonAsync<LoginModel>("/api/auth/login", user,
         System.Threading.CancellationToken.None))
         {
            if (msg.IsSuccessStatusCode)
                 {
                    await jsRuntime.InvokeVoidAsync("alert", "You Data Has Been Captured");
                 }
             }
        */
        private string storageAcc = "storageaccountxebecac6b";
        private string imgContainer = "images";

        private static int num = 1;
        async Task OnInputFileChangedAsync(InputFileChangeEventArgs e)
        {
            fileNames = e.File;
            progressBar = 0.ToString("0");
            status = new StringBuilder($"Uploading file {num++}");


            //Upload to blob - start
            status = new StringBuilder($"current file {fileNames.Name}");

            status.AppendLine("\n");

            // Change the blobStorage location still
            var blobUri = new Uri("https://"
                                  + storageAcc
                                  + ".blob.core.windows.net/" 
                                  + imgContainer
                                  + "/"
                                  + fileNames.Name);
            Console.WriteLine("fileName " + fileNames.Name);

            AzureSasCredential credential = new AzureSasCredential(
                "sp=racwdli&st=2022-02-28T08:30:27Z&se=2022-03-11T16:30:27Z&sv=2020-08-04&sr=c&sig=TE%2B2VCz%2B6KKFbYHIkQwxGPOYWVUtht3xBPYZ8bE3kH4%3D");
            BlobClient blobClient = new BlobClient(blobUri, credential, new BlobClientOptions());
            status.AppendLine("Created blob client");

            status.AppendLine("\n");
            status.AppendLine("Sending to blob");
            //displayProgress = true;
            var res = await blobClient.UploadAsync(fileNames.OpenReadStream(maxAllowedSize), new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = fileNames.ContentType },
                TransferOptions = new StorageTransferOptions
                {
                    InitialTransferSize = 1024 * 1024,
                    MaximumConcurrency = 10
                },
                ProgressHandler = new Progress<long>((progress) =>
                {
                    progressBar = (100.0 * progress / fileNames.Size).ToString("0");
                })
            });

            if (Convert.ToInt32(progressBar) == 100)
            {
                //var content = new StringContent($"\"{blobUri.ToString()}\"",  Encoding.UTF8, "applicationModel/json");

                var content = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("url", $"{blobUri.ToString()}")
                                });
                // var urlJson =
                //     new StringContent(JsonSerializer.Serialize("content"""), Encoding.UTF8, "applicationModel/json");

                //var response = await httpClient.GetAsync("https://xebecapi.azurewebsites.net/api/ResumeParser");


                var resp = await httpClient.PostAsync("http://localhost:5002/api/ResumeParser/", content);
                //status = new StringBuilder(resp.StatusCode.ToString());
                var respContent = await resp.Content.ReadAsStringAsync();

                resumeResultModel = JsonConvert.DeserializeObject<ResumeResultModel>(respContent);

                Console.WriteLine($"Content {respContent}");
                Console.WriteLine($"Result model {resumeResultModel}");
                //resumeResultModel =  System.Text.Json.JsonSerializer.Deserialize<ResumeResultModel>(respContent);


                personalInformation.FirstName = resumeResultModel.Name;
                personalInformation.Email = resumeResultModel.EmailAddress;

                education.Insitution = resumeResultModel.CollegeName;
                education.Qualification = resumeResultModel.CollegeName;

                workHistory.CompanyName = resumeResultModel.CompaniesWorkedAt;
                workHistory.JobTitle = resumeResultModel.Designation;



                //status = new StringBuilder(await resp.Content.ReadAsStringAsync());
            }

        }

        private void ResetFileNames()
        {
            fileNames = null;
        }

        void Upload()
        {
            //Upload the files here
            /*Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Add("TODO: Upload your files!", Severity.Normal);*/
        }

    }
}