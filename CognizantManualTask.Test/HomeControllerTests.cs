using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Zeebe.Client;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CognizantManualTask.Controllers;
using CognizantManualTask.Models;
using System.Linq;

namespace CognizantManualTask.Test
{
    public class HomeControllerTests
    {

        [Fact]
        public async Task TestIndexReturnsProperTasks()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var zeebeMock = new Mock<IZeebeClient>();
          
            //var asyncTask = Task<IList<Zeebe.Client.Api.Responses.IJob>>.Factory.StartNew(() => GetZeebeActiveJobs() );

            var activeJobResponseTask = Task<Zeebe.Client.Api.Responses.IActivateJobsResponse>.Factory.StartNew(() => GetActivateJobsResponse());

            var commandStep3 = new Mock<Zeebe.Client.Api.Commands.IActivateJobsCommandStep3>();
            commandStep3.Setup(cs => cs.Timeout(TimeSpan.FromSeconds(10))).Returns(commandStep3.Object);
            commandStep3.Setup(cs => cs.WorkerName("jobWorker")).Returns(commandStep3.Object);
            commandStep3.Setup(cs => cs.Send(null)).Returns(activeJobResponseTask);

            var commandStep2 = new Mock<Zeebe.Client.Api.Commands.IActivateJobsCommandStep2>();
            commandStep2.Setup(cs => cs.MaxJobsToActivate(10)).Returns(commandStep3.Object);

            var commandStep1 = new Mock<Zeebe.Client.Api.Commands.IActivateJobsCommandStep1>();
            commandStep1.Setup(cs => cs.JobType("ManualTask")).Returns(commandStep2.Object);

            zeebeMock.Setup(z => z.NewActivateJobsCommand()).Returns(commandStep1.Object);

            var controller = new HomeController(loggerMock.Object, zeebeMock.Object);

            // Act
            var response = controller.Index();

            // Assert
            var viewResult =  Assert.IsType<ViewResult>(response);
            var model = Assert.IsAssignableFrom<IEnumerable<ManualTaskView>>(
                viewResult.ViewData.Model);
            Assert.Single(model);
        }

        private Zeebe.Client.Api.Responses.IActivateJobsResponse GetActivateJobsResponse()
        {
            var job = new Mock<Zeebe.Client.Api.Responses.IJob>();
            var jobs = new List<Zeebe.Client.Api.Responses.IJob>
            {
                job.Object,
            };

            var activeJobResponse = new Mock<Zeebe.Client.Api.Responses.IActivateJobsResponse>();
            activeJobResponse.Setup(r => r.Jobs).Returns(jobs);
            return activeJobResponse.Object;
        }
    }
}