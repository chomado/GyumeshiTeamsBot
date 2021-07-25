// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.14.0

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GyumeshiTeamsBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICustomVisionPredictionClient _customVisionPredictionClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EchoBot> _logger;

        public EchoBot(IHttpClientFactory httpClientFactory, ICustomVisionPredictionClient customVisionPredictionClient, IConfiguration configuration, ILogger<EchoBot> logger)
        {
            _httpClientFactory = httpClientFactory;
            _customVisionPredictionClient = customVisionPredictionClient;
            _configuration = configuration;
            _logger = logger;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string replyText = default;

            if (turnContext.Activity.Attachments?.Any() ?? false)
            {
                var attachment = turnContext.Activity.Attachments.First();

                _logger.LogInformation("★★★" + JsonConvert.SerializeObject(attachment));

                using var image = await _httpClientFactory.CreateClient().GetStreamAsync(attachment.ContentUrl);
                
                var customVision = _configuration.GetSection("CustomVisionConfiguration");
                var projectId = customVision.GetValue<string>("ProjectId");
                var publishedName = customVision.GetValue<string>("PublishedName");
                var classifiedResult = await _customVisionPredictionClient.ClassifyImageAsync(new Guid(projectId), publishedName, image);
                var prediction = classifiedResult.Predictions.FirstOrDefault();
                if (prediction != null && prediction.Probability > 0.75)
                {
                    replyText = $"これは: {prediction.TagName}";
                }
                else
                {
                    replyText = "わかりませんでした💦";
                }
            }
            else
            {
                replyText = "画像を送ってください";
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
