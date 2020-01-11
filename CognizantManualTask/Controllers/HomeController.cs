using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CognizantManualTask.Models;
using Zeebe.Client;
using Newtonsoft.Json;

namespace CognizantManualTask.Controllers
{
    public class HomeController : Controller
    {
        // Inner class for handling POST data from ajax calls
        public class TaskResult
        {
            public string result { get; set; }

            public bool IsValid()
            {
                return result == "PASS" || result == "FAIL";
            }
        }

        private readonly ILogger<HomeController> _logger;
        private readonly IZeebeClient _zeebeClient;

                    
        public HomeController(ILogger<HomeController> logger, IZeebeClient client)
        {
            _logger = logger;
            _zeebeClient = client;
        }

        public IActionResult Index()
        {
            try
            {
                var manualTasks = GetJobs()
                .Select(task => new ManualTaskView()
                {
                    Id = task.WorkflowInstanceKey,
                    CurrentState = ManualTaskView.State.Fail
                })
                .ToList();

                return View(manualTasks);
            }
            catch (Exception e) // TODO specify exception type
            {
                return NotFound();
            }
        }

        public IActionResult Create()
        {
            try
            {
                _zeebeClient.NewCreateWorkflowInstanceCommand()
                .BpmnProcessId("ManualTaskWorkflow")
                .LatestVersion()
                .Send()
                .GetAwaiter()
                .GetResult();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e) // TODO specify exception type
            {
                return NotFound();
            }
        }

        [HttpPost]
        public JsonResult SendResult(long? id, [FromBody] TaskResult result)
        {
            try
            {
                if (id == null)
                {
                    return Json("invalid id");
                }

                if (result == null || !result.IsValid())
                {
                    return Json("invalid result object");
                }

                var job = GetJobs().FirstOrDefault(j => j.WorkflowInstanceKey == id);
                if (job == null)
                {
                    return Json("Workflow instance not found");
                }

                var jobResponse = _zeebeClient.NewCompleteJobCommand(job.Key)
                    .Variables(JsonConvert.SerializeObject(result))
                    .Send()
                    .GetAwaiter()
                    .GetResult();

                return Json("OK");
            }
            catch (Exception e) // TODO specify exception type
            {
                return Json(e);
            }            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IList<Zeebe.Client.Api.Responses.IJob> GetJobs()
        {
            var manualTasks = _zeebeClient.NewActivateJobsCommand()
                .JobType("ManualTask")
                .MaxJobsToActivate(10)
                .Timeout(TimeSpan.FromSeconds(10))
                .WorkerName("jobWorker")
                .Send()
                .GetAwaiter()
                .GetResult();

            return manualTasks.Jobs;
        }
    }
}
